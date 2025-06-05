using static System.Net.Mime.MediaTypeNames;
using System.Net.NetworkInformation;
using System.Numerics;
using System;

namespace LingoEngine.Sounds
{
    /// <summary>
    /// Represents an individual sound channel found within the Sound object
    /// </summary>
    public interface ILingoSoundChannel
    {
        /// <summary>
        /// Sound Channel property; determines the number of channels in the currently playing or paused sound in a given sound channel.Read-only.
        /// This property is useful for determining whether a sound is in monaural or in stereo.
        /// </summary>
        int ChannelCount { get; }
        /// <summary>
        /// Sound Channel property; indicates the start time of the currently playing or paused sound as set when the sound was queued.Read-only.
        /// This property cannot be set after the sound has been queued.If no value was supplied when the sound was queued, this property returns 0.
        /// </summary>
        float StartTime { get; }
        /// <summary>
        /// Sound Channel property; gives the time, in milliseconds, that the current sound member in a sound channel has been playing.Read-only.
        /// The elapsed time starts at 0 when the sound begins playing and increases as the sound plays, regardless of any looping, setting of the currentTime or other manipulation.Use the currentTime to test for the current absolute time within the sound.
        /// The value of this property is a floating-point number, allowing for measurement of sound playback to fractional milliseconds.
        /// </summary>
        float ElapsedTime { get; }
        /// <summary>
        /// Sound Channel property; specifies the end time of the currently playing, paused, or queued sound.Read/write.
        /// The end time is the time within the sound member when it will stop playing.It’s a floating-point value, allowing for measurement and control of sound playback to fractions of milliseconds.The default value is the normal end of the sound.
        /// This property may be set to a value other than the normal end of the sound only when passed as a parameter with the queue() or setPlayList() methods.
        /// </summary>
        float EndTime { get; }
        /// <summary>
        /// Sound Channel property; specifies the total number of times the current sound in a sound channel is set to loop.Read-only.
        /// The default value of this property is 1 for sounds that are simply queued with no internal loop.
        /// You can loop a portion of a sound by passing the parameters loopStartTime, loopEndTime, and loopCount with a queue() or setPlayList() method.These are the only methods for setting this property.
        /// If loopCount is set to 0, the loop will repeat forever.If the sound cast member’s loop property is set to TRUE, loopCount will return 0.
        /// </summary>
        int LoopCount { get; }
        /// <summary>
        /// Sound Channel property; specifies the end time, in milliseconds, of the loop set in the current sound playing in a sound channel.Read-only.
        /// The value of this property is a floating-point number, allowing you to measure and control sound playback to fractions of a millisecond.
        /// This property can only be set when passed as a property in a queue() or setPlaylist() command.
        /// </summary>
        float LoopEndTime { get; }
        /// <summary>
        /// Sound Channel property; specifies the start time, in milliseconds, of the loop for the current sound being played by a sound channel.Read-only.
        /// Its value is a floating-point number, allowing you to measure and control sound playback to fractions of a millisecond.The default is the startTime of the sound if no loop has been defined.
        /// This property can only be set when passed as a property in a queue() or setPlaylist() methods.
        /// </summary>
        float LoopStartTime { get; }
        /// <summary>
        /// Gets or sets the current time within the sound (in milliseconds).
        /// </summary>
        float CurrentTime { get; set; }


        float PreloadTime { get; set; }
        LingoMemberSound? Member { get; }
        int Number { get; }
        /// <summary>
        /// Sound Channel property; indicates the left/right balance of the sound playing in a sound channel.
        /// The range of values is from -100 to 100. -100 indicates only the left channel is heard. 100 indicate only the right channel is being heard.A value of 0 indicates even left/right balance, causing the sound source to appear to be centered.For mono sounds, pan affects which speaker (left or right) the sound plays through.
        /// You can change the pan of a sound object at any time, but if the sound channel is currently performing a fade, the new pan setting doesn’t take effect until the fade is complete.
        /// </summary>
        int Pan { get; set; }
        int SampleCount { get; }
        int SampleRate { get; }

