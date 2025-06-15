using ProjectorRays.Common;
using System.Collections.Generic;

namespace ProjectorRays.LingoDec;

public class Datum
{
    public DatumType Type;
    public int I;
    public double F;
    public string S = string.Empty;
    public List<Datum> L = new();

    public Datum() { Type = DatumType.kDatumVoid; }
    public Datum(int val) { Type = DatumType.kDatumInt; I = val; }
    public Datum(double val) { Type = DatumType.kDatumFloat; F = val; }
    public Datum(DatumType t, string val) { Type = t; S = val; }

    public int ToInt()
    {
        return Type switch
        {
            DatumType.kDatumInt => I,
            DatumType.kDatumFloat => (int)F,
            _ => 0
        };
    }

    public void WriteScriptText(CodeWriter code, bool dot, bool sum)
    {
        switch (Type)
        {
            case DatumType.kDatumVoid:
                code.Write("VOID");
                break;
            case DatumType.kDatumSymbol:
                code.Write("#" + S);
                break;
            case DatumType.kDatumVarRef:
                code.Write(S);
                break;
            case DatumType.kDatumString:
                if (S.Length == 0)
                    code.Write("EMPTY");
                else
                    code.Write('"' + S + '"');
                break;
            case DatumType.kDatumInt:
                code.Write(I.ToString());
                break;
            case DatumType.kDatumFloat:
                code.Write(Util.FloatToString(F));
                break;
            default:
                code.Write("LIST");
                break;
        }
    }
}
