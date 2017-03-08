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

//<target xsi:type="HipChat"
//              name="h"
//              layout="${uppercase:${level}} | ${message} | ${exception} | ${stacktrace:format=Flat} | ${appdomain}"
//              token="KmT2m6HnkcgpUk1adAMunvRrxlOzF4b5ZMFhZiyc"
//              roomid="2774407"
//              url="https://cignium.hipchat.com/v2/room/{0}/notification?auth_token={1}"
//              />


//    {
//  "error": {
//    "code": 400,
//    "description": "Value {value!r} is not of type {expected_type!r}",
//    "expected_type": [
//      {
//        "type": "null"
//      },
//      {
//        "additionalProperties": true,
//        "properties": {
//          "activity": {
//            "additionalProperties": false,
//            "description": "The activity will generate a collapsable card of one line showing the html\nand the ability to maximize to see  all the content.",
//            "optional": true,
//            "properties": {
//              "html": {
//                "description": "Html for the activity to show in one line a summary of the action that happened",
//                "minLength": 1,
//                "type": "string"
//              },
//              "icon": {
//                "optional": true,
//                "type": [
//                  {
//                    "type": "string"
//                  },
//                  {
//                    "additionalProperties": true,
//                    "properties": {
//                      "url": {
//                        "description": "The url where the icon is",
//                        "minLength": 1,
//                        "type": "string"
//                      },
//                      "url@2x": {
//                        "description": "The url for the icon in retina",
//                        "minLength": 1,
//                        "optional": true,
//                        "type": "string"
//                      }
//                    },
//                    "type": "object"
//                  }
//                ]
//              }
//            },
//            "type": "object"
//          },
//          "attributes": {
//            "description": "List of attributes to show below the card. Sample {label}:{value.icon} {value.label}",
//            "items": {
//              "additionalProperties": false,
//              "properties": {
//                "label": {
//                  "maxLength": 50,
//                  "minLength": 1,
//                  "optional": true,
//                  "type": "string"
//                },
//                "value": {
//                  "additionalProperties": false,
//                  "properties": {
//                    "icon": {
//                      "optional": true,
//                      "type": [
//                        {
//                          "type": "string"
//                        },
//                        {
//                          "additionalProperties": true,
//                          "properties": {
//                            "url": {
//                              "description": "The url where the icon is",
//                              "minLength": 1,
//                              "type": "string"
//                            },
//                            "url@2x": {
//                              "description": "The url for the icon in retina",
//                              "minLength": 1,
//                              "optional": true,
//                              "type": "string"
//                            }
//                          },
//                          "type": "object"
//                        }
//                      ]
//                    },
//                    "label": {
//                      "description": "The text representation of the value",
//                      "minLength": 1,
//                      "type": "string"
//                    },
//                    "style": {
//                      "description": "AUI Integrations for now supporting only lozenges",
//                      "enum": [
//                        "lozenge-success",
//                        "lozenge-error",
//                        "lozenge-current",
//                        "lozenge-complete",
//                        "lozenge-moved",
//                        "lozenge"
//                      ],
//                      "minLength": 1,
//                      "optional": true,
//                      "type": "string"
//                    },
//                    "url": {
//                      "description": "Url to be opened when a user clicks on the label",
//                      "minLength": 1,
//                      "optional": true,
//                      "type": "string"
//                    }
//                  },
//                  "type": "object"
//                }
//              },
//              "type": "object"
//            },
//            "maxLength": 10,
//            "minLength": 1,
//            "optional": true,
//            "type": "array"
//          },
//          "description": {
//            "optional": true,
//            "type": [
//              {
//                "maxLength": 500,
//                "minLength": 0,
//                "type": "string"
//              },
//              {
//                "additionalProperties": false,
//                "properties": {
//                  "format": {
//                    "description": "The format that can be html or text",
//                    "enum": [
//                      "html",
//                      "text"
//                    ],
//                    "minLength": 1,
//                    "type": "string"
//                  },
//                  "value": {
//                    "description": "The description in the specific format",
//                    "maxLength": 1000,
//                    "minLength": 1,
//                    "type": "string"
//                  }
//                },
//                "type": "object"
//              }
//            ]
//          },
//          "format": {
//            "description": "Application cards can be compact (1 to 2 lines) or medium (1 to 5 lines)",
//            "enum": [
//              "compact",
//              "medium"
//            ],
//            "maxLength": 25,
//            "minLength": 1,
//            "optional": true,
//            "type": "string"
//          },
//          "icon": {
//            "optional": true,
//            "type": [
//              {
//                "type": "string"
//              },
//              {
//                "additionalProperties": true,
//                "properties": {
//                  "url": {
//                    "description": "The url where the icon is",
//                    "minLength": 1,
//                    "type": "string"
//                  },
//                  "url@2x": {
//                    "description": "The url for the icon in retina",
//                    "minLength": 1,
//                    "optional": true,
//                    "type": "string"
//                  }
//                },
//                "type": "object"
//              }
//            ]
//          },
//          "id": {
//            "description": "An id that will help HipChat recognise the same card when it is sent multiple times",
//            "minLength": 1,
//            "type": "string"
//          },
//          "style": {
//            "description": "Type of the card",
//            "enum": [
//              "file",
//              "image",
//              "application",
//              "link",
//              "media"
//            ],
//            "maxLength": 16,
//            "minLength": 1,
//            "type": "string"
//          },
//          "thumbnail": {
//            "additionalProperties": true,
//            "optional": true,
//            "properties": {
//              "height": {
//                "description": "The original height of the image",
//                "optional": true,
//                "type": "number"
//              },
//              "url": {
//                "description": "The thumbnail url",
//                "maxLength": 250,
//                "minLength": 1,
//                "type": "string"
//              },
//              "url@2x": {
//                "description": "The thumbnail url in retina",
//                "maxLength": 250,
//                "minLength": 1,
//                "optional": true,
//                "type": "string"
//              },
//              "width": {
//                "description": "The original width of the image",
//                "optional": true,
//                "type": "number"
//              }
//            },
//            "type": "object"
//          },
//          "title": {
//            "description": "The title of the card",
//            "maxLength": 500,
//            "minLength": 1,
//            "type": "string"
//          },
//          "url": {
//            "description": "The url where the card will open",
//            "minLength": 1,
//            "optional": true,
//            "type": "string"
//          }
//        },
//        "type": "object"
//      }
//    ],
//    "field": "card",
//    "message": "Value {u'style': u'application', u'description': u'This is an excption', u'title': u'Test Trace', u'url': u'https://www.hipchat.com', u'format': u'medium', u'thumbnail': {u'url': u'https://cdn3.iconfinder.com/data/icons/trees-volume-1/72/43-256.png', u'width': u'43', u'height': u'64'}, u'attributes': [{u'value': {u'style': u'lozenge', u'label': u'2017-03-07T23:17:43.7180085-05:00'}, u'label': u'TimeStamp'}, {u'value': {u'style': u'lozenge', u'label': u'Fatal'}, u'label': u'Level'}, {u'value': {u'style': u'lozenge', u'label': u'd7bda646-94c2-4592-8f98-218528198706'}, u'label': u'Id'}, {u'value': {u'style': u'lozenge', u'label': u'value1'}, u'label': u'key1'}], u'id': u'514a3468-ff0e-40bc-8eee-6710cd4c2cc6', u'icon': {u'url': u'https://cdn3.iconfinder.com/data/icons/trees-volume-1/72/43-128.png'}} for field 'card' is not of type [{'type': 'null'}, {'additionalProperties': True, 'type': 'object', 'properties': {u'style': {'description': 'Type of the card', 'minLength': 1, 'enum': ['file', 'image', 'application', 'link', 'media'], 'maxLength': 16, 'type': 'string'}, u'description': {'optional': True, 'type': [{'minLength': 0, 'type': 'string', 'maxLength': 500}, {'additionalProperties': False, 'type': 'object', 'properties': {u'value': {'minLength': 1, 'type': 'string', 'description': 'The description in the specific format', 'maxLength': 1000}, u'format': {'minLength': 1, 'enum': ['html', 'text'], 'type': 'string', 'description': 'The format that can be html or text'}}}]}, u'format': {'description': 'Application cards can be compact (1 to 2 lines) or medium (1 to 5 lines)', 'minLength': 1, 'enum': ['compact', 'medium'], 'optional': True, 'maxLength': 25, 'type': 'string'}, u'url': {'minLength': 1, 'type': 'string', 'description': 'The url where the card will open', 'optional': True}, u'title': {'minLength': 1, 'type': 'string', 'description': 'The title of the card', 'maxLength': 500}, u'thumbnail': {'additionalProperties': True, 'optional': True, 'type': 'object', 'properties': {u'url': {'minLength': 1, 'type': 'string', 'description': 'The thumbnail url', 'maxLength': 250}, u'width': {'optional': True, 'type': 'number', 'description': 'The original width of the image'}, 'url@2x': {'description': 'The thumbnail url in retina', 'minLength': 1, 'optional': True, 'maxLength': 250, 'type': 'string'}, u'height': {'optional': True, 'type': 'number', 'description': 'The original height of the image'}}}, u'activity': {'description': 'The activity will generate a collapsable card of one line showing the html\\nand the ability to maximize to see  all the content.', 'type': 'object', 'additionalProperties': False, 'optional': True, 'properties': {u'html': {'minLength': 1, 'type': 'string', 'description': 'Html for the activity to show in one line a summary of the action that happened'}, u'icon': {'optional': True, 'type': [{'type': 'string'}, {'additionalProperties': True, 'type': 'object', 'properties': {u'url': {'minLength': 1, 'type': 'string', 'description': 'The url where the icon is'}, 'url@2x': {'minLength': 1, 'type': 'string', 'description': 'The url for the icon in retina', 'optional': True}}}]}}}, u'attributes': {'description': 'List of attributes to show below the card. Sample {label}:{value.icon} {value.label}', 'items': {'additionalProperties': False, 'type': 'object', 'properties': {u'value': {'additionalProperties': False, 'type': 'object', 'properties': {u'url': {'minLength': 1, 'type': 'string', 'description': 'Url to be opened when a user clicks on the label', 'optional': True}, u'style': {'description': 'AUI Integrations for now supporting only lozenges', 'minLength': 1, 'enum': ['lozenge-success', 'lozenge-error', 'lozenge-current', 'lozenge-complete', 'lozenge-moved', 'lozenge'], 'optional': True, 'type': 'string'}, u'label': {'minLength': 1, 'type': 'string', 'description': 'The text representation of the value'}, u'icon': {'optional': True, 'type': [{'type': 'string'}, {'additionalProperties': True, 'type': 'object', 'properties': {u'url': {'minLength': 1, 'type': 'string', 'description': 'The url where the icon is'}, 'url@2x': {'minLength': 1, 'type': 'string', 'description': 'The url for the icon in retina', 'optional': True}}}]}}}, u'label': {'minLength': 1, 'type': 'string', 'optional': True, 'maxLength': 50}}}, 'optional': True, 'maxLength': 10, 'type': 'array', 'minLength': 1}, u'id': {'minLength': 1, 'type': 'string', 'description': 'An id that will help HipChat recognise the same card when it is sent multiple times'}, u'icon': {'optional': True, 'type': [{'type': 'string'}, {'additionalProperties': True, 'type': 'object', 'properties': {u'url': {'minLength': 1, 'type': 'string', 'description': 'The url where the icon is'}, 'url@2x': {'minLength': 1, 'type': 'string', 'description': 'The url for the icon in retina', 'optional': True}}}]}}}]",
//    "type": "Bad Request",
//    "validation": "type",
//    "value": {
//      "attributes": [
//        {
//          "label": "TimeStamp",
//          "value": {
//            "label": "2017-03-07T23:17:43.7180085-05:00",
//            "style": "lozenge"
//          }
//        },
//        {
//          "label": "Level",
//          "value": {
//            "label": "Fatal",
//            "style": "lozenge"
//          }
//        },
//        {
//          "label": "Id",
//          "value": {
//            "label": "d7bda646-94c2-4592-8f98-218528198706",
//            "style": "lozenge"
//          }
//        },
//        {
//          "label": "key1",
//          "value": {
//            "label": "value1",
//            "style": "lozenge"
//          }
//        }
//      ],
//      "description": "This is an excption",
//      "format": "medium",
//      "icon": {
//        "url": "https://cdn3.iconfinder.com/data/icons/trees-volume-1/72/43-128.png"
//      },
//      "id": "514a3468-ff0e-40bc-8eee-6710cd4c2cc6",
//      "style": "application",
//      "thumbnail": {
//        "height": "64",
//        "url": "https://cdn3.iconfinder.com/data/icons/trees-volume-1/72/43-256.png",
//        "width": "43"
//      },
//      "title": "Test Trace",
//      "url": "https://www.hipchat.com"
//    }
//  }
//}