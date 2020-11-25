using Midnight.Unity;

namespace VirtualHole.Client.Data
{
	public class Selection : Singleton<Selection>
	{
		public static Selection instance => _instance;

		public CreatorDTO creatorDTO = null;
	}
}
