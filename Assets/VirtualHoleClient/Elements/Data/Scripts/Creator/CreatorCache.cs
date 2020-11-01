﻿using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using Midnight.Web;

namespace VirtualHole.Client.Data
{
	using Api.DB.Contents.Creators;

	public static class CreatorCache
	{
		public static Creator selectedCreator = null;

		private static Dictionary<string, Creator> _creatorLookup = new Dictionary<string, Creator>();
		private static Dictionary<string, Sprite> _avatarLookup = new Dictionary<string, Sprite>();

		public static async Task<Sprite> GetAvatarAsync(
			Creator creator, CancellationToken cancellationToken = default)
		{
			return await GetAvatarAsync(
				creator.universalId, creator.avatarUrl, 
				cancellationToken);
		}

		public static async Task<Sprite> GetAvatarAsync(
			string universalId, string url, 
			CancellationToken cancellationToken = default)
		{
			if(!_avatarLookup.TryGetValue(universalId, out Sprite avatar)) {
				_avatarLookup[universalId] =
					avatar = await ImageGetWebRequest.GetAsync(
						url, null,
						cancellationToken);
			}

			return avatar;
		}

		public static Creator Get(string universalId)
		{
			_creatorLookup.TryGetValue(universalId, out Creator creator);
			return creator;
		}

		public static void Add(IEnumerable<Creator> creators)
		{
			if(creators == null) { return; }
			foreach(Creator creator in creators) { Add(creator); }
		}

		public static void Add(Creator creator)
		{
			Assert.IsNotNull(creator);
			_creatorLookup[creator.universalId] = creator;
		}
	}
}
