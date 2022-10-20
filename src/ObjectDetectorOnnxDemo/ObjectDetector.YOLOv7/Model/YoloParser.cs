// Based on: https://github.com/mentalstack/yolov5-net

namespace Deltatre.ObjectDetector.YOLOv7.Model
{
    using Deltatre.ObjectDetector.YOLOv7.Extensions;
    using Deltatre.ObjectDetector.YOLOv7.MLModels.Abstract;
    using Microsoft.ML.OnnxRuntime.Tensors;
    using System.Collections.Concurrent;
    using System.Drawing;

    /// <summary>
    /// YOLO model outputs parser
    /// </summary>
    public class YoloParser<T> where T : YoloModel
    {
        private readonly T m_model;
        public YoloParser(T model)
        {
            m_model = model;
        }


        /// <summary>
        /// Returns value clamped to the inclusive range of min and max
        /// </summary>
        private static float Clamp(float value, float min, float max)
        {
            return (value < min) ? min : (value > max) ? max : value;
        }

        /// <summary>
        /// Outputs value between 0 and 1
        /// </summary>
        private float Sigmoid(float value)
        {
            return 1 / (1 + (float)Math.Exp(-value));
        }

        /// <summary>
        /// Converts xywh bbox format to xyxy
        /// </summary>
        private static float[] Xywh2xyxy(float[] source)
        {
            var result = new float[4];

            result[0] = source[0] - source[2] / 2f;
            result[1] = source[1] - source[3] / 2f;
            result[2] = source[0] + source[2] / 2f;
            result[3] = source[1] + source[3] / 2f;

            return result;
        }

        /// <summary>
        /// Parses network outputs (sigmoid or detection layer) to predictions
        /// </summary>
        public IEnumerable<YoloPrediction> ParseOutput(List<DenseTensor<float>> output, int imageWidth, int imageHeight)
        {
            return Suppress(m_model.UseDetect ? ParseDetect(output[0], imageWidth, imageHeight) : ParseSigmoid(output, imageWidth, imageHeight));
        }

        public IEnumerable<YoloPrediction> ParseOutput(DenseTensor<float> output, int imageWidth, int imageHeight)
        {
            return Suppress(ParseDetect(output, imageWidth, imageHeight));
        }

        /// <summary>
        /// Parses network output (detection) to predictions
        /// </summary>
        private IEnumerable<YoloPrediction> ParseDetect(DenseTensor<float> output, int imageWidth, int imageHeight)
        {
            var result = new ConcurrentBag<YoloPrediction>();

            var (w, h) = (imageWidth, imageHeight); // image w and h
            var (xGain, yGain) = (m_model.Width / (float)w, m_model.Height / (float)h); // x, y gains
            var gain = Math.Min(xGain, yGain); // gain = resized / original

            var (xPad, yPad) = ((m_model.Width - w * gain) / 2, (m_model.Height - h * gain) / 2); // left, right pads

            //var output = outputRaw.Reshape(new int[] { outputRaw.Dimensions[0], outputRaw.Dimensions[1] * outputRaw.Dimensions[2] * outputRaw.Dimensions[3], outputRaw.Dimensions[4] });
            Parallel.For(0, (int)output.Length / m_model.Dimensions, (i) =>
            {
                if (output[0, i, 4] <= m_model.Confidence) return; // skip low obj_conf results

                Parallel.For(5, m_model.Dimensions, (j) =>
                {
                    output[0, i, j] = output[0, i, j] * output[0, i, 4]; // mul_conf = obj_conf * cls_conf
                });

                Parallel.For(5, m_model.Dimensions, (k) =>
                {
                    if (output[0, i, k] <= m_model.MulConfidence) return; // skip low mul_conf results

                    YoloLabel label = m_model.Labels[k - 5];
                    //if (label.Id != 1 || label.Id != 33) return; // Filter predictions by Person or Ball only (?)

                    float xMin = ((output[0, i, 0] - output[0, i, 2] / 2) - xPad) / gain; // unpad bbox tlx to original
                    float yMin = ((output[0, i, 1] - output[0, i, 3] / 2) - yPad) / gain; // unpad bbox tly to original
                    float xMax = ((output[0, i, 0] + output[0, i, 2] / 2) - xPad) / gain; // unpad bbox brx to original
                    float yMax = ((output[0, i, 1] + output[0, i, 3] / 2) - yPad) / gain; // unpad bbox bry to original

                    xMin = YoloParser<T>.Clamp(xMin, 0, w - 0); // clip bbox tlx to boundaries
                    yMin = YoloParser<T>.Clamp(yMin, 0, h - 0); // clip bbox tly to boundaries
                    xMax = YoloParser<T>.Clamp(xMax, 0, w - 1); // clip bbox brx to boundaries
                    yMax = YoloParser<T>.Clamp(yMax, 0, h - 1); // clip bbox bry to boundaries

                    var prediction = new YoloPrediction(label, output[0, i, k])
                    {
                        Rectangle = new RectangleF(xMin, yMin, xMax - xMin, yMax - yMin)
                    };

                    
                    result.Add(prediction);
                });
            });

            return result;
        }

