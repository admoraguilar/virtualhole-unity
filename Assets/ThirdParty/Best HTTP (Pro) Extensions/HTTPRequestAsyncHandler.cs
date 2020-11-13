﻿using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics;
using Midnight;

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

			string message = 
				$"[HTTP]" +
				$"Start Request: {_request.Uri.AbsoluteUri} {Environment.NewLine}";
			if(_request.RawData != null) {
				message += $"Request Body: {Encoding.UTF8.GetString(_request.RawData)} {Environment.NewLine}";
			}
			MLog.Log(message);

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
				MLog.Log(
					$"[HTTP]" +
					$"End Request Result: ({_stopwatch.Elapsed.TotalSeconds:0.00}s | " +
						$"{ConvertSizeByteToKilobyte(req.Downloaded):0.00}kb) " +
							$"({req.Uri.AbsoluteUri}) {Environment.NewLine}");

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
