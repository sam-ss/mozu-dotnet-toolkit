
using System;
using Newtonsoft.Json;

namespace Mozu.Api.ToolKit.Models
{
    public class ErrorInfo
    {
        public string Message { get; set;  }
        public Exception Exception { get; set; }
        public IApiContext Context { get; set; }

        public string ApiContextStr
        {
            get { return JsonConvert.SerializeObject(Context); }
        }
    }
}
