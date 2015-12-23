using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

namespace HttpTool
{
     public sealed partial class Scenario1:Page
    {
        private MainPage rootPage;
        int cacheTime = 0;
        public Scenario1()
        {
            this.InitializeComponent();
            btnRequest.Click += BtnRequest_Click;
            btnGetWeb.Click += BtnGetWeb_Click;
            btnGetText.Click += BtnGetText_Click;
            btnGetImage.Click += BtnGetImage_Click;
            btnUseCache.Click += BtnUseCache_Click;
            btnClearCache.Click += BtnClearCache_Click;
            btnCancel.Click += BtnCancel_Click;
        }


        private void BtnCancel_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            HttpTool.CancelAllRequest();
        }

        private void BtnClearCache_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            HttpTool.ClearAllCache();
        }

        private void BtnUseCache_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            if(cacheTime == 0)
            {
                if (!int.TryParse(tbxCacheTime.Text,out cacheTime))
                {
                    rootPage.NotifyUser("要写整数", NotifyType.ErrorMessage);
                    return;
                }
                btnUseCache.Content = "不用缓存";
            }
            else
            {
                cacheTime = 0;
                btnUseCache.Content = "使用缓存";
            }
           
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            rootPage = MainPage.Current;
        }
        private void BtnGetImage_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            //HttpTool.Get("http://img1.gamersky.com/image2015/08/20150808ge_7/gamersky_019origin_037_2015881829E76.jpg", null, cacheTime, null, async (buffer, response, error) =>
            //{
            //    if (error != null)
            //    {
            //        rootPage.NotifyUser(error.Message, NotifyType.ErrorMessage);
            //        return;
            //    }
            //    using (InMemoryRandomAccessStream stream = new InMemoryRandomAccessStream())
            //    {
            //        stream.Seek(0);
            //        await stream.WriteAsync(buffer);
            //        BitmapImage image = new BitmapImage();
            //        stream.Seek(0);
            //        image.SetSource(stream);
            //        webView.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            //        tbx.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            //        img.Visibility = Windows.UI.Xaml.Visibility.Visible;
            //        img.Source = image;
            //    }

            //});
            HttpTool.Get("http://img1.gamersky.com/image2015/09/20150919ge_6/gamersky_19origin_37_20159191747BF0.jpg", null, cacheTime, "GetImage", null, null, null,  (progressValue, progress) =>
            {
                txtState.Text = HttpTool.ConvertState(progress.Stage);
                progressBar.Value = progressValue;
            }, async (buffer, response, error) =>
            {
                if (error != null)
                {
                    rootPage.NotifyUser(error.Message, NotifyType.ErrorMessage);
                    return;
                }
                if (response != null)
                    tbxUrl.Text = response.RequestMessage.RequestUri.AbsoluteUri;
                using (InMemoryRandomAccessStream stream = new InMemoryRandomAccessStream())
                {
                    stream.Seek(0);
                    await stream.WriteAsync(buffer);
                    BitmapImage image = new BitmapImage();
                    stream.Seek(0);
                    image.SetSource(stream);
                    webView.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                    tbx.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                    img.Visibility = Windows.UI.Xaml.Visibility.Visible;
                    img.Source = image;
                }
            });
        }

        private void BtnGetText_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            //HttpTool.Get("https://api.douban.com/v2/movie/in_theaters", null, cacheTime, null,  (buffer, response, error) =>
            //{
            //    if (error != null)
            //    {
            //        rootPage.NotifyUser(error.Message, NotifyType.ErrorMessage);
            //        return;
            //    }
            //    using (DataReader dataReader = DataReader.FromBuffer(buffer))
            //    {
            //        webView.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            //        tbx.Visibility = Windows.UI.Xaml.Visibility.Visible;
            //        img.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            //        tbx.Text = dataReader.ReadString(buffer.Length);
            //    }
            //});
            HttpTool.Get("https://api.douban.com/v2/movie/in_theaters", null, cacheTime, "GetJSON", null, null, null,   (progressValue, progress) =>
            {
                txtState.Text = HttpTool.ConvertState(progress.Stage);
                this.progressBar.Value = progressValue;
            },  (buffer, response, error) =>
            {
                if (error != null)
                {
                    rootPage.NotifyUser(error.Message, NotifyType.ErrorMessage);
                    return;
                }
                if (response != null)
                    tbxUrl.Text = response.RequestMessage.RequestUri.AbsoluteUri;
                using (DataReader dataReader = DataReader.FromBuffer(buffer))
                {
                    webView.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                    tbx.Visibility = Windows.UI.Xaml.Visibility.Visible;
                    img.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                    tbx.Text = dataReader.ReadString(buffer.Length);
                }
            });
        }

        private void BtnGetWeb_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            //HttpTool.Get("http://www.baidu.com", null, cacheTime, null, (buffer, response, error) =>
            //{
            //    if (error != null)
            //    {
            //        rootPage.NotifyUser(error.Message, NotifyType.ErrorMessage);
            //        return;
            //    }
            //    using (DataReader dataReader = DataReader.FromBuffer(buffer))
            //    {
            //        webView.Visibility = Windows.UI.Xaml.Visibility.Visible;
            //        tbx.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            //        img.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            //        string txt= dataReader.ReadString(buffer.Length);
            //        webView.NavigateToString(txt);
            //    }
            //});
            HttpTool.Get("http://www.baidu.com", null, cacheTime, "GetWebPage", null, null, null,  (progressValue, progress) =>
            {
                txtState.Text = HttpTool.ConvertState(progress.Stage);
                progressBar.Value = progressValue;
            },  (buffer, response, error) =>
             {
                 if (error != null)
                 {
                     rootPage.NotifyUser(error.Message, NotifyType.ErrorMessage);
                     return;
                 }
                 if(response != null) 
                   tbxUrl.Text = response.RequestMessage.RequestUri.AbsoluteUri;
                 using (DataReader dataReader = DataReader.FromBuffer(buffer))
                 {
                     webView.Visibility = Windows.UI.Xaml.Visibility.Visible;
                     tbx.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                     img.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                     string txt = dataReader.ReadString(buffer.Length);
                     webView.NavigateToString(txt);
                 }
             });
        }

        private void BtnRequest_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            //Uri uri;
            //if (!Uri.TryCreate(tbxUrl.Text, UriKind.Absolute, out uri))
            //{
            //    rootPage.NotifyUser("Invalid uri", NotifyType.ErrorMessage);
            //    return;
            //}

            HttpTool.Get(tbxUrl.Text, null, cacheTime, null, (buffer, response, error) =>
            {
                if (error != null)
                {
                    rootPage.NotifyUser(error.Message, NotifyType.ErrorMessage);
                    return;
                }
                using (DataReader dataReader = DataReader.FromBuffer(buffer))
                {
                    webView.Visibility = Windows.UI.Xaml.Visibility.Visible;
                    tbx.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                    img.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                    string txt = dataReader.ReadString(buffer.Length);
                    webView.NavigateToString(txt);
                }
            });
        }
    }
}
