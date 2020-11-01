using VirtualHole.Client.ComponentMaps;
using VirtualHole.Client.Data;
using VirtualHole.Client.UI;
using Midnight.FlowTree;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace VirtualHole.Client.Controllers
{
	public class SupportPageController : MonoBehaviour
	{
		[SerializeField]
		private VirtualHoleStorageClientObject _client = null;
		
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
			SupportListQuery supportListQuery = new SupportListQuery(_client.client);
			_supportInfos = await supportListQuery.LoadAsync(cancellationToken);

			IEnumerable<InfoButtonData> data = await UIFactory.CreateInfoButtonDataAsync(supportListQuery, cancellationToken);
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
