using LingoEngine.Director.Core.Styles;
using LingoEngine.FrameworkCommunication;
using LingoEngine.Gfx;
using LingoEngine.Primitives;
using LingoEngine.Sprites;

namespace LingoEngine.Director.Core.Inspector
{
    public interface IDirectorBehaviorDescriptionManager
    {
        /// <summary>Builds a popup window for editing the given behavior.</summary>
        LingoGfxWindow? BuildBehaviorPopup(LingoSpriteBehavior behavior, Action onClose);
    }

    internal class DirectorBehaviorDescriptionManager : IDirectorBehaviorDescriptionManager
    {
        private readonly ILingoFrameworkFactory _factory;

        public DirectorBehaviorDescriptionManager(ILingoFrameworkFactory factory)
        {
            _factory = factory;
            
        }

       
        public LingoGfxWindow? BuildBehaviorPopup(LingoSpriteBehavior behavior, Action onClose)
        {
            if (!(behavior is ILingoPropertyDescriptionList))
            return null;
            var width = 390;
            var height = 250;
            var rightWidth = 90;

            var win = _factory.CreateWindow("BehaviorParams", $"Parameters for \"{behavior.Name}\"");
            var root = _factory.CreateWrapPanel(LingoOrientation.Horizontal, "BehaviorPopupRoot");
            win.AddItem(root);
            win.Width = width;
            win.Height = height;
            win.BackgroundColor = DirectorColors.BG_WhiteMenus;
            win.IsPopup = true;

            var panel = BuildBehaviorPanel(behavior, width- rightWidth-10, height);
            root.AddItem(panel);

            var vLine = _factory.CreateVerticalLineSeparator("BehaviorPopupLine");
            vLine.Height = height;
            root.AddItem(vLine);

            var right = _factory.CreateWrapPanel(LingoOrientation.Vertical, "BehaviorPopupRight");
            right.Width = rightWidth;
            var ok = _factory.CreateButton("BehaviorPopupOk", "OK");
            ok.Width = 74;
            float margin = (rightWidth - ok.Width) / 2f;
            ok.Margin = new LingoMargin(margin, 0, margin, 0);
            ok.Pressed += () =>
            {
                win.Hide();
                onClose();
            };
            right.AddItem(ok);
            root.AddItem(right);

            return win;
        }

        private ILingoGfxLayoutNode BuildBehaviorPanel(LingoSpriteBehavior behavior, int width, int height)
        {
            // todo : fix container with scroll
            //var scroller = _factory.CreateScrollContainer("BehaviorPanelScroller");
            //scroller.Width = width;
            //scroller.Height = height;
            var container = _factory.CreateWrapPanel(LingoOrientation.Vertical, "BehaviorPanel");
            container.Width = width;
            container.Height = height;
            //scroller.AddItem(container);
            //var propsPanel = BuildProperties(behavior);
            //container.AddItem(propsPanel);
            if (behavior is ILingoPropertyDescriptionList descProvider)
                BuildDescriptionList(behavior, container, descProvider);

            return container;
        }


        private void BuildDescriptionList(LingoSpriteBehavior behavior, LingoGfxWrapPanel container, ILingoPropertyDescriptionList descProvider)
        {
            string? desc = descProvider.GetBehaviorDescription();
            if (!string.IsNullOrEmpty(desc))
            {
                var descLabel = _factory.CreateLabel("BehaviorDescLabel_"+ behavior.Name, desc);
                descLabel.FontColor = LingoColorList.Black;
                descLabel.Width = 200;
                descLabel.FontSize = 10;
                descLabel.WrapMode = LingoTextWrapMode.WordSmart;
                container.AddItem(descLabel);
            }

            var props = behavior.UserProperties;
            var definitions = descProvider.GetPropertyDescriptionList();
            if (definitions != null)
            {
                foreach (var prop in definitions)
                {
                    var row = _factory.CreateWrapPanel(LingoOrientation.Horizontal, $"PropRow_{prop.Key}");
                    string labelText = !string.IsNullOrEmpty(prop.Value.Comment) ? prop.Value.Comment! : prop.Key.ToString();
                    var label = _factory.CreateLabel($"PropLabel_{prop.Key}", labelText);
                    label.Width = 80;
                    label.WrapMode = LingoTextWrapMode.WordSmart;
                    label.FontColor = DirectorColors.TextColorLabels;
                    label.FontSize = 10;
                    row.AddItem(label);

                    object? current = props[prop.Key];
                    if (current == null)
                        current = prop.Value.Default;

                    string format = prop.Value.Format.Name.ToLowerInvariant();
                    if (format == "string")
                    {
                        var input = _factory.CreateInputText($"PropInput_{prop.Key}");
                        input.Width = 70;
                        input.Text = current?.ToString() ?? string.Empty;
                        input.ValueChanged += () => props[prop.Key] = input.Text;
                        row.AddItem(input);
                    }
                    else if (format == "int" || format == "integer")
                    {
                        var input = _factory.CreateInputNumber($"PropInput_{prop.Key}");
                        input.Width = 70;
                        input.NumberType = LingoNumberType.Integer;
                        if (current is int i)
                            input.Value = i;
                        else if (current is float f)
                            input.Value = f;
                        else if (current != null && float.TryParse(current.ToString(), out var fv))
                            input.Value = fv;
                        input.ValueChanged += () => props[prop.Key] = (int)input.Value;
                        row.AddItem(input);
                    }
                    else if (format == "boolean" || format == "bool")
                    {
                        var input = _factory.CreateInputCheckbox($"PropInput_{prop.Key}");
                        input.Width = 70;
                        if (current is bool bval)
                            input.Checked = bval;
                        else if (current is string s && bool.TryParse(s, out var bv))
                            input.Checked = bv;
                        input.ValueChanged += () => props[prop.Key] = input.Checked;
                        row.AddItem(input);
                    }
                    else
                    {
                        var input = _factory.CreateInputText($"PropInput_{prop.Key}");
                        input.Width = 70;
                        input.Text = current?.ToString() ?? string.Empty;
                        input.ValueChanged += () => props[prop.Key] = input.Text;
                        row.AddItem(input);
                    }

                    container.AddItem(row);
                }
            }
            //// old, wrong
            //if (props.Count > 0)
            //{
            //    container.AddItem(_factory.CreateLabel("PropsLabel", "Properties"));
            //    foreach (var item in props)
            //    {
            //        string labelText = item.Key.ToString();
            //        if (props.DescriptionList != null && props.DescriptionList.TryGetValue(item.Key, out var desc2) && !string.IsNullOrEmpty(desc2.Comment))
            //            labelText = desc2.Comment!;
            //        var row = _factory.CreateWrapPanel(LingoOrientation.Horizontal, "BehPropRow");
            //        row.AddItem(_factory.CreateLabel("PropName", labelText));
            //        row.AddItem(_factory.CreateLabel("PropVal", item.Value?.ToString() ?? string.Empty));
            //        container.AddItem(row);
            //    }
            //}
        }