        LingoSoundChannelStatus Status { get; }
        int Volume { get; set; }
        /// <summary>
        /// Sound Channel method; causes the currently looping sound in channel soundChannelObjRef to stop looping and play through to its endTime.
        /// If there is no current loop, this method has no effect
        /// </summary>
        void BreakLoop();
        /// <summary>
        /// Sound Channel method; immediately sets the volume of a sound channel to zero and then brings it back to the current volume over a given number of milliseconds.
        /// </summary> 
        /// <param name="milliseconds">Optional. An integer that specifies the number of milliseconds over which the volume is increased back to its original value.The default is 1000 milliseconds (1 second) if no value is given.</param>  
        void FadeIn(int milliseconds = 1000);
        /// <summary>
        /// Sound Channel method; gradually reduces the volume of a sound channel to zero over a given number of milliseconds.
        /// The current pan setting is retained for the entire fade
        /// </summary>
        /// <param name="milliseconds"> Optional. An integer that specifies the number of milliseconds over which the volume is reduced to zero.The default is 1000 milliseconds(1 second) if no value is given.</param>

        void FadeOut(int milliseconds = 1000);
        /// <summary>
        /// Sound Channel method; gradually changes the volume of a sound channel to a specified volume over a given number of milliseconds.
        /// The current pan setting is retained for the entire fade.
        /// </summary>
        /// <param name="volume"> Required. An integer that specifies the volume level to change to. The range of values for intVolume volume is 0 to 255.</param>
        /// <param name="milliseconds"> Optional. An integer that specifies the number of milliseconds over which the volume is reduced to zero.The default is 1000 milliseconds(1 second) if no value is given.</param>
        void FadeTo(int volume, int milliseconds = 1000);
        /// <summary>
        /// Sound Channel method; returns a copy of the list of queued sounds for a sound channel.
        /// The returned list does not include the currently playing sound, nor may it be edited directly.You must use setPlayList().
        /// </summary>
        LingoPlayListSound[] GetPlayList();
        /// <summary>
        /// Sound Channel method; sets or resets the playlist of a sound channel. 
        /// This method is useful for queueing several sounds at once
        /// </summary>
        void SetPlayList(IEnumerable<LingoPlayListSound> playList);
        void Pause();
        /// <summary>
        /// Sound Channel method; begins playing any sounds queued in a sound channel, or queues and begins playing a given cast member. 
        /// Sound cast members take some time to load into RAM before they can begin playback. It’s recommended that you queue sounds 
        /// with queue() before you want to begin playing them and then use the first form of this method.The second two forms do not take advantage of the preloading accomplished with the queue() command.
        /// </summary>
        void Play(LingoMemberSound? member = null, float startTime = -1, float endTime = -1, int loopCount = -1, float loopStartTime = -1, float loopEndTime = -1, float preloadTime = -1);
        /// <summary>
        /// Sound Channel method; plays the AIFF, SWA, AU, or WAV sound in a sound channel.
        /// For the sound to be played properly, the correct MIX Xtra must be available to the movie, usually in the Xtras folder of the application.
        /// When the sound file is in a different folder than the movie, stringFilePath must specify the full path to the file.
        /// To play sounds obtained from a URL, it’s usually a good idea to use downloadNetThing() or preloadNetThing() to download the file to a local disk first. This approach can minimize problems that may occur while the file is downloading.
        /// The playFile() method streams files from disk rather than playing them from RAM. As a result, using playFile() when playing digital video or when loading cast members into memory can cause conflicts when the computer tries to read the disk in two places at once.

