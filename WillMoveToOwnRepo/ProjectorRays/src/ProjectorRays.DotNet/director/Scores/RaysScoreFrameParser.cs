using Microsoft.Extensions.Logging;
using ProjectorRays.Common;
using System.ComponentModel.DataAnnotations;
using System.IO;
using static ProjectorRays.director.Scores.RaysScoreChunk;

namespace ProjectorRays.director.Scores
{
    internal class RaysScoreFrameParser
    {

        private readonly ILogger _logger;
        public RayStreamAnnotatorDecorator Annotator { get; }
        private BufferView _FrameDataBufferView;
        private List<BufferView> _FrameIntervalDescriptorBuffers = new();
        private List<BufferView> _BehaviorScriptBuffers = new();
        private List<FrameDelta> _frameTable = new List<FrameDelta>();
        private List<IntervalDescriptor> _FrameDescriptors;
        private Dictionary<int, IntervalDescriptor> _channelToDescriptor = new();

        /// <summary>List of behaviour script lists in the same order as the
        /// interval table.</summary>
        public List<List<RaysBehaviourRef>> FrameScripts { get; } = new();
        /// <summary>Frame order and additional metadata parsed from the chunk.</summary>
        public List<int> IntervalOrder { get; } = new();

        public int HighestFrameNumber { get; private set; }
        public int TotalSpriteCount { get; set; }
        public int SpriteSize { get; set; }
        public int SpriteChannelCount { get; set; }
        public List<RaySprite> AllSprites { get; private set; }

        public class FrameDeltaItem
        {
            public int Offset {get; private set; }
            public byte[] Data {get; private set; }
            public RayScoreTags.ScoreKeyframeTag? Type { get;  private set; }
            public FrameDeltaItem(int offset, byte[] data, RayScoreTags.ScoreKeyframeTag? type)
            {
                Offset = offset;
                Data = data;
            }
        }

        public class FrameDelta
        {
            public List<FrameDeltaItem> Items = new();
            public FrameDelta(List<FrameDeltaItem> items) => Items = items;
        }


        public RaysScoreFrameParser(ILogger logger, RayStreamAnnotatorDecorator streamAnnotator)
        {
            _logger = logger;
            Annotator = streamAnnotator;
        }

        #region DO NOT TOUCH

        public void ReadAllIntervals(int entryCount, ReadStream stream)
        {
            var s = new ReadStream(new BufferView(stream.Data, stream.Offset, stream.Size),
                stream.Endianness, stream.Pos, Annotator);

            // Offsets from the start of the entries area
            int[] offsets = new int[entryCount + 1];
            for (int i = 0; i < offsets.Length; i++)
            {
                offsets[i] = s.ReadInt32($"offset[{i}]");

                // Log each offset to ensure correctness
                //_logger.LogInformation($"Offset {i}: {offsets[i]:X}");
            }

            int entriesStart = s.Pos;

            // Parse frame data header and decode the delta encoded frames
            if (entryCount < 1)
                return;

            // Check the size calculation
            var size = offsets[1] - offsets[0];
            int absoluteStart = stream.Offset + entriesStart + offsets[0];

            // Log for debugging
            //_logger.LogInformation($"Frame data size: {size}, Absolute start: {absoluteStart:X}");

            _FrameDataBufferView = new BufferView(stream.Data, absoluteStart, size);

            // Interval order table
            if (entryCount >= 2)
            {
                size = offsets[2] - offsets[1];
                int absoluteStart2 = stream.Offset + entriesStart + offsets[1];
                var orderView = new BufferView(stream.Data, absoluteStart2, offsets[2] - offsets[1]);
                var os = new ReadStream(orderView, Endianness.BigEndian, annotator: Annotator);

                if (os.Size >= 4)
                {
                    int count = os.ReadInt32("orderCount");

                    // Log the count for debugging
                    //_logger.LogInformation($"IntervalOrder count: {count}");

                    for (int i = 0; i < count && os.Pos + 4 <= os.Size; i++)
                    {
                        int order = os.ReadInt32("order", new() { ["index"] = i });
                        IntervalOrder.Add(order);

                        // Log each entry added to the interval order
                        //_logger.LogInformation($"IntervalOrder[{i}]: {order}");
                    }

                    // Log IntervalOrder for debugging
                    //_logger.LogInformation($"Interval Order: {string.Join(", ", IntervalOrder)}");
                }
            }

            // Process entry indices based on interval order or fallback
            var entryIndices = IntervalOrder.Count > 0 ? IntervalOrder : null;
            if (entryIndices == null)
            {
                entryIndices = new List<int>();
                for (int i = 3; i + 2 < offsets.Length; i += 3)
                    entryIndices.Add(i);
            }

            _FrameIntervalDescriptorBuffers = new List<BufferView>();
            foreach (int primaryIdx in entryIndices)
            {
                if (primaryIdx + 2 >= offsets.Length)
                    continue;

                // Check size and calculate absolute start for the frame interval descriptors
                size = offsets[primaryIdx + 1] - offsets[primaryIdx];
                int absoluteStart2 = stream.Offset + entriesStart + offsets[primaryIdx];

                // Log for debugging
                //_logger.LogInformation($"Entry {primaryIdx}: size={size}, absoluteStart={absoluteStart2:X}");

                _FrameIntervalDescriptorBuffers.Add(new BufferView(stream.Data, absoluteStart2, size));

                // Secondary bytestring lists behaviour scripts
                var secSize = offsets[primaryIdx + 2] - offsets[primaryIdx + 1];
                if (secSize > 0)
                {
                    int absoluteStart3 = stream.Offset + entriesStart + offsets[primaryIdx + 1];

                    // Log secondary size and absolute start
                    //_logger.LogInformation($"Entry {primaryIdx}: Secondary size={secSize}, absoluteStart={absoluteStart3:X}");

                    var secView = new BufferView(stream.Data, absoluteStart3, secSize);
                    _BehaviorScriptBuffers.Add(secView);
                }
                else
                {
                   // _logger.LogWarning($"Frame {primaryIdx}: Secondary data size is zero, skipping behavior scripts.");
                }
            }
        }

        public void ReadFrameDescriptors()
        {
            _channelToDescriptor = new();
            _FrameDescriptors = new List<IntervalDescriptor>();
            int ind = 0;
            foreach (var frameIntervalDescriptor in _FrameIntervalDescriptorBuffers)
            {
                var ps = new ReadStream(frameIntervalDescriptor, Endianness.BigEndian,
                    annotator: Annotator);
                var descriptor = ReadFrameIntervalDescriptor(ind, ps);
                if (descriptor != null)
                {
                    _FrameDescriptors.Add(descriptor);
                    _channelToDescriptor[descriptor.Channel] = descriptor;
                }
                ind++;
            }
        }
        public void ReadBehaviors()
        {
            int ind = 0;
            foreach (var frameIntervalDescriptor in _BehaviorScriptBuffers)
            {
                var ps = new ReadStream(frameIntervalDescriptor, Endianness.BigEndian,
                    annotator: Annotator);
                var behaviourRefs = ReadBehaviors(ind, ps);
                _FrameDescriptors[ind].Behaviors.AddRange(behaviourRefs);
                FrameScripts.Add(behaviourRefs);
                ind++;
            }
        }



        public void ReadFrameData()
            => ReadFrameData(new ReadStream(_FrameDataBufferView,
                Endianness.BigEndian, annotator: Annotator));


