namespace ProjectorRays.Common;
public class RayStreamAnnotation
{
    public long Address { get; }
    public int Length { get; }
    public string Description { get; }
    public Dictionary<string, int> Keys { get; }

    public RayStreamAnnotation(long address, int length, string description, Dictionary<string, int>? keys = null)
    {
        Address = address;
        Length = length;
        Description = description;
        Keys = keys ?? new();
    }
}

public class RayStreamAnnotatorDecorator
{
    private readonly List<RayStreamAnnotation> _annotations = new();
    public IReadOnlyList<RayStreamAnnotation> Annotations => _annotations;
    public long StreamOffsetBase { get; }

    public RayStreamAnnotatorDecorator(long baseOffset)
    {
        StreamOffsetBase = baseOffset;
    }

    public void Annotate(long relativeOffset, int length, string description, Dictionary<string, int>? keys = null)
    {
        _annotations.Add(new RayStreamAnnotation(StreamOffsetBase + relativeOffset, length, description, keys));
    }

    public List<(long Start, long Length)> GetUnknownRanges(long totalLength)
    {
        var known = _annotations
            .OrderBy(a => a.Address)
            .Select(a => (Start: a.Address, End: a.Address + a.Length))
            .ToList();

        var unknown = new List<(long Start, long Length)>();
        long current = StreamOffsetBase;

        foreach (var (start, end) in known)
        {
            if (start > current)
                unknown.Add((current, start - current));

            current = Math.Max(current, end);
        }

        if (current < StreamOffsetBase + totalLength)
            unknown.Add((current, StreamOffsetBase + totalLength - current));

        return unknown;
    }
}
