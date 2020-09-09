using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
using Midnight;

namespace Euphoria.Backend
{
	using UDebug = UnityEngine.Debug;

	public static class UnityWebRequestUtilities
	{
		private static string _logPrepend => $"[{typeof(UnityWebRequestUtilities).Name}]";

		public static async Task<Sprite> SendImageRequestAsync(string url, CancellationToken cancellationToken = default)
		{
			if(!Uri.IsWellFormedUriString(url, UriKind.RelativeOrAbsolute)) {
				throw new UriFormatException();
			}

			// TODO:
			// Refactor this to a more robust way of creating APIRequest and APIResponse
			// ala Gamesparks or PlayFab
			Sprite sprite = null;

			using(UnityWebRequest webReq = new UnityWebRequest(url, UnityWebRequest.kHttpVerbGET)) {
				webReq.downloadHandler = new DownloadHandlerTexture();
				webReq.timeout = 240;

				bool isAborted = false;
				CancellationTokenRegistration abort = cancellationToken.Register(() => {
					webReq.Abort();
					isAborted = true;
				});

				UDebug.Log($"{_logPrepend} Send Start: {url}");

				Stopwatch stopwatch = new Stopwatch();
				stopwatch.Start();
				await webReq.SendWebRequest();
				stopwatch.Stop();

				UDebug.Log(
					$"{_logPrepend} Send Result: {url} {Environment.NewLine}" +
					$"Seconds: {stopwatch.Elapsed.TotalSeconds:0.00}s {Environment.NewLine}" +
					$"Size: {ConvertSizeByteToKilobyte(webReq.downloadedBytes):0.00}kb"
				);

				if(webReq.isHttpError || webReq.isNetworkError) {
					// TODO: Handle timeout for different scenarios
					// (e.g)
					// Listing Page: show a request timeout error modal instead of loading a page.
					// Library Page: catch the error and just show the library page and not update it with listing content
					UDebug.LogWarning($"{_logPrepend} {webReq.error}");
				} else {
					Texture2D texture2D = DownloadHandlerTexture.GetContent(webReq);
					sprite = Sprite.Create(
						texture2D, new Rect(Vector2.zero, new Vector2(texture2D.width, texture2D.height)),
						Vector2.one * .5f, 100f);
					sprite.name = url;
				}

				cancellationToken.ThrowIfCancellationRequested();
				abort.Dispose();
			}

			return sprite;
		}

		private static double ConvertSizeByteToKilobyte(ulong bytes)
		{
			return bytes / 1000f;
		}
	}
}
