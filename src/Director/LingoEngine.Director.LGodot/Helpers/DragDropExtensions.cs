using Godot;

namespace LingoEngine.Director.LGodot.Helpers
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
            var script = new GDScript
            {
                SourceCode = """
        extends Control
        func start_drag(data, preview):
            set_drag_preview(preview)
            return data
        """
            };
            script.Reload();

            var helper = new Control();
            helper.SetScript(script);
            control.AddChild(helper); // Must be in tree for set_drag_preview to work

            if (preview != null)
                helper.SetDragPreview(preview);

            helper.CallDeferred("start_drag", dragData); // ✅ FIXED
        }

    }

}
