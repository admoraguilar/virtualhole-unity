using UnityEngine.Networking;

namespace Midnight
{
	public static class UnityWebRequestAsyncOperationtExtensions
	{
		public static UnityWebRequestAwaiter GetAwaiter(this UnityWebRequestAsyncOperation asyncOp)
		{
			return new UnityWebRequestAwaiter(asyncOp);
		}
	}
}
