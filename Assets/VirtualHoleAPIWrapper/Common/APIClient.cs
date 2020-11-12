using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using BestHTTP;

namespace VirtualHole.APIWrapper
{
	public abstract class APIClient
	{
		public string domain { get; private set; } = string.Empty;
		public virtual string version { get { return "api"; } }
		public abstract string path { get; }

		public APIClient(string domain)
		{
			this.domain = domain;
		}

		public async Task<T> PostAsync<T>(
			Uri uri, object body,
			CancellationToken cancellationToken = default)
		{
			HTTPRequest request = new HTTPRequest(uri, HTTPMethods.Post);

			string bodyAsJson = JsonConvert.SerializeObject(body, JsonUtilities.DefaultSettings);
			request.RawData = Encoding.UTF8.GetBytes(bodyAsJson);

			HTTPRequestAsyncHandler requestAsync = new HTTPRequestAsyncHandler(request);

			T result = default;

			HTTPResponse response = await requestAsync.SendAsync(cancellationToken);
			if(response == null) {
				throw request.Exception;
			} else {
				if(request.State == HTTPRequestStates.Finished) {
					result = JsonConvert.DeserializeObject<T>(response.DataAsText, JsonUtilities.DefaultSettings);
					return result;
				}
			}

			return result;
		}

		public async Task<T> GetAsync<T>(
			Uri uri,
			CancellationToken cancellationToken = default)
		{
			HTTPRequest request = new HTTPRequest(uri, HTTPMethods.Get);

			HTTPRequestAsyncHandler requestAsync = new HTTPRequestAsyncHandler(request);

			T result = default;

			HTTPResponse response = await requestAsync.SendAsync(cancellationToken);
			if(response == null) {
				throw request.Exception;
			} else {
				if(request.State == HTTPRequestStates.Finished) {
					result = JsonConvert.DeserializeObject<T>(response.DataAsText, JsonUtilities.DefaultSettings);
					return result;
				}
			}

			return result;
		}

		protected Uri CreateUri(string slug)
		{
			return UriUtilities.CreateUri(
				new Uri(new Uri(domain), version).AbsoluteUri, 
				path, slug);
		}
	}
}
