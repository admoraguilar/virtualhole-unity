using UnityEngine;

namespace BestHTTP
{
	public class HTTPManagerInitializer
	{
		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
		private static void InitializeOnLoad()
		{
			HTTPManager.Setup();
		}
	}
}
