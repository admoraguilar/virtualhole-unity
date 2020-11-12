using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using BestHTTP;

namespace VirtualHole.APIWrapper
{
	public static class HTTPUtilities
	{
		public static async Task<Sprite> GetImageAsync(string url, CancellationToken cancellationToken = default)
		{
			HTTPRequest request = new HTTPRequest(new Uri(url));
			HTTPRequestAsyncHandler requestAsync = new HTTPRequestAsyncHandler(request);
			HTTPResponse response = await requestAsync.SendAsync(cancellationToken);

			Sprite result = null;
			if(response != null) {
				if(request.State == HTTPRequestStates.Finished) {

					Texture2D tex2D = response.DataAsTexture2D;
					result = Sprite.Create(
						tex2D, new Rect(Vector2.zero, new Vector2(tex2D.width, tex2D.height)),
						Vector2.one * .5f, 100f,
						0, SpriteMeshType.FullRect);
				}
			} else {
				throw request.Exception;
			}

			return result;
		}
	}
}
