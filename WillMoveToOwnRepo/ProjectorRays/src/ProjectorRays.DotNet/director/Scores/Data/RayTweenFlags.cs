namespace ProjectorRays.director.Scores.Data;

public struct RayTweenFlags
{
    public bool TweeningEnabled;
    public bool Path;
    public bool Size;
    public bool Rotation;
    public bool Skew;
    public bool Blend;
    public bool ForeColor;
    public bool BackColor;
    public override string ToString()
    {
        List<string> flags = new();
        if (Path) flags.Add("Path");
        if (Size) flags.Add("Size");
        if (Rotation) flags.Add("Rotation");
        if (Skew) flags.Add("Skew");
        if (Blend) flags.Add("Blend");
        if (ForeColor) flags.Add("ForeColor");
        if (BackColor) flags.Add("BackColor");
        return $"Tweening: {(TweeningEnabled ? "On" : "Off")} | " + string.Join(", ", flags);
    }

    public byte ToByte()
    {
        byte result = 0;
        if (TweeningEnabled) result |= 0x01;
        if (Path) result |= 0x02;
        if (Size) result |= 0x04;
        if (Rotation) result |= 0x08;
        if (Skew) result |= 0x10;
        if (Blend) result |= 0x20;
        if (ForeColor) result |= 0x40;
        if (BackColor) result |= 0x80;
        return result;
    }

}



