using System.Text;

namespace LingoEngine.Lingo.Core.Tokenizer
{
    /// <summary>
    /// Represents a dynamic value used in Lingo scripts, equivalent to the Datum type in ScummVM.
    /// </summary>
    public class LingoDatum
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
        public static LingoDatum Undefined => new LingoDatum();
        public DatumType Type { get; private set; }
        public object? Value { get; private set; }

        public LingoDatum()
        {
            Type = DatumType.Void;
            Value = null;
        }

        public LingoDatum(int value)
        {
            Type = DatumType.Integer;
            Value = value;
        }

        public LingoDatum(float value)
        {
            Type = DatumType.Float;
            Value = value;
        }

        public LingoDatum(string value, bool isSymbol = false)
        {
            Type = isSymbol ? DatumType.Symbol : DatumType.String;
            Value = value;
        }

        public static implicit operator LingoDatum(int value) => new LingoDatum(value);
        public static implicit operator LingoDatum(float value) => new LingoDatum(value);
        public static implicit operator LingoDatum(string value) => new LingoDatum(value);

        public int AsInt() => Type == DatumType.Integer ? (int)Value! : 0;
        public float AsFloat() => Type == DatumType.Float ? (float)Value! : 0f;
        public string AsString() => Value?.ToString() ?? string.Empty;
        public string AsSymbol() => Type == DatumType.Symbol ? (string)Value! : string.Empty;
        public string StringValue => AsString();
        public override string ToString() => Value?.ToString() ?? "void";

        public static LingoDatum Void => new LingoDatum();

        public static LingoDatum Symbol(string value) => new LingoDatum(value, isSymbol: true);

        public static LingoDatum FromObject(object? obj)
        {
            return obj switch
            {
                null => Void,
                int i => new LingoDatum(i),
                float f => new LingoDatum(f),
                string s => new LingoDatum(s),
                _ => new LingoDatum { Type = DatumType.Object, Value = obj }
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
