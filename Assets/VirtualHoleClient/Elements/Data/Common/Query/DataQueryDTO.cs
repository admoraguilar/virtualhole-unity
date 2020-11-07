
namespace VirtualHole.Client.Data
{
	public class DataQueryDTO<TRaw>
	{
		public TRaw raw;

		public DataQueryDTO(TRaw raw)
		{
			this.raw = raw;
		}
	}
}
