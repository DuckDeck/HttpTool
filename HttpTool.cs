using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.Web.Http;

namespace HttpTool
{
    public class HttpTool: DependencyObject
    {

        private static Dictionary<string, DateTime?> cacheDate;
        private HttpMethod httpMethod;
        private HttpClient httpClient;
        private Uri uri;
        private int cacheTime;
        private HttpRequestMessage requestMessage;
        private static List<HttpTool> quene;
        private string cancelToken;
        private object parameters;
        private Dictionary<string, string> queryPara;
        private Dictionary<string, string> headerFields;
        private CancellationTokenSource cancellationToken;
        private Action<IBuffer,HttpResponseMessage,Exception> onResponse;
        private Action<float,HttpProgress> progress;

        private static int Timeout = 20;

        private bool CacheValid{  //true表示缓存有效，false表示无效
            get {
                if (!cacheDate.ContainsKey(this.uri.AbsoluteUri))
                {
                    return false;
                }
                DateTime? expiredTime = cacheDate[this.uri.AbsoluteUri];
                if (expiredTime != null)
                {
                    if (expiredTime.Value.CompareTo(DateTime.Now)>0)
                    {
                        Debug.WriteLine("缓存失效");
                        return true;
                    }
                }
                return false;
            }
        }
        
        public static void SetGlobalTimeout(int timeout)
        {
            Timeout = timeout;
        }

        static HttpTool()
        {
            quene = new List<HttpTool>();
            cacheDate = new Dictionary<string, DateTime?>();
        }
        private HttpTool(string uri, HttpMethod httpMethod, object parameters,int cache,string cancelToken, 
            Dictionary<string,string> queryPara, Dictionary<string,object> requestOptions, 
            Dictionary<string,string> headerFields, Action<float,HttpProgress> progress = null,
            Action<IBuffer, HttpResponseMessage, Exception> onResponse = null)
        {
            this.httpClient = new HttpClient();
            this.httpMethod = httpMethod;
            this.cacheTime = cache;
            this.cancelToken = cancelToken;
            this.parameters = parameters;
            this.queryPara = queryPara;
            this.headerFields = headerFields;
            this.cancellationToken = new CancellationTokenSource(1000*Timeout);
            this.onResponse = onResponse;
            this.progress = progress;
            this.uri = new Uri(uri,UriKind.RelativeOrAbsolute);
        }
        public static HttpTool Get(string uri, object para,int cache, 
            string cancelToken, Action<IBuffer, HttpResponseMessage, Exception> onResponse = null)
        {
            HttpTool httpTool = new HttpTool(uri, HttpMethod.Get, para, cache, cancelToken, null, null, null, null, onResponse);
            quene.Add(httpTool);
            httpTool.StartRequsest();
            return httpTool;
        }


        public static HttpTool Get(string uri, object para, int cache, string cancelToken,
            Dictionary<string, string> queryPara, Dictionary<string, object> requestOptions,
            Dictionary<string, string> headerFields, Action<float, HttpProgress> progress = null,
             Action<IBuffer, HttpResponseMessage, Exception> onResponse = null)
        {
            HttpTool httpTool = new HttpTool(uri, HttpMethod.Get, para, cache, cancelToken, queryPara, requestOptions, headerFields,progress, onResponse);
            quene.Add(httpTool);
            httpTool.StartRequsest();
            return httpTool;
        }

        public static HttpTool Post(string uri, object para, string cancelToken, Action<IBuffer, HttpResponseMessage, Exception> onResponse = null)
        {
            HttpTool httpTool = new HttpTool(uri, HttpMethod.Post, para, 0, cancelToken, null, null, null, null, onResponse);
            quene.Add(httpTool);
            httpTool.StartRequsest();
            return httpTool;
        }

        public static HttpTool Post(string uri, object para, string cancelToken, Dictionary<string, string> queryPara, Action<float, HttpProgress> progress = null, Action<IBuffer, HttpResponseMessage, Exception> onResponse = null)
        {
            HttpTool httpTool = new HttpTool(uri, HttpMethod.Post, para, 0, cancelToken, queryPara, null, null, progress, onResponse);
            quene.Add(httpTool);
            httpTool.StartRequsest();
            return httpTool;
        }


        public static HttpTool Post(string uri, object para, string cancelToken, Dictionary<string, string> queryPara,
            Dictionary<string, object> requestOptions,
            Dictionary<string, string> headerFields,
            Action<float, HttpProgress> progress = null, Action<IBuffer, HttpResponseMessage, Exception> onResponse = null)
        {
            HttpTool httpTool = new HttpTool(uri, HttpMethod.Post, para, 0, cancelToken, queryPara, requestOptions, headerFields, progress, onResponse);
            quene.Add(httpTool);
            httpTool.StartRequsest();
            return httpTool;
        }