        /*
        Custom bytes blocks for keyframes:
        ----------------------------------
        Byte 1 : Unkown
        ---------------
        
        numbers found 
        02: key frame one blank
        08: 
        26: 
        28: 
        0A: 
        3E:
        36: 

        Byte 2,3: Sprite Channels mapping (for header byte 02 in Animation_types.dir)
        ---------------------------------
        These tags encode the channel by spacing each tag 0x30 apart
        tag = 0x0136 + (channel - 6) * 0x30
        And vice versa:
        channel = ((tag - 0x0136) / 0x30) + 6

        Byte 4: Flags if keyframe is real or tweeening
        ----------------------------------------------
        01 → a real keyframe
        81 → no new keyframe, just continuation or tween data
        Then next byte: 
        02 advance one keyframe
        */



        private void ReadFrameData(ReadStream readerSource)
        {
           // log all bytes

           //var frameBytes1 = readerSource.ReadBytes(readerSource.BytesLeft);
           // var frameData1 = new ReadStream(frameBytes1, frameBytes1.Length, readerSource.Endianness,
           //     annotator: Annotator);
           // var rawata1 = frameData1.LogHex(frameData1.Size);
           // _logger.LogInformation("FrameData:" + rawata1);
           // return;

            // Read the header (first 16 bytes or based on the known header size)
            int actualSize = readerSource.ReadInt32("actualSize");
            var blockBytes = readerSource.ReadBytes(actualSize-4);
            var reader = new ReadStream(blockBytes, blockBytes.Length, readerSource.Endianness, annotator: Annotator);

            var unkA1 = reader.ReadInt8(); // usually -3
            var unkA2 = reader.ReadInt8(); // 
            var unkA3 = reader.ReadInt8(); // 
            var unkA4 = reader.ReadInt8(); // 
            HighestFrameNumber = reader.ReadInt32("HighestFrameNumber");
            var unkB1 = reader.ReadInt8(); // "const4"
            var unkB2 = reader.ReadInt8();
            SpriteSize = reader.ReadInt16("spriteSize");
            var unkC1 = reader.ReadInt8(); // "const6"
            var unkC2 = reader.ReadInt8();
            SpriteChannelCount = reader.ReadInt16("SpriteChannelCount");
            var unkD1 = reader.ReadInt16();

            // TotalSpriteCount = Something + 6;

            // Log the parsed header values for verification
            _logger.LogInformation($"DB | actualSize: {actualSize}, SpriteChannelCount: {SpriteChannelCount}, spriteSize: {SpriteSize}, HighestFrameNumber: {HighestFrameNumber}");
            _logger.LogInformation($"DB | unkA1: {unkA1}, unkA2: {unkA2}, unkA3: {unkA3}, unkA4: {unkA4} | unkB1: {unkB1}: unkB2: {unkB2}| unkC1: {unkC1}: unkC2: {unkC2}| unkD1: {unkD1}");
            var offsetTable = new List<int>();
            
            var mappingTables = new List<(int offset, int size)>();
            ushort nextByte = 0;
            nextByte = reader.ReadUint16();
            while (nextByte != 48 && !reader.Eof) // check if its a keyframe?
            {
                if (nextByte == 2)
                    mappingTables.Add((reader.ReadUint16(), reader.ReadUint16()));
                else
                {
                    for (int i = 0; i < nextByte; i++)
                    {
                        if (reader.BytesLeft < 2)
                            break;
                        var val1 = reader.ReadUint16();
                        _logger.LogInformation($"unkown:{val1} ");
                    }
                }
                if (reader.BytesLeft < 2)
                    break;
                nextByte = reader.ReadUint16();
            }
            _logger.LogInformation("MappingTable:" + string.Join(", ", mappingTables.Select(m => $"{m.offset}({m.size})")));

            // Now continue reading the rest of the data blocks after the header
            //var dataBlocks = new List<(ushort tag, byte[] data)>();  // List to store parsed data blocks
            var firstRead = true;
            FrameDelta? lastKeyFrame =null;
            while (!reader.Eof)
            {
                if (!firstRead)
                    nextByte = reader.ReadUint16();
                else
                    firstRead = false;

                // Check if we are at a keyframe (SpriteSize = 48 bytes)
                if (nextByte == SpriteSize)
                {
                    byte[] mainKeyframeData = reader.ReadBytes(SpriteSize); // SpriteSize = 48
                    //_logger.LogInformation($"Keyframe ({SpriteSize} bytes) read.");
                    //dataBlocks.Add((0, mainKeyframeData));  // Tag 0 for keyframe as no specific tag is used here
                    lastKeyFrame = new FrameDelta([new FrameDeltaItem(reader.Pos - 48, mainKeyframeData,null)]);
                    _frameTable.Add(lastKeyFrame);
                    //var frameData = new ReadStream(mainKeyframeData, mainKeyframeData.Length, reader.Endianness, annotator: Annotator);
                    //var rawata = frameData.LogHex(frameData.Size);
                    //_logger.LogInformation("FrameData:" + rawata);
                    continue;
                }

                // skip 0 length
                while (nextByte == 0 && reader.BytesLeft>0) 
                    nextByte = reader.ReadUint16();
                if (reader.BytesLeft == 0)
                    break;

                var tag = nextByte;
                var tagEnum = (RayScoreTags.ScoreKeyframeTag?)Enum.ToObject(typeof(RayScoreTags.ScoreKeyframeTag), tag);
                var tagLength = tagEnum.HasValue ? RayScoreTags.GetDataLength(tagEnum.Value) : 0;

                // Read entire block: tag (2 bytes already read) + tagLength
                int length = tagLength + 2;
                byte[] blockData = new byte[length];
                BitConverter.GetBytes(tag).CopyTo(blockData, 0);
                byte[] rest = reader.ReadBytes(tagLength);
                Array.Copy(rest, 0, blockData, 2, tagLength);

                //_logger.LogInformation($"Read Tag: 0x{tag:X4} ({tagEnum?.ToString() ?? "Unknown"}) | Length: {length}");
                //dataBlocks.Add((tagEnum, blockData));
                lastKeyFrame.Items.Add(new FrameDeltaItem(reader.Pos - length, blockData, tagEnum));

                //var tag = nextByte;
                //var length = GetLengthFromTag(tag) + 2;
                //_logger.LogInformation($"length={length}, tag={tag}");
                //byte[] testData = reader.ReadBytes(length);
                //var bytesToRead = Math.Min(length, reader.BytesLeft);
                //HandleAnimationKeyframe(reader, (RayKeyframeEnabled)(tag & 0xFF));
                ////byte[] keyframeData = reader.ReadBytes(bytesToRead);
                ////dataBlocks.Add((tag, keyframeData));
                //while (nextByte != 48 && reader.BytesLeft > 0)
                //    nextByte = reader.ReadUint16();
                //firstRead = true;
            }

            // Optionally, log the total number of blocks read
            _logger.LogInformation($"Total parsed data blocks: {_frameTable.Count}");

            // You can now process `dataBlocks` further or store them as needed
        }
        




        private int GetLengthFromTag(ushort tag)
        {
            var enabled = (RayKeyframeEnabled)(tag & 0xFF);
            int length = 0;

            if (enabled.HasFlag(RayKeyframeEnabled.Path)) length += 4;
            if (enabled.HasFlag(RayKeyframeEnabled.Size)) length += 4;
            if (enabled.HasFlag(RayKeyframeEnabled.Rotation)) length += 4;
            if (enabled.HasFlag(RayKeyframeEnabled.Skew)) length += 4;
            if (enabled.HasFlag(RayKeyframeEnabled.Blend)) length += 1;
            if (enabled.HasFlag(RayKeyframeEnabled.ForeColor)) length += 1;
            if (enabled.HasFlag(RayKeyframeEnabled.BackColor)) length += 1;
            if (enabled.HasFlag(RayKeyframeEnabled.TweeningEnabled)) 
                length += 4;
            /*
             * tween enable willl be much more, but we have no test data yet.

On the screenshot we have:
Curvature? 1byte:
Checkbox Continuouos at endpoint : 1bit
Speed : 1bit
Ease-in : 1 byte?
Ease-out : 1 byte?
            */
            return length;
        }


