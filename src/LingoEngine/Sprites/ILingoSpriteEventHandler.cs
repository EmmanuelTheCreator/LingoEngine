namespace LingoEngine.Sprites
{
    internal interface ILingoSpriteEventHandler
    {

        /// <summary>
        ///  Triggered when the sprite receives focus
        /// </summary>
        void RaiseFocus();
        /// <summary>
        /// Triggered when the sprite loses focus
        /// </summary>
        void RaiseBlur();
    }
}
