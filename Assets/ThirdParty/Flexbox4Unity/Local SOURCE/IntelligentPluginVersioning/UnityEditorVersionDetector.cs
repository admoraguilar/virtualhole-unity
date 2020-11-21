namespace IntelligentPluginVersioning
{
	public class UnityEditorVersionDetector
	{
#if UNITY_2020_1
		public static string unityVersion = "2020.1";
#elif UNITY_2020_2
		public static string unityVersion = "2020.2";
#elif UNITY_2020_3
		public static string unityVersion = "2020.3";
#elif UNITY_2020
		public static string unityVersion = "2020.x";
#elif UNITY_2019_1
		public static string unityVersion = "2019.1";
#elif UNITY_2019_2
		public static string unityVersion = "2019.2";
#elif UNITY_2019_3
		public static string unityVersion = "2019.3";
#elif UNITY_2019_4
		public static string unityVersion = "2019.4";
#elif UNITY_2019
		public static string unityVersion = "2019.x";
#elif UNITY_2018_4
		public static string unityVersion = "2018.4";
#elif UNITY_2018_3
		public static string unityVersion = "2018.3";
#elif UNITY_2018_2
		public static string unityVersion = "2018.2";
#elif UNITY_2018_1
		public static string unityVersion = "2018.1";
#elif UNITY_2018
		public static string unityVersion = "2018.x";
#elif UNITY_2017_5
		public static string unityVersion = "2017.5";
#elif UNITY_2017_4
		public static string unityVersion = "2017.4";
#elif UNITY_2017_3
		public static string unityVersion = "2017.3";
#elif UNITY_2017_2
		public static string unityVersion = "2017.2";
#elif UNITY_2017_1
		public static string unityVersion = "2017.1";
#elif UNITY_2017
		public static string unityVersion = "2017.x";
#elif UNITY_5_6
		public static string unityVersion = "5.6";
#elif UNITY_5_5
		public static string unityVersion = "5.5";
#elif UNITY_5_4
		public static string unityVersion = "5.4";
#elif UNITY_5_3
		public static string unityVersion = "5.3";
#elif UNITY_5_2
		public static string unityVersion = "5.2";
#elif UNITY_5_1
		public static string unityVersion = "5.1";
#elif UNITY_5
		public static string unityVersion = "5.x";
#else
		public static string unityVersion = "(unknown)";
#endif
	}
}