        private void ReadFrameDataLog(ReadStream reader)
        {

            int actualSize = reader.ReadInt32("actualSize");
            int c2 = reader.ReadInt32("constMinus3"); // usually -3
            SpriteChannelCount = reader.ReadInt32("frameCount");
            short c4 = reader.ReadInt16("const4");
            SpriteSize = reader.ReadInt16("spriteSize");
            short c6 = reader.ReadInt16("const6");
            HighestFrameNumber = reader.ReadInt16("memberSpriteCount");
            // 6 sprites:
            // - 1 puppettempo
            // - 1 pallete
            // - 1 Transition
            // - 2 audio
            // - 1 framescript

            TotalSpriteCount = HighestFrameNumber + 6;

            _logger.LogInformation($"DB| Score root primary: header=(actualSize={actualSize}, {c2},frameCount= {SpriteChannelCount}, {c4},spriteSize= {SpriteSize}, {c6}, TotalSpriteCount={TotalSpriteCount})");

            int frameStart = reader.Pos;
            var decoder = new RayKeyframeDeltaDecoder(Annotator);
            
            for (int fr = 0; fr < SpriteChannelCount; fr++)
            {
                var fk = new Dictionary<string, int> { ["frame"] = fr };
                //ushort frameDataLensomething = reader.ReadUint16("frameLen");
                //ushort frameDataLen = reader.ReadUint16("frameLen", fk);

                var frameBytes = reader.ReadBytes(reader.BytesLeft); //, "frameBytes", fk);
                var frameData = new ReadStream(frameBytes, frameBytes.Length, reader.Endianness,
                    annotator: Annotator);
                var rawata = frameData.LogHex(frameData.Size);
                _logger.LogInformation("FrameData:" + rawata);
              
            }
        }



        private void ReadFrameDataGoodOne(ReadStream reader)
        {

            int actualSize = reader.ReadInt32("actualSize");
            int c2 = reader.ReadInt32("constMinus3"); // usually -3
            SpriteChannelCount = reader.ReadInt32("frameCount");
            short c4 = reader.ReadInt16("const4");
            SpriteSize = reader.ReadInt16("spriteSize");
            short c6 = reader.ReadInt16("const6");
            HighestFrameNumber = reader.ReadInt16("memberSpriteCount");
            // 6 sprites:
            // - 1 puppettempo
            // - 1 pallete
            // - 1 Transition
            // - 2 audio
            // - 1 framescript

            TotalSpriteCount = HighestFrameNumber + 6;

            _logger.LogInformation($"DB| Score root primary: header=(actualSize={actualSize}, {c2},frameCount= {SpriteChannelCount}, {c4},spriteSize= {SpriteSize}, {c6}, TotalSpriteCount={TotalSpriteCount})");

            int frameStart = reader.Pos;
            var decoder = new RayKeyframeDeltaDecoder(Annotator);
            for (int fr = 0; fr < SpriteChannelCount; fr++)
            {
                var fk = new Dictionary<string, int> { ["frame"] = fr };
                ushort frameDataLen = reader.ReadUint16("frameLen", fk);
                var frameBytes = reader.ReadBytes(frameDataLen - 2); //, "frameBytes", fk);
                var frameData = new ReadStream(frameBytes, frameDataLen - 2, reader.Endianness,
                    annotator: Annotator);
                var rawata = frameData.LogHex(frameData.Size);
                var items = new List<FrameDeltaItem>();
                while (!frameData.Eof)
                {
                    var itemLen = frameData.ReadUint16("deltaLen", fk);
                    ushort offset = frameData.ReadUint16("offset", fk);
                    if (frameData.Size - frameData.Position < 4)
                    {
                        //_logger.LogWarning($"Frame {fr}, channel {offset}: Invalid itemLen={itemLen}, skipping.");
                        break;
                    }
                    var pos = frameData.Pos;
                    byte[] data = frameData.ReadBytes(itemLen, "deltaBytes", new Dictionary<string, int> { ["frame"] = fr, ["offset"] = offset });
                    items.Add(new FrameDeltaItem(offset, data, null));

                    if (data.Length >= 48)
                    {
                        _logger.LogInformation("FrameData start offset:" + pos);
                        _logger.LogInformation("FrameData:" + rawata);
                    }
                }
                _frameTable.Add(new FrameDelta(items));
            }
        }  
        
        private void ReadFrameData444(ReadStream reader)
        {

            int actualSize = reader.ReadInt32("actualSize");
            int c2 = reader.ReadInt32("constMinus3"); // usually -3
            SpriteChannelCount = reader.ReadInt32("frameCount");
            short c4 = reader.ReadInt16("const4");
            SpriteSize = reader.ReadInt16("spriteSize");
            short c6 = reader.ReadInt16("const6");
            HighestFrameNumber = reader.ReadInt16("memberSpriteCount");
            // 6 sprites:
            // - 1 puppettempo
            // - 1 pallete
            // - 1 Transition
            // - 2 audio
            // - 1 framescript

            TotalSpriteCount = HighestFrameNumber + 6;

            _logger.LogInformation($"DB| Score root primary: header=(actualSize={actualSize}, {c2},frameCount= {SpriteChannelCount}, {c4},spriteSize= {SpriteSize}, {c6}, TotalSpriteCount={TotalSpriteCount})");

            int frameStart = reader.Pos;
            var decoder = new RayKeyframeDeltaDecoder(Annotator);
            var frame10And12found = false;
            RaySprite? sprite = null;
            var idx = 0;
            for (int fr = 0; fr < SpriteChannelCount; fr++)
            {
                var fk = new Dictionary<string, int> { ["frame"] = fr };
                ushort frameDataLen = reader.ReadUint16("frameLen", fk);
                var frameBytes = reader.ReadBytes(frameDataLen - 2); //, "frameBytes", fk);
                var frameData = new ReadStream(frameBytes, frameDataLen - 2, reader.Endianness,
                    annotator: Annotator);
                var rawata = frameData.LogHex(frameData.Size);
                _logger.LogInformation("FrameData:" + rawata);

                //var itemLen = frameData.ReadUint16("deltaLen", fk);
                //ushort offset2 = frameData.ReadUint16("offset", fk);
                var offset2 = idx * SpriteSize;
                //var flags3 = (RayKeyframeEnabled)something2;

                var keys = new Dictionary<string, int> { ["frame"] = fr, ["offset"] = offset2 };
                //if (item.Data.Length < 20)
                //{
                //    var stream = new ReadStream(item.Data, item.Data.Length, Endianness.BigEndian);
                //    var test = stream.LogHex(stream.Size);

                //    continue;
                //}
                var channelStream = new ReadStream(frameData.Data, frameData.Data.Length, Endianness.BigEndian,annotator: Annotator);
                ushort something = channelStream.ReadUint8("somethingA", fk);
                //ushort something2 = channelStream.ReadUint8("SomethingB", fk);
                /// first = 16 128 255 0 0 1
                byte flagByte = channelStream.ReadUint8("KeyframeType", fk);
                var flags = (RayKeyframeEnabled)flagByte;
                var flags2 = (RayKeyframeEnabled)something;

                var spriteNum = (offset2 / SpriteSize) - 6;
                if (channelStream.Data.Length < 4) continue;
                var itemLen2 = channelStream.ReadUint16("deltaLen", fk);
                //ushort offset3 = channelStream.ReadUint16("offset", fk);

                //if ((flags & RayKeyframeEnabled.TweeningEnabled) != 0)
                if (channelStream.Data.Length >= 48)
                {
                    sprite = ReadChannelSprite(frameData, flags, keys);
                    AllSprites.Add(sprite);

                    //var descriptor = _FrameDescriptors[idx];
                    //sprite.Behaviors = descriptor.Behaviors;
                    //sprite.StartFrame = descriptor.StartFrame;
                    //sprite.EndFrame = descriptor.EndFrame;
                    //sprite.SpriteNumber = descriptor.Channel;
                    //sprite.ExtraValues = descriptor.ExtraValues;
                    idx++;
                }
                else
                {
                    
                   

                    var keyFrame = HandleAnimationKeyframe(channelStream, flags, keys);
                    if (sprite != null && keyFrame != null)
                        sprite.Keyframes.Add(keyFrame);
                }
            }

        }

       


