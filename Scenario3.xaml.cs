using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage.Streams;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

// “空白页”项模板在 http://go.microsoft.com/fwlink/?LinkId=234238 上提供

namespace HttpTool
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class Scenario3 : Page
    {
        private MainPage rootPage;
        int cacheTime = 0;
        public Scenario3()
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
            if (cacheTime == 0)
            {
                if (!int.TryParse(tbxCacheTime.Text, out cacheTime))
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

            HttpToolManager.Get("http://img1.gamersky.com/image2015/09/20150919ge_6/gamersky_19origin_37_20159191747BF0.jpg").Cache(cacheTime).CancelToken("GetImage").Progress((progressValue, progress) =>
            {
                txtState.Text = HttpTool.ConvertState(progress.Stage);
                progressBar.Value = progressValue;
            }).Response(async (buffer, response, error) =>
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

            HttpToolManager.Get("https://api.douban.com/v2/movie/in_theaters").Cache(cacheTime).CancelToken("GetText").Progress((progressValue, progress) =>
            {
                txtState.Text = HttpTool.ConvertState(progress.Stage);
                this.progressBar.Value = progressValue;
            }).Response((buffer, response, error) =>
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

            HttpToolManager.Get("http://www.bing.com.cn").Cache(cacheTime).CancelToken("GetJson").Progress((progressValue, progress) =>
            {
                txtState.Text = HttpTool.ConvertState(progress.Stage);
                progressBar.Value = progressValue;
            }).Response((buffer, response, error) =>
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
            HttpToolManager.Get(tbxUrl.Text).Cache(cacheTime).CancelToken("GetRandom").Progress((progressValue, progress) =>
            {
                txtState.Text = HttpTool.ConvertState(progress.Stage);
                progressBar.Value = progressValue;
            }).Response((buffer, response, error) =>
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
