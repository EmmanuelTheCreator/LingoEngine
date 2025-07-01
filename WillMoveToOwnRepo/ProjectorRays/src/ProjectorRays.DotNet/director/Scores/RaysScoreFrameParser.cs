using Microsoft.Extensions.Logging;
using ProjectorRays.Common;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Threading.Channels;
using static ProjectorRays.director.Scores.RaysScoreChunk;

namespace ProjectorRays.director.Scores
{
    internal class RaysScoreFrameParser
    {
        private readonly ILogger _logger;
        private BufferView _FrameDataBufferView;
        private List<BufferView> _FrameIntervalDescriptorBuffers= new();
        private List<BufferView> _BehaviorScriptBuffers = new();
        private List<FrameDelta> _frameTable = new List<FrameDelta>();
        private List<IntervalDescriptor> _FrameDescriptors;
        private Dictionary<int, IntervalDescriptor> _channelToDescriptor = new();

        /// <summary>List of behaviour script lists in the same order as the
        /// interval table.</summary>
        public List<List<RaysBehaviourRef>> FrameScripts { get; } = new();
        /// <summary>Frame order and additional metadata parsed from the chunk.</summary>
        public List<int> IntervalOrder { get; } = new();

        public int MemberSpritesCount { get; private set; }
        public int TotalSpriteCount {get;set; }
        public int SpriteSize {get;set; }
        public int FrameCount { get;set; }

        public class FrameDeltaItem
        {
            public int Offset;
            public byte[] Data;
            public FrameDeltaItem(int offset, byte[] data)
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


        public RaysScoreFrameParser(ILogger logger)
        {
            _logger = logger;
        }


        public void ReadAllIntervals(int entryCount, ReadStream stream)
        {
            // Offsets from the start of the entries area
            int[] offsets = new int[entryCount + 1];
            for (int i = 0; i < offsets.Length; i++)
                offsets[i] = stream.ReadInt32();

            int entriesStart = stream.Pos;
            // Parse framedata header and decode the delta encoded frames
            if (entryCount < 1)
                return;
            var size = offsets[1] - offsets[0];
            int absoluteStart = stream.Offset + entriesStart + offsets[0];

            _FrameDataBufferView = new BufferView(stream.Data, absoluteStart, size);

            // Interval order table
            if (entryCount >= 2)
            {
                size = offsets[2] - offsets[1];
                int absoluteStart2 = stream.Offset + entriesStart + offsets[1];
                var orderView = new BufferView(stream.Data, absoluteStart2, offsets[2] - offsets[1]);
                var os = new ReadStream(orderView, Endianness.BigEndian);
                if (os.Size >= 4)
                {
                    int count = os.ReadInt32();
                    for (int i = 0; i < count && os.Pos + 4 <= os.Size; i++)
                        IntervalOrder.Add(os.ReadInt32());
                }
            }

            // Beginning at entry[3] the entries form triples that describe the
            // frame interval descriptors. Parse only the entries referenced by the
            // interval order list when available.
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
                size = offsets[primaryIdx + 1] - offsets[primaryIdx];
                int absoluteStart2 = stream.Offset + entriesStart + offsets[primaryIdx];
                var ps = new ReadStream(new BufferView(stream.Data, absoluteStart2, size), Endianness.BigEndian);
                _FrameIntervalDescriptorBuffers.Add(new BufferView(stream.Data, absoluteStart2, size));
                

                // Secondary bytestring lists behaviour scripts
                var secSize = offsets[primaryIdx + 2] - offsets[primaryIdx + 1];
                int absoluteStart3 = stream.Offset + entriesStart + offsets[primaryIdx + 1];
                var secView = new BufferView(stream.Data, absoluteStart3, secSize);
                _BehaviorScriptBuffers.Add(secView);
                // tertiary entry usually empty, skip
            }
        }
        public void ReadFrameDescriptors()
        {
            _channelToDescriptor = new();
            _FrameDescriptors = new List<IntervalDescriptor>();
            int ind = 0;
            foreach (var frameIntervalDescriptor in _FrameIntervalDescriptorBuffers)
            {
                var ps = new ReadStream(frameIntervalDescriptor, Endianness.BigEndian);
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
                var ps = new ReadStream(frameIntervalDescriptor, Endianness.BigEndian);
                var behaviourRefs = ReadBehaviors(ind, ps);
                _FrameDescriptors[ind].Behaviors.AddRange(behaviourRefs);
                FrameScripts.Add(behaviourRefs);
                ind++;
            }
        }

       

