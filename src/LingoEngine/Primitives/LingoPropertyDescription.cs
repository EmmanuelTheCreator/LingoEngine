using LingoEngine.Tools;
using System.Linq.Expressions;

namespace LingoEngine.Primitives
{
    /// <summary>
    /// Describes a configurable behavior property.
    /// </summary>
    public class LingoPropertyDescription
    {
        /// <summary>The property's initial value.</summary>
        public object? Default { get; set; }
        public string Key { get; }

        /// <summary>The expected data type of the value.</summary>
        public LingoSymbol Format { get; set; } = LingoSymbol.Empty;

        /// <summary>Label shown in the Parameters dialog.</summary>
        public string? Comment { get; set; }

        /// <summary>Optional range or list of valid values.</summary>
        public IEnumerable<object?>? Range { get; set; }
        public object? CurrentValue { get; set; }

        public LingoPropertyDescription(string key, LingoSymbol format, string? comment, object? @default, IEnumerable<object?>? range = null)
        {
            Default = @default;
            Key = key;
            Format = format;
            Comment = comment;
            Range = range;
        }
        public virtual void ApplyValue(object value)
        {

        }
    }
    public class LingoPropertyDescription<T, TValue> : LingoPropertyDescription
    {
        private readonly T _target;
        private readonly Expression<Func<T, TValue?>> _property;
        
        //public TValue? CurrentValue { get; private set; }

        public LingoPropertyDescription(T target, LingoSymbol format, string? comment, Expression<Func<T, TValue?>> property, TValue? @default) : base(GetPropName(property), format, comment, @default, null)
        {
            _target = target;
            _property = property;
            var currentValue = property.Compile().Invoke(target);
            if (currentValue != null)
                CurrentValue = currentValue;
            else
                CurrentValue = @default;
        }
        public override void ApplyValue(object value)
        {
            var stringValue = (TValue)value;
            _property.CompileSetter().Invoke(_target, stringValue);
        }
        private static string GetPropName<TProp>(Expression<Func<T, TProp>> property)
        {
            if (property.Body is MemberExpression member)
                return member.Member.Name;
            if (property.Body is UnaryExpression unary && unary.Operand is MemberExpression unaryMember)
                return unaryMember.Member.Name;
            throw new ArgumentException("Invalid expression. Expression should be a simple property access.");
        }
    }
    //public class LingoPropertyDescriptionString<T> : LingoPropertyDescription
    //{
    //    private readonly Expression<Func<T, string>> property;

    //    public LingoPropertyDescriptionString(T target, LingoSymbol format, string? comment, Expression<Func<T,string>> property, string? @default) : base(property.GetPropertyName(), format, comment, @default, null)
    //    {
    //        this.property = property;
    //    }
    //    public override void ApplyValue(object value)
    //    {
    //        var stringValue = value as string;
    //        property.CompileSetter().Invoke(;
    //    }
    //}
    //public class LingoPropertyDescriptionInt<T> : LingoPropertyDescription
    //{
    //    private readonly Expression<Func<T, int>> property;

    //    public LingoPropertyDescriptionInt(LingoSymbol format, string? comment, Expression<Func<T,int>> property, string? @default) : base(property.GetPropertyName(), format, comment, @default, null)
    //    {
    //        this.property = property;
    //    }
    //}
    //public class LingoPropertyDescriptionBool<T> : LingoPropertyDescription
    //{
    //    private readonly Expression<Func<T, bool>> property;

    //    public LingoPropertyDescriptionBool(LingoSymbol format, string? comment, Expression<Func<T,bool>> property, string? @default) : base(property.GetPropertyName(), format, comment, @default, null)
    //    {
    //        this.property = property;
    //    }
    //}
}
