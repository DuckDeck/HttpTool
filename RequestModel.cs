using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage.Streams;

namespace HttpTool
{
   public class RequestModel
    {
        public string url { get; set; }
        public string showName { get; set; }    
        public IBuffer buffer { get; set; }
        public string name { get; set; }
        public string key { get; set; }
        public int manageId { get; set; }
        public int libraryId { get; set; }
    }
}
