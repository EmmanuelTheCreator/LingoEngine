using System.Linq.Expressions;
using System.Reflection;

namespace LingoEngine.Tools
{
    public static class CommonExtensions
    {
        public static string GetPropertyName<T>(this Expression<Func<T, object?>> expression)
        {
            if (expression.Body is MemberExpression member)
            {
                return member.Member.Name;
            }
            if (expression.Body is UnaryExpression unary && unary.Operand is MemberExpression unaryMember)
            {
                return unaryMember.Member.Name;
            }

            throw new ArgumentException("Invalid expression. Expression should be a simple property access.");
        }
        public static Func<T, TResult?> CompileGetter<T, TResult>(this Expression<Func<T, TResult?>> expression)
        {
            return expression.Compile();
        }
        public static Action<T, TValue?> CompileSetter<T, TValue>(this Expression<Func<T, TValue?>> expression)
        {
            if (expression.Body is MemberExpression memberExpr && memberExpr.Member is PropertyInfo prop)
            {
                return (target, value) => prop.SetValue(target, value);
            }

            if (expression.Body is UnaryExpression { Operand: MemberExpression unaryMember } && unaryMember.Member is PropertyInfo propInfo)
            {
                return (target, value) => propInfo.SetValue(target, value);
            }

            throw new ArgumentException("Invalid expression. Must be a property access.");
        }
    }
}
