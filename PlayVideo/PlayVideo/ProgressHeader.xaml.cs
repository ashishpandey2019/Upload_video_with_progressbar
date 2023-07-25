using System;
using System.Globalization;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.PlatformConfiguration.TizenSpecific;
using Xamarin.Forms.Xaml;

namespace PlayVideo
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ProgressHeader : ContentPage
    {

        private long totalBytes;
        private HttpClient httpClient;
        private Progress<long> progress;
        private ProgressDelegatingHandler progressHandler;

        public ProgressHeader()
        {
            InitializeComponent();


            progress = new Progress<long>(bytesUploaded =>
            {
                Device.BeginInvokeOnMainThread(() =>
                {
                    double progressValue = (double)bytesUploaded / totalBytes;
                    progressBar.Progress = progressValue;
                });
            });
        }

        private async void OnUploadClicked(object sender, EventArgs e)
        {
            try
            {
                var url = "https://ciialphaapi.chipsoftindia.com/api/CIIWall/VideoUpload";
                //// Create HttpClient and set up the request
                var file = await PickAndReturnFileAsync();

                if (file is null)
                {
                    return;
                }

                //using (var httpClient = new HttpClient(new ProgressDelegatingHandler(progress)))
                //{
                //    var fileContent = new StreamContent(File.OpenRead(file.FullPath));
                //    fileContent.Headers.Add("Content-Type", "application/octet-stream");

                //    var request = new HttpRequestMessage(HttpMethod.Post, "https://ciialphaapi.chipsoftindia.com/api/CIIWall/VideoUpload")
                //    {
                //        Content = fileContent
                //    };

                //    // Get total content length
                //    var response = await httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, CancellationToken.None);
                //    totalBytes = response.Content.Headers.ContentLength ?? -1;

                //    // Process the response as needed
                //}

                UploadFile(file.FullPath, url);
            }
            catch (Exception ex)
            {
                // Handle the exception
            }
        }

        public  void UploadFile(string filePath, string url)
        {
            using (FileStream fileStream = File.Open(filePath, FileMode.Open))
            {
                long totalBytes = fileStream.Length;

                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
                request.Method = "POST";

                // Set the Content-Type header to multipart/form-data
                request.ContentType = "multipart/form-data; boundary=---------------------------" + DateTime.Now.Ticks.ToString("x");

                using (Stream requestStream = request.GetRequestStream())
                {
                    // Write the file data as form data
                    WriteFileData(requestStream, fileStream, totalBytes);

                    // Write the end boundary for the form data
                    WriteEndBoundary(requestStream);
                }

                using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                {
                    // Handle the response as needed
                }
            }
        }

        private void WriteFileData(Stream requestStream, FileStream fileStream, long totalBytes)
        {
            byte[] boundaryBytes = Encoding.ASCII.GetBytes("-----------------------------" + DateTime.Now.Ticks.ToString("x"));

            // Write the start boundary for the file data
            requestStream.Write(boundaryBytes, 0, boundaryBytes.Length);
            requestStream.Write(Encoding.ASCII.GetBytes("\r\n"), 0, 2);

            // Write the Content-Disposition header for the file data
            string header = $"Content-Disposition: form-data; name=\"file\"; filename=\"{Path.GetFileName(fileStream.Name)}\"\r\n";
            requestStream.Write(Encoding.ASCII.GetBytes(header), 0, header.Length);

            // Write the Content-Type header for the file data
            string contentType = "Content-Type: application/octet-stream\r\n";
            requestStream.Write(Encoding.ASCII.GetBytes(contentType), 0, contentType.Length);

            // Write the file data
            byte[] buffer = new byte[4096];
            int bytesRead = 0;
            long bytesSent = 0;

            while ((bytesRead = fileStream.Read(buffer, 0, buffer.Length)) > 0)
            {
                requestStream.Write(buffer, 0, bytesRead);
                bytesSent += bytesRead;

                int percentage = (int)((bytesSent * 100) / totalBytes);

                progressBar.Progress = percentage;

                Console.WriteLine($"Upload Progress: {percentage}%");
            }

            requestStream.Write(Encoding.ASCII.GetBytes("\r\n"), 0, 2);
        }

        private static void WriteEndBoundary(Stream requestStream)
        {
            byte[] boundaryBytes = Encoding.ASCII.GetBytes("-----------------------------" + DateTime.Now.Ticks.ToString("x") + "--");
            requestStream.Write(boundaryBytes, 0, boundaryBytes.Length);
            requestStream.Write(Encoding.ASCII.GetBytes("\r\n"), 0, 2);
        }

        private async Task<FileResult> PickAndReturnFileAsync()
        {
            var result = await FilePicker.PickAsync().ConfigureAwait(false);
            if (result != null)
                return result;

            return null;
        }

        private async void playVideo(object sender, EventArgs e)
        {
            var url = "http://commondatastorage.googleapis.com/gtv-videos-bucket/sample/BigBuckBunny.mp4";

            await Navigation.PushAsync(new WebviewPage(url));
        }
    }

    class ProgressDelegatingHandler : DelegatingHandler
    {
        private readonly IProgress<long> _progress;

        public ProgressDelegatingHandler(IProgress<long> progress)
        {
            _progress = progress;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            HttpResponseMessage response = null;
            try
            {
                response = await base.SendAsync(request, cancellationToken);

                if (response.IsSuccessStatusCode)
                {
                    var contentLength = response.Content.Headers.ContentLength;

                    if (contentLength.HasValue)
                    {
                        var totalBytes = contentLength.Value;
                        var bytesTransferred = 0L;

                        using (var stream = await response.Content.ReadAsStreamAsync())
                        {
                            var buffer = new byte[8192];
                            var bytesRead = 0;

                            while ((bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length, cancellationToken)) > 0)
                            {
                                bytesTransferred += bytesRead;
                                _progress.Report(bytesTransferred);
                            }
                        }
                    }
                }
            }
            catch (Exception ee)
            {

            }

            return response;
        }

        public class VideoSourceConverter : IValueConverter
        {
            public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            {
                if (value == null)
                    return null;

                if (string.IsNullOrWhiteSpace(value.ToString()))
                    return null;

                if (Device.RuntimePlatform == Device.UWP)
                    return new Uri($"ms-appx:///Assets/{value}");
                else
                    return new Uri($"ms-appx:///{value}");
            }

            public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            {
                throw new NotImplementedException();
            }
            // ...
        }
    }




}