        public LingoGfxWrapPanel BuildProperties(object obj)
        {
            ILingoFrameworkFactory factory = _factory;
            var root = factory.CreateWrapPanel(LingoOrientation.Vertical, $"{obj.GetType().Name}Props");
            foreach (var prop in obj.GetType().GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance))
            {
                if (!prop.CanRead)
                    continue;
                if (!IsSimpleType(prop.PropertyType) && prop.PropertyType != typeof(LingoPoint))
                    continue;

                var row = factory.CreateWrapPanel(LingoOrientation.Horizontal, prop.Name + "Row");
                var label = factory.CreateLabel(prop.Name + "Label", prop.Name);
                label.Width = 80;
                row.AddItem(label);

                object? val = prop.GetValue(obj);

                if (prop.PropertyType == typeof(bool))
                {
                    var cb = factory.CreateInputCheckbox(prop.Name + "Check");
                    cb.Checked = val is bool b && b;
                    if (prop.CanWrite)
                        cb.ValueChanged += () => prop.SetValue(obj, cb.Checked);
                    else
                        cb.Enabled = false;
                    row.AddItem(cb);
                }
                else if (prop.PropertyType == typeof(LingoPoint))
                {
                    var point = val is LingoPoint p ? p : new LingoPoint();
                    var xSpin = factory.CreateSpinBox(prop.Name + "X");
                    var ySpin = factory.CreateSpinBox(prop.Name + "Y");
                    xSpin.Value = point.X;
                    ySpin.Value = point.Y;
                    if (prop.CanWrite)
                    {
                        xSpin.ValueChanged += () =>
                        {
                            var pVal = (LingoPoint)prop.GetValue(obj)!;
                            pVal.X = xSpin.Value;
                            prop.SetValue(obj, pVal);
                        };
                        ySpin.ValueChanged += () =>
                        {
                            var pVal = (LingoPoint)prop.GetValue(obj)!;
                            pVal.Y = ySpin.Value;
                            prop.SetValue(obj, pVal);
                        };
                    }
                    else
                    {
                        xSpin.Enabled = false;
                        ySpin.Enabled = false;
                    }
                    row.AddItem(xSpin);
                    row.AddItem(ySpin);
                    root.AddItem(row);
                    continue;
                }
                else
                {
                    var text = factory.CreateInputText(prop.Name + "Text");
                    text.Text = val?.ToString() ?? string.Empty;
                    if (prop.CanWrite)
                        text.ValueChanged += () =>
                        {
                            try
                            {
                                prop.SetValue(obj, ConvertTo(text.Text, prop.PropertyType));
                            }
                            catch { }
                        };
                    else
                        text.Enabled = false;
                    row.AddItem(text);
                }

                root.AddItem(row);
            }
            return root;
        }

        private static bool IsSimpleType(Type t)
        {
            return t.IsPrimitive || t == typeof(string) || t.IsEnum || t == typeof(float) || t == typeof(double) || t == typeof(decimal);
        }

        private static object ConvertTo(string text, Type t)
        {
            if (t == typeof(string)) return text;
            if (t.IsEnum) return Enum.Parse(t, text);
            return Convert.ChangeType(text, t);
        }
    }
}
