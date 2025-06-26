using Director.IO;
using Director.Primitives;

namespace Director
{
    /// <summary>
    /// Represents the Score timeline of a Director movie (frames, actions, sprites).
    /// </summary>
    public class Score
    {
        /// <summary>Current frame rate of the movie (frames per second).</summary>
        public ushort CurrentFrameRate { get; set; }

        private ushort _framesVersion;
        private ushort _numChannels;
        private ushort _numChannelsDisplayed;
        private uint _numFrames;

        /// <summary>
        /// Loads the frame data from a stream.
        /// </summary>
        /// <param name="stream">The input stream containing VWSC (score) data.</param>
        /// <param name="version">The Director file format version.</param>
        public void LoadFrames(SeekableReadStreamEndian stream, int version)
        {
            // Basic header parsing based on the ScummVM implementation.
            uint framesSize = stream.ReadUInt32();
            if (version >= FileVersion.Ver400 && version < FileVersion.Ver600)
            {
                uint frame1Offset = stream.ReadUInt32();
                uint numOfFrames = stream.ReadUInt32();
                _framesVersion = stream.ReadUInt16();
                ushort spriteRecordSize = stream.ReadUInt16();
                _numChannels = stream.ReadUInt16();

                if (_framesVersion > 13)
                    _numChannelsDisplayed = stream.ReadUInt16();
                else
                {
                    _numChannelsDisplayed = (_framesVersion <= 7) ? (ushort)48 : (ushort)120;
                    stream.ReadUInt16();
                }

                _numFrames = numOfFrames;
                for (int ch = 0; ch < _numChannels; ch++)
                {
                    ReadChannelKeyframes(stream, ch);
                }

            }
            else
            {
                // Older or newer versions not yet supported
            }
        }

        /// <summary>
        /// Loads scripted actions associated with the score.
        /// </summary>
        /// <param name="stream">The input stream containing VWAC (action list) data.</param>
        public void LoadActions(SeekableReadStreamEndian stream)
        {
            // Actions consist of script IDs mapped to frame numbers.
            // The exact data is complex; for now just consume the stream.
            while (!stream.EOF)
                stream.ReadByte();
        }
        private void ReadChannelKeyframes(SeekableReadStreamEndian stream, int channelIndex)
        {
            ushort numKeyframes = stream.ReadUInt16();
            for (int i = 0; i < numKeyframes; i++)
            {
                uint offset = stream.ReadUInt32();
                ushort frameIndex = stream.ReadUInt16();
                // Store (frameIndex, offset) in your channel’s keyframe list.
            }
        }

    }
}
