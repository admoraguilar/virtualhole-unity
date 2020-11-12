﻿using System;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace VirtualHole.APIWrapper.Contents.Videos
{
	public class VideoClient : APIClient
	{
		public override string path => "Videos";

		public VideoClient(string domain) : base(domain)
		{ }

		public async Task<List<TVideo>> ListVideosAsync<TVideo, TRequest>(
			TRequest request, CancellationToken cancellationToken = default)
			where TVideo : Video
			where TRequest : ListVideosRequest
		{
			if(request is ListCreatorRelatedVideosRequest creatorRelatedVideosRequest) {
				return await ListCreatorRelatedVideosAsync<TVideo>(creatorRelatedVideosRequest, cancellationToken); 
			} else if(request is ListCreatorVideosRequest creatorVideosRequest) {
				return await ListCreatorVideosAsync<TVideo>(creatorVideosRequest, cancellationToken);
			} else {
				throw new NotSupportedException();
			}
		}

		public async Task<List<T>> ListCreatorRelatedVideosAsync<T>(
			ListCreatorRelatedVideosRequest request, CancellationToken cancellationToken = default)
		{
			string slug = "ListCreatorRelatedVideos";
			if(typeof(T) == typeof(Broadcast)) { slug = "ListCreatorRelatedBroadcast"; }
			return await PostAsync<List<T>>(CreateUri(slug), request, cancellationToken);
		}

		public async Task<List<T>> ListCreatorVideosAsync<T>(
			ListCreatorVideosRequest request, CancellationToken cancellationToken = default)
			where T : Video
		{
			string slug = "ListCreatorVideos";
			if(typeof(T) == typeof(Broadcast)) { slug = "ListCreatorBroadcast"; }
			return await PostAsync<List<T>>(CreateUri(slug), request, cancellationToken);
		}
	}
}
