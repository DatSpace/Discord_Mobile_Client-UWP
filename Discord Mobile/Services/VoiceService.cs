using Discord.WebSocket;
using System;
using System.Threading.Tasks;
using Windows.Media.Playback;

namespace Discord_Mobile.Services
{
    class VoiceService
    {
        //private static IAudioClient AudioClient;
        private static MediaPlayer VoicePlayer;
        //MediaCapture Capture;
        //InMemoryRandomAccessStream tempBuffer;
        //MediaCaptureInitializationSettings settings = new MediaCaptureInitializationSettings() { StreamingCaptureMode = StreamingCaptureMode.Audio };

        public async static Task Initialize(SocketVoiceChannel voiceChannel)
        {
            try
            {
                //AudioClient = await voiceChannel.ConnectAsync();
                //AudioClient.StreamCreated += AudioClient_StreamCreated;
            }
            catch (Exception e)
            {
                string temp = e.ToString();
            }
        }

        //private static Task AudioClient_StreamCreated(ulong arg1, AudioInStream arg2)
        //{
        //    MemoryStream tempStream = new MemoryStream();
        //    arg2.CopyToAsync(tempStream);
        //    MediaSource stream = MediaSource.CreateFromStream(tempStream.AsRandomAccessStream(), "");
        //    VoicePlayer.Source = stream;
        //    return Task.CompletedTask;
        //}

        //private async Task<bool> SetRecording()
        //{
        //    if (tempBuffer != null)
        //    {
        //        tempBuffer.Dispose();
        //    }

        //    tempBuffer = new InMemoryRandomAccessStream();

        //    if (Capture != null)
        //    {
        //        Capture.Dispose();
        //    }


        //    Capture = new MediaCapture();

        //    await Capture.InitializeAsync(settings);

        //    Capture.RecordLimitationExceeded += (MediaCapture sender) =>
        //    {
        //        //Stop 
        //        //   await capture.StopRecordAsync();
        //        throw new Exception("Record Limitation Exceeded ");
        //    };

        //    return true;
        //}

        //private async void Record_Click()
        //{
        //    await SetRecording();
        //    await Capture.StartRecordToStreamAsync(MediaEncodingProfile.CreateMp3(AudioEncodingQuality.Auto), tempBuffer);
        //}

        //public async Task<InMemoryRandomAccessStream> StopRecordingAndSend()
        //{
        //    await Capture.StopRecordAsync();
        //    return tempBuffer;
        //}

        //public void PlayVoice()
        //{
        //    var stream = MediaSource.CreateFromStream(tempBuffer, "audio/mp3");
        //    VoicePlayer.Source = stream;
        //}

    }
}
