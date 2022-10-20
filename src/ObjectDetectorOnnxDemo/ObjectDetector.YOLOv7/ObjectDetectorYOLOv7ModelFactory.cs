namespace Deltatre.ObjectDetector.YOLOv7
{
    using Deltatre.ObjectDetector.YOLOv7.Interfaces;
    using Deltatre.ObjectDetector.YOLOv7.Model;
    using Microsoft.ML.OnnxRuntime;
    using System;

    public class ObjectDetectorYOLOv7ModelFactory : IObjectDetectorModelFactory
    {
        public IObjectDetectorModel GetModel(string model, int deviceId)
        {
            SessionOptions? sessionOptions = null;
            if (deviceId >= 0)
            {
                sessionOptions = SessionOptions.MakeSessionOptionWithCudaProvider(deviceId);
            }

            return model switch
            {
                "YOLOv5m" => new OnnxRuntimeModelScorer<MLModels.Yolov5mModel>(sessionOptions),
                "YOLOv5m_custom" => new OnnxRuntimeModelScorer<MLModels.Yolov5mCustomModel>(sessionOptions),
                "YOLOv7_640" => new OnnxRuntimeModelScorer<MLModels.Yolov7_640Model>(sessionOptions),
                "YOLOv7_D6_1280" => new OnnxRuntimeModelScorer<MLModels.Yolov7d6_1280Model>(sessionOptions),
                "YOLOv7_E6_1280" => new OnnxRuntimeModelScorer<MLModels.Yolov7e6_1280Model>(sessionOptions),
                "YOLOv7_E6E_640" => new OnnxRuntimeModelScorer<MLModels.Yolov7e6e_640Model>(sessionOptions),
                "YOLOv7_W6_1280" => new OnnxRuntimeModelScorer<MLModels.Yolov7w6_1280Model>(sessionOptions),
                "YOLOv7_X_640" => new OnnxRuntimeModelScorer<MLModels.Yolov7x_640Model>(sessionOptions),
                _ => throw new NotSupportedException($"Selected object detector ({model}) is not supported"),
            };
        }
    }
}
