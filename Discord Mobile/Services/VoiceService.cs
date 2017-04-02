using System;
using System.Threading.Tasks;
using Windows.Media.Capture;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Core;

namespace Discord_Mobile.Services
{
    class RecordAudio
    {
            MediaCapture capture;
            InMemoryRandomAccessStream buffer;
            bool record;
            string filename;
            string audioFile = ".MP3";

            private async Task<bool> RecordProcess()
            {
                if (buffer != null)
                {
                    buffer.Dispose();
                }

                buffer = new InMemoryRandomAccessStream();

                if (capture != null)
                {
                    capture.Dispose();
                }

                MediaCaptureInitializationSettings settings = new MediaCaptureInitializationSettings() { StreamingCaptureMode = StreamingCaptureMode.Audio };

                    capture = new MediaCapture();
                    await capture.InitializeAsync(settings);

                    capture.RecordLimitationExceeded += (MediaCapture sender) =>
                    {
                        //Stop 
                        //   await capture.StopRecordAsync(); 
                        record = false;
                        throw new Exception("Record Limitation Exceeded ");
                    };

                return true;
            }
            public async Task PlayRecordedAudio(CoreDispatcher UiDispatcher)
            {
                IRandomAccessStream audio = buffer.CloneStream();

                if (audio == null)
                    throw new ArgumentNullException("buffer");
                StorageFolder storageFolder = Windows.ApplicationModel.Package.Current.InstalledLocation;
                if (!string.IsNullOrEmpty(filename))
                {
                    StorageFile original = await storageFolder.GetFileAsync(filename);
                    await original.DeleteAsync();
                }
                await UiDispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
                {
                    StorageFile storageFile = await storageFolder.CreateFileAsync(audioFile, CreationCollisionOption.GenerateUniqueName);
                    filename = storageFile.Name;
                    using (IRandomAccessStream fileStream = await storageFile.OpenAsync(FileAccessMode.ReadWrite))
                    {
                        await RandomAccessStream.CopyAndCloseAsync(audio.GetInputStreamAt(0), fileStream.GetOutputStreamAt(0));
                        await audio.FlushAsync();
                        audio.Dispose();
                    }
                    IRandomAccessStream stream = await storageFile.OpenAsync(FileAccessMode.Read);
                    //playback.SetSource(stream, storageFile.FileType);
                });
            }
        
        //await capture.StartRecordToStreamAsync(MediaEncodingProfile.CreateMp3(AudioEncodingQuality.Auto), buffer);

    }
}
