namespace Director.Primitives
{
    /// <summary>
    /// Blend modes used when rendering sprites and shapes.
    /// </summary>
    public enum InkType
    {
        Copy = 0,
        Transparent,
        Reverse,
        Ghost,
        NotCopy,
        NotTransparent,
        NotReverse,
        NotGhost,
        Matte,
        Mask,
        // Values 10..31 reserved
        Blend = 32,
        AddPin,
        Add,
        SubPin,
        BackgndTrans,
        Light,
        Sub,
        Dark
    }
}