        private Exception AddParametersToRequest(object parameters)
        {
            Exception ex = null;
            if (this.requestMessage.Method == HttpMethod.Post || this.requestMessage.Method == HttpMethod.Put)
            {
                if (parameters.GetType() == typeof(Dictionary<string, object>))
                {
                    Dictionary<string, object> items = parameters as Dictionary<string, object>;
                    HttpMultipartFormDataContent content = new HttpMultipartFormDataContent();
                    foreach (KeyValuePair<string, object> keyValuePair in items)
                    {
                        if (keyValuePair.Value is string)
                        {
                           // content.Add(new HttpStringContent((string)keyValuePair.Value), "\"" + keyValuePair.Key + "\"");
                            content.Add(new HttpStringContent((string)keyValuePair.Value), keyValuePair.Key );
                        }
                        else if (keyValuePair.Value is IRandomAccessStream)
                        {

                            IRandomAccessStream stream = (IRandomAccessStream)keyValuePair.Value;
                            stream.Seek(0);
                            //默认用Key当filename
                            HttpStreamContent streamContent = new HttpStreamContent(stream);
                            string mediaType = GetImageFormat(stream.AsStream(),out ex);//这个地方要读一次流，所以位置就有问题了，读完之后又要重设一次，干他妈的，这个问题困扰我好几的时间
                            stream.Seek(0);
                            Debug.WriteLine("The Image Media Type:" + mediaType);
                            if(string.IsNullOrEmpty(mediaType))
                              ex = new InvalidCastException("his media type can not support");
                            streamContent.Headers.ContentType = new Windows.Web.Http.Headers.HttpMediaTypeHeaderValue("Image/"+mediaType);
                            content.Add(streamContent,keyValuePair.Key,keyValuePair.Key + "." + mediaType);
                        }
                        else if(keyValuePair.Value is Stream)
                        {
                            Stream stream = (Stream)keyValuePair.Value;
                            stream.Seek(0, SeekOrigin.Begin);
                            IRandomAccessStream ias = stream.AsRandomAccessStream();
                            HttpStreamContent streamContent = new HttpStreamContent(ias);                      
                            string mediaType = GetImageFormat(stream, out ex);
                            ias.Seek(0);
                            if (string.IsNullOrEmpty(mediaType))
                                ex = new InvalidCastException("his media type can not support");
                            streamContent.Headers.ContentType = new Windows.Web.Http.Headers.HttpMediaTypeHeaderValue("image/" + mediaType);
                            content.Add(streamContent, keyValuePair.Key, keyValuePair.Key + "." + mediaType);
                        }
                    }
                    this.requestMessage.Content = content;
                }
            }
            else if (parameters != null && parameters.GetType() == typeof(Dictionary<string, object>))
            {
                Dictionary<string, object> items = parameters as Dictionary<string, object>;
                StringBuilder sb = new StringBuilder();
                foreach (var item in items)
                {
                    sb.Append(item.Key + "=" + item.Value.ToString() + "&");
                }
                if (sb.ToString().EndsWith("&"))
                {
                    sb.Remove(sb.Length - 1, 1);
                }
                if (sb.Length > 0)
                {
                    if (this.uri.AbsoluteUri.Contains("?"))
                    {
                        this.uri = new Uri(this.uri.AbsoluteUri + "&" + sb.ToString());
                    }
                    else
                    {
                        this.uri = new Uri(this.uri.AbsoluteUri + "?" + sb.ToString());
                    }
                    this.requestMessage.RequestUri = this.uri;
                }
            }
            if(this.queryPara != null)
            {
                StringBuilder sb = new StringBuilder();
                foreach (var item in this.queryPara)
                {
                    sb.Append(item.Key + "=" + item.Value.ToString() + "&");
                }
                if (sb.ToString().EndsWith("&"))
                {
                    sb.Remove(sb.Length - 1, 1);
                }
                if (sb.Length > 0)
                {
                    if (this.uri.AbsoluteUri.Contains("?"))
                    {
                        this.uri = new Uri(this.uri.AbsoluteUri + "&" + sb.ToString());
                    }
                    else
                    {
                        this.uri = new Uri(this.uri.AbsoluteUri + "?" + sb.ToString());
                    }
                    this.requestMessage.RequestUri = this.uri;
                }
            }
            return ex;

        }

