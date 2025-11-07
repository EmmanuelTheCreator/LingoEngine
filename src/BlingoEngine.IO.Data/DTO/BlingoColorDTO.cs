namespace BlingoEngine.IO.Data.DTO;

public struct BlingoColorDTO
{
    public int Code { get; set; }
    public string Name { get; set; }
    public byte R { get; set; }
    public byte G { get; set; }
    public byte B { get; set; }
    public byte A { get; set; }

    public BlingoColorDTO(byte r, byte g, byte b, byte a = 255)
    {
        R = r;
        G = g;
        B = b;
        A = a;
        Name = "";
        Code = -1;
    }
    public BlingoColorDTO(int code, string name, byte r, byte g, byte b, byte a)
    {
        Code = code;
        Name = name;
        R = r;
        G = g;
        B = b;
        A = a;
    }
    public static bool operator ==(BlingoColorDTO left, BlingoColorDTO right) =>
    left.R == right.R && left.G == right.G && left.B == right.B && left.A == right.A;

    public static bool operator !=(BlingoColorDTO left, BlingoColorDTO right) => !(left == right);

    public override bool Equals(object? obj) =>
        obj is BlingoColorDTO other && this == other;

    public override int GetHashCode() => HashCode.Combine(R, G, B, A);

    public BlingoColorDTO Clone() => new BlingoColorDTO(Code, Name, R, G, B, A);

}

