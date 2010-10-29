using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Web;
using System.Xml.Linq;
using RPXLib.Interfaces;

namespace RPXLib
{
	public class RPXApiWrapper : IRPXApiWrapper
	{
		private readonly string apiKey;
		private readonly string baseUrl;
		private readonly IWebProxy webProxy;

		//this is for my testing purposes only
		public RPXApiWrapper(IRPXApiSettings settings)
		{
			var url = settings.ApiBaseUrl;
			if (!url.EndsWith(@"/"))
				url = url + "/";

			baseUrl = url;
			apiKey = settings.ApiKey;
			webProxy = settings.WebProxy;
		}

		public string BaseUrl
		{
			get { return baseUrl; }
		}

		public string ApiKey
		{
			get { return apiKey; }
		}

		#region IRPXApiWrapper Members

		public XElement Call(string methodName, IDictionary<string, string> queryData)
		{
			var postData = GeneratePostData(queryData);
			var requestUri = new Uri(BaseUrl + methodName + "?" + postData);

			var request = BuildApiWebRequest(requestUri, postData);

			using (var response = (HttpWebResponse) request.GetResponse())
			using (var dataStream = response.GetResponseStream())
			using (var responseReader = new StreamReader(dataStream))
			{
				return RPXApiResponseParser.Parse(responseReader);
			}
		}

		#endregion

		private HttpWebRequest BuildApiWebRequest(Uri requestUri, string postData)
		{
			var apiWebRequest = (HttpWebRequest) WebRequest.Create(requestUri);

			if (webProxy != null)
				apiWebRequest.Proxy = webProxy;

			return apiWebRequest;
		}

		private string GeneratePostData(IDictionary<string, string> partialQuery)
		{
			IDictionary<string, string> query = new Dictionary<string, string>(partialQuery);
			query.Add("format", "xml");
			query.Add("apiKey", ApiKey);

			var sb = new StringBuilder();
			foreach (var e in query)
			{
				if (sb.Length > 0)
				{
					sb.Append('&');
				}

				sb.Append(HttpUtility.UrlEncode(e.Key, Encoding.UTF8));
				sb.Append('=');
				sb.Append(HttpUtility.UrlEncode(e.Value, Encoding.UTF8));
			}
			return sb.ToString();
		}
	}
}