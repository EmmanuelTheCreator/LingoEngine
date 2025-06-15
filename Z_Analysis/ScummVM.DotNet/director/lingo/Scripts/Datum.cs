using System.Text;

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
            Object,
            VarRef,
            Int,
            ArgList,
            ArgListNoRet,
            PropList
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

        public string Write()
        {
            return Type switch
            {
                DatumType.Void => "VOID",
                DatumType.Symbol => "#" + AsString(),
                DatumType.VarRef => AsString(),
                DatumType.String => EncodeString(AsString()),
                DatumType.Integer => AsInt().ToString(),
                DatumType.Float => AsFloat().ToString("G"),
                DatumType.ArgList or DatumType.ArgListNoRet or DatumType.List => WriteList(),
                DatumType.PropList => WritePropList(),
                _ => AsString()
            };
        }

        private string WriteList()
        {
            if (Value is not List<Node> list)
                return "[]";

            var sb = new StringBuilder();
            if (Type == DatumType.List) sb.Append("[");
            for (int i = 0; i < list.Count; i++)
            {
                if (i > 0) sb.Append(", ");
                sb.Append(list[i]);
            }
            if (Type == DatumType.List) sb.Append("]");
            return sb.ToString();
        }

        private string WritePropList()
        {
            if (Value is not List<Node> list)
                return "[:";

            var sb = new StringBuilder("[");
            if (list.Count == 0)
            {
                sb.Append(":");
            }
            else
            {
                for (int i = 0; i < list.Count; i += 2)
                {
                    if (i > 0) sb.Append(", ");
                    sb.Append(list[i]);
                    sb.Append(": ");
                    sb.Append(list[i + 1]);
                }
            }
            sb.Append("]");
            return sb.ToString();
        }

        private string EncodeString(string value)
        {
            if (value.Length == 0)
                return "EMPTY";
            if (value.Length == 1)
            {
                return value[0] switch
                {
                    '\x03' => "ENTER",
                    '\x08' => "BACKSPACE",
                    '\t' => "TAB",
                    '\r' => "RETURN",
                    '"' => "QUOTE",
                    _ => "\"" + value + "\""
                };
            }
            return "\"" + value.Replace("\"", "\\\"") + "\"";
        }
    }

}