        private void TryLogFixedRawKeyframeSpriteBlock(ReadStream frameData, int frameNum)
        {
            int originalPos = frameData.Position;

            try
            {
                if (frameData.Size < 48)
                {
                    _logger.LogDebug($"Frame {frameNum}: not a keyframe (size={frameData.Size})");
                    return;
                }

                byte[] buf = frameData.PeekBytes(64).Skip(16).Take(48).ToArray();


                // Log the actual raw bytes for inspection
                string hexDump = string.Join(" ", buf.Select(b => b.ToString("X2")));
                _logger.LogInformation($"🔍 Frame {frameNum} raw PeekBytes(48):\n{hexDump}");

                short locH = (short)((buf[0x02] << 8) | buf[0x03]);
                short locV = (short)((buf[0x00] << 8) | buf[0x01]);
                short width = (short)((buf[0x04] << 8) | buf[0x05]);
                short height = (short)((buf[0x06] << 8) | buf[0x07]);

                byte blendRaw = buf[0x09];
                byte ink = buf[0x0A];
                int blend = 100 - (int)Math.Round(blendRaw / 255f * 100f);

                byte foreColor = buf[0x0E];
                byte backColor = buf[0x0F];

                float rotation = (short)((buf[0x20] << 8) | buf[0x21]) / 100f;
                float skew = (short)((buf[0x22] << 8) | buf[0x23]) / 100f;

                ushort memberNum = (ushort)((buf[0x2A] << 8) | buf[0x2B]);
                ushort castLib = (ushort)((buf[0x2C] << 8) | buf[0x2D]);
                uint memberId = ((uint)castLib << 16) | memberNum;

                _logger.LogInformation(
                    $"🟨 Raw Sprite Block in Frame {frameNum}:\n" +
                    $"  LocH={locH}, LocV={locV}, Width={width}, Height={height}\n" +
                    $"  ForeColor={foreColor}, BackColor={backColor}, Ink={ink}, Blend={blend}%\n" +
                    $"  Rotation={rotation:F2}, Skew={skew:F2}, Member={memberId} (CastLib={castLib}, Member={memberNum})");
            }
            catch (Exception ex)
            {
                _logger.LogWarning($"Keyframe decode failed in frame {frameNum}: {ex.Message}");
            }
            finally
            {
                frameData.Position = originalPos;
            }
        }


    
        public List<RaySprite> ReadAllFrameSprites2()
        {
            return AllSprites;
        }
        public List<RaySprite> ReadAllFrameSprites()
        {
            var allFrames = new List<RaySprite>();
            var idx = 0;
            RaySprite? sprite = null;
            for (int i = 0; i < _frameTable.Count; i++)
            {
                var frame = _frameTable[i];
                var spritesInFrame = new List<RaySprite>();

                var mainFrame = frame.Items.FirstOrDefault();
                if (mainFrame == null)
                    continue;
                //foreach (var mainFrame in frame.Items)
                {
                    var keys = new Dictionary<string, int> { ["frame"] = i, ["offset"] = mainFrame.Offset };
                    //if (item.Data.Length < 20)
                    //{
                    //    var stream = new ReadStream(item.Data, item.Data.Length, Endianness.BigEndian);
                    //    var test = stream.LogHex(stream.Size);

                    //    continue;
                    //}
                    var channelStream = new ReadStream(mainFrame.Data, mainFrame.Data.Length, Endianness.BigEndian,
                        annotator: Annotator);
                    // Read the flag byte and convert it to the KeyframeFlags enum
                    

                    var spriteNum = (mainFrame.Offset / SpriteSize) - 6;
                    //if (channelStream.Data.Length < 4) continue;
                    //if ((flags & RayKeyframeEnabled.TweeningEnabled) != 0)
                    if (mainFrame.Type == null) // keyframe
                    {
                        // 16 36 203 -> 10 24 CB
                        sprite = ReadChannelSprite(channelStream, RayKeyframeEnabled.None, keys);
                        spritesInFrame.Add(sprite);

                        var descriptor = _FrameDescriptors[idx];
                        sprite.Behaviors = descriptor.Behaviors;
                        sprite.StartFrame = descriptor.StartFrame;
                        sprite.EndFrame = descriptor.EndFrame;
                        sprite.SpriteNumber = descriptor.Channel;
                        sprite.ExtraValues = descriptor.ExtraValues;
                        idx++;
                    }
                    //else
                    //{
                        //var keyFrame = RayScoreTags.CreateKeyFrameFromTags(frame);
                        //if (sprite != null && keyFrame != null)
                        //    sprite.Keyframes.Add(keyFrame);

                        //byte flags1 = channelStream.ReadUint8("Flag1", keys);
                        //byte flagByte = channelStream.ReadUint8("KeyframeType", keys);
                        //var flags = (RayKeyframeEnabled)flagByte;

                        //var keyFrame = HandleAnimationKeyframe(channelStream, flags, keys);
                        //if (sprite != null && keyFrame != null)
                        //    sprite.Keyframes.Add(keyFrame);
                    //}
                }
                if (spritesInFrame.Count == 0)
                    continue;

                allFrames.AddRange(spritesInFrame);
            }
            AllSprites = allFrames;
            return allFrames;
        }

        

        public RayKeyFrame? HandleAnimationKeyframe(ReadStream stream, RayKeyframeEnabled flags, Dictionary<string, int>? keys = null)
        {
            if (stream.Size < 8) return null;
            RayKeyFrame keyframe = new RayKeyFrame();

           
            if (flags == RayKeyframeEnabled.None) return null;

            // Parse properties based on flag bits
            if ((flags & RayKeyframeEnabled.Path) != 0) // Path = LocH + LocV
            {
                keyframe.LocH = stream.ReadInt16("LocH", keys);
                keyframe.LocV = stream.ReadInt16("LocV", keys);
            }

            if ((flags & RayKeyframeEnabled.Size) != 0)
            {
                keyframe.Width = stream.ReadInt16("Width", keys);
                keyframe.Height = stream.ReadInt16("Height", keys);
            }

            if ((flags & RayKeyframeEnabled.Rotation) != 0)
                keyframe.Rotation = stream.ReadFloat("Rotation", keys) / 100f; // ReadFloat , ReadInt16

            if ((flags & RayKeyframeEnabled.Skew) != 0)
                keyframe.Skew = stream.ReadFloat("Skew", keys) / 100f;// ReadFloat

            if ((flags & RayKeyframeEnabled.Blend) != 0)
            {
                byte blendByte = stream.ReadUint8("Blend", keys);
                keyframe.Blend = (int)((blendByte / 255f) * 100f);
            }

            if ((flags & RayKeyframeEnabled.ForeColor) != 0)
                keyframe.ForeColor = stream.ReadUint8("ForeColor", keys);

            if ((flags & RayKeyframeEnabled.BackColor) != 0)
                keyframe.BackColor = stream.ReadUint8("BackColor", keys);

            // Log the parsed keyframe
            _logger.LogInformation($"Parsed keyframe {(keys !=null ?string.Join(',', keys.Select(s => s.Key+"="+s.Value)):"")}: Flags={flags}; " +
                             $"LocH={keyframe.LocH}, LocV={keyframe.LocV}, " +
                             $"Width={keyframe.Width}, Height={keyframe.Height}, " +
                             $"Rotation={keyframe.Rotation}, Skew={keyframe.Skew}, " +
                             $"Blend={keyframe.Blend}, ForeColor={keyframe.ForeColor}, " +
                             $"BackColor={keyframe.BackColor}");

            return keyframe;
        }


