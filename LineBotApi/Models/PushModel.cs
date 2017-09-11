using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LineBotApi.Models
{
    public class PushModel
    {
        public class clsPush
        {
            public string to;
            public IList<clsMessages> messages;
        }
        public class clsMessages
        {
            public string type;
            public string text;
            public string altText;
            public clsTemplate template;
        }
        public class clsTemplate
        {
            public string type;
            public string thumbnailImageUrl;
            public string title;
            public string text;
            public IList<clsActions> actions;
        }
        public class clsActions
        {
            public string type;
            public string label;
            public string data;
        }
    }
}