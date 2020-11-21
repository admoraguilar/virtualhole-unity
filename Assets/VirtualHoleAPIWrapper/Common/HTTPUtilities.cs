using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Collections;
using UnityEngine;
using BestHTTP;
using StbImageSharp;
using SkiaSharp;
using Midnight;
using Midnight.Coroutines;

namespace VirtualHole.APIWrapper
{
	public static class HTTPUtilities
	{
		public static async Task<Sprite> GetImageAsync(string url, CancellationToken cancellationToken = default)
		{
			HTTPRequest request = null;
			HTTPResponse response = null;

			byte[] resizeImage = null;
			ImageResult finalImage = null;

			using(StopwatchScope s = new StopwatchScope(
				nameof(HTTPUtilities),
				$"Send: {url}",
				$"Receive: {url}")) {
				await Task.Run(async () => {
					request = new HTTPRequest(new Uri(url));
					response = await request.GetHTTPResponseAsync(cancellationToken);

					using(SKBitmap skBitmap = SKBitmap.Decode(response.Data)) {
						int width = (int)(skBitmap.Width * .4f);
						int height = (int)(skBitmap.Height * .4f);

						using(SKBitmap scaledBitmap = skBitmap.Resize(
							new SKImageInfo(width, height), SKFilterQuality.Medium)) {
							using(SKImage scaledImage = SKImage.FromBitmap(scaledBitmap)) {
								using(SKData imageData = scaledImage.Encode()) {
									resizeImage = imageData.ToArray();
								}
							}
						}
					}

				}, cancellationToken);
			}

			Sprite result = null;
			if(response != null) {
				if(request.State == HTTPRequestStates.Finished) {
					await Task.Run(() => {
						finalImage = ImageResult.FromMemory(resizeImage, ColorComponents.RedGreenBlueAlpha, true);
					}, cancellationToken);

					CoroutineJob job = new CoroutineJob() {
						coroutineFactory = CreateTexture,
						isDoneFactory = () => result != null
					};

					await job.ScheduleOnMainThreadAsync(cancellationToken);
				}
			} else {
				throw request.Exception;
			}

			return result;
		
			IEnumerator CreateTexture()
			{
				// Slice across several frames, byte[] to Texture2D operations as they are expensive.
				Texture2D tex2D = new Texture2D(finalImage.Width, finalImage.Height, TextureFormat.RGBA32, false);
				yield return null;

				tex2D.LoadRawTextureData(finalImage.Data);
				tex2D.Apply();

				result = Sprite.Create(
					tex2D, new Rect(Vector2.zero, new Vector2(tex2D.width, tex2D.height)),
					Vector2.one * .5f, 100f,
					0, SpriteMeshType.FullRect);
				yield break;
			}
		}
	}
}
