using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Threading.Tasks;
using System.IO;
using System.Text;
using System.Security.Cryptography;
using LineBotApi.Models;
using System.Web;

namespace LineBotApi.Controllers
{
    public class WebhookController : ApiController
    {
        private const string ChannelSecret = ApiKeys.LINE_ChannelSecret;
        private const string ChannelAccessToken = ApiKeys.LINE_ChannelAccessToken;
        private const string TalkApiKey = ApiKeys.A3RT_TalkApiKey;

        public IHttpActionResult Get()
        {
            string s = "LineBotApi";
            return Ok(s);
        }

        public IHttpActionResult Post(JObject value)
        {
            WriteFile(DateTime.Now.ToString());
            WriteFile(Request.Headers.ToString());
            WriteFile(value.ToString());

            WebhookModel data = JsonConvert.DeserializeObject<WebhookModel>(value.ToString());

            Request.Content.ReadAsStreamAsync().Result.Seek(0, SeekOrigin.Begin);
            string body = Request.Content.ReadAsStringAsync().Result;
            string validkey = Validation(body);

            WriteFile("key=" + validkey);

            IEnumerable<string> headers;
            string header = "";
            if (Request.Headers.TryGetValues("X-Line-Signature", out headers))
            {
                header = headers.FirstOrDefault();
            }
            if (header == validkey)
            {
                clsEvent events = data.events.FirstOrDefault();
                
                try
                {
                    string msg = "";
                    switch (events.type)
                    {
                        case "message":
                            if (events.message.text.IndexOf("天気") >= 0)
                            {
                                //天気テンプレート
                                msg = MakeJsonTemplate("weather", events.source.userId);
                                WriteFile("DataPost:" + "weather template");
                                DataPost(msg);
                            }
                            else
                            {
                                //TalkApiでメッセージを返す
                                WriteFile("DataPost:" + events.message.text);
                                string talkMessage = TalkDataGet(events.message.text);
                                msg = MakeJsonMessage(talkMessage, events.source.userId);
                                DataPost(msg);
                            }
                            break;
                        case "postback":
                            string postbackFunction = "";
                            Dictionary<string, string> pb = ParseQueryString(events.postback.data);
                            pb.TryGetValue("function", out postbackFunction);
                            switch (postbackFunction)
                            {
                                case "weather":
                                    string id = "";
                                    pb.TryGetValue("id", out id);
                                    msg = WeatherDataGet(id);
                                    WriteFile("DataPost:" + msg);
                                    msg = MakeJsonMessage(msg, events.source.userId);
                                    DataPost(msg);
                                    break;
                            }
                            break;
                    }
                }
                catch (Exception e)
                {
                    WriteFile("ERROR:" + e.Message);
                }   
            }

            string s = "LineBotApi-Post";
            return Ok(s);
        }
        private string Validation(string requestBody)
        {
            byte[] data = System.Text.Encoding.UTF8.GetBytes(requestBody);
            byte[] keyData = System.Text.Encoding.UTF8.GetBytes(ChannelSecret);
            string result = "";

            using (HMACSHA256 hmac = new HMACSHA256(keyData))
            {
                byte[] bs = hmac.ComputeHash(data);
                hmac.Clear();
                result = Convert.ToBase64String(bs);
            }
            
            return result;
        }
        private void DataPost(string sendMessage)
        {
            sendMessage = sendMessage.Replace("\r", @"\r").Replace("\n", @"\n");//このようにしないと400エラーになってしまう
            System.Text.Encoding enc = System.Text.Encoding.GetEncoding("UTF-8");

            string postData = sendMessage;
            byte[] postDataBytes = System.Text.Encoding.UTF8.GetBytes(postData);
            string requestUrl = string.Format("https://api.line.me/v2/bot/message/push");
            WebRequest req = WebRequest.Create(requestUrl);

            req.Headers.Set("Authorization", "Bearer " + ChannelAccessToken);
            req.Method = "POST";
            req.ContentType = "application/json";
            req.ContentLength = postDataBytes.Length;

            Stream reqStream = req.GetRequestStream();
            reqStream.Write(postDataBytes, 0, postDataBytes.Length);
            reqStream.Close();

            WebResponse res = req.GetResponse();
            Stream resStream = res.GetResponseStream();
            StreamReader sr = new StreamReader(resStream, enc);
            Console.WriteLine(sr.ReadToEnd());
            sr.Close();

        }
        private string WeatherDataGet(string id)
        {
            //東京の天気　固定
            string url = "http://weather.livedoor.com/forecast/webservice/json/v1?city=130010";
            string msg = "";

            JObject jo = JObject.Parse(DataGet(url));
            WriteFile(jo.ToString());
            WeatherModel.clsWeather weather = JsonConvert.DeserializeObject<WeatherModel.clsWeather>(jo.ToString());
            WeatherModel.clsForecasts f = null;
            switch (id)
            {
                case "1":
                    f = weather.forecasts.FirstOrDefault(a => a.dateLabel == "今日");
                    break;
                case "2":
                    f = weather.forecasts.FirstOrDefault(a => a.dateLabel == "明日");
                    break;
                case "3":
                    f = weather.forecasts.FirstOrDefault(a => a.dateLabel == "明後日");
                    break;
            }
            
            if (f == null)
            {
                msg = weather.description.text;
            }
            else
            {
                msg += f.dateLabel + "の天気";
                msg += "\r\n" + f.telop;
                if (f.temperature.max != null) { msg += "\r\n最高気温：" + f.temperature.max.celsius + "℃"; }
                if (f.temperature.min != null) { msg += "\r\n最低気温：" + f.temperature.min.celsius + "℃"; }
            }
            return msg;

        }
        private string DataGet(string url)
        {
            string source = "";
            WebRequest req = WebRequest.Create(url);
            using (WebResponse res = req.GetResponse())
            {
                using (Stream st = res.GetResponseStream())
                {
                    using (StreamReader sr = new StreamReader(st, Encoding.UTF8))
                    {
                        source = sr.ReadToEnd();
                    }
                }
            }
            return source;
        }
        private void WriteFile(string sMessage)
        {
            try
            {
#if DEBUG
                //System.IO.Directory.CreateDirectory(@"C:\temp");
                System.IO.StreamWriter sw = new System.IO.StreamWriter(
                @"C:\temp\LineBotApi" + DateTime.Now.ToString("yyyyMMdd") + ".log", true, System.Text.Encoding.GetEncoding("shift_jis"));
#else
                System.IO.Directory.CreateDirectory(@"D:\home\site\wwwroot\Log");
                System.IO.StreamWriter sw = new System.IO.StreamWriter(
                @"D:\home\site\wwwroot\Log\LineBotApi" + DateTime.Now.ToString("yyyyMMdd") + ".log", true, System.Text.Encoding.GetEncoding("shift_jis"));
#endif
                sw.Write(sMessage + "\r\n");
                sw.Close();
            }
            catch (Exception Err)
            {

            }
        }
        private Dictionary<string,string> ParseQueryString(string queryString)
        {
            var nvc = HttpUtility.ParseQueryString(queryString);
            return nvc.AllKeys.ToDictionary(k => k, k => nvc[k]);
        }
        private string MakeJsonMessage(string sendMessage, string messageTo)
        {
            PushModel.clsPush p = new PushModel.clsPush();
            p.to = messageTo;
            p.messages = new List<PushModel.clsMessages>();
            PushModel.clsMessages m = new PushModel.clsMessages();
            m.type = "text";
            m.text = sendMessage;
            p.messages.Add(m);
            return JsonConvert.SerializeObject(p);
        }
        public string MakeJsonTemplate(string function, string messageTo)
        {
            if (function == "weather")
            {
                PushModel.clsPush p = new PushModel.clsPush();
                p.to = messageTo;
                p.messages = new List<PushModel.clsMessages>();
                PushModel.clsMessages m = new PushModel.clsMessages();
                    m.type = "template";
                    m.altText = "天気予報";
                        PushModel.clsTemplate t = new PushModel.clsTemplate();
                        t.type = "buttons";
                        t.thumbnailImageUrl = "https://pics.prcm.jp/1f1d0f49e815a/70123492/jpeg/70123492.jpeg";
                        t.title = "東京の天気予報";
                        t.text = "知りたい情報を選んでください";
                        t.actions = new List<PushModel.clsActions>();
                            PushModel.clsActions a = new PushModel.clsActions();
                            a.type = "postback";
                            a.label = "天気概況";
                            a.data = "function=weather&id=0";
                        t.actions.Add(a);
                            a = new PushModel.clsActions();
                            a.type = "postback";
                            a.label = "今日の天気";
                            a.data = "function=weather&id=1";
                        t.actions.Add(a);
                            a = new PushModel.clsActions();
                            a.type = "postback";
                            a.label = "明日の天気";
                            a.data = "function=weather&id=2";
                        t.actions.Add(a);
                            a = new PushModel.clsActions();
                            a.type = "postback";
                            a.label = "明後日の天気";
                            a.data = "function=weather&id=3";
                        t.actions.Add(a);
                    m.template = t;
                p.messages.Add(m);
                return JsonConvert.SerializeObject(p);
            }
            else
            {
                return "";
            }

        }
        private string TalkDataGet(string msg)
        {
            string url = "https://api.a3rt.recruit-tech.co.jp/talk/v1/smalltalk";
            Encoding enc = Encoding.GetEncoding("UTF-8");
            string boundary = System.Environment.TickCount.ToString();

            HttpWebRequest req = (HttpWebRequest)WebRequest.Create(url);
            req.Method = "POST";
            req.ContentType = "multipart/form-data; boundary=" + boundary;

            string postData = "";
            postData = "--" + boundary + "\r\n" +
                "Content-Disposition: form-data; name=\"apikey\"\r\n\r\n" +
                TalkApiKey + "\r\n" +
                "--" + boundary + "\r\n" +
                "Content-Disposition: form-data; name=\"query\"\r\n\r\n" +
                msg + "\r\n";

            byte[] startData = enc.GetBytes(postData);   
            req.ContentLength = startData.Length;

            System.IO.Stream reqStream = req.GetRequestStream();            
            reqStream.Write(startData, 0, startData.Length);
            reqStream.Close();

            HttpWebResponse res = (HttpWebResponse)req.GetResponse();
            Stream resStream = res.GetResponseStream();
            StreamReader sr = new StreamReader(resStream, enc);
            string retJson = sr.ReadToEnd();
            sr.Close();
            return TalkDataSerialize(retJson);
        }
        private string TalkDataSerialize(string jsonData)
        {
            JObject jo = JObject.Parse(jsonData);
            TalkModel talk = JsonConvert.DeserializeObject<TalkModel>(jo.ToString());
            return talk.results[0].reply;
        }
    }
}