        private IntervalDescriptor? ReadFrameIntervalDescriptor(int index, ReadStream stream)
        {
            if (stream.Size < 44)
                return null;

            var desc = new IntervalDescriptor();
            var k = new Dictionary<string, int> { ["entry"] = index };
            desc.StartFrame = stream.ReadInt32("startFrame", k);
            desc.EndFrame = stream.ReadInt32("endFrame", k);
            desc.Unknown1 = stream.ReadInt32("unk1", k);
            desc.Unknown2 = stream.ReadInt32("unk2", k);
            desc.Channel = stream.ReadInt32("channel", k); // after top channels , so start at 6 if 2 audio channels
            desc.UnknownAlwaysOne = stream.ReadInt16("const1", k); // seems always 1
            desc.UnknownNearConstant15_0 = stream.ReadInt32("const15", k);  // always 0F
            desc.UnknownE1 = stream.ReadUint8("e1", k);  // always E1
            desc.UnknownFD = stream.ReadUint8("fd", k);  // always FD
                                                         //desc.SpriteNumber = stream.ReadInt32(); <- not found
                                                         //stream.Skip(16);
            desc.Unknown7 = stream.ReadInt16("unk7", k);
            desc.Unknown8 = stream.ReadInt32("unk8", k);
            while (stream.Pos + 4 <= stream.Size)
                desc.ExtraValues.Add(stream.ReadInt32("extra", k));
            _logger.LogInformation($"Item Desc. {index}: Start={desc.StartFrame}, End={desc.EndFrame}, Channel={desc.Channel}, U1={desc.Unknown1}, U2={desc.Unknown2}, U3={desc.UnknownAlwaysOne}, U4={desc.UnknownNearConstant15_0}, U5={desc.UnknownE1}, U6={desc.UnknownFD}");
            return desc;
        }

        private List<RaysBehaviourRef> ReadBehaviors(int ind, ReadStream ss)
        {
            // Secondary bytestring lists behaviour scripts
            var behaviours = new List<RaysBehaviourRef>();
            while (ss.Pos + 8 <= ss.Size)
            {
                var k = new Dictionary<string, int> { ["entry"] = ind };
                short cl = ss.ReadInt16("castLib", k);
                short cm = ss.ReadInt16("castMmb", k);
                ss.ReadInt32("zero", k); // constant 0
                behaviours.Add(new RaysBehaviourRef { CastLib = cl, CastMmb = cm });
            }
            return behaviours;
        }



        [Flags]
        public enum RayKeyFrameType
        {
            None = 0,
            Type0 = 1 << 0,
            Type1 = 1 << 1,
            Type2 = 1 << 2,
            Type3 = 1 << 3,
            Type4 = 1 << 4,
            Type5 = 1 << 5,
            Type6 = 1 << 6,
            Type7 = 1 << 7,      
        }
        // First byte type:
        //If keyframeType indicates a sprite, the frame data might include properties like position(LocH, LocV), size, rotation, skew, color, etc.

        // If it's an animation keyframe, it could contain information about the animation state (e.g., blending modes, speed).
        private RaySprite ReadChannelSprite(ReadStream stream, RayKeyframeEnabled flags, Dictionary<string, int>? keys = null)
        {
            var sprite = new RaySprite();
            // info from scummVM:
            // 1 byte for the type  // always 16 it seems
            // 2 bytes for size -> // 0 ,8, 36, 1 , 2 -> this seems strange because we read the flags

            var flags1 = stream.ReadUint8(); 
            var flags11 = (RayKeyFrameType)(flags1 & 0xFF);
            _logger.LogInformation("Sprite flags=" + flags11.ToString());
            if (!flags11.HasFlag(RayKeyFrameType.Type4) && flags1 != 0)
            {
                var keyframeType = stream.ReadUint8(); 
                var keyframeSize = stream.ReadUint8();
                _logger.LogInformation($"something1={keyframeType}, something2={keyframeSize}");
            }
            
            byte inkByte = stream.ReadUint8("ink", keys);
            sprite.Ink = inkByte & 0x7F;
            sprite.ForeColor = stream.ReadUint8("foreColor", keys);
            sprite.BackColor = stream.ReadUint8("backColor", keys);
            sprite.MemberCastLib = (int)stream.ReadUint16("memberCastLib", keys);
            sprite.MemberNum = (int)stream.ReadUint16("memberNum", keys);
            stream.Skip(2); // unknown
            sprite.SpritePropertiesOffset = stream.ReadUint16("propOffset", keys); //18,27,30,33,36
            sprite.LocV = stream.ReadInt16("locV", keys);
            sprite.LocH = stream.ReadInt16("locH", keys);
            sprite.Height = stream.ReadInt16("height", keys);
            sprite.Width = stream.ReadInt16("width", keys);
            byte colorcode = stream.ReadUint8("color", keys);
            sprite.Editable = (colorcode & 0x40) != 0;
            sprite.ScoreColor = colorcode & 0x0F;
            var blend = stream.ReadUint8("blendRaw", keys);
            sprite.Blend = (int)Math.Round(100f - blend / 255f * 100f);
            byte flag2 = stream.ReadUint8("flipFlags", keys);
            sprite.FlipV = (flag2 & 0x04) != 0;
            sprite.FlipH = (flag2 & 0x02) != 0;
            stream.Skip(5);
            if (stream.Size > 28)
            {
                //var test = stream.ReadInt16();
                sprite.Rotation = stream.ReadInt32("rotation", keys) / 100f;
                sprite.Skew = stream.ReadInt32("skew", keys) / 100f;
            }
            //stream.Skip(12);
            _logger.LogInformation($"{sprite.LocH}x{sprite.LocV}:Ink={sprite.Ink}:Blend={sprite.Blend}:Skew={sprite.Skew}:Rot={sprite.Rotation}:PropOffset={sprite.SpritePropertiesOffset}:Member={sprite.MemberCastLib},{sprite.MemberNum}");
            return sprite;
        }


        private class IntervalDescriptor
        {
            public int StartFrame { get; internal set; }
            public int EndFrame { get; internal set; }
            public int Unknown1 { get; internal set; }
            public int Unknown2 { get; internal set; }
            public int SpriteNumber { get; internal set; }
            public int UnknownAlwaysOne { get; internal set; }
            public int UnknownNearConstant15_0 { get; internal set; }
            public int UnknownE1 { get; internal set; }
            public int UnknownFD { get; internal set; }
            public int Unknown7 { get; internal set; }
            public int Unknown8 { get; internal set; }
            public List<int> ExtraValues { get; } = new();
            public int Channel { get; internal set; }
            public List<RaysBehaviourRef> Behaviors { get; internal set; } = new List<RaysBehaviourRef>();
        }




        #endregion






