using AbstUI.Components;
using AbstUI.Components.Containers;
using AbstUI.Components.Inputs;
using AbstUI.Primitives;
using AbstUI.Windowing;
using BlingoEngine.Director.Core.Styles;
using BlingoEngine.FrameworkCommunication;
using BlingoEngine.Primitives;
using BlingoEngine.Sprites;

namespace BlingoEngine.Director.Core.Inspector
{
    public interface IDirectorBehaviorDescriptionManager
    {
        /// <summary>Builds a popup window for editing the given behavior.</summary>
        IAbstWindowDialogReference? BuildBehaviorPopup(BlingoSpriteBehavior behavior, Action onClose);
    }

    internal class DirectorBehaviorDescriptionManager : IDirectorBehaviorDescriptionManager
    {
        private readonly IBlingoFrameworkFactory _factory;
        private readonly IAbstWindowManager _windowManager;
        public Action<bool>? _OnWindowStateChanged;
        private IAbstWindowDialogReference? _popup;

        public DirectorBehaviorDescriptionManager(IBlingoFrameworkFactory factory, IAbstWindowManager windowManager)
        {
            _factory = factory;
            _windowManager = windowManager;
        }

        
        public IAbstWindowDialogReference? BuildBehaviorPopup(BlingoSpriteBehavior behavior, Action onClose)
        {
            if (!(behavior is IBlingoPropertyDescriptionList))
               return null;
            if (behavior is IBlingoPropertyDescriptionListDialog dialog)
                dialog.RunPropertyDialog(behavior.UserProperties);

            var width = 390;
            var height = 250;
            var rightWidth = 90;

            var behaviorPanel = BuildBehaviorPanel(behavior, width - rightWidth - 10, height);
            if (behaviorPanel == null ) return null;
            var panel = behaviorPanel.Value.Node;

             //var win = _factory.CreateWindow("BehaviorParams", $"Parameters for \"{behavior.Name}\"");
            var root = _factory.CreatePanel("BehaviorPopupRoot");
            var wrap = _factory.CreateWrapPanel(AOrientation.Horizontal, "BehaviorPopupWrap");
            root.Width = width;
            root.Height = height;
            root.AddItem(wrap);
            //win.AddItem(root);
            //win.Width = width;
            //win.Height = height;
            //win.BackgroundColor = DirectorColors.BG_WhiteMenus;
            //win.IsPopup = true;
            //_OnWindowStateChanged = (state) =>
            //{
            //    if (!state)
            //    {
            //        win.OnWindowStateChanged -= _OnWindowStateChanged;
            //        onClose.Invoke();
            //    }
            //};

            //win.OnWindowStateChanged += _OnWindowStateChanged;



            wrap.AddItem(panel);

            var vLine = _factory.CreateVerticalLineSeparator("BehaviorPopupLine");
            vLine.Height = height;
            wrap.AddItem(vLine);

            var right = _factory.CreateWrapPanel(AOrientation.Vertical, "BehaviorPopupRight");
            right.Width = rightWidth;
            var ok = _factory.CreateButton("BehaviorPopupOk", "OK");
            ok.Width = 74;
            float margin = (rightWidth - ok.Width) / 2f;
            ok.Margin = new AMargin(margin, 0, margin, 0);
            ok.Pressed += () =>
            {
                behavior.UserProperties.Apply(behaviorPanel.Value.Definitions);
                _popup?.Close();
            };
            right.AddItem(ok);
            wrap.AddItem(right);

            _popup = _windowManager.ShowCustomDialog($"Parameters for \"{behavior.Name}\"", root.Framework<IAbstFrameworkPanel>());
            
            return _popup;
        }

        private (IAbstLayoutNode Node, BehaviorPropertyDescriptionList Definitions)? BuildBehaviorPanel(BlingoSpriteBehavior behavior, int width, int height)
        {
            

            // todo : fix container with scroll
            //var scroller = _factory.CreateScrollContainer("BehaviorPanelScroller");
            //scroller.Width = width;
            //scroller.Height = height;
            var container = _factory.CreateWrapPanel(AOrientation.Vertical, "BehaviorPanel");
            container.Width = width;
            container.Height = height;
            //scroller.AddItem(container);
            //var propsPanel = BuildProperties(behavior);
            //container.AddItem(propsPanel);
            BehaviorPropertyDescriptionList? definitions = null;
            if (behavior is IBlingoPropertyDescriptionList descProvider)
            {
                definitions = BuildDescriptionList(behavior, container, descProvider);
                if (definitions == null)
                {
                    container.Dispose();
                    return null;
                }
                return (container, definitions);
            }
            return null;
        }


