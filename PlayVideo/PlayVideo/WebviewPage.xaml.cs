using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace PlayVideo
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class WebviewPage : ContentPage
    {
        public WebviewPage()
        {
            InitializeComponent();
        }
        public WebviewPage(string url)
        {
            InitializeComponent();
            var htmlSource = new HtmlWebViewSource
            {
                Html = @"<!DOCTYPE html>
<html>
<head>
  <title>Video Player</title>
</head>
<body>
  <video width=""640"" height=""360"" controls>
    <source src=""http://commondatastorage.googleapis.com/gtv-videos-bucket/sample/BigBuckBunny.mp4"" type=""video/mp4"">
  </video>
</body>
</html>"
            };

            webview.Source = htmlSource;

        }

       
    }
}