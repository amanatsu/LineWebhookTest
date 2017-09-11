using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LineBotApi.Models
{
    public class WebhookModel
    {
        public IList<clsEvent> events;
    }
    public class clsMessage
    {
        public string id;
        public string type;
        public string text;
    }
    public class clsSource
    {
        public string type;
        public string userId;
    }
    public class clsPostBack
    {
        public string data;
    }
    public class clsEvent
    {
        public string replyToken;
        public string type;
        public long timestamp;
        public clsSource source;
        public clsMessage message;
        public clsPostBack postback;
    }

}