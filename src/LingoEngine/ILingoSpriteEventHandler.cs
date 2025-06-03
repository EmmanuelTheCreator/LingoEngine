namespace LingoEngine
{
    internal interface ILingoSpriteEventHandler
    {

        /// <summary>
        ///  Triggered when the sprite receives focus
        /// </summary>
        void DoFocus();
        /// <summary>
        /// Triggered when the sprite loses focus
        /// </summary>
        void DoBlur(); 
    }
}