        private void SetHttpHeader()
        {
            if(this.headerFields != null)
            {
               foreach(var k in this.headerFields)
               {
                    this.requestMessage.Headers.Add(k);
               }
            }
        }

        private async void StartRequsest()
        {
          
            HttpResponseMessage responseMessage;
            try
            {
                this.requestMessage = new HttpRequestMessage(this.httpMethod, this.uri);
                Exception exc = AddParametersToRequest(parameters);
                if (exc != null)
                {
                    FinishRequest(null, null, exc);
                    return;
                }
                IProgress<HttpProgress> progre = new Progress<HttpProgress>(ProgressHandler);
                if (this.cacheTime > 0)
                {
                    if (this.CacheValid) //如果缓存有效
                    {
                        StorageFile file =await GetCacheFile();
                        IBuffer bu =await FileIO.ReadBufferAsync(file);
                        FinishRequest(bu, null, null); //用缓存就没有responseMessage
                        return;
                    }
                }
                responseMessage = await httpClient.SendRequestAsync(this.requestMessage).AsTask(this.cancellationToken.Token, progre);

                IBuffer buffer =await responseMessage.Content.ReadAsBufferAsync();
                if(this.cacheTime > 0)
                {
                    if (!this.CacheValid)//如果失效
                    {
                        cacheDate[this.uri.AbsoluteUri] = DateTime.Now.AddSeconds(this.cacheTime);
                        StorageFile file =await GetCacheFile();
                        await FileIO.WriteBufferAsync(file, buffer);
                    }
                }
                FinishRequest(buffer, responseMessage, null);
            }
            catch (TaskCanceledException exception)
            {
                FinishRequest(null, null, exception);
            }
            catch (Exception ex)
            {
                FinishRequest(null, null, ex);

            }
            finally
            {
               // this.httpClient.Dispose();
            }
        }

        private void ProgressHandler(HttpProgress progress)
        {
            
            if(this.progress != null)
            {
                if(progress.Stage != HttpProgressStage.ReceivingContent && progress.Stage != HttpProgressStage.SendingContent)
                {
                    this.progress(0, progress);
                    return;
                }
                if(this.requestMessage.Method == HttpMethod.Get || this.requestMessage.Method == HttpMethod.Delete)
                {
                    if(progress.TotalBytesToReceive != null && progress.TotalBytesToReceive != 0)
                    {
                       this.progress((float)progress.BytesReceived / (float)progress.TotalBytesToReceive.Value, progress);
                    }
                    else
                    {
                        this.progress(0, progress);
                    }
                }
                else
                {
                    if (progress.TotalBytesToSend != null && progress.TotalBytesToSend != 0)
                    {
                        this.progress((float)progress.BytesSent / (float)progress.TotalBytesToSend.Value, progress);
                    }
                    else
                    {
                        this.progress(0, progress);
                    }
                }
            }
        }