        private BehaviorPropertyDescriptionList? BuildDescriptionList(BlingoSpriteBehavior behavior, AbstWrapPanel container, IBlingoPropertyDescriptionList descProvider)
        {
            BehaviorPropertyDescriptionList? definitions = descProvider.GetPropertyDescriptionList();
            if (definitions == null || definitions.Count == 0)
                return null;
            string? desc = descProvider.GetBehaviorDescription();


            if (!string.IsNullOrEmpty(desc))
            {
                var descLabel = _factory.CreateLabel("BehaviorDescLabel_"+ behavior.Name, desc);
                descLabel.FontColor = AColors.Black;
                descLabel.Width = 200;
                descLabel.FontSize = 14;
                descLabel.WrapMode = ATextWrapMode.WordSmart;
                container.AddItem(descLabel);
            }

            var properties = behavior.UserProperties;
            foreach (var propDefinition in definitions)
            {
                var row = _factory.CreateWrapPanel(AOrientation.Horizontal, $"PropRow_{propDefinition.Key}");
                string labelText = !string.IsNullOrEmpty(propDefinition.Value.Comment) ? propDefinition.Value.Comment! : propDefinition.Key.ToString();
                var label = _factory.CreateLabel($"PropLabel_{propDefinition.Key}", labelText);
                label.Width = 80;
                label.WrapMode = ATextWrapMode.WordSmart;
                label.FontColor = DirectorColors.TextColorLabels;
                label.FontSize = 10;
                row.AddItem(label);

                object? propValue = propDefinition.Value.CurrentValue;

                string format = propDefinition.Value.Format;
                if (format == BlingoSymbol.String)
                    row.AddItem(CreateString(properties, propDefinition.Key, propValue));
                else if (format == BlingoSymbol.Int)
                    row.AddItem(CreateInt(properties, propDefinition.Key, propValue)); 
                else if (format == BlingoSymbol.Float)
                    row.AddItem(CreateFloat(properties, propDefinition.Key, propValue));
                else if (format == BlingoSymbol.Boolean)
                    row.AddItem(CreateBoolean(properties, propDefinition.Key, propValue));
                else
                {
                    var input = _factory.CreateLabel($"PropInput_{propDefinition.Key}","Not implemtype "+format);
                    row.AddItem(input);
                }

                container.AddItem(row);
            }
            return definitions;
           
        }

        private AbstInputText CreateString(BehaviorPropertiesContainer properties, string key, object? propValue)
        {
            var input = _factory.CreateInputText($"PropInput_{key}");
            input.Width = 70;
            input.Text = propValue?.ToString() ?? string.Empty;
            input.OnCommit += () => properties[key] = input.Text;
            return input;
        }

        private AbstInputNumber<int> CreateInt(BehaviorPropertiesContainer properties, string key, object? propValue)
        {
            var input = _factory.CreateInputNumberInt($"PropInput_{key}");
            input.Width = 70;
            input.NumberType = ANumberType.Integer;
            if (propValue is int i)
                input.Value = i;
            //else if (propValue is float f)
            //    input.Value = f;
            //else if (propValue != null && float.TryParse(propValue.ToString(), out var fv))
            //    input.Value = fv;
            input.OnCommit += () => properties[key] = (int)input.Value;
            return input;
        }
        private AbstInputNumber<float> CreateFloat(BehaviorPropertiesContainer properties, string key, object? propValue)
        {
            var input = _factory.CreateInputNumberFloat($"PropInput_{key}");
            input.Width = 70;
            input.NumberType = ANumberType.Float;
            if (propValue is int i)
                input.Value = i;
            else if (propValue is float f)
                input.Value = f;
            else if (propValue != null && float.TryParse(propValue.ToString(), out var fv))
                input.Value = fv;
            input.OnCommit +=
                () =>
                {
                    properties[key] = input.Value;
                };
            return input;
        }

        private AbstInputCheckbox CreateBoolean(BehaviorPropertiesContainer properties, string key, object? propValue)
        {
            var input = _factory.CreateInputCheckbox($"PropInput_{key}");
            input.Width = 70;
            if (propValue is bool bval)
                input.Checked = bval;
            else if (propValue is string s && bool.TryParse(s, out var bv))
                input.Checked = bv;
            input.ValueChanged += () => properties[key] = input.Checked;
            return input;
        }

        public AbstWrapPanel BuildProperties(object obj)
        {
            IBlingoFrameworkFactory factory = _factory;
            var root = factory.CreateWrapPanel(AOrientation.Vertical, $"{obj.GetType().Name}Props");
            foreach (var prop in obj.GetType().GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance))
            {
                if (!prop.CanRead)
                    continue;
                if (!IsSimpleType(prop.PropertyType) && prop.PropertyType != typeof(APoint))
                    continue;

                var row = factory.CreateWrapPanel(AOrientation.Horizontal, prop.Name + "Row");
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
                else if (prop.PropertyType == typeof(APoint))
                {
                    var point = val is APoint p ? p : new APoint();
                    var xSpin = factory.CreateSpinBox(prop.Name + "X");
                    var ySpin = factory.CreateSpinBox(prop.Name + "Y");
                    xSpin.Value = point.X;
                    ySpin.Value = point.Y;
                    if (prop.CanWrite)
                    {
                        xSpin.ValueChanged += () =>
                        {
                            var pVal = (APoint)prop.GetValue(obj)!;
                            pVal.X = xSpin.Value;
                            prop.SetValue(obj, pVal);
                        };
                        ySpin.ValueChanged += () =>
                        {
                            var pVal = (APoint)prop.GetValue(obj)!;
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
                        text.OnCommit += () =>
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

