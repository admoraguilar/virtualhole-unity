using System.Collections.Generic;
using Midnight;

namespace VirtualHole.Client.Data
{
	public class UserPersonalizationV1 : IDataVersion
	{
		public int dataVersion => 1;

		public List<string> followedCreatorUniversalIds = new List<string>();

		public IDataVersion ToNext() => null;

		public IDataVersion ToPrev() => null;
	}
}
