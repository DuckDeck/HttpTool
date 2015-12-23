using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Data.Json;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.Storage.Pickers;
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
    public sealed partial class Scenario2 : Page
    {
        private Stream currentImageStream;
        private MainPage rootPage;
        private int managerId;
        private string managerKey;
        private int libraryId;
        public Scenario2()
        {
            this.InitializeComponent();
            btnLogin.Click += BtnLogin_Click;
        }

        private void BtnLogin_Click(object sender, RoutedEventArgs e)
        {
            Dictionary<string, object> loginPara = new Dictionary<string, object>();
            loginPara.Add("Acton", "MoreLogin");
            loginPara.Add("UserName", "qfqts1");
            loginPara.Add("Password", "111111");
            loginPara.Add("TermOfValidity", "2160");
            loginPara.Add("Captcha", "0");
            loginPara.Add("Version", "1");
            HttpToolManager.Post("http://api.qingfanqie.com/Login/More/MoreLogin").AddParams(loginPara).Progress((progressValue, progress) => {
                tbkProgress.Text = HttpTool.ConvertState(progress.Stage);
                this.progress.Value = progressValue;
            }).Response((buffer, response, error) =>
              {
                  if (error != null)
                  {
                      rootPage.NotifyUser(error.Message, NotifyType.ErrorMessage);
                      return;
                  }
                  using (DataReader dataReader = DataReader.FromBuffer(buffer))
                  {
                      string txt = dataReader.ReadString(buffer.Length);
                      JsonObject json;
                      if(!JsonObject.TryParse(txt,out json))
                      {
                          rootPage.NotifyUser("返回的格式不能转换成Json", NotifyType.ErrorMessage);
                          return;
                      }
                      if(json["iResultCode"].GetNumber() != 200)
                      {
                          rootPage.NotifyUser(json["sResultMsgCN"].GetString(), NotifyType.ErrorMessage);
                          return;
                      }
                      var a1 = JsonObject.Parse(json["oResultContent"].Stringify());
                      var result = JsonObject.Parse(a1["Info"].Stringify());
                
                      managerId = (int)result.GetNamedNumber("ManagerId");
                      managerKey = result.GetNamedString("ManagerKey");
                      libraryId = (int)result.GetNamedNumber("InLibraryId");

                  }
              });
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            rootPage = MainPage.Current;
        }
        private async void btnChooseImage_Click(object sender, RoutedEventArgs e)
        {
            FileOpenPicker openPicker = new FileOpenPicker();
            openPicker.ViewMode = PickerViewMode.Thumbnail;
            openPicker.SuggestedStartLocation = PickerLocationId.PicturesLibrary;
            openPicker.FileTypeFilter.Add(".jpg");
            openPicker.FileTypeFilter.Add(".jpeg");
            openPicker.FileTypeFilter.Add(".png");
            openPicker.FileTypeFilter.Add(".gif");
            openPicker.FileTypeFilter.Add(".bmp");
            openPicker.FileTypeFilter.Add(".rar");
            openPicker.FileTypeFilter.Add(".txt");
            StorageFile file = await openPicker.PickSingleFileAsync();
            if(file==null)
            {
                rootPage.NotifyUser("你没有选择图片", NotifyType.ErrorMessage);
                return;
            }
            currentImageStream = await file.OpenStreamForReadAsync();
            if (file.FileType.Contains("rar"))
            {
                return;
            }
            BitmapImage image = new BitmapImage();
            image.SetSource(currentImageStream.AsRandomAccessStream());
            img.Source = image;
        }

        private void btnUploadImage_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(managerKey))
            {
                rootPage.NotifyUser("没有管理员Key，你要先登录", NotifyType.ErrorMessage);
                return;
            }
            Dictionary<string, object> para = new Dictionary<string, object>();
            para.Add("ImageTitle", "好东西");
            para.Add("Label", "我的麒麟臂把持不我的麒麟臂把持不我的麒麟臂把持不我的麒麟臂把持不");
            //BitmapImage image = img.Source as BitmapImage;  //此路不通
            if(currentImageStream == null)
            {
                rootPage.NotifyUser("你没有图片", NotifyType.ErrorMessage);
                return;
            }
            para.Add("img", currentImageStream);
            string url = string.Format("http://api.qingfanqie.com/InLibraryConsole/Showcase/UploadWindow/{0}/{1}/{2}", managerId, managerKey, libraryId);
            HttpTool.Post(url, para, "请求", null, (a, b) => {
                tbkProgress.Text = HttpTool.ConvertState(b.Stage);
                progress.Value = a;
            }, async (buffer, response, error) =>
            {
                if (error != null)
                {
                    rootPage.NotifyUser(error.Message, NotifyType.ErrorMessage);
                    return;
                }
                using (DataReader dataReader = DataReader.FromBuffer(buffer))
                {
                    rootPage.NotifyUser(dataReader.ReadString(buffer.Length), NotifyType.StatusMessage);
                }
                IInputStream sm = await response.RequestMessage.Content.ReadAsInputStreamAsync();
                using (StreamReader reader = new StreamReader(sm.AsStreamForRead()))
                {
                    string txt = reader.ReadToEnd();
                    tbxRequestContnet.Text = txt;
                }
            });

        }

        private void btnCancelUploadImage_Click(object sender, RoutedEventArgs e)
        {
            HttpTool.CancelRequestWithIdentity("请求");
        }
    }
}
