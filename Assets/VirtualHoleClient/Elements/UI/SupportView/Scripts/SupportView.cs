using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace VirtualHole.Client.UI
{
	using Client.Data;

	public class SupportView : UILifecycle
	{
		public Action<InfoButtonData> OnInfoButtonDataCreated = delegate { };

		public SupportListQuery query { get; set; } = null;

		public InfoButton infoButtonPrefab => _infoButtonPrefab;
		[SerializeField]
		private InfoButton _infoButtonPrefab = null;

		public RectTransform contentContainer => _contentContainer;
		[SerializeField]
		private RectTransform _contentContainer = null;

		private List<InfoButton> _instances = new List<InfoButton>();

		protected override async Task InitializeAsync_Impl(CancellationToken cancellationToken = default)
		{
			IEnumerable<InfoButtonData> buttonData = await UIFactory.CreateInfoButtonDataAsync(query, cancellationToken);
			foreach(InfoButtonData data in buttonData) { OnInfoButtonDataCreated(data); }

			foreach(InfoButtonData data in buttonData) {
				InfoButton button = Instantiate(_infoButtonPrefab, _contentContainer, false);
				button.name = data.header;

				button.button.onClick.RemoveAllListeners();
				button.button.onClick.AddListener(() => data.onClick());

				button.image.sprite = data.image;
				button.headerText.text = data.header;
				button.contentText.text = data.content;

				_instances.Add(button);
			}
		}

		protected override async Task UnloadAsync_Impl()
		{
			await Task.CompletedTask;
			foreach(InfoButton instance in _instances) {
				Destroy(instance.gameObject);
			}
			_instances.Clear();
		}
	}
}
