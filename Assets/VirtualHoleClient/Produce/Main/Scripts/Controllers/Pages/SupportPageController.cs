using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;
using Midnight.FlowTree;

namespace VirtualHole.Client.Controllers
{
	using Client.UI;
	using Client.Data;
	using Client.ComponentMaps;

	public class SupportPageController : MonoBehaviour
	{
		[SerializeField]
		private VirtualHoleStorageClientObject _client = null;
		
		private Node _supportNode => _mainFlowMap.supportNode;
		[SerializeField]
		private MainFlowMap _mainFlowMap = null;

		private SupportView _supportView => _supportFlowMap.supportView;
		[SerializeField]
		private SupportFlowMap _supportFlowMap = null;

		private async Task SupportViewDataFactoryAsync(CancellationToken cancellationToken = default)
		{
			SupportListData supportListData = new SupportListData(_client.GetClient());
			IEnumerable<InfoButtonData> data = await UIFactory.CreateInfoButtonDataAsync(supportListData, cancellationToken);
			_supportView.infoButtonData = data;
		}

		private async void OnSupportVisit()
		{
			_supportView.SetDataAsyncFactory(SupportViewDataFactoryAsync);
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