        public void ReadFrameData5()
        {
            var reader = new ReadStream(_FrameDataBufferView, Endianness.BigEndian, annotator: Annotator);
            int actualSize = reader.ReadInt32("actualSize");
            int c2 = reader.ReadInt32("constMinus3"); // usually -3
            SpriteChannelCount = reader.ReadInt32("frameCount");
            short c4 = reader.ReadInt16("const4");
            SpriteSize = reader.ReadInt16("spriteSize");
            short c6 = reader.ReadInt16("const6");
            HighestFrameNumber = reader.ReadInt16("memberSpriteCount");
            // 6 sprites:
            // - 1 puppettempo
            // - 1 pallete
            // - 1 Transition
            // - 2 audio
            // - 1 framescript

            TotalSpriteCount = HighestFrameNumber + 6;

            _logger.LogInformation($"Frame Count: {SpriteChannelCount}, Sprite Size: {SpriteSize}");

            int frameStart = reader.Pos;
            for (int fr = 0; fr < SpriteChannelCount; fr++)
            {
                var fk = new Dictionary<string, int> { ["frame"] = fr };
                ushort frameDataLen = reader.ReadUint16("frameLen", fk);

                // Skip frames with zero-length data
                if (frameDataLen == 0)
                {
                    _logger.LogWarning($"Frame {fr}: Zero-length frame data, skipping.");
                    continue;
                }

                var frameBytes = reader.ReadBytes(frameDataLen - 2, "frameBytes", fk);
                var frameData = new ReadStream(frameBytes, frameDataLen - 2, reader.Endianness, annotator: Annotator);

                // Check if frame data size is valid
                if (frameData.Size < 4)
                {
                    _logger.LogWarning($"Frame {fr}: Frame data is too small (size={frameData.Size}), skipping.");
                    continue;
                }

                // Validate span or offset for this frame
                if (frameData.Position >= frameData.Size || frameData.Position + 8 > frameData.Size)
                {
                    _logger.LogWarning($"Frame {fr}: Invalid span or offset. Start={frameData.Position}, End={frameData.Position + 8}, Skipping frame.");
                    continue;
                }

                var deltaItems = ReadKeyframeData(frameData, fr);
                _frameTable.Add(new FrameDelta(deltaItems));
            }
        }


        private List<FrameDeltaItem> ReadKeyframeData(ReadStream frameData, int frameIndex)
        {
            var items = new List<FrameDeltaItem>();
            int totalSize = frameData.Size;
            int startPosition = frameData.Position;

            while (!frameData.Eof)
            {
                if (frameData.Size - frameData.Position < 4)
                {
                    _logger.LogWarning($"Frame {frameIndex}: Not enough bytes to read item header. Skipping frame.");
                    break;
                }

                ushort itemLen = frameData.ReadUint16("deltaLen", new() { ["frame"] = frameIndex });
                ushort offset = frameData.ReadUint16("offset", new() { ["frame"] = frameIndex });

                if (itemLen == 0 || frameData.Position + itemLen > totalSize)
                {
                    _logger.LogWarning($"Frame {frameIndex}, offset={offset}: Invalid itemLen={itemLen}, skipping.");
                    break;
                }

                byte[] data = frameData.ReadBytes(itemLen); //, "deltaBytes", new Dictionary<string, int> { ["frame"] = frameIndex, ["offset"] = offset });
                items.Add(new FrameDeltaItem(offset, data, null));
            }

            // Add a check to log when we haven't added items for the frame
            if (items.Count == 0)
            {
                _logger.LogWarning($"Frame {frameIndex}: No valid keyframe data found.");
            }

            return items;
        }


        private void LogKeyframeSpriteProperties(ReadStream frameData, int frameNum)
        {
            int originalPos = frameData.Position;

            try
            {
                // Ensure there are enough bytes to process
                if (frameData.Size < 48)
                {
                    _logger.LogWarning($"Frame {frameNum}: Not enough data to parse sprite properties (size={frameData.Size})");
                    return;
                }

                // Ensure we have enough bytes to read (after skipping the first 16 bytes)
                byte[] buf = frameData.PeekBytes(64);
                if (buf.Length < 48)
                {
                    _logger.LogWarning($"Frame {frameNum}: Not enough data in the sprite block (size={buf.Length})");
                    return;
                }

                // Skip the first 16 bytes and take the next 48 bytes for sprite properties
                buf = buf.Skip(16).Take(48).ToArray();

                string hexDump = string.Join(" ", buf.Select(b => b.ToString("X2")));
                _logger.LogInformation($"🔍 Frame {frameNum} raw PeekBytes(48):\n{hexDump}");

                short locH = (short)((buf[0x02] << 8) | buf[0x03]);
                short locV = (short)((buf[0x00] << 8) | buf[0x01]);
                short width = (short)((buf[0x04] << 8) | buf[0x05]);
                short height = (short)((buf[0x06] << 8) | buf[0x07]);

                byte blendRaw = buf[0x09];
                byte ink = buf[0x0A];
                int blend = 100 - (int)Math.Round(blendRaw / 255f * 100f);

                byte foreColor = buf[0x0E];
                byte backColor = buf[0x0F];

                float rotation = (short)((buf[0x20] << 8) | buf[0x21]) / 100f;
                float skew = (short)((buf[0x22] << 8) | buf[0x23]) / 100f;

                ushort memberNum = (ushort)((buf[0x2A] << 8) | buf[0x2B]);
                ushort castLib = (ushort)((buf[0x2C] << 8) | buf[0x2D]);
                uint memberId = ((uint)castLib << 16) | memberNum;

                _logger.LogInformation(
                    $"🟨 Raw Sprite Block in Frame {frameNum}:\n" +
                    $"  LocH={locH}, LocV={locV}, Width={width}, Height={height}\n" +
                    $"  ForeColor={foreColor}, BackColor={backColor}, Ink={ink}, Blend={blend}%\n" +
                    $"  Rotation={rotation:F2}, Skew={skew:F2}, Member={memberId} (CastLib={castLib}, Member={memberNum})");
            }
            catch (Exception ex)
            {
                _logger.LogWarning($"Keyframe decode failed in frame {frameNum}: {ex.Message}");
            }
            finally
            {
                frameData.Position = originalPos;
            }
        }







        #region OLD ChatGPT test



        public Dictionary<int, KeyFrameState> ParseAllFrameDeltasSafe()
        {
            var reader = new ReadStream(_FrameDataBufferView, Endianness.BigEndian, annotator: Annotator);
            return ParseAllFramesDelta(reader);
        }

        public Dictionary<int, KeyFrameState> ParseAllFramesDelta(ReadStream reader)
        {
            var spriteStates = new Dictionary<int, KeyFrameState>();
            int position = 0;

            // Convert the reader stream to a byte array
            byte[] frameBytes = reader.ReadBytes(reader.Size);//, "frameBytes");

            while (position < frameBytes.Length)
            {
                // Read the frame length (2 bytes) at the current position
                ushort frameDataLen = BitConverter.ToUInt16(frameBytes, position);
                position += 2;

                // If the frame data length is too small or exceeds available data, skip
                if (frameDataLen <= 2 || position + frameDataLen > frameBytes.Length)
                {
                    position += 2; // Skip the invalid frame data length
                    continue;
                }

                // Extract the frame data from the byte array based on the frameDataLen
                byte[] frameData = new byte[frameDataLen];
                Array.Copy(frameBytes, position, frameData, 0, frameDataLen);
                position += frameDataLen;

                // Inspect LocH and LocV for this frame (LocH at 0x0188, LocV at 0x018A)
                InspectLocHLocV(frameData, position);

                // Optionally: Store or process KeyFrameState here
                // var state = new KeyFrameState();
                // spriteStates[position] = state;

                // Process further data or states if needed, e.g., applying deltas
                // ApplyDeltaFrame(frameData, state);
            }

            return spriteStates;
        }







