using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace VirtualHole.Client.UI
{
	public class SupportView : UILifecycle
	{
		public IEnumerable<InfoButtonData> infoButtonData { get; set; } = null;

		public InfoButton infoButtonPrefab => _infoButtonPrefab;
		[SerializeField]
		private InfoButton _infoButtonPrefab = null;

		public RectTransform contentContainer => _contentContainer;
		[SerializeField]
		private RectTransform _contentContainer = null;

		private List<InfoButton> _instances = new List<InfoButton>();

		protected override async Task InitializeAsync_Impl(CancellationToken cancellationToken = default)
		{
			await Task.CompletedTask;
			foreach(InfoButtonData data in infoButtonData) {
				InfoButton button = Instantiate(_infoButtonPrefab, _contentContainer, false);
				button.name = data.header;
				button.image.sprite = data.sprite;
				button.headerText.text = data.header;
				button.contentText.text = data.content;
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
