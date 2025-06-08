using Godot;

namespace LingoEngineGodot
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
    using LingoEngine.FrameworkCommunication;
    using LingoEngine.Pictures.LingoEngine;
    using LingoEngineGodot.Pictures;
    using System;
    using static Godot.Input;

    public partial class LingoGodotMouse : Area2D, ILingoFrameworkMouse
    {
        private readonly Lazy<LingoMouse> _lingoMouse;
        private DateTime _lastClickTime = DateTime.MinValue;
        private const double DOUBLE_CLICK_TIME_LIMIT = 0.25;  // 250 milliseconds for double-click detection
        private CollisionShape2D _collisionShape2D = new();
        private RectangleShape2D _RectangleShape2D = new();

        public LingoGodotMouse(Node rootNode, Lazy<LingoMouse> lingoMouse)
        {
            Name = "MouseConnector";
            _lingoMouse = lingoMouse;
            rootNode.AddChild(this);
            AddChild(_collisionShape2D);
            _RectangleShape2D.Size = new Vector2(1000, 1000);
            _collisionShape2D.Shape = _RectangleShape2D;
            Input.MouseMode = Input.MouseModeEnum.Hidden;
        }
       

        // This method will be called when Godot's input_event is triggered
            public override void _InputEvent(Viewport viewport, InputEvent inputEvent, int shapeIdx)
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
                        _lingoMouse.Value.MouseDown = true;
                        _lingoMouse.Value.LeftMouseDown = true;
                        DetectDoubleClick();
                        _lingoMouse.Value.DoMouseDown();
                    }
                    else if (mouseButtonEvent.ButtonIndex == MouseButton.Right)
                    {
                        // Handle Right Button Down
                        _lingoMouse.Value.RightMouseDown = true;
                        _lingoMouse.Value.DoMouseDown();
                    }
                    else if (mouseButtonEvent.ButtonIndex == MouseButton.Middle)
                    {
                        // Handle Middle Button Down
                        _lingoMouse.Value.MiddleMouseDown = true;
                        _lingoMouse.Value.DoMouseDown();
                    }              
                }
                // Handle Mouse Up event
                else
                {
                    if (mouseButtonEvent.ButtonIndex == MouseButton.Left)
                    {
                        _lingoMouse.Value.MouseDown = false;
                        _lingoMouse.Value.LeftMouseDown = false;
                        _lingoMouse.Value.DoMouseUp();
                    }
                    else if (mouseButtonEvent.ButtonIndex == MouseButton.Right)
                    {
                        _lingoMouse.Value.RightMouseDown = false;
                        _lingoMouse.Value.DoMouseUp();
                    }
                    else if (mouseButtonEvent.ButtonIndex == MouseButton.Middle)
                    {
                        _lingoMouse.Value.MiddleMouseDown = false;
                        _lingoMouse.Value.DoMouseUp();
                    }
                }
            }
            // Handle Mouse Motion (MouseMove)
            else if (inputEvent is InputEventMouseMotion mouseMotionEvent)
            {
                var x = mouseMotionEvent.Position.X;
                var y = mouseMotionEvent.Position.Y;
                _lingoMouse.Value.MouseH = x;
                _lingoMouse.Value.MouseV = y;
                _lingoMouse.Value.DoMouseMove();
            }              
        }

        private void DetectDoubleClick()
        {
            // Get the current time
            DateTime currentTime = DateTime.Now;

            // Check if double-click occurred within the time limit
            if (_lastClickTime != DateTime.MinValue && (currentTime - _lastClickTime).TotalSeconds <= DOUBLE_CLICK_TIME_LIMIT)
                _lingoMouse.Value.DoubleClick = true;
            else
                _lingoMouse.Value.DoubleClick = false;

            _lastClickTime = currentTime; // Update last click time
        }

        public void HideMouse(bool state)
        {
            MouseMode = state ? MouseModeEnum.Hidden : MouseModeEnum.Visible;
        }
        public void SetCursor(LingoMemberPicture image)
        {
            var frameworkObj = image.Framework<LingoGodotMemberPicture>();
            if (frameworkObj.Texture == null)
                return;
            DisplayServer.Singleton.CursorSetCustomImage(frameworkObj.Texture, DisplayServer.CursorShape.Arrow, hotspot: Vector2.Zero);
        }

        public void SetCursor(LingoMouseCursor cursor)
        {
            if (cursor == LingoMouseCursor.Blank)
            {
                MouseMode = MouseModeEnum.Hidden;
                return;
            }
            var godotCursor = ToGodotCursor(cursor);
            DisplayServer.Singleton.CursorSetShape(godotCursor);

        }
        /// <summary>
        /// Converts a LingoMouseCursor to the equivalent Godot CursorShape.
        /// </summary>
        public static DisplayServer.CursorShape ToGodotCursor(LingoMouseCursor cursor)
        {
            return cursor switch
            {
                LingoMouseCursor.Arrow => DisplayServer.CursorShape.Arrow,
                LingoMouseCursor.Cross => DisplayServer.CursorShape.Cross,
                LingoMouseCursor.Watch => DisplayServer.CursorShape.Wait,
                LingoMouseCursor.IBeam => DisplayServer.CursorShape.Ibeam,
                LingoMouseCursor.SizeAll => DisplayServer.CursorShape.Move,
                LingoMouseCursor.SizeNWSE => DisplayServer.CursorShape.Bdiagsize,
                LingoMouseCursor.SizeNESW => DisplayServer.CursorShape.Fdiagsize,
                LingoMouseCursor.SizeWE => DisplayServer.CursorShape.Hsize,
                LingoMouseCursor.SizeNS => DisplayServer.CursorShape.Vsize,
                LingoMouseCursor.UpArrow => DisplayServer.CursorShape.Arrow, // not correct
                LingoMouseCursor.Blank => DisplayServer.CursorShape.Arrow, // not correct
                LingoMouseCursor.Finger => DisplayServer.CursorShape.PointingHand,
                LingoMouseCursor.Drag => DisplayServer.CursorShape.Drag,
                LingoMouseCursor.Help => DisplayServer.CursorShape.Help,
                LingoMouseCursor.Wait => DisplayServer.CursorShape.Busy,
                _ => DisplayServer.CursorShape.Arrow
            };
        }
    }

}