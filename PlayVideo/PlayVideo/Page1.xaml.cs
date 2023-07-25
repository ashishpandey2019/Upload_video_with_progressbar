using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using System.Xml;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace PlayVideo
{
    public partial class Page1 : ContentPage
    {
        private string videoUrl = "http://commondatastorage.googleapis.com/gtv-videos-bucket/sample/ElephantsDream.mp4";
        public Page1()
        {
            InitializeComponent();

         

        }


        private async void UploadButton_Clicked(object sender, EventArgs e)
        {
            try
            {
                var file = await PickAndReturnFileAsync();

                if (file != null)
                {
                   // UploadButton.IsEnabled = false;
                    ProgressBar.IsVisible = true;
                    ProgressBar.Progress = 0;

                    var httpClient = new HttpClient();
                    var content = new MultipartFormDataContent();

                    using (var stream = await file.OpenReadAsync())
                    {
                        var progressStream = new ProgressStream(stream);
                        progressStream.ProgressChanged += (s, args) =>
                        {
                            double progressPercentage = (double)args.BytesRead / args.TotalLength;
                            ProgressBar.Progress = progressPercentage;
                        };

                        content.Add(new StreamContent(progressStream), "file", file.FileName);
                        var response = await httpClient.PostAsync("https://ciialphaapi.chipsoftindia.com/api/CIIWall/VideoUpload", content).ConfigureAwait(false);

                        if (response.IsSuccessStatusCode)
                        {
                            var tempResponseContent = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                            await Device.InvokeOnMainThreadAsync(async () =>
                            {
                                await DisplayAlert("Success", $"File  uploaded successfully", "OK");
                            });
                        }
                        else
                        {
                           await Device.InvokeOnMainThreadAsync(async () =>
                            {
                                await DisplayAlert("Error", "File uploaded failed.", "OK");
                            });
                        }
                    }

                    ProgressBar.IsVisible = false;
                    UploadButton.IsEnabled = true;
                }



            }
            catch (Exception ee)
            {
                Console.WriteLine(ee);
            }
        }

        public class ProgressStream : Stream
        {
            private readonly Stream _stream;
            private long _bytesRead;
            private long _totalLength;

            public event EventHandler<ProgressChangedEventArgs> ProgressChanged;

            public ProgressStream(Stream stream)
            {
                _stream = stream;
                _totalLength = stream.Length;
            }

            public override bool CanRead => _stream.CanRead;
            public override bool CanSeek => _stream.CanSeek;
            public override bool CanWrite => _stream.CanWrite;
            public override long Length => _stream.Length;
            public override long Position { get => _stream.Position; set => _stream.Position = value; }

            public override void Flush()
            {
                _stream.Flush();
            }

            public override int Read(byte[] buffer, int offset, int count)
            {
                int bytesRead = _stream.Read(buffer, offset, count);
                _bytesRead += bytesRead;
                OnProgressChanged(_bytesRead, _totalLength);
                return bytesRead;
            }

            public override async Task<int> ReadAsync(byte[] buffer, int offset, int count, System.Threading.CancellationToken cancellationToken)
            {
                int bytesRead = await _stream.ReadAsync(buffer, offset, count, cancellationToken);
                _bytesRead += bytesRead;
                OnProgressChanged(_bytesRead, _totalLength);
                return bytesRead;
            }

            public override long Seek(long offset, SeekOrigin origin)
            {
                return _stream.Seek(offset, origin);
            }

            public override void SetLength(long value)
            {
                _stream.SetLength(value);
            }

            public override void Write(byte[] buffer, int offset, int count)
            {
                _stream.Write(buffer, offset, count);
            }

            public override async Task WriteAsync(byte[] buffer, int offset, int count, System.Threading.CancellationToken cancellationToken)
            {
                await _stream.WriteAsync(buffer, offset, count, cancellationToken);
            }

            protected virtual void OnProgressChanged(long bytesRead, long totalLength)
            {
                var eventArgs = new ProgressChangedEventArgs(bytesRead, totalLength);
                ProgressChanged?.Invoke(this, eventArgs);
            }
        }

        public class ProgressChangedEventArgs : EventArgs
        {
            public long BytesRead { get; }
            public long TotalLength { get; }

            public ProgressChangedEventArgs(long bytesRead, long totalLength)
            {
                BytesRead = bytesRead;
                TotalLength = totalLength;
            }
        }


        private async Task<FileResult> PickAndReturnFileAsync()
        {
            var result = await FilePicker.PickAsync().ConfigureAwait(false);
            if (result != null)
                return result;

            return null;
        }

        private void Button_Clicked(object sender, EventArgs e)
        {
            
        }
    }

}
