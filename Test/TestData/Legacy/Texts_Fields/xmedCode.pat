
#include <std/string.pat>
#include <std/mem.pat>
#pragma endian big

struct HeaderBlock{
    char TxtValue1[4];
    char TxtValue2[4];
    char TxtValue3[4];
    char TxtValue4[4];
    char TxtValue5[4];
    u16 Value1 = std::string::parse_int(this.TxtValue1,16);
    u16 Value2 = std::string::parse_int(this.TxtValue2,16);
    u16 Value3 = std::string::parse_int(this.TxtValue3,16);
    u16 Value4 = std::string::parse_int(this.TxtValue4,16);
    u16 Value5 = std::string::parse_int(this.TxtValue5,16);
};
struct FiveHexValue{
    char TxtValue1[4];
    char TxtValue2[4];
    char TxtValue3[4];
    char TxtValue4[4];
    char TxtValue5[4];
    u16 Value1 = std::string::parse_int(this.TxtValue1,16);
    u16 Value2 = std::string::parse_int(this.TxtValue2,16);
    u16 Value3 = std::string::parse_int(this.TxtValue3,16);
    u16 Value4 = std::string::parse_int(this.TxtValue4,16);
    u16 Value5 = std::string::parse_int(this.TxtValue5,16);
};


struct CommaBlock {
    char BlockLength[2];  
    char SeparatorComma; 
    u16 Length = std::string::parse_int(this.BlockLength,16);
    char Value[this.Length];
};

struct TextPart {
    char BlockLength[textcharLength];  // Can be more then one, split by comma  
    char SeparatorComma; 
    char TheText[std::string::parse_int(this.BlockLength,16)] ;
};
struct FontStyleBlock {
    char BlockLength[2];  
    char SeparatorComma; 
    u8 FontNameLength;
    char FontName[this.FontNameLength];
    u8 Padding1[std::string::parse_int(this.BlockLength,16) - this.FontNameLength];
    CommaBlock SomeBlock;
    char Val1[2];
    u16 Val2;
    u16 Val3;
    u16 Val4;
    u16 Val5;
    u16 Val6;
    u16 Val7;
    u16 Val8;
    u16 Val9;
    u16 Val10;
    u16 Val11;
    u16 Val12;
    u16 Val13;
    u16 Val14;
    u16 Val15;
    u16 Val16;
    u16 Val17;
    u16 Val18;
    u16 Val19;
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
u32 FieldWidth; // 0x0018
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

struct SomeBlockHexStartA{
    FiveHexValue HexValues1;
    u16  Val1;
    u16  Val2;
    u16  Val3;
    u16  Val4;
    u16  Val5;
    u16  Val6;
    u16  Val7;
    u16  Val8;
    u16  Val9;
};
struct AfterStyles{
    SomeBlockHexStartA block1;
    u8 padding1;
    SomeBlockHexStartA block2;
    u8 padding2;
    FiveHexValue HexValues1;
    u16  Val1;
    u16  Val2;
    u8 padding3;
    FiveHexValue HexValues2;
    u16  Val3;
    u16  Val4;
    u16  Val5;
    u16  Val6;
    u16  Val7;
    u16  Val8;
    u16  Val9;
    SomeBlockHexStartA block3;
};


struct EndBlockA{
    u8 Padding1;
    char TxtValue1[4];
    u16  Val1;
    u16  Val2;
    u8 Padding2;
    char TxtValue2[4];
    u16  Val3;
    u16 Val4;
    u16 Value1 = std::string::parse_int(this.TxtValue1,16);
    u16 Value2 = std::string::parse_int(this.TxtValue2,16);
};

struct EndBlockB{
    FiveHexValue HexValues1;
    u16 Val1;
    u16 Val2;
    u16 Val3;
    u16 Val4;
    u16 Val5;
    FiveHexValue HexValues2;
};



HeaderBlock         headerBlock         @ 0x00;
TextPart            text1               @ textStructAddress;
FontStyleBlock      style1              @ styleAddresses[0];
FontStyleBlock      style2              @ styleAddresses[1];
AfterStyles         afterStyles         @ styleAddresses[1] + 174;
CommaBlock          alwaysTheSemeSeems  @ offsetSameBlock;
EndBlockA           endBlockA           @ offsetSameBlock+alwaysTheSemeSeems.Length+3;
EndBlockB           endBlockB           @ offsetSameBlock+alwaysTheSemeSeems.Length+3 + 19;

FontTextStyle PossibleStyleByte @ PossibleStyleByteAA;
/*for (u8 i = 0, i < styleCount, i = i + 1) {
    FontStyleBlock style1    @ styleAddresses[i];
    std::print(style1.FontName);
};*/

// FontStyle fs1 @   0x1D;// todo find address
std::print(text1.TheText);
std::print("HeaderValues={0},{1},{2},{3},{4}",headerBlock.Value1,headerBlock.Value2,headerBlock.Value3, 
            headerBlock.Value4,headerBlock.Value5);