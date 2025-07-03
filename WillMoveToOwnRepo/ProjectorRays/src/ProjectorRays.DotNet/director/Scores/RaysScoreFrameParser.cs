using Microsoft.Extensions.Logging;
using ProjectorRays.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Reflection.PortableExecutable;
using System.Threading.Channels;
using static ProjectorRays.director.Scores.RaysScoreChunk;
using static ProjectorRays.director.Scores.RaysScoreFrameParser;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace ProjectorRays.director.Scores
{
    internal class RaysScoreFrameParser
    {
        public enum KeyframeType : byte
        {
            Sprite = 0x01,
            Animation = 0x02
        }

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

        public int MemberSpritesCount { get; private set; }
        public int TotalSpriteCount { get; set; }
        public int SpriteSize { get; set; }
        public int FrameCount { get; set; }
        public List<RaySprite> AllSprites { get; private set; }

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



        private void ReadFrameData(ReadStream reader)
        {

            int actualSize = reader.ReadInt32("actualSize");
            int c2 = reader.ReadInt32("constMinus3"); // usually -3
            FrameCount = reader.ReadInt32("frameCount");
            short c4 = reader.ReadInt16("const4");
            SpriteSize = reader.ReadInt16("spriteSize");
            short c6 = reader.ReadInt16("const6");
            MemberSpritesCount = reader.ReadInt16("memberSpriteCount");
            // 6 sprites:
            // - 1 puppettempo
            // - 1 pallete
            // - 1 Transition
            // - 2 audio
            // - 1 framescript

            TotalSpriteCount = MemberSpritesCount + 6;

            _logger.LogInformation($"DB| Score root primary: header=(actualSize={actualSize}, {c2},frameCount= {FrameCount}, {c4},spriteSize= {SpriteSize}, {c6}, TotalSpriteCount={TotalSpriteCount})");

            int frameStart = reader.Pos;
            var decoder = new RayKeyframeDeltaDecoder(Annotator);
            var frame10And12found = false;
            for (int fr = 0; fr < FrameCount; fr++)
            {
                var fk = new Dictionary<string, int> { ["frame"] = fr };
                ushort frameDataLen = reader.ReadUint16("frameLen", fk);
                var frameBytes = reader.ReadBytes(frameDataLen - 2); //, "frameBytes", fk);
                var frameData = new ReadStream(frameBytes, frameDataLen - 2, reader.Endianness,
                    annotator: Annotator);
                var rawata = frameData.LogHex(frameData.Size);
                //_logger.LogInformation("FrameData:" + rawata);
                //if (rawata.Contains("3C") || rawata.Contains("00 2E"))
                //{
                //    frame10And12found = true;
                //   // var blobs = TryExtractSingleKeyframe(frameBytes);

                //}
                //TryLogRawKeyframeColor(frameData, fr);

                //    var extracted = TryExtractSingleKeyframe(frameBytes);
                //    foreach (var item in extracted)
                //    {
                //        //var hex = new ReadStream(item.Data, item.Data.Length, Endianness.BigEndian).LogHex(item.Data.Length);
                //    //_logger.LogInformation($"EXTRACTED Keyframe: offset={item.Offset:X4} size={item.Data.Length}\n{hex}");
                //    TryParseMinimalSpriteProperties(frameBytes, fr);
                //}
                //TryLogFixedRawKeyframeSpriteBlock(frameData, fr);



                var items = new List<FrameDeltaItem>();
                while (!frameData.Eof)
                {
                    //var itemLen = frameData.ReadUint16() ;
                    //ushort offset = frameData.ReadUint16();
                    //byte[] data = frameData.ReadBytes(itemLen);
                    //items.Add(new FrameDeltaItem(offset, data));
                    var itemLen = frameData.ReadUint16("deltaLen", fk);
                    ushort offset = frameData.ReadUint16("offset", fk);
                    if (frameData.Size - frameData.Position < 4)
                    {
                        //_logger.LogWarning($"Frame {fr}, channel {offset}: Invalid itemLen={itemLen}, skipping.");
                        break;
                    }

                    byte[] data = frameData.ReadBytes(itemLen, "deltaBytes", new Dictionary<string, int> { ["frame"] = fr, ["offset"] = offset });
                    items.Add(new FrameDeltaItem(offset, data));


                }


                _frameTable.Add(new FrameDelta(items));
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


        #region OK




        public List<RaySprite> ReadAllFrameSprites()
        {
            var allFrames = new List<RaySprite>();
            var idx = 0;
            RaySprite? sprite = null;
            for (int i = 0; i < _frameTable.Count; i++)
            {
                var frame = _frameTable[i];
                var spritesInFrame = new List<RaySprite>();


                foreach (var item in frame.Items)
                {
                    //if (item.Data.Length < 20)
                    //{
                    //    var stream = new ReadStream(item.Data, item.Data.Length, Endianness.BigEndian);
                    //    var test = stream.LogHex(stream.Size);

                    //    continue;
                    //}
                    var channelStream = new ReadStream(item.Data, item.Data.Length, Endianness.BigEndian,
                        annotator: Annotator);
                    var test = channelStream.LogHex(8);
                    //_logger.LogInformation($"Frame {item.Offset}:  {test}");
                    //continue;
                    //byte keyframeTypeByte = channelStream.ReadUint8("keyframeType", new Dictionary<string, int> { ["item.offset"] = item.Offset });
                    //KeyframeType keyframeType = (KeyframeType)keyframeTypeByte;
                    var keyframeType =  KeyframeType.Sprite;
                    //_logger.LogInformation($"Frame {item.Offset}: Keyframe type = {keyframeType}");

                    var spriteNum = (item.Offset / SpriteSize) - 6 ;


                switch (keyframeType)
                    {
                        case KeyframeType.Sprite:
                            if (item.Data.Length < 20)
                            {
                                var stream = new ReadStream(item.Data, item.Data.Length, Endianness.BigEndian);
                                var test1 = stream.LogHex(stream.Size);
                                //_logger.LogInformation("Raw data:" + test1);
                                continue;
                            }
                            sprite = ReadChannelSprite(channelStream, new() { ["frame"] = i, ["channel"] = spriteNum });
                            spritesInFrame.Add(sprite);

                            var descriptor = _FrameDescriptors[idx];
                            sprite.Behaviors = descriptor.Behaviors;
                            sprite.StartFrame = descriptor.StartFrame;
                            sprite.EndFrame = descriptor.EndFrame;
                            sprite.SpriteNumber = descriptor.Channel;
                            sprite.ExtraValues = descriptor.ExtraValues;
                            idx++;
                            break;

                        case KeyframeType.Animation:
                            if (channelStream.Size <= 16)
                            {
                                _logger.LogWarning($"Frame {spriteNum}: Frame data is too small (size={channelStream.Size}), skipping.");
                                continue; // Skip this frame if it's too short to process
                            }
                            var keyFrame = HandleAnimationKeyframe(channelStream, item.Offset);
                            if (sprite != null)
                                sprite.Keyframes.Add(keyFrame);
                            break;

                        default:
                           // _logger.LogWarning($"Frame {spriteNum}: Unknown keyframe type {keyframeType}, skipping.");
                            break;
                    }


                }
                if (spritesInFrame.Count == 0)
                    continue;

                allFrames.AddRange(spritesInFrame);
            }
            AllSprites = allFrames;
            return allFrames;
        }

        [Flags]
        public enum KeyframeFlags
        {
            None = 0,
            TweeningEnabled = 1 << 0,  // Bit 0 for Tweening
            Path = 1 << 1,             // Bit 1 for Path (LocH + LocV)
            Size = 1 << 2,             // Bit 2 for Size (Width + Height)
            Rotation = 1 << 3,         // Bit 3 for Rotation
            Skew = 1 << 4,             // Bit 4 for Skew
            Blend = 1 << 5,            // Bit 5 for Blend
            ForeColor = 1 << 6,        // Bit 6 for ForeColor
            BackColor = 1 << 7         // Bit 7 for BackColor
        }

        public RayKeyFrame HandleAnimationKeyframe(ReadStream stream, int frameIndex)
        {
            RayKeyFrame keyframe = new RayKeyFrame();

            // Read the flag byte and convert it to the KeyframeFlags enum
            byte flagByte = stream.ReadUint8("FlagByte", new Dictionary<string, int> { ["frame"] = frameIndex });
            KeyframeFlags flags = (KeyframeFlags)flagByte;

            // Parse properties based on flag bits
            if ((flags & KeyframeFlags.Path) != 0) // Path = LocH + LocV
            {
                keyframe.LocH = stream.ReadInt16("LocH", new Dictionary<string, int> { ["frame"] = frameIndex });
                keyframe.LocV = stream.ReadInt16("LocV", new Dictionary<string, int> { ["frame"] = frameIndex });
            }

            if ((flags & KeyframeFlags.Size) != 0)
            {
                keyframe.Width = stream.ReadUint16("Width", new Dictionary<string, int> { ["frame"] = frameIndex }) / 100f;
                keyframe.Height = stream.ReadUint16("Height", new Dictionary<string, int> { ["frame"] = frameIndex }) / 100f;
            }

            if ((flags & KeyframeFlags.Rotation) != 0)
                keyframe.Rotation = stream.ReadFloat("Rotation", new Dictionary<string, int> { ["frame"] = frameIndex });

            if ((flags & KeyframeFlags.Skew) != 0)
                keyframe.Skew = stream.ReadFloat("Skew", new Dictionary<string, int> { ["frame"] = frameIndex });

            if ((flags & KeyframeFlags.Blend) != 0)
            {
                byte blendByte = stream.ReadUint8("Blend", new Dictionary<string, int> { ["frame"] = frameIndex });
                keyframe.Blend = (int)((blendByte / 255f) * 100f);
            }

            if ((flags & KeyframeFlags.ForeColor) != 0)
                keyframe.ForeColor = stream.ReadUint8("ForeColor", new Dictionary<string, int> { ["frame"] = frameIndex });

            if ((flags & KeyframeFlags.BackColor) != 0)
                keyframe.BackColor = stream.ReadUint8("BackColor", new Dictionary<string, int> { ["frame"] = frameIndex });

            // Log the parsed keyframe
            _logger.LogDebug($"Parsed keyframe {frameIndex}: Flags={flags}; " +
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


        


        // First byte type:
        //If keyframeType indicates a sprite, the frame data might include properties like position(LocH, LocV), size, rotation, skew, color, etc.

        // If it's an animation keyframe, it could contain information about the animation state (e.g., blending modes, speed).
        private RaySprite ReadChannelSprite(ReadStream stream, Dictionary<string, int>? keys = null)
        {
            var sprite = new RaySprite();
            // info from scummVM:
            //uint8 keyframeType = stream.readUint8(); // 1 byte for the type  // always 16 it seems
            //uint16 keyframeSize = stream.readUint16(); // 2 bytes for size -> // 0 ,8, 36, 1 , 2 -> this seems strange because we read the flags

            //var always16 = stream.ReadUint8(); // always 16 it seems
            //var val2 = stream.ReadUint8(); // 0 ,8, 36, 1 , 2
            var val3 = stream.ReadUint8("flags", keys); // flags, unused
            byte inkByte = stream.ReadUint8("ink", keys);
            sprite.Ink = inkByte & 0x7F;
            sprite.ForeColor = stream.ReadUint8("foreColor", keys);
            sprite.BackColor = stream.ReadUint8("backColor", keys);
            sprite.DisplayMember = (int)stream.ReadUint32("member", keys);
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
                sprite.Rotation = stream.ReadUint32("rotation", keys) / 100f;
                sprite.Skew = stream.ReadUint32("skew", keys) / 100f;
            }
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


        #endregion


        #endregion






        public void ReadFrameData5()
        {
            var reader = new ReadStream(_FrameDataBufferView, Endianness.BigEndian, annotator: Annotator);
            int actualSize = reader.ReadInt32("actualSize");
            int c2 = reader.ReadInt32("constMinus3"); // usually -3
            FrameCount = reader.ReadInt32("frameCount");
            short c4 = reader.ReadInt16("const4");
            SpriteSize = reader.ReadInt16("spriteSize");
            short c6 = reader.ReadInt16("const6");
            MemberSpritesCount = reader.ReadInt16("memberSpriteCount");
            // 6 sprites:
            // - 1 puppettempo
            // - 1 pallete
            // - 1 Transition
            // - 2 audio
            // - 1 framescript

            TotalSpriteCount = MemberSpritesCount + 6;

            _logger.LogInformation($"Frame Count: {FrameCount}, Sprite Size: {SpriteSize}");

            int frameStart = reader.Pos;
            for (int fr = 0; fr < FrameCount; fr++)
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
                items.Add(new FrameDeltaItem(offset, data));
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