        /// <summary>
        /// Parses network outputs (sigmoid) to predictions
        /// </summary>
        private IEnumerable<YoloPrediction> ParseSigmoid(List<DenseTensor<float>> output, int imageWidth, int imageHeight)
        {
            var result = new ConcurrentBag<YoloPrediction>();

            var (w, h) = (imageWidth, imageHeight); // image w and h
            var (xGain, yGain) = (m_model.Width / (float)w, m_model.Height / (float)h); // x, y gains
            var gain = Math.Min(xGain, yGain); // gain = resized / original

            var (xPad, yPad) = ((m_model.Width - w * gain) / 2, (m_model.Height - h * gain) / 2); // left, right pads

            Parallel.For(0, output.Count, (i) => // iterate model outputs
            {
                int shapes = m_model.Shapes[i]; // shapes per output

                Parallel.For(0, m_model.Anchors[0].Length, (a) => // iterate anchors
                {
                    Parallel.For(0, shapes, (y) => // iterate shapes (rows)
                    {
                        Parallel.For(0, shapes, (x) => // iterate shapes (columns)
                        {
                            int offset = (shapes * shapes * a + shapes * y + x) * m_model.Dimensions;

                            float[] buffer = output[i].Skip(offset).Take(m_model.Dimensions).Select(Sigmoid).ToArray();

                            if (buffer[4] <= m_model.Confidence) return; // skip low obj_conf results

                            List<float> scores = buffer.Skip(5).Select(b => b * buffer[4]).ToList(); // mul_conf = obj_conf * cls_conf

                            float mulConfidence = scores.Max(); // max confidence score

                            if (mulConfidence <= m_model.MulConfidence) return; // skip low mul_conf results

                            float rawX = (buffer[0] * 2 - 0.5f + x) * m_model.Strides[i]; // predicted bbox x (center)
                            float rawY = (buffer[1] * 2 - 0.5f + y) * m_model.Strides[i]; // predicted bbox y (center)

                            float rawW = (float)Math.Pow(buffer[2] * 2, 2) * m_model.Anchors[i][a][0]; // predicted bbox w
                            float rawH = (float)Math.Pow(buffer[3] * 2, 2) * m_model.Anchors[i][a][1]; // predicted bbox h

                            float[] xyxy = YoloParser<T>.Xywh2xyxy(new float[] { rawX, rawY, rawW, rawH });

                            float xMin = YoloParser<T>.Clamp((xyxy[0] - xPad) / gain, 0, w - 0); // unpad, clip tlx
                            float yMin = YoloParser<T>.Clamp((xyxy[1] - yPad) / gain, 0, h - 0); // unpad, clip tly
                            float xMax = YoloParser<T>.Clamp((xyxy[2] - xPad) / gain, 0, w - 1); // unpad, clip brx
                            float yMax = YoloParser<T>.Clamp((xyxy[3] - yPad) / gain, 0, h - 1); // unpad, clip bry

                            YoloLabel label = m_model.Labels[scores.IndexOf(mulConfidence)];

                            var prediction = new YoloPrediction(label, mulConfidence)
                            {
                                Rectangle = new RectangleF(xMin, yMin, xMax - xMin, yMax - yMin)
                            };

                            result.Add(prediction);
                        });
                    });
                });
            });

            return result;
        }

        /// <summary>
        /// Removes overlapped duplicates (nms)
        /// </summary>
        private List<YoloPrediction> Suppress(IEnumerable<YoloPrediction> items)
        {
            var result = new List<YoloPrediction>(items);

            foreach (var item in items) // Iterate every prediction
            {
                foreach (var current in result.ToList()) // Make a copy for each iteration
                {
                    if (current == item) continue;

                    var (rect1, rect2) = (item.Rectangle, current.Rectangle);

                    RectangleF intersection = RectangleF.Intersect(rect1, rect2);

                    float intArea = intersection.Area(); // intersection area
                    float unionArea = rect1.Area() + rect2.Area() - intArea; // union area
                    float overlap = intArea / unionArea; // overlap ratio

                    if (overlap >= m_model.Overlap)
                    {
                        if (item.Score >= current.Score)
                        {
                            result.Remove(current);
                        }
                    }
                }
            }

            return result;
        }
    }
}
