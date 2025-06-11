namespace Director.Primitives
{
    internal class DirConstants
    {
        public static readonly byte[] BlankScoreD2 = new byte[]
     {
        0x00, 0x00, 0x00, 0x06, // _framesStreamSize
        0x00, 0x02              // frame with empty channel information
     };

        public static readonly byte[] BlankScoreD4 = new byte[]
        {
        0x00, 0x00, 0x00, 0x12, // _framesStreamSize
        0x00, 0x00, 0x00, 0x10, // frame1Offset
        0x00, 0x00, 0x00, 0x01, // numOfFrames
        0x00, 0x07,             // _framesVersion
        0x00, 0x00,             // _numChannels
        0x00, 0x00,             // skipped
        0x00, 0x02              // frame with empty channel information
        };


        
       
    }
}
