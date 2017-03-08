using System;
using System.Collections.Generic;
using System.Configuration;
using System.Net;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NLog;
using NLog.Common;
using NLog.Config;
using NLog.Targets;

namespace NLog.Hipchat
{
    [Target("HipChat")]
    public class HipChatTarget : TargetWithLayout
    {
        [RequiredParameter]
        public string Token { get; set; }
        [RequiredParameter]
        public string RoomId { get; set; }
        [RequiredParameter]
        public string Style { get; set; }
        [RequiredParameter]
        public string Host { get; set; }
        public string Site { get; set; }
        public string Icon { get; set; }
        public string ExpandMessage { get; set; }
        public string Thumbnail { get; set; }
        public string ThumbnailWidth { get; set; }
        public string ThumbnailHeight { get; set; }
        public bool Activity { get; set; }

        protected override void Write(LogEventInfo logEvent)
        {
            string json = BuildLog(logEvent);
            WriteLogMessage(json);
        }

        protected override void Write(AsyncLogEventInfo[] logEvents)
        {
            StringBuilder builder = new StringBuilder();
            foreach (AsyncLogEventInfo item in logEvents)
            {
                string json = BuildLog(item.LogEvent);
                builder.Append(json);
                builder.AppendLine();
            }
            this.WriteLogMessage(builder.ToString());
        }

        private string BuildLog(LogEventInfo item)
        {
            JObject json = new JObject();
            json["message"] = item.Message;            
            json["card"] = new JObject();
            json["card"]["style"] = Style;
            json["card"]["url"] = Site;
            json["card"]["format"] = "medium";
            json["card"]["id"] = Guid.NewGuid();
            json["card"]["title"] = item.Message;
            if (Activity)
            {
                json["card"]["activity"] = new JObject();
                json["card"]["activity"]["html"] = "This is a notification about <b>an object</b>";
            }
            if (item.Exception != null)
            {
                json["card"]["description"] = item.Exception.Message;
            }
            json["card"]["icon"] = new JObject();
            json["card"]["icon"]["url"] = Icon;
            json["card"]["icon"]["url@2x"] = Icon;
            JArray attributes = new JArray();
            JObject attribute = new JObject();
            attribute["label"] = "TimeStamp";
            attribute["value"] = new JObject();
            attribute["value"]["label"] = item.TimeStamp;
            attribute["value"]["style"] = "lozenge";
            attributes.Add(attribute);
            attribute = new JObject();
            attribute["label"] = "Level";
            attribute["value"] = new JObject();
            attribute["value"]["label"] = item.Level.Name;
            attribute["value"]["style"] = GetAttributeStyle(item.Level.Name);
            attributes.Add(attribute);
            foreach (KeyValuePair<object, object> p in item.Properties)
            {
                if (p.Key.ToString() != "CallerFilePath" && p.Key.ToString() != "CallerMemberName" && p.Key.ToString() != "CallerLineNumber")
                {
                    attribute = new JObject();
                    attribute["label"] = p.Key.ToString();
                    attribute["value"] = new JObject();
                    attribute["value"]["label"] = p.Value.ToString();
                    attribute["value"]["style"] = "lozenge";
                    attributes.Add(attribute);
                }
            }
            json["card"]["attributes"] = attributes;
            if (!string.IsNullOrEmpty(Thumbnail))
            {
                json["thumbnail"] = new JObject();
                json["thumbnail"]["url"] = Thumbnail;
                json["thumbnail"]["url@2x"] = Thumbnail;
                json["thumbnail"]["width"] = ThumbnailWidth;
                json["thumbnail"]["height"] = ThumbnailHeight;
            }
            return json.ToString();
        }

        private string GetAttributeStyle(string level)
        {
            string style = string.Empty;
            switch (level)
            {
                case "Fatal":
                    style = "lozenge";
                    break;
                case "Error":
                    style = "lozenge-error";
                    break;
                case "Warn":
                    style = "lozenge-current";
                    break;
                case "Info":
                    style = "lozenge-complete";
                    break;
                case "Debug":
                    style = "lozenge-success";
                    break;
                case "Trace":
                    style = "lozenge-moved";
                    break;
            }
            return style;
        }

        private void WriteLogMessage(string builder)
        {
            ServicePointManager.ServerCertificateValidationCallback += (sender, certificate, chain, sslPolicyErrors) => true;
            using (var client = new HttpClient())
            {
                string url = $"{Host}/v2/room/{RoomId}/notification?auth_token={Token}";
                var response = client.PostAsync(url, new StringContent(builder, Encoding.UTF8, "application/json")).Result.Content.ReadAsStringAsync();
            }
        }
    }
}