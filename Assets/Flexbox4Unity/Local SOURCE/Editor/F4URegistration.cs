using System;
using System.Collections;
using System.Collections.Generic;
using Flexbox4Unity;
using UnityEditor;
using UnityEngine;

namespace Flexbox4Unity
{
	public class F4URegistration
	{
		public static void SendSupportEmail()
		{
			var settings = EditorProjectSettings.requireProjectSettings;
				
			string mailtoString = "mailto:";
			string supportEmail = "adam.m.s.martin@gmail.com";
#if LITE_VERSION
		string subject = "Support Issue (CSS Flexbox LITE)";
#else
			string subject = "Support Issue (Flexbox4Unity)";
#endif
			string body = "(Write your mesage here)\n\n";

			body += "--- do not alter anything below this line ---\n";
#if LITE_VERSION
#else
			string invoiceNumber = GetRegisteredInvoiceNumber();
			if( invoiceNumber != null )
				body += "(Invoice: " + invoiceNumber+")\n";
#endif
			body += "Flexbox4 version:" + Flexbox4UnityProjectSettings.builtVersion + "\n";
			body += "Unity build version: " + Flexbox4UnityProjectSettings.builtForUnityVersion + "\n";
			body += "Current Editor version: " + Application.unityVersion + "\n";

			string subjectLine = "?subject=" + subject.Replace(" ", "%20");
			string bodyLine = "&body=" + body.Replace(" ", "%20").Replace("\n", "%0D%0A");
			mailtoString = mailtoString + supportEmail + subjectLine + bodyLine;
			Debug.Log("Sending email with mailto = " + mailtoString);
			Application.OpenURL(mailtoString);

			_LocalSetRegistered();
#if LITE_VERSION
#else
			_LocalSetFlexboxInvoiceNumber_Full(invoiceNumber);
#endif
		}

#if LITE_VERSION
public static void RegisterViaEmail()
#else
		public static void RegisterViaEmail(string invoiceNumber)
#endif
		{
			var settings = EditorProjectSettings.requireProjectSettings;
			
			string mailtoString = "mailto:";
			string supportEmail = "adam.m.s.martin@gmail.com";
#if LITE_VERSION
		string subject = "Registering CSS-Flexbox (LITE)";
#else
			string subject = "Registering Flexbox4Unity";
#endif
			string body = "";
			body += "Auto-registration\n\n";
#if LITE_VERSION
#else
			body += "Invoice number: " + invoiceNumber;
#endif
			body += "--- do not alter anything below this line ---\n";
			body += "Flexbox4 version:" + Flexbox4UnityProjectSettings.builtVersion + "\n";
			body += "Unity build version: " + Flexbox4UnityProjectSettings.builtForUnityVersion + "\n";
			body += "Current Editor version: " + Application.unityVersion + "\n";

			string subjectLine = "?subject=" + subject.Replace(" ", "%20");
			string bodyLine = "&body=" + body.Replace(" ", "%20").Replace("\n", "%0D%0A");
			mailtoString = mailtoString + supportEmail + subjectLine + bodyLine;
			Debug.Log("Registering with mailto = " + mailtoString);
			Application.OpenURL(mailtoString);

			_LocalSetRegistered();
#if LITE_VERSION
#else
			_LocalSetFlexboxInvoiceNumber_Full(invoiceNumber);
#endif
		}

		public static bool IsRegistered()
		{
			return EditorPrefs.GetBool(Flexbox4UnityProjectSettings.EditorPrefsKeyPrefix + "." + "IsRegistered");
		}

		public static string GetRegisteredInvoiceNumber()
		{
			if( IsRegistered() )
				return EditorPrefs.GetString(Flexbox4UnityProjectSettings.EditorPrefsKeyPrefix + "." + "Flexbox4UnityInvoiceNumber");
			else
				return null;
		}

		private static void _LocalSetRegistered()
		{
			EditorPrefs.SetBool( Flexbox4UnityProjectSettings.EditorPrefsKeyPrefix+ "." + "IsRegistered", true);
		}

		private static void _LocalSetFlexboxInvoiceNumber_Full(string invoiceNumber)
		{
			EditorPrefs.SetString( Flexbox4UnityProjectSettings.EditorPrefsKeyPrefix+ "." + "Flexbox4UnityInvoiceNumber", invoiceNumber);
		}
	}
}