using System.Threading.Tasks;

namespace VirtualHole.Client.Data
{
	public class UserDataClient
	{
		public string idToken { get; private set; } = string.Empty;

		public string localRootPath { get; private set; } = string.Empty;
		public string localSubPath { get; private set; } = string.Empty;

		public UserPersonalizationClient personalization { get; private set; } = null;

		public UserDataClient(
			string idToken, string localRootPath, 
			string localSubPath)
		{
			this.idToken = idToken;
			this.localRootPath = localRootPath;
			this.localSubPath = localSubPath;

			personalization = new UserPersonalizationClient(this);
		}

		public async Task LoadAsync()
		{
			await Task.CompletedTask;
			await personalization.GetAsync();
		}

		public async Task SaveAsync()
		{
			await Task.CompletedTask;
			await personalization.UpsertAsync();
		}
	}
}