        public void ReadFrameData()
            => ReadFrameData(new ReadStream(_FrameDataBufferView));
        private void ReadFrameData(ReadStream reader)
        {
            int actualSize = reader.ReadInt32();
            int c2 = reader.ReadInt32(); // usually -3
            FrameCount = reader.ReadInt32();
            short c4 = reader.ReadInt16();
            SpriteSize = reader.ReadInt16();
            short c6 = reader.ReadInt16();
            MemberSpritesCount = reader.ReadInt16();
            // 6 sprites:
            // - 1 puppettempo
            // - 1 pallete
            // - 1 Transition
            // - 2 audio
            // - 1 framescript

            TotalSpriteCount = MemberSpritesCount + 6;

            _logger.LogInformation($"DB| Score root primary: header=(actualSize={actualSize}, {c2},frameCount= {FrameCount}, {c4},spriteSize= {SpriteSize}, {c6}, TotalSpriteCount={TotalSpriteCount})");

            int frameStart = reader.Pos;
            var decoder = new RayKeyframeDeltaDecoder();
            for (int fr = 0; fr < FrameCount; fr++)
            {
                ushort frameDataLen = reader.ReadUint16();
                var frameData = new ReadStream(reader.ReadBytes(frameDataLen - 2), frameDataLen - 2, reader.Endianness);
                var rawata = frameData.LogHex(frameData.Size);
                //_logger.LogInformation("FrameData:" + rawata);
                if (rawata.Contains("3C"))
                {

                }
                var items = new List<FrameDeltaItem>();
                var readIndex = 0;
                while (!frameData.Eof)
                {
                    //var itemLen = frameData.ReadUint16() ;
                    //ushort offset = frameData.ReadUint16();
                    //byte[] data = frameData.ReadBytes(itemLen);
                    //items.Add(new FrameDeltaItem(offset, data));
                    var itemLen = frameData.ReadUint16();
                    ushort offset = frameData.ReadUint16();
                    byte[] data = frameData.ReadBytes(itemLen);
                    items.Add(new FrameDeltaItem(offset, data));

                    // Check if this is a potential keyframe (size + signature)
                    if (data.Length > 0 && data[0] == 0x10)
                    {
                        try
                        {
                            //_logger.LogInformation($"RAW: {BitConverter.ToString(data)}");
                            var result = decoder.Decode(fr, offset, data);
                            _logger.LogInformation(
                                $"KeyFrame: {result.SpriteChannel}x{result.Frame}:Ink=?,Blend={result.Blend}:Skew={result.Skew},Rot={result.Rotation}," +
                                $"Loc=({result.LocH},{result.LocV}),Size=({result.Width},{result.Height})," +
                                $"Fore={result.ForeColor},Back={result.BackColor},Member={result.Member}");
                        }
                        catch (Exception ex)
                        {
                            _logger.LogWarning($"Decode failed at frame {fr}, offset {offset}: {ex.Message}");
                        }
                    }
                    readIndex++;
                }



                _frameTable.Add(new FrameDelta(items));
            }

            
        }
        public Dictionary<int, List<RayKeyframeBlock>> ReadKeyFramesPerChannel()
        {
            var result = new Dictionary<int, List<RayKeyframeBlock>>();

            for (int frameIndex = 0; frameIndex < _frameTable.Count; frameIndex++)
            {
                var frame = _frameTable[frameIndex];
                foreach (var item in frame.Items)
                {
                    if (item.Data.Length < 20)
                        continue;

                    var channelStream = new ReadStream(item.Data, item.Data.Length, Endianness.BigEndian);
                    var sprite = ReadChannelSprite(channelStream);

                    // Match descriptor by index in _FrameDescriptors
                    var spriteNumber = (item.Offset / SpriteSize)-6;
                    if (spriteNumber >= _FrameDescriptors.Count)
                        continue;

                    var descriptor = _FrameDescriptors[spriteNumber];
                    int channel = descriptor.Channel;

                    //var keyFrame = new RayKeyframeBlock
                    //{
                    //    Frame = frameIndex + 1,
                    //    LocH = sprite.LocH,
                    //    LocV = sprite.LocV,
                    //    Rotation = sprite.Rotation,
                    //    Skew = sprite.Skew,
                    //    Blend = sprite.Blend,
                    //    Width = sprite.Width,
                    //    Height = sprite.Height,
                    //    Member = sprite.DisplayMember
                    //};

                    //if (!result.TryGetValue(channel, out var list))
                    //    result[channel] = list = new List<RayKeyFrame>();

                    //list.Add(keyFrame);
                }
            }

            return result;
        }


        private IntervalDescriptor? ReadFrameIntervalDescriptor(int index,ReadStream stream)
        {
            if (stream.Size < 44)
                return null;
            
            var desc = new IntervalDescriptor();
            desc.StartFrame = stream.ReadInt32();
            desc.EndFrame = stream.ReadInt32();
            desc.Unknown1 = stream.ReadInt32();
            desc.Unknown2 = stream.ReadInt32();
            desc.Channel = stream.ReadInt32(); // after top channels , so start at 6 if 2 audio channels
            desc.UnknownAlwaysOne = stream.ReadInt16(); // seems always 1
            desc.UnknownNearConstant15_0 = stream.ReadInt32();  // always 0F
            desc.UnknownE1 = stream.ReadUint8();  // always E1
            desc.UnknownFD = stream.ReadUint8();  // always FD
                                                  //desc.SpriteNumber = stream.ReadInt32(); <- not found
                                                  //stream.Skip(16);
            desc.Unknown7 = stream.ReadInt16();
            desc.Unknown8 = stream.ReadInt32();
            while (stream.Pos + 4 <= stream.Size)
                desc.ExtraValues.Add(stream.ReadInt32());
            _logger.LogInformation($"Item Desc. {index}: Start={desc.StartFrame}, End={desc.EndFrame}, Channel={desc.Channel}, U1={desc.Unknown1}, U2={desc.Unknown2}, U3={desc.UnknownAlwaysOne}, U4={desc.UnknownNearConstant15_0}, U5={desc.UnknownE1}, U6={desc.UnknownFD}");
            return desc;
        }

