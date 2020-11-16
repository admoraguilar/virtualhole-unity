using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace IntelligentPluginTools
{
/**
 * Enhances Unity's Gizmos system by adding the obvious missing API methods
 */
	public class ExtendedGizmos
	{
		public static void DrawWireframeRectangle(Vector3[] corners, Color colour, Vector2 insetAmount)
		{
			Vector3[] finalRect = InsetRectanglePixels(corners, insetAmount);
			Gizmos.color = colour;
			Gizmos.DrawLine(finalRect[0], finalRect[1]);
			Gizmos.DrawLine(finalRect[1], finalRect[2]);
			Gizmos.DrawLine(finalRect[2], finalRect[3]);
			Gizmos.DrawLine(finalRect[3], finalRect[0]);
		}

		public static void DrawWireframeRectangleFractionalSides(Vector3[] finalRect, float fractionFromCorners)
		{
			DrawWireframeRectangleFractionalSides(finalRect, new Color(0,0,0,0), fractionFromCorners );
		}

		public static void DrawWireframeRectangleFractionalSides(Vector3[] finalRect, Color colour, float fractionFromCorners)
		{
			fractionFromCorners = Math.Min(fractionFromCorners, 0.5f); // clamp it to 0-0.5

			if( colour.a > 0f ) // if the incoming colour is blank, don't set it (use whatever the caller had already set instead)
				Gizmos.color = colour;
			
			for( int i = 0; i < 4; i++ )
			{
				int s = i;
				int e = (i + 1) < 4 ? i + 1 : 0;


				Gizmos.DrawLine(finalRect[s], finalRect[s] + fractionFromCorners * (finalRect[e] - finalRect[s]));
				Gizmos.DrawLine(finalRect[e] + fractionFromCorners * (finalRect[s] - finalRect[e]), finalRect[e]);
				//Gizmos.DrawLine(finalRect[0], finalRect[1]);
			}
		}

		public static Vector3[] InsetRectanglePixels(Vector3[] corners, Vector2 insetAmount)
		{
			Vector3 center = (corners[0] + corners[2]) / 2f;

			Vector3[] result = new Vector3[4];
			for( int i = 0; i < 4; i++ )
			{
				int iPrev = i > 0 ? i - 1 : 3;
				int iNext = (i + 1) < 4 ? i + 1 : 0;

				Vector3 toNext = corners[iNext] - corners[i];
				Vector3 toPrevious = corners[iPrev] - corners[i];

				float insetToNext = (i == 1 || i == 3) ? insetAmount.x : insetAmount.y;
				float insetToPrev = (i == 0 || i == 2) ? insetAmount.x : insetAmount.y;
				
				result[i] = corners[i]
				            + (insetToNext / toNext.magnitude) * toNext
				            + (insetToPrev / toPrevious.magnitude) * toPrevious;
			}

			return result;
		}

		public static Vector3[] InsetRectanglePercent(Vector3[] corners, float insetAmount)
		{
			Vector3 center = (corners[0] + corners[2]) / 2f;

			Vector3[] result = new Vector3[4];
			for( int i = 0; i < 4; i++ )
			{
				result[i] = corners[i] + Vector3.Lerp(corners[i], center, insetAmount);
			}

			return result;
		}
	}
}