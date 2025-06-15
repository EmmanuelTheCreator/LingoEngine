namespace Director.Primitives
{
    public enum ScriptType
    {
        None = 0,
        Movie = 1,
        Score = 2,
        Cast = 3,
        Sprite = 4,
        Field = 5,
        Button = 6,
        Window = 7,
        Menu = 8,
        Parent = 9,
        Child = 10
    }

    public static class ScriptTypeHelper
    {
        public static string GetName(ScriptType type) => type switch
        {
            ScriptType.Movie => "Movie",
            ScriptType.Score => "Score",
            ScriptType.Cast => "Cast",
            ScriptType.Sprite => "Sprite",
            ScriptType.Field => "Field",
            ScriptType.Button => "Button",
            ScriptType.Window => "Window",
            ScriptType.Menu => "Menu",
            ScriptType.Parent => "Parent",
            ScriptType.Child => "Child",
            _ => $"Unknown ({(int)type})"
        };
    }
}


