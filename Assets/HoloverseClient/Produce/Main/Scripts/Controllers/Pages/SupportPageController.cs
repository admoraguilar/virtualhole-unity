using Holoverse.Client.ComponentMaps;
using Holoverse.Client.Data;
using Holoverse.Client.UI;
using Midnight.FlowTree;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Holoverse.Client.Controllers
{
	public class SupportPageController : MonoBehaviour
	{
		[SerializeField]
		private string _cdnUrl = string.Empty;

		[SerializeField]
		private SupportInfo[] _supportInfos = null;

		private Node _supportNode => _mainFlowMap.supportNode;
		[SerializeField]
		private MainFlowMap _mainFlowMap = null;

		private SupportView _supportView => _supportFlowMap.supportView;
		[SerializeField]
		private SupportFlowMap _supportFlowMap = null;

		private async Task SupportViewDataFactoryAsync(CancellationToken cancellationToken = default)
		{
			foreach(SupportInfo supportInfo in _supportInfos) {
				supportInfo.imageUrl = $"{_cdnUrl}{supportInfo.imageUrl}";
			}

			IEnumerable<InfoButtonData> data = await UIFactory.CreateInfoButtonDataAsync(_supportInfos, cancellationToken);
			_supportView.SetData(data);
		}

		private async void OnSupportVisit()
		{
			_supportView.SetData(SupportViewDataFactoryAsync);
			await _supportView.InitializeAsync();
		}

		private void OnEnable()
		{
			_supportNode.OnVisit += OnSupportVisit;
		}

		private void OnDisable()
		{
			_supportNode.OnVisit -= OnSupportVisit;
		}
	}
}
