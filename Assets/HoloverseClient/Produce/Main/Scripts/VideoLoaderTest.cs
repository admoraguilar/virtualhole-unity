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
using Holoverse.Client.UI;
using Newtonsoft.Json.Linq;

namespace Holoverse.Client
{
	public class VideoLoaderTest : MonoBehaviour
	{
		public class VideoIterator : IList<VideoInfo>, IDisposable
		{
			private List<VideoInfo> _videoInfos = new List<VideoInfo>();
			private List<VideoInfo> _subset = new List<VideoInfo>();
			private JArray _array = new JArray();

			private MemoryStream _memoryStream = null;
			private FileStream _fileStream = null;
			private StreamReader _streamReader = null;
			private JsonReader _jsonReader = null;

			private JsonSerializer _jsonSerializer = null;
			private string _filePath = string.Empty;
			private bool _isInit = false;

			private ulong _videoFile = default;

			public VideoIterator(string filePath)
			{
				_filePath = filePath;
			}

			public VideoInfo this[int index] { 
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

				_fileStream.Close();
				_fileStream.Dispose();
			}

			public async Task<IEnumerable<VideoInfo>> LoadAsync(int amount)
			{
				if(!_isInit) {
					_isInit = true;

					GenericGetWebRequest request = new GenericGetWebRequest();
					TaskExt.FireForget(request.SendAsync(_filePath));

					MLog.Log($"Test: {request.request.downloadProgress}");
					while(request.request.downloadProgress < .1f) {
						MLog.Log($"Downloading at {request.request.downloadProgress}...");
						await Task.Yield();
					}
					MLog.Log("Now streaming!");

					Stream stream = null;
					if(Application.platform != RuntimePlatform.Android) {
						stream = _fileStream = new FileStream(_filePath, FileMode.Open);
					} else {
						byte[] data = request.request.downloadHandler.data;
						stream = _memoryStream = new MemoryStream(data);
					}
					
					_streamReader = new StreamReader(stream);
					_jsonReader = new JsonTextReader(_streamReader);

					_jsonSerializer = new JsonSerializer();
				}

				_subset.Clear();
				_array.Clear();

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

			//private void Load()
			//{
				//byte[] byteArray = Encoding.UTF8.GetBytes(json);
				//using(MemoryStream ms = new MemoryStream(byteArray)) {
				//	using(StreamReader sr = new StreamReader(ms)) {
				//		using(JsonReader reader = new JsonTextReader(sr)) {
				//			JsonSerializer serializer = new JsonSerializer();
				//			int count = 0;

				//			while(reader.Read()) {
				//				if(amount >= 0 && count >= amount) { break; }

				//				if(reader.TokenType == JsonToken.StartObject) {
				//					VideoInfo video = serializer.Deserialize<VideoInfo>(reader);
				//					if(video != null) {
				//						result.Add(video);
				//						count++;
				//					}
				//				}
				//			}
				//		}
				//	}
				//}
			//}

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

		public VideoScrollView videoScrollView = null;
		public int videoAmount = 50;

		private VideoIterator iterator
		{
			get {
				if(_iterator == null) {
					_iterator = new VideoIterator(
						//PathUtilities.CreateDataPath(
						//	"StreamingAssets", "videos.json", false, true
						//)
						$"{Application.streamingAssetsPath}/videos.json"
					);
				}
				return _iterator;
			}
		}
		private VideoIterator _iterator = null;
		private bool _isLoading = false;

		private List<VideoScrollViewCellData> _cellData = new List<VideoScrollViewCellData>();

		private void OnScrollerPositionChanged(float position)
		{
			if(position >= videoScrollView.itemCount - 5) {
				TaskExt.FireForget(LoadVideos());
				//MLog.Log("Near end of line.");
			}
		}

		private async Task LoadVideos()
		{
			if(_isLoading) {
				await Task.CompletedTask;
				return; 
			}
			_isLoading = true;

			MLog.Log($"[{nameof(VideoLoaderTest)}] Loading of videos started");

			foreach(VideoInfo videoInfo in await iterator.LoadAsync(videoAmount)) {
				_cellData.Add(new VideoScrollViewCellData {
					thumbnail = await ImageGetWebRequest.GetAsync(videoInfo.mediumResThumbnailUrl),
					title = videoInfo.title,
					channel = videoInfo.channel,
					onClick = () => Application.OpenURL(videoInfo.url)
				});
				videoScrollView.UpdateData(_cellData);
			}

			_isLoading = false;
		}

		private void Start()
		{
			TaskExt.FireForget(LoadVideos());
		}

		private void OnEnable()
		{
			videoScrollView.OnScrollerPositionChanged += OnScrollerPositionChanged;
		}

		private void OnDisable()
		{
			videoScrollView.OnScrollerPositionChanged -= OnScrollerPositionChanged;
		}
	}
}
