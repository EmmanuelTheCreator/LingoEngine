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

        public enum BlShapeType
        {
            Rectangle,
            RoundRectangle,
            Oval,
            Line,
            PolyLine
        }
    }
}
