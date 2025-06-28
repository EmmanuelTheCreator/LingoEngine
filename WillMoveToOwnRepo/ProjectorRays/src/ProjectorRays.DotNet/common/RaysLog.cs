namespace ProjectorRays.Common;

public static class RaysLog
{
    public static bool Verbose = false;

    public static void Write(string msg)
    {
        System.Console.WriteLine(msg);
    }

    public static void Write(System.FormattableString msg)
    {
        System.Console.WriteLine(msg.ToString());
    }

    public static void Debug(string msg)
    {
        if (Verbose)
            Write(msg);
    }

    public static void Debug(System.FormattableString msg)
    {
        if (Verbose)
            Write(msg);
    }

    public static void Warning(string msg)
    {
        System.Console.Error.WriteLine(msg);
    }

    public static void Warning(System.FormattableString msg)
    {
        System.Console.Error.WriteLine(msg.ToString());
    }
}
