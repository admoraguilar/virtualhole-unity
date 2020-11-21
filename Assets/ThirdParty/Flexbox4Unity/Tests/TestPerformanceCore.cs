#define DEBUG_BREAK
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Flexbox4Unity;
using NUnit.Framework;
using UnityEngine;
using static Tests.FlexboxTestHelpers;
using static Tests.FlexboxTestPerformanceHelpers;
using static Tests.WidthHeighMatch;
using Assert = UnityEngine.Assertions.Assert;
using NAssert = NUnit.Framework.Assert;
using Object = UnityEngine.Object;

namespace Tests
{
    public class TestPerformanceCore
    {
		private float cw, ch;
		private FlexContainer fCanvas;
		private FlexContainer rootContainer;
		private FlexItem rootItem;

		[SetUp]
		public void PerTestSetup()
		{
			FlexCanvasFullSizeForTesting(out cw, out ch, out fCanvas, new float[0]);
			rootContainer = fCanvas.AddChildFlexContainerAndFlexItem("rootContainer", out rootItem);
			rootContainer.RelayoutAfterCreatingAtRuntime(); // required, or else the container itself won't get sized by its parent
		}

		[TearDown]
		public void PerTestCleanup()
		{
			var guiElements = Object.FindObjectsOfType<RectTransform>();
			foreach( var e in guiElements )
				Object.DestroyImmediate(e.gameObject);
		}

		private List<FlexContainer> _AddContainers( List<FlexContainer> parents, int numToAdd, FlexDirection colsOrRows )
		{
			List<FlexContainer> @return = new List<FlexContainer>();
			foreach( var parent in parents )
			{
				parent.direction = colsOrRows == FlexDirection.ROW ? FlexDirection.COLUMN : FlexDirection.ROW; // note reversed: set to ROW to make a set of COLUMNS
				for( int i = 0; i < numToAdd; i++ )
				{
					var fc = parent.AddChildFlexContainerAndFlexItem( "auto", out FlexItem fi );
					@return.Add( fc );
				}
			}

			return @return;
		}

		[Test]
		public void Test_Perf_AUTO_sized_nestedContainers()
		{
			List<FlexContainer> level1 = _AddContainers( new List<FlexContainer> { rootContainer }, 3, FlexDirection.COLUMN );
			List<FlexContainer> level2 = _AddContainers( level1, 4, FlexDirection.ROW );
			
			Dictionary<string,List<long>> runTimesPerSampler = new Dictionary<string, List<long>>(); 
			for( int i = 0; i < 5; i++ )
			{
				/** Start the performance test */
				StartPerfTest();

				/** Run */
				rootContainer.RelayoutAfterChangingSelfOrChildrenFromScript(); // will automatically relayout all direct-children because its a container            

				/** Finish */
				var results = EndPerfTestMicroseconds();
				/** Gather results */
				foreach( var row in results )
				{
					if( !runTimesPerSampler.ContainsKey( row.Key ) )
						runTimesPerSampler[row.Key] = new List<long>();
					runTimesPerSampler[row.Key].Add( row.Value );
				}
			}
			
			/** Nothing took longer than max */
			AssertSampleLongestAndAverageLessThanScaled( 13*1000, 7 * 1000, runTimesPerSampler );
		}
		
		[Test]
		public void Test_Perf_AUTO_sized_3_cols_text()
		{
			List<FlexContainer> level1 = _AddContainers( new List<FlexContainer> { rootContainer }, 3, FlexDirection.COLUMN );
			foreach( var fc in level1 )
			{
				for( int i = 0; i < 4; i++ )
				{
					fc.AddFlexTemplatedText( "Text row " + i );
				}
			}
			
			Dictionary<string,List<long>> runTimesPerSampler = new Dictionary<string, List<long>>(); 
			for( int i = 0; i < 5; i++ )
			{
				/** Start the performance test */
				StartPerfTest();

				/** Run */
				rootContainer.RelayoutAfterChangingSelfOrChildrenFromScript(); // will automatically relayout all direct-children because its a container            

				/** Finish */
				var results = EndPerfTestMicroseconds();
				/** Gather results */
				foreach( var row in results )
				{
					if( !runTimesPerSampler.ContainsKey( row.Key ) )
						runTimesPerSampler[row.Key] = new List<long>();
					runTimesPerSampler[row.Key].Add( row.Value );
				}
			}
			
			/** Nothing took longer than max */
			AssertSampleLongestAndAverageLessThanScaled( 10*1000, 5*1000, runTimesPerSampler );
		}
		
		[Test]
		public void Test_Perf_AUTO_sized_RowsOfColsOfTwoRows_OfText()
		{
			List<FlexContainer> level1 = _AddContainers( new List<FlexContainer> { rootContainer }, 5, FlexDirection.ROW );
			List<FlexContainer> level2 = _AddContainers( level1, 3, FlexDirection.COLUMN );
			List<FlexContainer> level3 = _AddContainers( level2, 2, FlexDirection.ROW );
			foreach( var fc in level3 )
			{
				for( int i = 0; i < 2; i++ )
				{
					fc.AddFlexTemplatedText( "Text row " + i );
				}
			}
			
			Dictionary<string,List<long>> runTimesPerSampler = new Dictionary<string, List<long>>(); 
			for( int i = 0; i < 5; i++ )
			{
				/** Start the performance test */
				StartPerfTest();

				/** Run */
				rootContainer.RelayoutAfterChangingSelfOrChildrenFromScript(); // will automatically relayout all direct-children because its a container            

				/** Finish */
				var results = EndPerfTestMicroseconds();
				/** Gather results */
				foreach( var row in results )
				{
					if( !runTimesPerSampler.ContainsKey( row.Key ) )
						runTimesPerSampler[row.Key] = new List<long>();
					runTimesPerSampler[row.Key].Add( row.Value );
				}
			}
			
			/** Nothing took longer than max */
			AssertSampleLongestAndAverageLessThanScaled( 50*1000, 40*1000, runTimesPerSampler );
		}
	}
}