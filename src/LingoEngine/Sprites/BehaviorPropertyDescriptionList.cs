using System.Linq.Expressions;
using LingoEngine.Primitives;

namespace LingoEngine.Sprites;

public class BehaviorPropertyDescriptionList : LingoPropertyList<LingoPropertyDescription>
{
    public BehaviorPropertyDescriptionList Add<TBehavior>(TBehavior behavior, Expression<Func<TBehavior, string?>> property, string? comment = null, string? @default = null)
        where TBehavior : LingoSpriteBehavior
    {
        var stringProp = new LingoPropertyDescription<TBehavior,string>(behavior, LingoSymbol.String, comment, property, @default);
        Add(stringProp.Key, stringProp);
        return this;
    } 
    public BehaviorPropertyDescriptionList Add<TBehavior>(TBehavior behavior, Expression<Func<TBehavior, int>> property, string? comment = null, int @default = 0)
        where TBehavior : LingoSpriteBehavior
    {
        var stringProp = new LingoPropertyDescription<TBehavior, int>(behavior, LingoSymbol.Int, comment, property, @default);
        Add(stringProp.Key, stringProp);
        return this;
    } 
    public BehaviorPropertyDescriptionList Add<TBehavior>(TBehavior behavior, Expression<Func<TBehavior, float>> property, string? comment = null, int @default = 0)
        where TBehavior : LingoSpriteBehavior
    {
        var stringProp = new LingoPropertyDescription<TBehavior, float>(behavior, LingoSymbol.Float, comment, property, @default);
        Add(stringProp.Key, stringProp);
        return this;
    }
    public BehaviorPropertyDescriptionList Add<TBehavior>(TBehavior behavior, Expression<Func<TBehavior, bool>> property, string? comment = null, bool @default = false)
        where TBehavior : LingoSpriteBehavior
    {
        var stringProp = new LingoPropertyDescription<TBehavior, bool>(behavior, LingoSymbol.Boolean, comment, property, @default);
        Add(stringProp.Key, stringProp);
        return this;
    }
}
