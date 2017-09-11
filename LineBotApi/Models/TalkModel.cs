using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LineBotApi.Models
{
    public class TalkModel
    {
        public int status;
        public string message;
        public IList<clsResults> results;

        public class clsResults
        {
            public float perplexity;
            public string reply;
        }
    }
}