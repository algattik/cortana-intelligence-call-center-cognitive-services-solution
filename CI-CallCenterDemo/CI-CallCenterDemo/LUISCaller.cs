using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Web;
using System.Net;
using System.Net.Http;
using System.Media;
using System.Threading.Tasks;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Configuration;

namespace ContosoInsurance_CallCenterDemo
{
    [DataContract]
    public class LUISResponse
    {
        [DataMember]
        public string query { get; set; }
        [DataMember]
        public intentobj[] intents { get; set; }
        [DataMember]
        public entityobj[] entities { get; set; }
    }
    [DataContract]
    public class intentobj
    {
        [DataMember]
        public string intent { get; set; }
        [DataMember]
        public decimal score { get; set; }
    }
    [DataContract]
    public class entityobj
    {
        [DataMember]
        public string entity { get; set; }
        [DataMember]
        public string type { get; set; }
        [DataMember]
        public int startIndex { get; set; }
        [DataMember]
        public int endIndex { get; set; }
        [DataMember]
        public decimal score { get; set; }
    }
    /// <summary>
    /// This class demonstrates how to get a valid O-auth token
    /// </summary>
    public class LUISCaller
    {
        public static readonly string AccessUri = "https://api.projectoxford.ai/luis/v1/application?";
        private string AppId = ConfigurationManager.AppSettings["luisAppID"];
        private string AppIdCh = ConfigurationManager.AppSettings["luisAppIDChinese"];
        private string luisAPIAccountKey = ConfigurationManager.AppSettings["luisAPIAccountKey"];
        private string request;
        private string _recoLanguage;

        public LUISCaller(string recoLanguage)
        {
            _recoLanguage = recoLanguage;
        }
        public LUISResponse Call(string message)
        {
            // If clientid or client secret has special characters, encode before sending request 
            if (this._recoLanguage == "zh-CN")
                this.request = string.Format("id={0}&subscription-key={1}&q={2}",
                              HttpUtility.UrlEncode(AppIdCh),
                              HttpUtility.UrlEncode(luisAPIAccountKey),
                              HttpUtility.UrlEncode(message.Replace("?", "")));
            else
                this.request = string.Format("id={0}&subscription-key={1}&q={2}",
                                          HttpUtility.UrlEncode(AppId),
                                          HttpUtility.UrlEncode(luisAPIAccountKey),
                                          HttpUtility.UrlEncode(message.Replace("?", "")));
            Console.WriteLine(this.request);
            LUISResponse token = HttpGet(AccessUri, this.request);
            return token;
        }
        private LUISResponse HttpGet(string accessUri, string requestDetails)
        {
            //Prepare OAuth request 
            WebRequest webRequest = WebRequest.Create(string.Concat(accessUri, requestDetails));

            webRequest.Method = "GET";
            using (WebResponse webResponse = webRequest.GetResponse())
            {
                DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(LUISResponse));
                LUISResponse token = (LUISResponse)serializer.ReadObject(webResponse.GetResponseStream());
                return token;
            }
        }
    }
}
