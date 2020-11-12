﻿using System;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics;

namespace BestHTTP
{
	using UDebug = UnityEngine.Debug;

	public class HTTPRequestAsyncHandler
	{
		private HTTPRequest _request = null;

		private Stopwatch _stopwatch = new Stopwatch();

		public HTTPRequestAsyncHandler(HTTPRequest request)
		{
			_request = request;
		}

		public async Task<HTTPResponse> SendAsync(CancellationToken cancellationToken = default)
		{
			HTTPResponse response = null;
			bool isDone = false;

			_request.Callback = OnRequestFinished;

			UDebug.Log(
				$"{nameof(HTTPRequestAsyncHandler)} {Environment.NewLine}" +
				$"Start Requst: {_request.Uri.AbsoluteUri} {Environment.NewLine}");

			_stopwatch.Start();
			_request.Send();

			while(!isDone) {
				if(cancellationToken.IsCancellationRequested) {
					_request.Abort();
				}

				cancellationToken.ThrowIfCancellationRequested();
				await Task.Delay(1);
			}

			return response;

			void OnRequestFinished(HTTPRequest req, HTTPResponse res)
			{
				_stopwatch.Stop();
				UDebug.Log(
					$"{nameof(HTTPRequestAsyncHandler)} {Environment.NewLine}" +
					$"End Request Result: {req.Uri.AbsoluteUri} {Environment.NewLine}" +
					$"Seconds: {_stopwatch.Elapsed.TotalSeconds:0.00}s {Environment.NewLine}" +
					$"Size: {ConvertSizeByteToKilobyte(req.Downloaded):0.00}kb");

				response = res;
				isDone = true;
			}

			double ConvertSizeByteToKilobyte(int bytes)
			{
				return bytes / 1000f;
			}
		}
	}
}
