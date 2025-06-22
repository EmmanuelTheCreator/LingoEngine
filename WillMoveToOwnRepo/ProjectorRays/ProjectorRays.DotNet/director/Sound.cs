using ProjectorRays.Common;

namespace ProjectorRays.Director;

public static class Sound
{
    public static int DecompressSnd(ReadStream input, WriteStream output, int castId)
    {
        // TODO: implement MP3 decoding similar to the original C++ version.
        // For now, simply copy the remaining bytes.
        if (input.Size == 0)
            return 0;

        int toCopy = (int)(input.Size - input.Pos);
        var data = input.ReadBytes(toCopy);
        output.WriteBytes(data);
        return toCopy;
    }
}
