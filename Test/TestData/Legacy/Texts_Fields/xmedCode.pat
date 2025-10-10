
struct SomethingLengthA
{
    u8 _ForLength4;    //@ offsetEnd1; //0x418
    u8 _padding1[8];
    u16 _ForLength_A1; // @ offsetEnd1 + 10;
    u8 _padding2[10];
    u16 _ForLength_A2; // @ offsetEnd1 + 14;
    //u8 _padding3[2];
    //u16 _ForLength_A3; // @ offsetEnd1 + 16;
};
struct CommaBlock {
    char BlockLength[2];  
    char SeparatorComma; 
    char Value[std::string::parse_int(this.BlockLength,16)] ;
};


struct FontStyleBlock {
    char BlockLength[2];  
    char SeparatorComma; 
    u8 FontNameLength;
    char FontName[this.FontNameLength];
    u8 Padding1[std::string::parse_int(this.BlockLength,16) - this.FontNameLength];
    CommaBlock SomeBlock;
};


bitfield FontTextStyle{
    Bold           : 1;
    Italic         : 1;
    Underline      : 1;
    Strikeout      : 1;
    Subscript      : 1;
    Superscript    : 1;
    Outline        : 1;
    EditableField  : 1;
};

enum TextAlignment : u8{
    Center      = 0,
    Right       = 1,
    Left        = 2,
    Justified   = 3
};
bitfield Layout{
    TextAlignment Alignment       : 2;
    bool Unknown1        : 1;
    bool WrapDisabled    : 1;
    bool TabPresent      : 1;
    bool Unknown2        : 1;
    bool Unknown3        : 1;
    bool Unknown4        : 1;
};

struct TextMetrics{
    u32 LineSpacing;    // 0x003C
    u32 FontSize;       // 0x0040 unsure
    u32 TextLength;     // 0x004C unsure
};
struct TextMargins{
    u16 LeftMargin;     // 0x04DA
    u16 RightMargin;
    u16 FirstLineIndent;
};
