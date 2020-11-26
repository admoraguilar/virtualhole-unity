using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.Profiling;
using NAssert = NUnit.Framework.Assert;

namespace Tests
{
	public class FlexboxTestPerformanceHelpers : MonoBehaviour
	{
		public static List<string> defaultSamplers
		{
			get
			{
				List<string> samplerNames = new List<string>();
				Sampler.GetNames( samplerNames );
				/** Filter */
				return samplerNames.Where( s => s.StartsWith( "flex." ) ).ToList();
			}
		}

		public static void StartPerfTest( List<string> samplerNames = null )
		{
			if( samplerNames == null )
				samplerNames = defaultSamplers;

			/** Start */
			foreach( var s in samplerNames )
			{
				Recorder r = Sampler.Get( s ).GetRecorder();
				//Debug.Log( s+": sampler nanos before starting: "+r.elapsedNanoseconds );
				r.enabled = true;
			}
		}

		public static Dictionary<string, long> EndPerfTestMicroseconds( List<string> samplerNames = null )
		{
			if( samplerNames == null )
				samplerNames = defaultSamplers;

			/** Finish */
			Dictionary<string, long> samplerResults = new Dictionary<string, long>();
			//Debug.Log( "Profile results: ----------------" );
			foreach( var s in samplerNames )
			{
				Recorder r = Sampler.Get( s ).GetRecorder();
				r.enabled = false;
				samplerResults[s] = r.elapsedNanoseconds / 1000;
				r.enabled = true;
				//Debug.Log( s+": sampler nanos after resuming: "+r.elapsedNanoseconds );
			}

			return samplerResults;
		}

		public static long AssertLongestSampleInMicroseconds( long maxMicroseconds, Dictionary<string, long> samplerResults, bool allowZeroSamplers = false )
		{
			if( !allowZeroSamplers && samplerResults.Count < 1 )
				NAssert.Fail( "Performance test found no Samplers; code does not allow performance testing" );
			/** Nothing took longer than max */
			//long maxNanoseconds = maxMicroseconds * 1000;
			long longest = 0;
			foreach( var row in samplerResults )
			{
				longest = Math.Max( longest, row.Value );
				NAssert.Less( row.Value, maxMicroseconds, string.Format( "Sampler took too long: {0:0.000} ms (max allowed: {1:0.000} ms) {2}", row.Value / 1000f, maxMicroseconds / 1000f, row.Key ) );
			}
			
			if( !allowZeroSamplers ) 
				NAssert.Greater( longest, 0, "Performance test found no Samplers; code does not allow performance testing" );
			
			return longest;
		}

		/**
		 * Outputs a Log statement with the summary times because with performance testing you always want to improve it,
		 * it's not enough to merely pass, you want to see what the benchmark was while testing and decide whether to
		 * raise the bar
		 */
		public static void AssertSampleLongestAndAverageLessThan( long maxLongestMicroseconds, long maxAverageMicroseconds, Dictionary<string, List<long>> runTimesPerSampler )
		{
			if( runTimesPerSampler.Count < 1 )
				NAssert.Fail( "Performance test found no Samplers; code does not allow performance testing" );
			long longestSampler = -1;
			foreach( var sampler in runTimesPerSampler )
			{
				long total = 0;
				long count = 0;
				long longest = 0;
				foreach( var sample in sampler.Value )
				{
					total += sample;
					longest = Math.Max( longest, sample );
					count++;
					NAssert.Less( sample, maxLongestMicroseconds, string.Format( "Sampler took too long in WORST case: {0:0.000} ms (max allowed: {1:0.000} ms) {2}", sample / 1000f, maxLongestMicroseconds / 1000f, sampler.Key ) );
				}

				long avg = total / count;
				NAssert.Less( avg, maxAverageMicroseconds, string.Format( "Sampler took too long on AVERAGE: {0:0.000} ms (max allowed: {1:0.000} ms) {2}", avg / 1000f, maxAverageMicroseconds / 1000f, sampler.Key ) );
				
				Debug.LogFormat( "["+TestContext.CurrentContext.Test.Name+"] avg: {1:0.000} ms, worst: {2:0.000} ms --- '{0}'", sampler.Key, avg/1000f, longest/1000f );
				longestSampler = Math.Max( longestSampler, longest );
			}

			NAssert.Greater( longestSampler, 0, "Performance test found no Samplers; code does not allow performance testing" );
		}

		public static long AssertLongestSampleScaledMicros( long maxMicroseconds, Dictionary<string, long> samplerResults, bool allowZeroSamplers = false )
		{
			long referenceFrequency = 3000; // Average slow/cheap, 3-5 years old, gaming PC in 2020: 3 Ghz
			long scalesMaxMics = (SystemInfo.processorFrequency < 1 ? maxMicroseconds : (maxMicroseconds * referenceFrequency) / SystemInfo.processorFrequency);
			return AssertLongestSampleInMicroseconds( scalesMaxMics, samplerResults, allowZeroSamplers );
		}

		public static void AssertSampleLongestAndAverageLessThanScaled( long maxLongestMicroseconds, long maxAverageMicroseconds, Dictionary<string, List<long>> runTimesPerSampler )
		{
			long referenceFrequency = 3000; // Average slow/cheap, 3-5 years old, gaming PC in 2020: 3 Ghz
			long scaledLongest = (SystemInfo.processorFrequency < 1 ? maxLongestMicroseconds : (maxLongestMicroseconds * referenceFrequency) / SystemInfo.processorFrequency);
			long scaledAverage = (SystemInfo.processorFrequency < 1 ? maxAverageMicroseconds : (maxAverageMicroseconds * referenceFrequency) / SystemInfo.processorFrequency);
			AssertSampleLongestAndAverageLessThan( scaledLongest, scaledAverage, runTimesPerSampler );
		}
	}
}