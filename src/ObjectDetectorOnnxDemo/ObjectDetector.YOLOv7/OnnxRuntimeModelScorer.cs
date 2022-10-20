namespace Deltatre.ObjectDetector.YOLOv7
{
    using Deltatre.ObjectDetector.YOLOv7.Interfaces;
    using Deltatre.ObjectDetector.YOLOv7.MLModels.Abstract;
    using Deltatre.ObjectDetector.YOLOv7.Model;
    using Microsoft.ML.OnnxRuntime;
    using Microsoft.ML.OnnxRuntime.Tensors;
    using OpenCvSharp;
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.Drawing.Imaging;
    using System.IO;
    using System.Numerics;
    using System.Runtime.InteropServices;

    /// <summary>
    /// YOLOv7 scorer
    /// </summary>
    public class OnnxRuntimeModelScorer<T> : IObjectDetectorModel, IDisposable where T : YoloModel
    {
        #region Private fields
        private readonly T m_model;
        private readonly YoloParser<T> m_outputParser;
        private readonly InferenceSession m_inferenceSession;
        private readonly DenseTensor<float> m_inputTensor;
        private readonly DenseTensor<float> m_outputTensor;
        private readonly float[] m_tempInFloatBuffer;
        private readonly IReadOnlyCollection<FixedBufferOnnxValue> m_fixedInBufferOnnxValues;
        private readonly IReadOnlyCollection<FixedBufferOnnxValue> m_fixedOutBufferOnnxValues;
        private readonly IReadOnlyCollection<string> m_namedOnnxInputs;
        private readonly IReadOnlyCollection<string> m_namedOnnxOutputs;
        private bool m_disposedValue;
        #endregion

        #region Properties
        public string Name => $"Onnx[{m_model.Name}]";
        #endregion

        #region Constructor
        public OnnxRuntimeModelScorer(SessionOptions? opts = null)
        {
            m_model = Activator.CreateInstance<T>();
            m_outputParser = new YoloParser<T>(m_model);
            m_inferenceSession = new InferenceSession(File.ReadAllBytes(m_model.ModelWeightsFilePath), opts ?? new SessionOptions());

            m_inputTensor = new DenseTensor<float>(dimensions: new[] { m_model.BatchSize, m_model.Depth, m_model.Height, m_model.Width });
            m_tempInFloatBuffer = new float[m_model.BatchSize * m_model.Depth * m_model.Height * m_model.Width];

            var totalSize = 0;
            foreach (var shape in m_model.Shapes)
            {
                totalSize += m_model.Depth * shape * shape;
            }
            m_outputTensor = new DenseTensor<float>(dimensions: new[] { m_model.BatchSize, totalSize, m_model.Dimensions });

            m_fixedInBufferOnnxValues = new List<FixedBufferOnnxValue>
            {
                FixedBufferOnnxValue.CreateFromTensor(m_inputTensor)
            };

            m_fixedOutBufferOnnxValues = new List<FixedBufferOnnxValue>
            {
                FixedBufferOnnxValue.CreateFromTensor(m_outputTensor)
            };

            m_namedOnnxInputs = m_model.Inputs.ToList().AsReadOnly();
            m_namedOnnxOutputs = m_model.Outputs.ToList().AsReadOnly();
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!m_disposedValue)
            {
                if (disposing)
                {
                    // Dispose managed state (managed objects)
                    m_inferenceSession.Dispose();
                    foreach (var v in m_fixedInBufferOnnxValues)
                    {
                        v.Dispose();
                    }
                    foreach (var v in m_fixedOutBufferOnnxValues)
                    {
                        v.Dispose();
                    }
                }

                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                // TODO: set large fields to null
                m_disposedValue = true;
            }
        }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Runs object detection on the provided image data
        /// </summary>
        /// <remarks>This method is not thread-safe, please call from 1 thread only</remarks>
        public IEnumerable<YoloPrediction> Score(byte[] imageData, int width, int height)
        {
            // Prepare input for inference
            //////////////////////////////

            byte[] inputBuffer = ScaleFrameWithPaddingIfNeeded(imageData, width, height);

            //System.Drawing.Image image = ImageFromRawBGRArray(imageData, width, height, PixelFormat.Format24bppRgb);
            //image.Save("debug_original.png", ImageFormat.Png);
            //image.Dispose();

            //image = ImageFromRawBGRArray(inputBuffer, m_model.Width, m_model.Height, PixelFormat.Format24bppRgb);
            //image.Save("debug_scaled.png", ImageFormat.Png);
            //image.Dispose();

            ImageToTensor(inputBuffer, m_inputTensor, m_tempInFloatBuffer, m_model.Depth, m_model.Height, m_model.Width);

            // Execute inference
            ////////////////////

            m_inferenceSession.Run(inputNames: m_namedOnnxInputs, inputValues: m_fixedInBufferOnnxValues, outputNames: m_namedOnnxOutputs, outputValues: m_fixedOutBufferOnnxValues);

            // Retrieve and parse results
            /////////////////////////////

            return m_outputParser.ParseOutput(m_outputTensor, width, height);
        }
        #endregion

        #region Private methods
        private static void ImageToTensor(byte[] pixels, DenseTensor<float> tensor, float[] data, int bytesPerPixel, int height, int width)
        {
            var channelSize = width * height;
            var redChannel = 0;
            var greenChannel = channelSize;
            var blueChannel = channelSize * 2;

            int floatSlots = Vector<float>.Count;
            var stride = width * bytesPerPixel;
            var floatSlotSize = floatSlots * bytesPerPixel;

            const float normalizeFactor = 1.0F / 255.0F;

            _ = Parallel.For(0, height, y =>
            {
                int rowStart = y * stride;
                int rowWidth = stride;

                var row = new Span<byte>(pixels, rowStart, rowWidth);

                int numOfVectors = row.Length / floatSlotSize;
                int ceiling = numOfVectors * floatSlotSize;

                var reds = new float[floatSlots];
                var greens = new float[floatSlots];
                var blues = new float[floatSlots];

                for (int i = 0; i < ceiling; i += floatSlotSize)
                {
                    for (int j = 0; j < floatSlotSize; j += bytesPerPixel)
                    {
                        var index = j / bytesPerPixel;
                        reds[index] = row[i + j + 2] * normalizeFactor;     // r
                        greens[index] = row[i + j + 1] * normalizeFactor;   // g
                        blues[index] = row[i + j] * normalizeFactor;        // b
                    }

                    var vecRed = new Vector<float>(reds);
                    var vecGreen = new Vector<float>(greens);
                    var vecBlue = new Vector<float>(blues);

                    var arrIndex = i / bytesPerPixel + y * width;

                    vecRed.CopyTo(data, redChannel + arrIndex);
                    vecGreen.CopyTo(data, greenChannel + arrIndex);
                    vecBlue.CopyTo(data, blueChannel + arrIndex);
                }

                for (int i = ceiling; i < row.Length; i += bytesPerPixel)
                {
                    var index = i / bytesPerPixel + y * width;

                    data[redChannel + index] = row[i + 2] * normalizeFactor;   // r
                    data[greenChannel + index] = row[i + 1] * normalizeFactor; // g
                    data[blueChannel + index] = row[i] * normalizeFactor;      // b
                }
            });

            data.CopyTo(tensor.Buffer);
        }

        private byte[] ScaleFrameWithPaddingIfNeeded(byte[] inputBuffer, int width, int height)
        {
            if (width != m_model.Width || height != m_model.Height)
            {
                var outputBuffer = new byte[m_model.Width * m_model.Height * m_model.Depth];

                var imgIn = new Mat(height, width, MatType.CV_8UC3, inputBuffer);

                float ratio = (float)Math.Max(m_model.Width, m_model.Height) / Math.Max(width, height);
                var outputSize = new OpenCvSharp.Size(width * ratio, height * ratio);
                var imgOut = new Mat(outputSize, MatType.CV_8UC3);
                Cv2.Resize(imgIn, imgOut, outputSize, interpolation: InterpolationFlags.Linear); // Fit image size to specified input size

                // Add padding (as done in Ultralytics reference code)
                // See: https://github.com/ultralytics/yolov5/blob/f0e5a608f50ac647827bede88fded7908c7edeab/utils/augmentations.py#L109
                var delta_w = m_model.Width - outputSize.Width;
                var delta_h = m_model.Height - outputSize.Height;
                var top = (int)Math.Round(delta_h / 2 - 0.1);
                var bottom = (int)Math.Round(delta_h - (delta_h / 2) + 0.1);
                var left = (int)Math.Round(delta_w / 2 - 0.1);
                var right = (int)Math.Round(delta_w - (delta_w / 2) + 0.1);
                Cv2.CopyMakeBorder(imgOut, imgOut, top, bottom, left, right, BorderTypes.Constant, value: Scalar.FromRgb(114, 114, 114));

                Marshal.Copy(imgOut.Data, outputBuffer, 0, outputBuffer.Length);
                inputBuffer = outputBuffer;
            }

            return inputBuffer;
        }

        private static Image ImageFromRawBGRArray(byte[] arr, int width, int height, PixelFormat pixelFormat)
        {
            var output = new Bitmap(width, height, pixelFormat);
            var rect = new Rectangle(0, 0, width, height);
            var bmpData = output.LockBits(rect, ImageLockMode.ReadWrite, output.PixelFormat);

            // Full buffer copy
            Marshal.Copy(arr, 0, bmpData.Scan0, arr.Length);
            output.UnlockBits(bmpData);
            return output;
        }
        #endregion
    }
}
