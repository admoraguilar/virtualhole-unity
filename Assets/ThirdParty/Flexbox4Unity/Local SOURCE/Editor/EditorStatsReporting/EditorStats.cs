using System;
using System.Collections;
using System.Collections.Generic;
using EditorStatsReporting;
using EditorStatsReporting.Parameters.ContentInformation;
using EditorStatsReporting.Parameters.ECommerce;
using EditorStatsReporting.Parameters.EventTracking;
using UnityEngine;

/**
 * Simple, anonymous, editor-stats - uses a cutdown version of GoogleAnalytics, with all the detailed
 * tracking stuff removed.
 *
 * AUTOMATICALLY deleted / compiled-out when you make game builds.
 */
public class EditorStats
{
	private static EditorStats _shared;
	public static EditorStats sharedInstance
	{
		get
		{
			_shared = null;
			if( _shared == null )
			{
				_shared = new EditorStats();
			}

			return _shared;
		}
	}

	private static Guid _anonymousSessionID;

	public static void SetSessionID(Guid g)
	{
		_anonymousSessionID = g;
	}
	public static Guid anonymousSessionID
	{
		get
		{
			if( _anonymousSessionID == null ) _anonymousSessionID = Guid.NewGuid();
			return _anonymousSessionID;
		}
	}

	public void SendEvent( string cat, string act, string label, int val )
	{
		#if UNITY_EDITOR
		/** If we're not on WiFi/LAN: discard all data */
		if(Application.internetReachability == NetworkReachability.ReachableViaLocalAreaNetwork)
		try
		{
			//Create a factory based on your tracking id
			var factory = new GoogleAnalyticsRequestFactory("UA-63917454-2");

			//Create a PageView request by specifying request type
			var request = factory.CreateRequest(HitTypes.Event);

			/** four things required here */
			request.Parameters.Add(new EventCategory(cat));
			request.Parameters.Add(new EventAction(act));
			request.Parameters.Add(new EventLabel(label));
			request.Parameters.Add(new EventValue(val));

			//Make a Post request which will contain all information from above
			//Debug.Log("Sending: "+anonymousSessionID+" / "+request );
			request.Post(anonymousSessionID);
		}
		catch( Exception )
		{
			// ignored
		}
#else
		// Do nothing: all stats are compiled-out for Player builds. We don't want to record data on games, only Editor
		#endif
	}
}