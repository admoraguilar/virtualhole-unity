#define DEBUG_BREAK
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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
    public class TestAutoRelayout
    {
		private float cw, ch;
		private FlexContainer fCanvas;
		private FlexContainer rootContainer;
		private FlexItem rootItem;
		
		long timeAllowedForNoOpLayout;

		[SetUp]
		public void PerTestSetup()
		{
			timeAllowedForNoOpLayout = 400;
			FlexCanvasFullSizeForTesting(out cw, out ch, out fCanvas, new float[0]);
			rootContainer = fCanvas.AddChildFlexContainerAndFlexItem("rootContainer", out rootItem);
			rootContainer.RelayoutAfterCreatingAtRuntime(); // required, or else the container itself won't get sized by its parent
		}

		[TearDown]
		public void PerTestCleanup()
		{
			/** Unique to this class! Make sure you re-enable automatic relayout even if a test failed or crashed */
			FlexContainer.EnableAutomaticRelayout();
			
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
		public void Test_Perf_RelayoutDisabled_SetLayoutHorizontal()
		{
			FlexContainer.DisableAutomaticRelayout( true );
			Dictionary<string, List<long>> runTimesPerSampler = new Dictionary<string, List<long>>();

			List<FlexContainer> level1 = _AddContainers( new List<FlexContainer> {rootContainer}, 5, FlexDirection.ROW );
			List<FlexContainer> level2 = _AddContainers( level1, 3, FlexDirection.COLUMN );
			List<FlexContainer> level3 = _AddContainers( level2, 2, FlexDirection.ROW );

			/** Prepare to access non-public method */
			string targetMethodName = "ILayoutController.SetLayoutHorizontal";
			MethodInfo targetNonPublic = typeof(FlexContainer).GetMethod( targetMethodName, BindingFlags.NonPublic | BindingFlags.Instance );
			timeAllowedForNoOpLayout += 500; // to allow for the cost of Reflection
			
			/** Start the performance test: second the relayout (should have no effect, take almost zero time) */
			StartPerfTest();

			/** Run */
			targetNonPublic.Invoke( rootContainer, new object[] { } );

			/** Finish */
			var results = EndPerfTestMicroseconds();
			
			/** finally check that doing the layout manually still WORKS and produces correct results */
			rootContainer.RelayoutAfterChangingSelfOrChildrenFromScript(); // will automatically relayout all direct-children because its a container

			/** Nothing took longer than max */
			AssertLongestSampleScaledMicros( timeAllowedForNoOpLayout, results, true );
		}
		
		[Test]
		public void Test_Perf_RelayoutDisabled_OnRectTransformDimensionsChange()
		{
			FlexContainer.DisableAutomaticRelayout( true );
			Dictionary<string, List<long>> runTimesPerSampler = new Dictionary<string, List<long>>();

			List<FlexContainer> level1 = _AddContainers( new List<FlexContainer> {rootContainer}, 5, FlexDirection.ROW );
			List<FlexContainer> level2 = _AddContainers( level1, 3, FlexDirection.COLUMN );
			List<FlexContainer> level3 = _AddContainers( level2, 2, FlexDirection.ROW );

			/** Prepare to access non-public method */
			string targetMethodName = "OnRectTransformDimensionsChange";
			MethodInfo targetNonPublic = typeof(FlexContainer).GetMethod( targetMethodName, BindingFlags.NonPublic | BindingFlags.Instance );
			timeAllowedForNoOpLayout += 500; // to allow for the cost of Reflection
			
			/** Start the performance test: second the relayout (should have no effect, take almost zero time) */
			StartPerfTest();

			/** Run */
			// NB: sadly this method mostly tests the speed of C# reflection, but since we expect this line to be a no-op, anything more than the cost of the method call will be a FAIL 
			targetNonPublic.Invoke( rootContainer, new object[] { } );

			/** Finish */
			var results = EndPerfTestMicroseconds();
			
			/** finally check that doing the layout manually still WORKS and produces correct results */
			rootContainer.RelayoutAfterChangingSelfOrChildrenFromScript(); // will automatically relayout all direct-children because its a container

			/** Nothing took longer than max */
			AssertLongestSampleScaledMicros( timeAllowedForNoOpLayout, results, true );
		}
		
		[Test]
		public void Test_Perf_RelayoutDisabled_OnChildRectTransformPositionChange()
		{
			FlexContainer.DisableAutomaticRelayout( true );
			Dictionary<string, List<long>> runTimesPerSampler = new Dictionary<string, List<long>>();

			List<FlexContainer> level1 = _AddContainers( new List<FlexContainer> {rootContainer}, 5, FlexDirection.ROW );
			List<FlexContainer> level2 = _AddContainers( level1, 3, FlexDirection.COLUMN );
			List<FlexContainer> level3 = _AddContainers( level2, 2, FlexDirection.ROW );

			/** Start the performance test: second the relayout (should have no effect, take almost zero time) */
			StartPerfTest();

			/** Run */
			RectTransform nullTransform = default(RectTransform);
			rootContainer.OnChildRectTransformPositionChange( nullTransform );

			/** Finish */
			var results = EndPerfTestMicroseconds();
			
			/** finally check that doing the layout manually still WORKS and produces correct results */
			rootContainer.RelayoutAfterChangingSelfOrChildrenFromScript(); // will automatically relayout all direct-children because its a container

			/** Nothing took longer than max */
			AssertLongestSampleScaledMicros( timeAllowedForNoOpLayout, results, true );
		}
		
		[Test]
		public void Test_Perf_RelayoutDisabled_OnRectTransformPositionChange()
		{
			FlexContainer.DisableAutomaticRelayout( true );
			Dictionary<string, List<long>> runTimesPerSampler = new Dictionary<string, List<long>>();

			List<FlexContainer> level1 = _AddContainers( new List<FlexContainer> {rootContainer}, 5, FlexDirection.ROW );
			List<FlexContainer> level2 = _AddContainers( level1, 3, FlexDirection.COLUMN );
			List<FlexContainer> level3 = _AddContainers( level2, 2, FlexDirection.ROW );

			/** Start the performance test: second the relayout (should have no effect, take almost zero time) */
			StartPerfTest();

			/** Run */
			rootContainer.OnRectTransformPositionChange();

			/** Finish */
			var results = EndPerfTestMicroseconds();
			
			/** finally check that doing the layout manually still WORKS and produces correct results */
			rootContainer.RelayoutAfterChangingSelfOrChildrenFromScript(); // will automatically relayout all direct-children because its a container

			/** Nothing took longer than max */
			AssertLongestSampleScaledMicros( timeAllowedForNoOpLayout, results, true );
		}
		
		[Test]
		public void Test_Perf_RelayoutDisabled_OnTransformChildrenChanged()
		{
			FlexContainer.DisableAutomaticRelayout( true );
			Dictionary<string, List<long>> runTimesPerSampler = new Dictionary<string, List<long>>();

			List<FlexContainer> level1 = _AddContainers( new List<FlexContainer> {rootContainer}, 5, FlexDirection.ROW );
			List<FlexContainer> level2 = _AddContainers( level1, 3, FlexDirection.COLUMN );
			List<FlexContainer> level3 = _AddContainers( level2, 2, FlexDirection.ROW );

			/** Prepare to access non-public method */
			string targetMethodName = "OnTransformChildrenChanged";
			MethodInfo targetNonPublic = typeof(FlexContainer).GetMethod( targetMethodName, BindingFlags.NonPublic | BindingFlags.Instance );
			timeAllowedForNoOpLayout += 500; // to allow for the cost of Reflection
			
			/** Start the performance test: second the relayout (should have no effect, take almost zero time) */
			StartPerfTest();

			/** Run */
			targetNonPublic.Invoke( rootContainer, new object[] { } );

			/** Finish */
			var results = EndPerfTestMicroseconds();
			
			/** finally check that doing the layout manually still WORKS and produces correct results */
			rootContainer.RelayoutAfterChangingSelfOrChildrenFromScript(); // will automatically relayout all direct-children because its a container

			/** Nothing took longer than max */
			AssertLongestSampleScaledMicros( timeAllowedForNoOpLayout, results, true );
		}
		
		[Test]
		public void Test_Perf_RelayoutDisabled_OnValidate()
		{
			FlexContainer.DisableAutomaticRelayout( true );
			Dictionary<string, List<long>> runTimesPerSampler = new Dictionary<string, List<long>>();

			List<FlexContainer> level1 = _AddContainers( new List<FlexContainer> {rootContainer}, 5, FlexDirection.ROW );
			List<FlexContainer> level2 = _AddContainers( level1, 3, FlexDirection.COLUMN );
			List<FlexContainer> level3 = _AddContainers( level2, 2, FlexDirection.ROW );

			/** Prepare to access non-public method */
			string targetMethodName = "OnValidate";
			MethodInfo targetNonPublic = typeof(FlexContainer).GetMethod( targetMethodName, BindingFlags.NonPublic | BindingFlags.Instance );
			timeAllowedForNoOpLayout += 500; // to allow for the cost of Reflection
			
			/** Start the performance test: second the relayout (should have no effect, take almost zero time) */
			StartPerfTest();

			/** Run */
			targetNonPublic.Invoke( rootContainer, new object[] { } );

			/** Finish */
			var results = EndPerfTestMicroseconds();
			
			/** finally check that doing the layout manually still WORKS and produces correct results */
			rootContainer.RelayoutAfterChangingSelfOrChildrenFromScript(); // will automatically relayout all direct-children because its a container

			/** Nothing took longer than max */
			AssertLongestSampleScaledMicros( timeAllowedForNoOpLayout, results, true );
		}
		
		[Test]
		public void Test_Perf_RelayoutDisabled_RelayoutBecauseChildAddedOrRemoved()
		{
			FlexContainer.DisableAutomaticRelayout( true );
			Dictionary<string, List<long>> runTimesPerSampler = new Dictionary<string, List<long>>();

			List<FlexContainer> level1 = _AddContainers( new List<FlexContainer> {rootContainer}, 5, FlexDirection.ROW );
			List<FlexContainer> level2 = _AddContainers( level1, 3, FlexDirection.COLUMN );
			List<FlexContainer> level3 = _AddContainers( level2, 2, FlexDirection.ROW );

			/** Prepare to access non-public method */
			string targetMethodName = "RelayoutBecauseChildAddedOrRemoved";
			MethodInfo targetNonPublic = typeof(FlexContainer).GetMethod( targetMethodName, BindingFlags.NonPublic | BindingFlags.Instance );
			timeAllowedForNoOpLayout += 500; // to allow for the cost of Reflection

			/** Start the performance test: second the relayout (should have no effect, take almost zero time) */
			StartPerfTest();

			/** Run */
			targetNonPublic.Invoke( rootContainer, new object[] { } );

			/** Finish */
			var results = EndPerfTestMicroseconds();
			
			/** finally check that doing the layout manually still WORKS and produces correct results */
			rootContainer.RelayoutAfterChangingSelfOrChildrenFromScript(); // will automatically relayout all direct-children because its a container

			/** Nothing took longer than max */
			AssertLongestSampleScaledMicros( timeAllowedForNoOpLayout, results, true );
		}
		
		[Test]
		public void Test_Perf_RelayoutDisabled_RelayoutBecauseRectTransformChanged()
		{
			FlexContainer.DisableAutomaticRelayout( true );
			Dictionary<string, List<long>> runTimesPerSampler = new Dictionary<string, List<long>>();

			List<FlexContainer> level1 = _AddContainers( new List<FlexContainer> {rootContainer}, 5, FlexDirection.ROW );
			List<FlexContainer> level2 = _AddContainers( level1, 3, FlexDirection.COLUMN );
			List<FlexContainer> level3 = _AddContainers( level2, 2, FlexDirection.ROW );

			/** Prepare to access non-public method */
			string targetMethodName = "RelayoutBecauseRectTransformChanged";
			MethodInfo targetNonPublic = typeof(FlexContainer).GetMethod( targetMethodName, BindingFlags.NonPublic | BindingFlags.Instance );
			timeAllowedForNoOpLayout += 500; // to allow for the cost of Reflection
			
			/** Start the performance test: second the relayout (should have no effect, take almost zero time) */
			StartPerfTest();

			/** Run */
			targetNonPublic.Invoke( rootContainer, new object[] { } );

			/** Finish */
			var results = EndPerfTestMicroseconds();
			
			/** finally check that doing the layout manually still WORKS and produces correct results */
			rootContainer.RelayoutAfterChangingSelfOrChildrenFromScript(); // will automatically relayout all direct-children because its a container

			/** Nothing took longer than max */
			AssertLongestSampleScaledMicros( timeAllowedForNoOpLayout, results, true );
		}

		/**
		 * This test is not active yet: we don't have a situation in which we fully suppress layout
		 */
		[Test]
		public void Test_Perf_AllLayoutDisabled()
		{
			FlexContainer.DisableAutomaticRelayout( true );
			Dictionary<string,List<long>> runTimesPerSampler = new Dictionary<string, List<long>>();
			
			/** Start the performance test: first the basic construction (should have no effect, take almost zero time) */
			NAssert.Fail("Not implemented yet: A way to suppress internal layout happening as byproduct of adding containers" );
			StartPerfTest();
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
			/** Finish */
			var results1 = EndPerfTestMicroseconds();
			/** Gather results */
			foreach( var row in results1 )
			{
				if( !runTimesPerSampler.ContainsKey( row.Key ) )
					runTimesPerSampler[row.Key] = new List<long>();
				runTimesPerSampler[row.Key].Add( row.Value );
			}
			 
			for( int i = 0; i < 5; i++ )
			{
				/** Start the performance test: second the relayout (should have no effect, take almost zero time) */
				StartPerfTest();
				
				/** Run */
				rootContainer.RelayoutAfterChangingSelfOrChildrenFromScript(); // will automatically relayout all direct-children because its a container            

				/** Finish */
				var results2 = EndPerfTestMicroseconds();
				/** Gather results */
				foreach( var row in results2 )
				{
					if( !runTimesPerSampler.ContainsKey( row.Key ) )
						runTimesPerSampler[row.Key] = new List<long>();
					runTimesPerSampler[row.Key].Add( row.Value );
				}
			}
			
			/** Nothing took longer than max */
			AssertSampleLongestAndAverageLessThanScaled( 100, 100, runTimesPerSampler );
		}
		
	}
}