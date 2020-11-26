using System.Collections.Generic;
using System.Collections.ObjectModel;
using Flexbox4Unity;
using UnityEngine;

[CreateAssetMenu]
public class FlexboxLayoutAlgorithm313 : IFlexboxLayoutAlgorithmV3
{
	private ReadOnlyCollection<string> _featureDesc = new ReadOnlyCollection<string>( new List<string>  {"N/A"} );
	public override ReadOnlyCollection<string> featureDescription { get { return _featureDesc; } }
	public override void AlgorithmLayout( FlexContainer fc, Vector2 availableSize )
	{
	}

	public override string defaultAssetName { get; }
	
}