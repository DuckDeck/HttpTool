using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;

namespace HttpTool
{
    public partial class MainPage:Page
    {
        List<Scenario> scenarios = new List<Scenario>
        {
            new Scenario() { Title = "Get",   ClassType = typeof(Scenario1)  },
            new Scenario() { Title = "Post",  ClassType = typeof(Scenario2)  },
            new Scenario() { Title = "函数式Get",ClassType = typeof(Scenario3)  },
            //new Scenario() { Title = "Writing and reading bytes in a file",                  ClassType = typeof(SDKTemplate.Scenario4)  },
            //new Scenario() { Title = "Writing and reading using a stream",                   ClassType = typeof(SDKTemplate.Scenario5)  },
            //new Scenario() { Title = "Displaying file properties",                           ClassType = typeof(SDKTemplate.Scenario6)  },
            //new Scenario() { Title = "Persisting access to a storage item for future use",   ClassType = typeof(SDKTemplate.Scenario7)  },
            //new Scenario() { Title = "Copying a file",                                       ClassType = typeof(SDKTemplate.Scenario8)  },
            //new Scenario() { Title = "Comparing two files to see if they are the same file", ClassType = typeof(SDKTemplate.Scenario9)  },
            //new Scenario() { Title = "Deleting a file",                                      ClassType = typeof(SDKTemplate.Scenario10) },
            //new Scenario() { Title = "Attempting to get a file with no error on failure",    ClassType = typeof(SDKTemplate.Scenario11) },
        };
       
    }


    public class Scenario
    {
        public string Title { get; set; }
        public Type ClassType { get; set; }
    }
}
