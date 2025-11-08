using BlingoEngine.IO.Data.DTO;
using System.Diagnostics;

namespace BlingoEngine.IO.Legacy.Cast.Data
{
    public class BlCastRawMemberShape : BlCastRawMemberItem
    {
        public BlCastRawMemberShape()
        {
            MemberType = BlLegacyCastMemberType.Shape;
        }

        public BlShapeType ShapeType { get; set; }
        public bool Fill { get; set; }
        public int Height { get; set; }
        public int Width { get; set; }
        public bool GradientIsRadial { get; internal set; }
        public int GradientXOffset { get; internal set; }
        public int GradientyOffset { get; internal set; }
        public int GradientyCycles { get; internal set; }
        public float GradientSpread { get; internal set; }
        public float GradientAngle { get; internal set; }
        public List<BlShapeCurve>? Curves { get; set; }
        public bool IsGradientFill { get; internal set; }
        public float StrokeWidth { get; internal set; }
        public BlShapeScaleMode ScaleMode { get; internal set; }
        public float Scale { get; internal set; }

        public enum BlShapeType
        {
            Rectangle,
            RoundRectangle,
            Oval,
            Line,
            PolyLine
        }
        public enum BlShapeScaleMode
        {
            ShowAll,
            NoBorder,
            ExactFit,
            AutoSize,
            NoScale
        }
        public class BlShapeCurve
        {
            public List<BlShapeVertex> Vertices { get; set; } = new();
        }
        [DebuggerDisplay("PointDto:Pos={Position}|Handle1={Handle1.GetValueOrDefault()}|Handle2={Handle2.GetValueOrDefault()}")]
        public class BlShapeVertex
        {
            public BlingoPointDTO Position { get; set; }
            public BlingoPointDTO? Handle1 { get; set; }
            public BlingoPointDTO? Handle2 { get; set; }

        }
    }
}
