using System;
using AbstUI.Primitives;
using AbstUI.Styles.Components;

namespace AbstUI.Components
{
    /// <summary>
    /// Base class for all engine level graphics nodes exposing common properties.
    /// </summary>
    public abstract class AbstNodeBase<TFramework> : IAbstNode, IDisposable
        where TFramework : IAbstFrameworkNode
    {
#pragma warning disable CS8618
        protected TFramework _framework;
#pragma warning restore CS8618

        public virtual bool Visibility { get => _framework != null? _framework.Visibility: false; set => _framework.Visibility = value; }
        public virtual string Name { get => _framework.Name; set => _framework.Name = value; }
        public virtual AMargin Margin { get => _framework.Margin; set => _framework.Margin = value; }

        public virtual float Width { get => _framework.Width; set => _framework.Width = value; }
        public virtual float Height { get => _framework.Height; set => _framework.Height = value; }
        public virtual int ZIndex { get => _framework.ZIndex; set => _framework.ZIndex = value; }

        #region Styling
        /// <summary>
        /// Apply a style to this component.
        /// </summary>
        public void SetStyle(AbstComponentStyle componentStyle)
        {
            if (componentStyle.Visibility.HasValue)
                Visibility = componentStyle.Visibility.Value;
            if (componentStyle.Margin.HasValue)
                Margin = componentStyle.Margin.Value;
            if (componentStyle.Width.HasValue)
                Width = componentStyle.Width.Value;
            if (componentStyle.Height.HasValue)
                Height = componentStyle.Height.Value;

            OnSetStyle(componentStyle);
        }

        /// <summary>
        /// Retrieve the current style applied to this component.
        /// </summary>
        /// <typeparam name="TStyle">Specific style type to return.</typeparam>
        /// <returns>Populated style instance.</returns>
        public TStyle GetStyle<TStyle>() where TStyle : AbstComponentStyle, new()
        {
            var style = new TStyle
            {
                Visibility = Visibility,
                Margin = Margin,
                Width = Width,
                Height = Height,
            };

            OnGetStyle(style);
            return style;
        }

        /// <summary>
        /// Allows derived components to handle style-specific properties when applying styles.
        /// </summary>
        /// <param name="componentStyle">Style being applied.</param>
        protected virtual void OnSetStyle(AbstComponentStyle componentStyle) { }

        /// <summary>
        /// Allows derived components to handle style-specific properties when retrieving styles.
        /// </summary>
        /// <param name="componentStyle">Style being populated.</param>
        protected virtual void OnGetStyle(AbstComponentStyle componentStyle) { }
        #endregion

        public virtual T Framework<T>() where T : IAbstFrameworkNode => (T)(object)_framework;
        public virtual IAbstFrameworkNode FrameworkObj { get => _framework; set => _framework = (TFramework)value; }


        public virtual void Init(TFramework framework) => _framework = framework;


        public virtual void Dispose() => (_framework as IDisposable)?.Dispose();
    }

    public abstract class AbstNodeLayoutBase<TFramework> : AbstNodeBase<TFramework>, IAbstLayoutNode
       where TFramework : IAbstFrameworkLayoutNode
    {


        public virtual float X { get => _framework.X; set => _framework.X = value; }
        public virtual float Y { get => _framework.Y; set => _framework.Y = value; }



    }
}
