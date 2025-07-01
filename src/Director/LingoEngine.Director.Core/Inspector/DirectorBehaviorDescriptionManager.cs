using LingoEngine.FrameworkCommunication;
using LingoEngine.Gfx;
using LingoEngine.Primitives;
using LingoEngine.Sprites;

namespace LingoEngine.Director.Core.Inspector
{
    public interface IDirectorBehaviorDescriptionManager
    {
        LingoGfxWrapPanel BuildBehaviorPanel(LingoSpriteBehavior behavior);
    }

    internal class DirectorBehaviorDescriptionManager : IDirectorBehaviorDescriptionManager
    {
        private readonly ILingoFrameworkFactory _factory;

        public DirectorBehaviorDescriptionManager(ILingoFrameworkFactory factory)
        {
            _factory = factory;
        }

        public LingoGfxWrapPanel BuildBehaviorPanel(LingoSpriteBehavior behavior)
        {
            var container = _factory.CreateWrapPanel(LingoOrientation.Vertical, "BehaviorPanel");
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
                    //if (Symbol Type == #string)
                    // show label with value from behavior.UserProperties
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
