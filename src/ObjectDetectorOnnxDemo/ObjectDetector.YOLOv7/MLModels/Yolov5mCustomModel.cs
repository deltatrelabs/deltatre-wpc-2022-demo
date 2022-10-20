// Based on: https://github.com/mentalstack/yolov5-net

namespace Deltatre.ObjectDetector.YOLOv7.MLModels
{
    using System.Collections.Generic;
    using Model;
    using Abstract;
    public class Yolov5mCustomModel : YoloModel
    {
        //https://github.com/ultralytics/yolov5/blob/master/models/yolov5m.yaml
        public override string Name { get; set; } = "Yolov5mCustom";
        public override string ModelWeightsFilePath { get; set; } = "Assets/ModelWeights/yolov5m_custom.onnx";

        public override int BatchSize { get; set; } = 1;
        public override int Width { get; set; } = 640;
        public override int Height { get; set; } = 640;
        public override int Depth { get; set; } = 3;

        public override int Dimensions { get; set; } = 8;

        public override int[] Strides { get; set; } = new int[] { 8, 16, 32 };

        /*Default anchors for COCO data
        https://github.com/ultralytics/yolov5/blob/master/models/hub/anchors.yaml
        # P6-1280:  thr=0.25: 0.9950 BPR, 5.55 anchors past thr, n=12, img_size=1280, metric_all=0.281/0.714-mean/best, past_thr=0.468-mean: 19,27,  44,40,  38,94,  96,68,  86,152,  180,137,  140,301,  303,264,  238,542,  436,615,  739,380,  925,792
        anchors_p6_1280:
                - [10,13, 16,30, 33,23]  # P3/8
                - [30,61, 62,45, 59,119]  # P4/16
                - [116,90, 156,198, 373,326]  # P5/32
        */
        public override int[][][] Anchors { get; set; } = new int[][][]
        {
            new int[][] { new int[] { 010, 13 }, new int[] { 016, 030 }, new int[] { 033, 023 } },
            new int[][] { new int[] { 030, 61 }, new int[] { 062, 045 }, new int[] { 059, 119 } },
            new int[][] { new int[] { 116, 90 }, new int[] { 156, 198 }, new int[] { 373, 326 } }
        };
        public override int[] Shapes { get; set; } = new int[] { 80, 40, 20 };

        public override float Confidence { get; set; } = 0.20f;
        public override float MulConfidence { get; set; } = 0.25f;
        public override float Overlap { get; set; } = 0.45f;

        public override string[] Inputs { get; set; } = new[] { "images" };
        public override string[] Outputs { get; set; } = new[] { "output" };

        public override List<YoloLabel> Labels { get; set; } = new List<YoloLabel>()
        {
            new YoloLabel { Id = 0, Name = "unknown" },
            new YoloLabel { Id = 1, Name = "ball" },
            new YoloLabel { Id = 2, Name = "person" }
        };

        public override bool UseDetect { get; set; } = true;

        public Yolov5mCustomModel()
        {

        }
    }
}