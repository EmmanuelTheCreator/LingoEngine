using Godot;

namespace LingoEngine.Director.LGodot.Tools
{
    /// <summary>
    /// Adds drag-and-drop support via internal GDScript helper.
    /// </summary>
    internal static class DragDropExtensions
    {
        /// <summary>
        /// Starts a Godot-style drag operation from C# using an internal GDScript bridge.
        /// </summary>
        /// <param name="control">The control initiating the drag.</param>
        /// <param name="dragData">The data to return from _GetDragData().</param>
        /// <param name="preview">Optional drag preview node (must be a Control).</param>
        public static void StartDragWorkaround(this Control control, Variant dragData, Control? preview = null)
        {
            if (preview != null)
            {
                preview.CustomMinimumSize = new Vector2(100, 100);
                preview.SizeFlagsHorizontal = Control.SizeFlags.ShrinkCenter;
                preview.SizeFlagsVertical = Control.SizeFlags.ShrinkCenter;
                preview.Modulate = new Color(0, 1, 0, 0.5f); // translucent green
                control.SetDragPreview(preview);
            }

            var script = new GDScript
            {
                SourceCode = """
                extends Control
                func start_drag(data):
                    return data
                """
            };
            script.Reload();

            var helper = new Control();
            helper.SetScript(script);
            control.AddChild(helper);
            helper.CallDeferred("start_drag", dragData);
        }
    }
}
