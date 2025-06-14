namespace Director.Primitives
{
    public static class VersionHelper
    {
        public static int HumanVersion(int ver)
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
    }
}
