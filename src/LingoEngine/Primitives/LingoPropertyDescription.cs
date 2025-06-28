namespace LingoEngine.Primitives
{
    /// <summary>
    /// Describes a configurable behavior property.
    /// </summary>
    public class LingoPropertyDescription
    {
        /// <summary>The property's initial value.</summary>
        public object? Default { get; set; }

        /// <summary>The expected data type of the value.</summary>
        public LingoSymbol Format { get; set; } = LingoSymbol.Empty;

        /// <summary>Label shown in the Parameters dialog.</summary>
        public string? Comment { get; set; }

        /// <summary>Optional range or list of valid values.</summary>
        public IEnumerable<object?>? Range { get; set; }
    }
}
