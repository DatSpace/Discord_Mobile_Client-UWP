using System;
using System.Threading.Tasks;
using Windows.Media.Capture;
using Windows.Media.MediaProperties;
using Windows.Storage.Streams;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace AudioTesting
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        MediaCapture capture;
        InMemoryRandomAccessStream buffer;
        MediaCaptureInitializationSettings settings = new MediaCaptureInitializationSettings() { StreamingCaptureMode = StreamingCaptureMode.Audio };

        //string filename;
        //string audioFile = ".MP3";

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


            capture = new MediaCapture();

            await capture.InitializeAsync(settings);

            capture.RecordLimitationExceeded += (MediaCapture sender) =>
            {
                //Stop 
                //   await capture.StopRecordAsync();
                throw new Exception("Record Limitation Exceeded ");
            };

            return true;
        }

        public MainPage()
        {
            InitializeComponent();
            //Record.AddHandler(HoldingEvent, new HoldingEventHandler(Record_Click), true);
            //Record.AddHandler(, new PointerEventHandler(Stop_Click), true);
        }

        private async void Record_Click(object sender, RoutedEventArgs e)
        {
            await RecordProcess();
            await capture.StartRecordToStreamAsync(MediaEncodingProfile.CreateMp3(AudioEncodingQuality.Auto), buffer);
        }

        private async void Stop_Click(object sender, RoutedEventArgs e)
        {
            await capture.StopRecordAsync();
        }

        private void Play_Click(object sender, RoutedEventArgs e)
        {
            MediaPlayer.SetSource(buffer, "");
        }
    }
}