        private List<RaysBehaviourRef> ReadBehaviors(int ind, ReadStream ss)
        {
            // Secondary bytestring lists behaviour scripts
            var behaviours = new List<RaysBehaviourRef>();
            while (ss.Pos + 8 <= ss.Size)
            {
                short cl = ss.ReadInt16();
                short cm = ss.ReadInt16();
                ss.ReadInt32(); // constant 0
                behaviours.Add(new RaysBehaviourRef { CastLib = cl, CastMmb = cm });
            }
           return behaviours;
        }

        public List<RaySprite> ReadAllFrameSprites()
        {
            var allFrames = new List<RaySprite>();
            var idx = 0;
            for (int i = 0; i < _frameTable.Count; i++)
            {
                var frame = _frameTable[i];
                var spritesInFrame = new List<RaySprite>();
                

                foreach (var item in frame.Items)
                {
                    if (item.Data.Length < 20)
                    {
                        var stream = new ReadStream(item.Data, item.Data.Length, Endianness.BigEndian);
                        var test = stream.LogHex(stream.Size);
                        //_logger.LogInformation(idx+") Rest Data: " + test);
                        //if (item.Data.Length >= 2) // This might be a position update keyframe
                        //{
                            
                        //    //ushort maybeLocV = stream.ReadUint16(10); // adjust to real offset
                        //    //ushort maybeLocH = stream.ReadUint16(12);
                        //    //if (keyframeByOffset.TryGetValue(item.Offset, out var existing))
                        //    //{
                        //    //    existing.EndLocH = maybeLocH;
                        //    //    existing.EndLocV = maybeLocV;
                        //    //    existing.HasKeyframeEnd = true;
                        //    //}
                        //}
                        continue;
                    }
                    var channelStream = new ReadStream(item.Data, item.Data.Length, Endianness.BigEndian);
                    var sprite = ReadChannelSprite(channelStream);
                    spritesInFrame.Add(sprite);
                    
                    var descriptor = _FrameDescriptors[idx];
                    sprite.Behaviors = descriptor.Behaviors;
                    sprite.StartFrame = descriptor.StartFrame;
                    sprite.EndFrame = descriptor.EndFrame;
                    sprite.SpriteNumber = descriptor.Channel;
                    sprite.ExtraValues = descriptor.ExtraValues;
                    idx++;
                }
                if (spritesInFrame.Count == 0)
                    continue;
                
                allFrames.AddRange(spritesInFrame);
            }

            return allFrames;
        }

        private RaySprite ReadChannelSprite(ReadStream stream)
        {
            var sprite = new RaySprite();
            //var always16 = stream.ReadUint8(); // always 16 it seems
            //var val2 = stream.ReadUint8(); // 0 ,8, 36, 1 , 2
            var val3 = stream.ReadUint8();// flags, unused
            byte inkByte = stream.ReadUint8();
            sprite.Ink = inkByte & 0x7F;
            sprite.ForeColor = stream.ReadUint8();
            sprite.BackColor = stream.ReadUint8();
            sprite.DisplayMember = (int)stream.ReadUint32();
            stream.Skip(2); // unknown
            sprite.SpritePropertiesOffset = stream.ReadUint16(); //18,27,30,33,36
            sprite.LocV = stream.ReadInt16();
            sprite.LocH = stream.ReadInt16();
            sprite.Height = stream.ReadInt16();
            sprite.Width = stream.ReadInt16();
            byte colorcode = stream.ReadUint8();
            sprite.Editable = (colorcode & 0x40) != 0;
            sprite.ScoreColor = colorcode & 0x0F;
            var blend = stream.ReadUint8();
            sprite.Blend = (int)Math.Round(100f - blend / 255f * 100f);
            byte flag2 = stream.ReadUint8();
            sprite.FlipV = (flag2 & 0x04) != 0;
            sprite.FlipH = (flag2 & 0x02) != 0;
            stream.Skip(5);
            //var test = stream.ReadInt16();
            sprite.Rotation = stream.ReadUint32() / 100f;
            sprite.Skew = stream.ReadUint32() / 100f;
            //stream.Skip(12);
            _logger.LogInformation($"{sprite.LocH}x{sprite.LocV}:Ink={sprite.Ink}:Blend={sprite.Blend}:Skew={sprite.Skew}:Rot={sprite.Rotation}:PropOffset={sprite.SpritePropertiesOffset}:Member={sprite.DisplayMember}");
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
    }
}