        private ushort GetFrameDataLengthFromStructure(byte[] frameHeader)
        {
            // Implement the logic to read the frame length from the frame's structure,
            // likely by using a specific header or predefined format for your frames.
            // This is a placeholder for how you would retrieve frame data length.
            return BitConverter.ToUInt16(frameHeader, 0); // Modify this to match actual structure.
        }







        public void LogFinalValues(int frameIndex, KeyFrameState keyframe)
        {
            _logger.LogInformation($"Frame {frameIndex}: LocH={keyframe.LocH}, LocV={keyframe.LocV}, Width={keyframe.Width}, Height={keyframe.Height}, Rotation={keyframe.Rotation}, Skew={keyframe.Skew}, Blend={keyframe.Blend}, Ink={keyframe.Ink}, ForeColor={keyframe.ForeColor}, BackColor={keyframe.BackColor}");
        }







        public void InspectLocHLocV(byte[] frameData, int frameIndex)
        {
            return;
            // Log the raw frame data as hex for inspection
            //var rawata = BitConverter.ToString(frameData).Replace("-", " ");
            //_logger.LogInformation($"FrameIndex {frameIndex + 1} Raw Data: {rawata}");

            // Define the offsets for LocH and LocV from the table (0x0188 for LocH, 0x018A for LocV)
            int locHOffset = 0x0188;  // LocH offset at byte position 0x0188
            int locVOffset = 0x018A;  // LocV offset at byte position 0x018A

            // Ensure we're reading within the bounds of the frameData
            if (frameData.Length > locVOffset)
            {
                // Read LocH and LocV from their respective positions (16-bit values)
                short locH = BitConverter.ToInt16(frameData, locHOffset);
                short locV = BitConverter.ToInt16(frameData, locVOffset);

                // Log the raw data for LocH and LocV
                _logger.LogInformation($"FrameIndex {frameIndex + 1}: LocH Raw Data: {locH}, LocV Raw Data: {locV}");
            }
            else
            {
                _logger.LogWarning($"FrameIndex {frameIndex + 1}: Frame data is too short to read LocH and LocV.");
            }
        }









        public void ApplyDeltaFrame(byte[] frameData, KeyFrameState currentState)
        {
            var parser = new TaggedFieldParser(frameData);
            var loggedUnknownTags = new HashSet<ushort>();
            var rawata = BitConverter.ToString(frameData).Replace("-", " ");
            _logger.LogInformation($"Frame Raw Data: {rawata}");

            while (parser.TryReadNextField(out ushort tag, out ushort value))
            {
                switch (tag)
                {
                    //case 0x01EC: // MemberNum low 16 bits
                    //    {
                    //        var high = currentState.MemberNum >> 16;
                    //        currentState.MemberNum = (int)((high << 16) | value);
                    //        _logger.LogInformation($"Tag 0x01EC: Updated MemberNum low 16 bits to {value} => MemberNum now {currentState.MemberNum}");
                    //        break;
                    //    }
                    //case 0x01A2: // CastLib (high 16 bits)
                    //    {
                    //        var low = currentState.MemberCastLib & 0xFFFF;
                    //        currentState.MemberCastLib = (int)((value << 16) | low);
                    //        _logger.LogInformation($"Tag 0x01A2: Updated CastLib (high 16 bits) to {value} => MemberCastLib now {currentState.MemberCastLib}");
                    //        break;
                    //    }
                    //case 0x019E: // Ink
                    //    currentState.Ink = (byte)(value & 0xFF);
                    //    _logger.LogInformation($"Tag 0x019E: Updated Ink to {currentState.Ink}");
                    //    break;

                    //case 0x01FE: // Blend (inverse)
                    //    currentState.Blend = 100 - (int)Math.Round(value / 255f * 100f);
                    //    _logger.LogInformation($"Tag 0x01FE: Updated Blend to {currentState.Blend}%");
                    //    break;

                    case 0x0136: // LocH
                        currentState.LocH = (short)value;
                        _logger.LogInformation($"Tag 0x0136: Updated LocH to {currentState.LocH}");
                        break;

                    case 0x0132: // LocV
                        currentState.LocV = (short)value;
                        _logger.LogInformation($"Tag 0x0132: Updated LocV to {currentState.LocV}");
                        break;

                    //case 0x0210: // Width
                    //    currentState.Width = (short)value;
                    //    _logger.LogInformation($"Tag 0x0210: Updated Width to {currentState.Width}");
                    //    break;

                    //case 0x0212: // Height
                    //    currentState.Height = (short)value;
                    //    _logger.LogInformation($"Tag 0x0212: Updated Height to {currentState.Height}");
                    //    break;

                    //case 0x0200: // Rotation
                    //    float rotation = ((value >> 8) & 0xFF) + ((value & 0xFF) / 256f);
                    //    currentState.Rotation = rotation;
                    //    _logger.LogInformation($"Tag 0x0200: Updated Rotation to {rotation:F2} degrees");
                    //    break;

                    //case 0x0202: // Skew
                    //    float skew = ((value >> 8) & 0xFF) + ((value & 0xFF) / 256f);
                    //    currentState.Skew = skew;
                    //    _logger.LogInformation($"Tag 0x0202: Updated Skew to {skew:F2}");
                    //    break;

                    //case 0x013E: // ForeColor
                    //    currentState.ForeColor = (byte)(value & 0xFF);
                    //    _logger.LogInformation($"Tag 0x013E: Updated ForeColor to {currentState.ForeColor}");
                    //    break;

                    //case 0x0140: // BackColor
                    //    currentState.BackColor = (byte)(value & 0xFF);
                    //    _logger.LogInformation($"Tag 0x0140: Updated BackColor to {currentState.BackColor}");
                    //    break;

                    default:
                        if (!loggedUnknownTags.Contains(tag))
                        {
                            // _logger.LogInformation($"Unknown tag 0x{tag:X4} with value {value} encountered.");
                            loggedUnknownTags.Add(tag);
                        }
                        break;
                }
            }
        }
        public void ApplyDeltasAndLog(ReadStream frameData, int frameIndex)
        {
            var spriteState = new KeyFrameState();  // Hold the state of the current keyframe

            while (!frameData.Eof)
            {
                // Check if we have enough bytes to read a delta item
                if (frameData.BytesLeft < 4)
                {
                    break;
                }

                ushort deltaLen = frameData.ReadUint16("deltaLen", new() { ["frame"] = frameIndex });
                ushort offset = frameData.ReadUint16("offset", new() { ["frame"] = frameIndex });

                // Skip empty delta items
                if (deltaLen == 0)
                {
                    continue;
                }

                // Read delta bytes
                byte[] deltaBytes = frameData.ReadBytes(deltaLen, "deltaBytes", new() { ["frame"] = frameIndex, ["offset"] = offset });

                // Apply deltas based on tags (e.g., LocH, LocV, Blend)
                foreach (var deltaByte in deltaBytes)
                {
                    // Example for applying specific delta tags:
                    if (offset == 0x0136)  // LocH tag
                    {
                        spriteState.LocH += deltaByte; // Apply delta to LocH
                    }
                    else if (offset == 0x0132)  // LocV tag
                    {
                        spriteState.LocV += deltaByte; // Apply delta to LocV
                    }
                    else if (offset == 0x0210)  // Width tag
                    {
                        spriteState.Width += deltaByte; // Apply delta to Width
                    }
                    else if (offset == 0x0212)  // Height tag
                    {
                        spriteState.Height += deltaByte; // Apply delta to Height
                    }
                    else if (offset == 0x0200)  // Rotation tag
                    {
                        spriteState.Rotation += deltaByte; // Apply delta to Rotation
                    }
                    else if (offset == 0x0202)  // Skew tag
                    {
                        spriteState.Skew += deltaByte; // Apply delta to Skew
                    }
                    else if (offset == 0x019E)  // Ink tag
                    {
                        spriteState.Ink += deltaByte; // Apply delta to Ink
                    }
                    else if (offset == 0x01FE)  // Blend tag
                    {
                        spriteState.Blend += deltaByte; // Apply delta to Blend
                    }
                    else if (offset == 0x013E)  // ForeColor tag
                    {
                        spriteState.ForeColor += deltaByte; // Apply delta to ForeColor
                    }
                    else if (offset == 0x0140)  // BackColor tag
                    {
                        spriteState.BackColor += deltaByte; // Apply delta to BackColor
                    }
                }

                // Log final keyframe values for this frame after applying deltas
                LogKeyframeValues(frameIndex, spriteState);
            }
        }

