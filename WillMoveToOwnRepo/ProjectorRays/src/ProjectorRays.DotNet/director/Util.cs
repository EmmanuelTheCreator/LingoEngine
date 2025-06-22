namespace ProjectorRays.Director;

public static class Util
{
    public static uint HumanVersion(uint ver)
    {
        if (ver >= 1951) return 1200;
        if (ver >= 1922) return 1150;
        if (ver >= 1921) return 1100;
        if (ver >= 1851) return 1000;
        if (ver >= 1700) return 850;
        if (ver >= 1410) return 800;
        if (ver >= 1224) return 700;
        if (ver >= 1218) return 600;
        if (ver >= 1201) return 500;
        if (ver >= 1117) return 404;
        if (ver >= 1115) return 400;
        if (ver >= 1029) return 310;
        if (ver >= 1028) return 300;
        return 200;
    }

    public static string VersionNumber(uint ver, string fverVersionString)
    {
        uint major = ver / 100;
        uint minor = (ver / 10) % 10;
        uint patch = ver % 10;

        if (string.IsNullOrEmpty(fverVersionString))
        {
            string res = major + "." + minor;
            if (patch != 0)
                res += "." + patch;
            return res;
        }
        else
        {
            return fverVersionString;
        }
    }

    public static string VersionString(uint ver, string fverVersionString)
    {
        uint major = ver / 100;
        string versionNum = VersionNumber(ver, fverVersionString);

        if (major >= 11)
            return "Adobe Director " + versionNum;
        if (major == 10)
            return "Macromedia Director MX 2004 (" + versionNum + ")";
        if (major == 9)
            return "Macromedia Director MX (" + versionNum + ")";
        return "Macromedia Director " + versionNum;
    }
}