        /// </summary>
        void PlayFile(string stringFilePath);
        /// <summary>
        /// Sound Channel method; immediately interrupts playback of the current sound playing in a sound channel and begins playing the next queued sound. 
        /// If no more sounds are queued in the given channel, the sound simply stops playing.
        /// </summary>
        void PlayNext();
        /// <summary>
        /// Sound Channel method; adds a sound cast member to the queue of a sound channel.
        /// Once a sound has been queued, it can be played immediately with the play() method.This is because Director preloads a certain amount of each sound that is queued, preventing any delay between the play() method and the start of playback.The default amount of sound that is preloaded is 1500 milliseconds.This parameter can be modified by passing a property list containing one or more parameters with the queue() method. These parameters can also be passed with the setPlayList() method.
        /// </summary>
        void Queue(LingoMemberSound member, float startTime = -1, float endTime = -1, int loopCount = -1, float loopStartTime = -1, float loopEndTime = -1, float preloadTime = -1);
        void Rewind();

        void Stop();
        /// <summary>
        /// Sound Channel method; determines whether a sound is playing (TRUE) or not playing (FALSE) in a sound channel.
        /// Make sure that the playhead has moved before using isBusy() to check the sound channel.If this function continues to return FALSE after a sound should be playing, add the updateStage() method to start playing the sound before the playhead moves again. This method works for those sound channels occupied by actual audio cast members.
        /// Consider using the status property of a sound channel instead of isBusy(). The status property can be more accurate under many circumstances.
        /// </summary>
        bool IsBusy();
    }

    public class LingoSoundChannel : ILingoSoundChannel
    {
        private Queue<LingoPlayListSound> _playlist = new();
        private int _currentLoop;
        private readonly ILingoFrameworkSoundChannel _frameworkSoundChannel;
        public T FrameworkObj<T>() where T : ILingoFrameworkSoundChannel => (T)_frameworkSoundChannel;

        /// <inheritdoc/>
        public int Number { get; private set; }
        /// <inheritdoc/>
        public int Volume { get => _frameworkSoundChannel.Volume; set => _frameworkSoundChannel.Volume = value; }
        /// <inheritdoc/>
        public LingoSoundChannelStatus Status { get; private set; }
        /// <inheritdoc/>
        public int ChannelCount => _frameworkSoundChannel.ChannelCount;
        /// <inheritdoc/>
        public int SampleCount => _frameworkSoundChannel.SampleCount;
        /// <inheritdoc/>
        public int SampleRate => _frameworkSoundChannel.SampleRate;
        /// <inheritdoc/>
        public LingoMemberSound? Member { get; private set; }
        /// <inheritdoc/>
        public int Pan { get => _frameworkSoundChannel.Pan; set => _frameworkSoundChannel.Pan = value; }
        /// <inheritdoc/>
        public float ElapsedTime => _frameworkSoundChannel.ElapsedTime;
        /// <inheritdoc/>
        public float StartTime { get => _frameworkSoundChannel.StartTime; set => _frameworkSoundChannel.StartTime = value; }
        /// <inheritdoc/>
        public float EndTime { get => _frameworkSoundChannel.EndTime; set => _frameworkSoundChannel.EndTime = value; }
        /// <inheritdoc/>
        public int LoopCount { get; private set; } = 1;
        /// <inheritdoc/>
        public float LoopStartTime { get; private set; }

        /// <inheritdoc/>
        public float LoopEndTime { get; private set; }
        /// <inheritdoc/>
        public float PreloadTime { get; set; }
        public float CurrentTime { get => _frameworkSoundChannel.CurrentTime; set => _frameworkSoundChannel.CurrentTime = value; }

        public LingoSoundChannel(ILingoFrameworkSoundChannel frameworkSoundChannel, int number)
        {
            _frameworkSoundChannel = frameworkSoundChannel;
            Number = number;
        }
        /// <inheritdoc/>
        public void BreakLoop()
        {

        }
        /// <inheritdoc/>
        public void FadeIn(int milliseconds = 1000) { }
        /// <inheritdoc/>
        public void FadeOut(int milliseconds = 1000) { }
        /// <inheritdoc/>
        public void FadeTo(int volume, int milliseconds = 1000)
        {

        }