        public void LogKeyframeValues(int frameIndex, KeyFrameState keyframe)
        {
            _logger.LogInformation($"Frame {frameIndex}: LocH={keyframe.LocH}, LocV={keyframe.LocV}, Width={keyframe.Width}, Height={keyframe.Height}, Rotation={keyframe.Rotation}, Skew={keyframe.Skew}, Blend={keyframe.Blend}, Ink={keyframe.Ink}, ForeColor={keyframe.ForeColor}, BackColor={keyframe.BackColor}");
        }








        public class KeyFrameState
        {
            public int LocH { get; set; }
            public int LocV { get; set; }
            public int Width { get; set; }
            public int Height { get; set; }
            public float Rotation { get; set; }
            public float Skew { get; set; }
            public int Blend { get; set; }
            public int Ink { get; set; }
            public int ForeColor { get; set; }
            public int BackColor { get; set; }
            public int MemberCastLib { get; set; }
            public int MemberNum { get; set; }

            public KeyFrameState()
            {
                // Initialize defaults or previous frame values if needed
            }
        }

        public void ParseFromSprite(KeyFrameState keyframe, RaySprite sprite)
        {
            keyframe.LocH = sprite.LocH;
            keyframe.LocV = sprite.LocV;
            keyframe.Width = sprite.Width;
            keyframe.Height = sprite.Height;
            keyframe.Rotation = sprite.Rotation;
            keyframe.Skew = sprite.Skew;
            keyframe.Blend = sprite.Blend;
            keyframe.Ink = sprite.Ink;
            keyframe.ForeColor = sprite.ForeColor;
            keyframe.BackColor = sprite.BackColor;
            keyframe.MemberNum = sprite.MemberNum;
            keyframe.MemberCastLib = sprite.MemberCastLib;
        }
        public void CalculateFinalKeyFrame(KeyFrameState keyframeState, RayKeyFrame finalKeyframe, RayKeyFrame previousKeyframe)
        {
            if (previousKeyframe == null)
            {
                // For the first frame, directly set the keyframe values
                finalKeyframe.LocH = keyframeState.LocH;
                finalKeyframe.LocV = keyframeState.LocV;
                finalKeyframe.Width = keyframeState.Width;
                finalKeyframe.Height = keyframeState.Height;
                finalKeyframe.Rotation = keyframeState.Rotation;
                finalKeyframe.Skew = keyframeState.Skew;
                finalKeyframe.Blend = keyframeState.Blend;
                finalKeyframe.Ink = keyframeState.Ink;
                finalKeyframe.ForeColor = keyframeState.ForeColor;
                finalKeyframe.BackColor = keyframeState.BackColor;
                finalKeyframe.MemberNum = keyframeState.MemberNum;
                finalKeyframe.MemberCastLib = keyframeState.MemberCastLib;
            }
            else
            {
                // For subsequent frames, accumulate deltas
                finalKeyframe.LocH = previousKeyframe.LocH + keyframeState.LocH;
                finalKeyframe.LocV = previousKeyframe.LocV + keyframeState.LocV;
                finalKeyframe.Width = previousKeyframe.Width + keyframeState.Width;
                finalKeyframe.Height = previousKeyframe.Height + keyframeState.Height;
                finalKeyframe.Rotation = previousKeyframe.Rotation + keyframeState.Rotation;
                finalKeyframe.Skew = previousKeyframe.Skew + keyframeState.Skew;
                finalKeyframe.Blend = previousKeyframe.Blend + keyframeState.Blend;
                finalKeyframe.Ink = previousKeyframe.Ink + keyframeState.Ink;
                finalKeyframe.ForeColor = previousKeyframe.ForeColor + keyframeState.ForeColor;
                finalKeyframe.BackColor = previousKeyframe.BackColor + keyframeState.BackColor;
                finalKeyframe.MemberNum = keyframeState.MemberNum; // This might not need to be accumulated
                finalKeyframe.MemberCastLib = keyframeState.MemberCastLib; // Same as MemberNum, unless it changes
            }
        }


        public void ParseFromPreviousKeyframe(KeyFrameState keyframe, KeyFrameState previousKeyframe, FrameDeltaItem deltaItem)
        {
            // Apply deltas to the previous keyframe's values
            foreach (var delta in deltaItem.Data)
            {
                // Apply changes to each property based on the delta value
                if (delta == 0x0136)  // Example for LocH delta tag
                {
                    keyframe.LocH = previousKeyframe.LocH + (short)delta;  // Apply LocH delta
                }
                else if (delta == 0x0132)  // Example for LocV delta tag
                {
                    keyframe.LocV = previousKeyframe.LocV + (short)delta;  // Apply LocV delta
                }
                // Repeat for other fields like Width, Height, Rotation, Skew, etc.
            }
        }



        public class TaggedFieldParser
        {
            private readonly byte[] _buffer;
            private int _position;

            public TaggedFieldParser(byte[] buffer)
            {
                _buffer = buffer;
                _position = 0;
            }

            public bool TryReadNextField(out ushort tag, out ushort value)
            {
                tag = 0;
                value = 0;
                if (_position + 4 > _buffer.Length)
                    return false;

                tag = (ushort)((_buffer[_position] << 8) | _buffer[_position + 1]);
                value = (ushort)((_buffer[_position + 2] << 8) | _buffer[_position + 3]);
                _position += 4;
                return true;
            }
        }

        private bool IsFullKeyframe(ReadStream frameData)
        {
            // Minimum expected size of a full keyframe block (48 bytes + possible header)
            const int MinKeyframeSize = 48;

            // If frameData size is less than minimum keyframe block, it's not full
            if (frameData.Size < MinKeyframeSize)
                return false;

            // Peek first bytes to check for typical keyframe structure signatures if available
            // Example: check if first bytes look like reasonable LocV, LocH, Width, Height (int16 pairs)
            byte[] buf = frameData.PeekBytes(MinKeyframeSize);

            short locV = (short)((buf[0] << 8) | buf[1]);
            short locH = (short)((buf[2] << 8) | buf[3]);
            short width = (short)((buf[4] << 8) | buf[5]);
            short height = (short)((buf[6] << 8) | buf[7]);

            // Sanity check ranges (tune as needed)
            if (locV < 0 || locV > 1000) return false;
            if (locH < 0 || locH > 1000) return false;
            if (width <= 0 || width > 1024) return false;
            if (height <= 0 || height > 1024) return false;

            // If all checks passed, assume full keyframe
            return true;
        }

        #endregion

    }
}
