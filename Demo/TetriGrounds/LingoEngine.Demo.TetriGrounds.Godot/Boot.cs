using Godot;
public partial class Boot : Node
{
    public override void _Ready()
    {
        // Apply user-defined settings BEFORE loading main scene
        ProjectSettings.SetSetting("display/window/stretch/mode", "disabled");
        ProjectSettings.SetSetting("display/window/stretch/aspect", "ignore");

        // Set fixed window size
        DisplayServer.WindowSetSize(new Vector2I(1600, 1000));

        // Load your main scene manually
        GetTree().ChangeSceneToFile("res://scenes/root_node_tetri_grounds.tscn");
    }
}

