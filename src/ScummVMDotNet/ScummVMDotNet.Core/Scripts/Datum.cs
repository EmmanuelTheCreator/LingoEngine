namespace Director.Scripts
{
    /// <summary>
    /// Represents a dynamic value used in Lingo scripts, equivalent to the Datum type in ScummVM.
    /// </summary>
    public class Datum
    {
        

        public enum DatumType
        {
            Void,
            Integer,
            Float,
            String,
            Symbol,
            List,
            Point,
            Rect,
            Object
        }
        public static Datum Undefined => new Datum();
        public DatumType Type { get; private set; }
        public object? Value { get; private set; }

        public Datum()
        {
            Type = DatumType.Void;
            Value = null;
        }

        public Datum(int value)
        {
            Type = DatumType.Integer;
            Value = value;
        }

        public Datum(float value)
        {
            Type = DatumType.Float;
            Value = value;
        }

        public Datum(string value, bool isSymbol = false)
        {
            Type = isSymbol ? DatumType.Symbol : DatumType.String;
            Value = value;
        }

        public static implicit operator Datum(int value) => new Datum(value);
        public static implicit operator Datum(float value) => new Datum(value);
        public static implicit operator Datum(string value) => new Datum(value);

        public int AsInt() => Type == DatumType.Integer ? (int)Value! : 0;
        public float AsFloat() => Type == DatumType.Float ? (float)Value! : 0f;
        public string AsString() => Value?.ToString() ?? string.Empty;
        public string AsSymbol() => Type == DatumType.Symbol ? (string)Value! : string.Empty;
        public string StringValue => AsString();
        public override string ToString() => Value?.ToString() ?? "void";

        public static Datum Void => new Datum();

        public static Datum Symbol(string value) => new Datum(value, isSymbol: true);

        public static Datum FromObject(object? obj)
        {
            return obj switch
            {
                null => Void,
                int i => new Datum(i),
                float f => new Datum(f),
                string s => new Datum(s),
                _ => new Datum { Type = DatumType.Object, Value = obj }
            };
        }
    }

}