        /// <inheritdoc/>
        public void PlayFile(string stringFilePath)
        {
            // A string that specifies the name of the file to play. When the sound file is in a different folder than the currently playing movie, stringFilePath must also specify the full path to the file.
            _frameworkSoundChannel.PlayFile(stringFilePath);
        }
        public void PlayNext()
        {
            if (Status == LingoSoundChannelStatus.Playing)
                Stop();
            if (_playlist.Count == 0) return;
            var song = _playlist.Dequeue();
            ApplyPlaylistSound(song);
            if (Member == null) return;
            PlayNow(Member);
        }

        private void ApplyPlaylistSound(LingoPlayListSound song)
        {
            StartTime = song.StartTime;
            EndTime = song.EndTime;
            LoopCount = song.LoopCount;
            LoopStartTime = song.LoopStartTime;
            LoopEndTime = song.LoopEndTime;
            PreloadTime = song.PreloadTime;
            if (song.Volume.HasValue) Volume = song.Volume.Value;
            if (song.Pan.HasValue) Pan = song.Pan.Value;
            Member = song.Member;
        }

        /// <inheritdoc/>
        public void Play(LingoMemberSound? member = null, float startTime = -1, float endTime = -1, int loopCount = -1, float loopStartTime = -1, float loopEndTime = -1, float preloadTime = -1)
        {
            if (member != null) Member = member;
            if (Member == null)
            {
                if (_playlist.Count == 0) return;
                ApplyPlaylistSound(_playlist.Dequeue());
                if (Member == null) return;
            }
            if (startTime > -1) StartTime = startTime;
            if (endTime > -1) EndTime = endTime;
            if (loopCount > -1) LoopCount = loopCount;
            if (loopStartTime > -1) LoopStartTime = loopStartTime;
            if (loopEndTime > -1) LoopEndTime = loopEndTime;
            if (preloadTime > -1) PreloadTime = preloadTime;
            PlayNow(Member);
        }
        private void PlayNow(LingoMemberSound member)
        {
            Status = LingoSoundChannelStatus.Playing;
            _frameworkSoundChannel.PlayNow(member);
        }
        /// <inheritdoc/>
        public void Pause()
        {
            Status = LingoSoundChannelStatus.Paused;
            _frameworkSoundChannel.Pause();
        }
        /// <inheritdoc/>
        public void Stop()
        {
            Status = LingoSoundChannelStatus.Idle;
            _frameworkSoundChannel.Stop();
        }
        /// <inheritdoc/>
        public void Queue(LingoMemberSound member, float startTime = -1, float endTime = -1, int loopCount = -1, float loopStartTime = -1, float loopEndTime = -1, float preloadTime = -1)
        {
            var sound = new LingoPlayListSound(member);
            if (startTime > -1) sound.StartTime = startTime;
            if (endTime > -1) sound.EndTime = endTime;
            if (loopCount > -1) sound.LoopCount = loopCount;
            if (loopStartTime > -1) sound.LoopStartTime = loopStartTime;
            if (loopEndTime > -1) sound.LoopEndTime = loopEndTime;
            if (preloadTime > -1) sound.PreloadTime = preloadTime;
            _playlist.Enqueue(sound);
        }
       
        /// <inheritdoc/>
        public void Rewind() { _frameworkSoundChannel.Rewind(); }
        /// <inheritdoc/>
        public LingoPlayListSound[] GetPlayList() => _playlist.ToArray();
        /// <inheritdoc/>
        public void SetPlayList(IEnumerable<LingoPlayListSound> playList)
        {
            _playlist = new Queue<LingoPlayListSound>(playList);
        }
        /// <inheritdoc/>
        public bool IsBusy() => Status == LingoSoundChannelStatus.Playing;

        public void SoundFinished()
        {
            if (LoopCount > 1)
            {
                _currentLoop++;
                
                if (_currentLoop >= LoopCount)
                {
                    Status = LingoSoundChannelStatus.Idle;
                    _currentLoop = 0;
                    return;
                }
                _frameworkSoundChannel.Repeat();
                Status = LingoSoundChannelStatus.Playing;
            }
            else
            {
                Status = LingoSoundChannelStatus.Idle;
            }
            
        }
    }

}
