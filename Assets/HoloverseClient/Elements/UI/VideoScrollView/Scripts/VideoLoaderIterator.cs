using System;
using System.IO;
using System.Collections;
using System.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;
using Midnight;
using Midnight.Web;
using Midnight.Concurrency;
using Newtonsoft.Json;
using Holoverse.Backend.YouTube;

namespace Holoverse.Client
{
	public class VideoLoaderIterator : IList<VideoInfo>, IDisposable
	{
		private static string _debugPrepend => $"[{nameof(VideoLoaderIterator)}]";

		private List<VideoInfo> _videoInfos = new List<VideoInfo>();
		private List<VideoInfo> _subset = new List<VideoInfo>();

		private MemoryStream _memoryStream = null;
		private FileStream _fileStream = null;
		private StreamReader _streamReader = null;
		private JsonReader _jsonReader = null;

		private JsonSerializer _jsonSerializer = null;
		private string _url = string.Empty;
		private bool _isInit = false;

		public VideoLoaderIterator(string url)
		{
			_url = url;
		}

		public VideoInfo this[int index]
		{
			get => _videoInfos[index];
			set => _videoInfos[index] = value;
		}

		public int Count => _videoInfos.Count;

		public bool IsReadOnly => ((ICollection<VideoInfo>)_videoInfos).IsReadOnly;

		public void Dispose()
		{
			_jsonReader.Close();
			((IDisposable)_jsonReader).Dispose();

			_streamReader.Close();
			_streamReader.Dispose();

			if(_memoryStream != null) {
				_memoryStream.Close();
				_memoryStream.Dispose();
			}

			if(_url != null) {
				_fileStream.Close();
				_fileStream.Dispose();
			}
		}

		public async Task<IEnumerable<VideoInfo>> LoadAsync(int amount)
		{
			if(!_isInit) {
				_isInit = true;

				Stream stream = null;
				bool isLocal = Path.GetPathRoot(_url) != string.Empty ? true : false;

				if(isLocal && Application.platform != RuntimePlatform.Android) {
					stream = _fileStream = new FileStream(_url, FileMode.Open);
				} else {
					GenericGetWebRequest request = new GenericGetWebRequest();
					TaskExt.FireForget(request.SendAsync(_url));

					MLog.Log($"{_debugPrepend} Iterator started: {request.request.downloadProgress}");
					while(request.request.downloadProgress < .1f) {
						MLog.Log($"{_debugPrepend} Downloading at {request.request.downloadProgress}...");
						await Task.Yield();
					}
					MLog.Log($"{_debugPrepend} Now streaming!");

					byte[] data = request.request.downloadHandler.data;
					stream = _memoryStream = new MemoryStream(data);
				}

				_streamReader = new StreamReader(stream);
				_jsonReader = new JsonTextReader(_streamReader);
				_jsonSerializer = new JsonSerializer();
			}

			_subset.Clear();
			while(_jsonReader.Read()) {
				if(_subset.Count >= amount) { break; }

				if(_jsonReader.TokenType == JsonToken.StartObject) {
					VideoInfo video = _jsonSerializer.Deserialize<VideoInfo>(_jsonReader);
					if(video != null) { _subset.Add(video); }
				}
			}

			_videoInfos.AddRange(_subset);
			return _subset;
		}

		public void Add(VideoInfo item)
		{
			_videoInfos.Add(item);
		}

		public void Clear()
		{
			_videoInfos.Clear();
		}

		public bool Contains(VideoInfo item)
		{
			return _videoInfos.Contains(item);
		}

		public void CopyTo(VideoInfo[] array, int arrayIndex)
		{
			_videoInfos.CopyTo(array, arrayIndex);
		}

		public IEnumerator<VideoInfo> GetEnumerator()
		{
			return _videoInfos.GetEnumerator();
		}

		public int IndexOf(VideoInfo item)
		{
			return _videoInfos.IndexOf(item);
		}

		public void Insert(int index, VideoInfo item)
		{
			_videoInfos.Insert(index, item);
		}

		public bool Remove(VideoInfo item)
		{
			return _videoInfos.Remove(item);
		}

		public void RemoveAt(int index)
		{
			_videoInfos.RemoveAt(index);
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return _videoInfos.GetEnumerator();
		}
	}
}