
namespace VirtualHole.Client.Data
{
	public interface ILocatableAPI
	{
		string domain { get; }
		string path { get; }
		string slug { get; }
	}
}
