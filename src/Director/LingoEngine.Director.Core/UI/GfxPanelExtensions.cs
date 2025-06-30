using LingoEngine.Director.Core.Styles;
using LingoEngine.FrameworkCommunication;
using LingoEngine.Gfx;
using System.Linq.Expressions;
using LingoEngine.Tools;
using LingoEngine.Bitmaps;
using LingoEngine.Director.Core.Inspector;


namespace LingoEngine.Director.Core.UI
{
    public static class GfxPanelExtensions
    {
        
        public static GfxPanelBuilder Compose(this LingoGfxPanel panel, ILingoFrameworkFactory factory)
        {
            var builder = new GfxPanelBuilder(panel, factory);
            return builder;
        }
        public static LingoGfxLabel SetLabelAt(this LingoGfxPanel container, ILingoFrameworkFactory factory, string name, float x, float y, string? text = null, int fontSize = 11)
        {
            LingoGfxLabel lbl = factory.CreateLabel(name,text ??"");
            lbl.FontColor = DirectorColors.TextColorLabels;
            lbl.FontSize = fontSize;
            container.AddItem(lbl, x, y);
            return lbl;
        }
        public static LingoGfxInputText SetInputTextAt<T>(this LingoGfxPanel container, ILingoFrameworkFactory factory, T element, string name, float x, float y, int width, Expression<Func<T,string?>> property, int maxLength = 0)
        {
            Action<T, string?> setter = property.CompileSetter();
            var control = factory.CreateInputText(name, maxLength,x => setter(element,x));
            control.Text = property.CompileGetter()(element)?.ToString() ?? string.Empty;
            control.Width = width;
            container.AddItem(control, x, y);
            return control;
        }
        public static LingoGfxInputNumber SetInputNumberAt<T>(this LingoGfxPanel container, ILingoFrameworkFactory factory, T element, string name, float x, float y, int width, Expression<Func<T,float>> property, float min = 0, float max = 100)
        {
            Action<T, float> setter = property.CompileSetter();
            LingoGfxInputNumber control = factory.CreateInputNumber(name, min, max, x => setter(element, x));
            control.Value = property.CompileGetter()(element);
            control.Width = width;
            container.AddItem(control, x, y);
            return control;
        }
        public static LingoGfxInputCheckbox SetCheckboxAt<T>(this LingoGfxPanel container, ILingoFrameworkFactory factory, T element, string name, float x, float y, Expression<Func<T,bool>> property)
        {
            Action<T, bool> setter = property.CompileSetter();
            LingoGfxInputCheckbox control = factory.CreateInputCheckbox(name,x => setter(element, x));
            control.Checked = property.CompileGetter()(element);
            container.AddItem(control, x, y);
            return control;
        } 
        public static LingoGfxInputCombobox SetComboBoxAt<T>(this LingoGfxPanel container, ILingoFrameworkFactory factory, T element, string name, float x, float y, Expression<Func<T,string?>> propertyKey)
        {
            Action<T, string?> setter = propertyKey.CompileSetter();
            LingoGfxInputCombobox control = factory.CreateInputCombobox(name,  x => setter(element, x));
            control.SelectedKey = propertyKey.CompileGetter()(element);
            container.AddItem(control, x, y);
            return control;
        }

        public static LingoGfxStateButton SetStateButtonAt<T>(this LingoGfxPanel container, ILingoFrameworkFactory factory, T element, string name, float x, float y, Expression<Func<T,bool>> property, ILingoImageTexture? texture = null)
        {
            Action<T, bool> setter = property.CompileSetter();
            LingoGfxStateButton control = factory.CreateStateButton(name, texture, string.Empty, onChange: val => setter(element, val));
            control.IsOn = property.CompileGetter()(element);
            container.AddItem(control, x, y);
            return control;
        }

       
    }
}
