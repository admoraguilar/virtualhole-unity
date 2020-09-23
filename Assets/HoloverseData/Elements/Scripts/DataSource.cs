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
using Newtonsoft.Json.Linq;

namespace Holoverse.Data
{
	public class DataSource<T> : IList<T>, IDisposable
	{
		private static string _debugPrepend => $"[{nameof(DataSource<T>)}]";

		private List<T> _allList = new List<T>();
		private List<T> _subsetList = new List<T>();

		private MemoryStream _memoryStream = null;
		private FileStream _fileStream = null;
		private StreamReader _streamReader = null;
		private JsonReader _jsonReader = null;

		private JsonSerializer _jsonSerializer = null;
		private string _url = string.Empty;
		private bool _isInit = false;

		public T this[int index]
		{
			get => _allList[index];
			set => _allList[index] = value;
		}

		public int Count => _allList.Count;

		public bool IsReadOnly => ((ICollection<T>)_allList).IsReadOnly;

		public DataSource(string url)
		{
			_url = url;
		}

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

		public async Task<IEnumerable<T>> LoadAsync(int amount)
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

					MLog.Log($"{_debugPrepend} Loading started: {request.request.downloadProgress}");
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

			string subsetJson = "[";
			int subsetJsonObjectsCount = 0;
			while(_jsonReader.Read()) {
				if(subsetJsonObjectsCount > amount) {
					subsetJson = subsetJson.Substring(0, subsetJson.Length - 1);
					subsetJson += "]";
					break;
				}

				if(_jsonReader.TokenType == JsonToken.StartObject) {
					subsetJsonObjectsCount++;
					subsetJson += $"{JObject.Load(_jsonReader)},";
				}
			}

			_subsetList.Clear();

			IEnumerable<T> subsetObjs = JsonConvert.DeserializeObject<T[]>(subsetJson);
			_subsetList.AddRange(subsetObjs);
			_allList.AddRange(subsetObjs);

			return _subsetList;
		}

		public void Add(T item)
		{
			_allList.Add(item);
		}

		public void Clear()
		{
			_allList.Clear();
		}

		public bool Contains(T item)
		{
			return _allList.Contains(item);
		}

		public void CopyTo(T[] array, int arrayIndex)
		{
			_allList.CopyTo(array, arrayIndex);
		}

		public IEnumerator<T> GetEnumerator()
		{
			return _allList.GetEnumerator();
		}

		public int IndexOf(T item)
		{
			return _allList.IndexOf(item);
		}

		public void Insert(int index, T item)
		{
			_allList.Insert(index, item);
		}

		public bool Remove(T item)
		{
			return _allList.Remove(item);
		}

		public void RemoveAt(int index)
		{
			_allList.RemoveAt(index);
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return _allList.GetEnumerator();
		}
	}
}
