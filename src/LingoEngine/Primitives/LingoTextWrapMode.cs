namespace LingoEngine.Primitives
{
    public enum LingoTextWrapMode : int
    {
        /// <summary>
        /// <para>Autowrap is disabled.</para>
        /// </summary>
        Off = 0,
        /// <summary>
        /// <para>Wraps the text inside the node's bounding rectangle by allowing to break lines at arbitrary positions, which is useful when very limited space is available.</para>
        /// </summary>
        Arbitrary = 1,
        /// <summary>
        /// <para>Wraps the text inside the node's bounding rectangle by soft-breaking between words.</para>
        /// </summary>
        Word = 2,
        /// <summary>
        /// <para>Behaves similarly to <see cref="Godot.TextServer.AutowrapMode.Word"/>, but force-breaks a word if that single word does not fit in one line.</para>
        /// </summary>
        WordSmart = 3
    }

}
