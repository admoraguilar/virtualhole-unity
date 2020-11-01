using System.Collections.Generic;
using Midnight;

namespace VirtualHole.Client.Data
{
	using Api.DB.Contents.Creators;

	public class UserProfile : IDataVersion
	{
		public int dataVersion => 1;

		public List<Creator> followedCreators = new List<Creator>();

		public IDataVersion GetNext() => null;
		public IDataVersion GetPrev() => null;
	}
}