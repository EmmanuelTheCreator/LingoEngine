using Godot;

namespace ArkGodot.GodotLinks
{

    /*
    # In Godot's GDScript
var lingoMouse = LingoMouse.new()  # Create a LingoMouse instance
var lingoGodotMouse = LingoGodotMouse.new(lingoMouse)
add_child(lingoGodotMouse)  # Add it to the scene
    */
    /// <summary>
    /// Communication between the Godot engine and the Lingo mouse object
    /// </summary>
    using Godot;
    using LingoEngine.Core;
    using System;

    public partial class LingoGodotMouse : Node
    {
        private readonly LingoMouse _lingoMouse;
        private DateTime _lastClickTime = DateTime.MinValue;
        private const double DOUBLE_CLICK_TIME_LIMIT = 0.25;  // 250 milliseconds for double-click detection


        public LingoGodotMouse(LingoMouse lingoMouse)
        {
            _lingoMouse = lingoMouse;
        }

        // Called when the node enters the scene tree for the first time.
        public override void _Ready()
        {
            // Connect the input_event signal to our handler method
            this.Connect("input_event", new Callable(this, nameof(OnInputEvent)));
        }

        // This method will be called when Godot's input_event is triggered
        private void OnInputEvent(Node camera, InputEvent inputEvent, int shapeIdx)
        {
            // Handle mouse button events (MouseDown and MouseUp)
            if (inputEvent is InputEventMouseButton mouseButtonEvent)
            {
                var x = mouseButtonEvent.Position.X;
                var y = mouseButtonEvent.Position.Y;

                // Handle Mouse Down event
                if (mouseButtonEvent.Pressed)
                {
                    if (mouseButtonEvent.ButtonIndex == MouseButton.Left)
                    {
                        // Handle Left Button Down
                        _lingoMouse.MouseDown = true;
                        _lingoMouse.LeftMouseDown = true;
                        DetectDoubleClick();
                        _lingoMouse.DoMouseDown();
                    }
                    else if (mouseButtonEvent.ButtonIndex == MouseButton.Right)
                    {
                        // Handle Right Button Down
                        _lingoMouse.RightMouseDown = true;
                        _lingoMouse.DoMouseDown();
                    }
                    else if (mouseButtonEvent.ButtonIndex == MouseButton.Middle)
                    {
                        // Handle Middle Button Down
                        _lingoMouse.MiddleMouseDown = true;
                        _lingoMouse.DoMouseDown();
                    }
                }
                // Handle Mouse Up event
                else
                {
                    if (mouseButtonEvent.ButtonIndex == MouseButton.Left)
                    {
                        _lingoMouse.MouseDown = false;
                        _lingoMouse.LeftMouseDown = false;
                        _lingoMouse.DoMouseUp();
                    }
                    else if (mouseButtonEvent.ButtonIndex == MouseButton.Right)
                    {
                        _lingoMouse.RightMouseDown = false;
                        _lingoMouse.DoMouseUp();
                    }
                    else if (mouseButtonEvent.ButtonIndex == MouseButton.Middle)
                    {
                        _lingoMouse.MiddleMouseDown = false;
                        _lingoMouse.DoMouseUp();
                    }
                }
            }
            // Handle Mouse Motion (MouseMove)
            else if (inputEvent is InputEventMouseMotion mouseMotionEvent)
            {
                var x = mouseMotionEvent.Position.X;
                var y = mouseMotionEvent.Position.Y;
                _lingoMouse.MouseH = x;
                _lingoMouse.MouseV = y;
                _lingoMouse.DoMouseMove();
            }
        }

        private void DetectDoubleClick()
        {
            // Get the current time
            DateTime currentTime = DateTime.Now;

            // Check if double-click occurred within the time limit
            if (_lastClickTime != DateTime.MinValue && (currentTime - _lastClickTime).TotalSeconds <= DOUBLE_CLICK_TIME_LIMIT)
                _lingoMouse.DoubleClick = true;
            else
                _lingoMouse.DoubleClick = false;

            _lastClickTime = currentTime; // Update last click time
        }
    }

}