        private async void FinishRequest(IBuffer buffer, HttpResponseMessage response, Exception exception)
        {
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                onResponse(buffer, response, exception);
                
                quene.Remove(this);
                if (httpClient != null)
                    httpClient.Dispose();
            });
        }

        public static void CancelRequestWithIdentity(string identity)
        {
            HttpTool httpTool = quene.Find(s => s.cancelToken == identity);   
            if(httpTool != null)
            {
                httpTool.cancellationToken.Cancel();
                quene.Remove(httpTool);
                httpTool.httpClient.Dispose();
            }
        }

        public static void CancelAllRequest()
        {
            foreach(HttpTool httpTool in quene)
            {
                httpTool.cancellationToken.Cancel();
                httpTool.httpClient.Dispose();
            }
            quene.Clear();
        }

        public static async void ClearAllCache()
        {
            StorageFolder local = ApplicationData.Current.LocalCacheFolder;
            var cacheFolder = await local.CreateFolderAsync("HttpToolCache", CreationCollisionOption.OpenIfExists);
            await cacheFolder.DeleteAsync();
            cacheDate.Clear();
        }

        public static async void ClearCacheWithUrl(string uri)
        {
            if (cacheDate.ContainsKey(uri))
                cacheDate.Remove(uri);
            StorageFolder local = ApplicationData.Current.LocalCacheFolder;
            var cacheFolder = await local.CreateFolderAsync("HttpToolCache", CreationCollisionOption.OpenIfExists);
            StorageFile file = await cacheFolder.CreateFileAsync(ConvertUrlToFileName(uri), CreationCollisionOption.ReplaceExisting);
            await file.DeleteAsync();
        }

        private string GetImageFormat(Stream stream,out Exception e)
        {
            e = null;
            stream.Seek(0, SeekOrigin.Begin);
            System.IO.BinaryReader r = new System.IO.BinaryReader(stream);
            string fileclass = "";
            //这里的位长要具体判断.
            byte buffer;
            try
            {
                buffer = r.ReadByte();
                fileclass = buffer.ToString();
                buffer = r.ReadByte();
                fileclass += buffer.ToString();

            }
            catch(Exception ex)
            {
                e = ex;
            }
            string mediaType = string.Empty;
            switch (fileclass)  //说明：255216是jpg;7173是gif;6677是BMP,13780是PNG;7790是exe,8297是rar

            {
                case "255216": mediaType = "jpg"; break;
                case "7173": mediaType ="gif";break;
                case "6677": mediaType = "bmp"; break;
                case "13780": mediaType = "png";break;
                case "8297": mediaType ="rar";break;
            }
            return mediaType;
        }

        private string ConvertUrlToFileName()
        {
            return this.uri.AbsoluteUri.Replace('\\', '_').Replace('?', '_').Replace('&', '-').Replace(':', '_').Replace('/', '_');
        }

        private static string   ConvertUrlToFileName(string uri)
        {
            return uri.Replace('\\', '_').Replace('?', '_').Replace('&', '-').Replace(':', '_').Replace('/', '_');
        }

        private async Task<StorageFile> GetCacheFile()
        {
            StorageFolder local = ApplicationData.Current.LocalCacheFolder;
            var cacheFolder = await local.CreateFolderAsync("HttpToolCache", CreationCollisionOption.OpenIfExists);
            StorageFile file =await cacheFolder.CreateFileAsync(ConvertUrlToFileName(), CreationCollisionOption.OpenIfExists);
            return file;
        }

        public static String ConvertState(HttpProgressStage stage)
        {
            switch (stage)
            {
                case HttpProgressStage.None: return "无状态";
                case HttpProgressStage.ConnectingToServer: return "连接服务器中";
                case HttpProgressStage.DetectingProxy: return "检测代理中";
                case HttpProgressStage.NegotiatingSsl: return "SSL 协商";
                case HttpProgressStage.ReceivingContent: return "正在接收数据";
                case HttpProgressStage.ReceivingHeaders: return "下在接收关文件";
                case HttpProgressStage.ResolvingName:return "正在解析 HTTP 连接的主机名";
                case HttpProgressStage.SendingContent:return "发送内容中";
                case HttpProgressStage.SendingHeaders:return "发送头文件中";
                case HttpProgressStage.WaitingForResponse:return "等待响应";
                default: return "无";
            }
        }
    }

    public class HttpToolManager
    {
        private HttpMethod httpMethod;
        private string uri;
        private int cacheTime;
        private string cancelToken;
        private object parameters;
        private Dictionary<string, string> queryPara;
        private Dictionary<string, string> headerFields;
        private Action<IBuffer, HttpResponseMessage, Exception> onResponse;
        private Action<float, HttpProgress> progress;
       public static HttpToolManager Get(string url)
        {
            HttpToolManager manager = new HttpToolManager();
            manager.uri = url;
            manager.httpMethod = HttpMethod.Get;
            return manager;
        }
       public static HttpToolManager Post(string url)
        {
            HttpToolManager manager = new HttpToolManager();
            manager.uri = url;
            manager.httpMethod = HttpMethod.Post;
            return manager;
        }
        public HttpToolManager AddParams(object para)
        {
            this.parameters = para;
            return this;
        }

        public HttpToolManager Cache(int cacheTime)
        {
            this.cacheTime = cacheTime;
            return this;
        }

        public HttpToolManager CancelToken(string token)
        {
            this.cancelToken = token;
            return this;
        }

        public HttpToolManager QueryPara(Dictionary<string, string> queryPara)
        {
            this.queryPara = queryPara;
            return this;
        }
        public HttpToolManager HeaderFields(Dictionary<string, string> headerFields)
        {
            this.headerFields = headerFields;
            return this;
        }
        public HttpToolManager Progress(Action<float, HttpProgress> progress)
        {
            this.progress = progress;
            return this;
        }

        public void Response(Action<IBuffer, HttpResponseMessage, Exception> response)
        {
            this.onResponse = response;
            if(this.httpMethod == HttpMethod.Get)
            {
                HttpTool.Get(this.uri,parameters, cacheTime, cancelToken, queryPara, null, headerFields, progress, onResponse);
            }
            else if (this.httpMethod == HttpMethod.Post)
            {
                HttpTool.Post(this.uri, parameters, cancelToken, queryPara, null, headerFields, progress, onResponse);
            }
        }

        
    }
}
