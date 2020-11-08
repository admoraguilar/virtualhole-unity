using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using BestHTTP;
using Midnight;

namespace VirtualHole.Client.Data
{
	public static class QueryUtilities
	{
		public static async Task<Sprite> GetImageAsync(string url, CancellationToken cancellationToken = default)
		{
			using(StopwatchScope s = new StopwatchScope(
				nameof(QueryUtilities),
				$"Start BestHTTP: {url}..",
				$"End BestHTTP: {url}..")) {
				Texture2D tex2D = new Texture2D(1, 1);
				bool isDone = false;

				HTTPRequest request = new HTTPRequest(new Uri(url), OnRequestFinished);
				request.Tag = tex2D;
				request.Send();

				Sprite result = null;
				Exception e = null;
				while(!isDone) {
					cancellationToken.ThrowIfCancellationRequested();
					await Task.Delay(10); 
				}

				if(result != null && e == null) {
					return result;
				} else {
					if(e != null) { throw e; }
					else { throw new Exception("Unknown exception."); }
				}

				void OnRequestFinished(HTTPRequest req, HTTPResponse res)
				{
					switch(req.State) {
						case HTTPRequestStates.Finished:
							tex2D = res.DataAsTexture2D;
							result = Sprite.Create(
								tex2D, new Rect(Vector2.zero, new Vector2(tex2D.width, tex2D.height)),
								Vector2.one * .5f, 100f,
								0, SpriteMeshType.FullRect);
							break;
						default:
							e = req.Exception;
							break;
					}

					isDone = true;
				}
			}
		}
	}
}
