using UnityEngine;
using UnityEngine.UI;

public class ScrollIndexCallback3 : MonoBehaviour, ILoopScrollIndexReceiver
{
	public Text text;

	void ILoopScrollIndexReceiver.ScrollCellIndex(int index)
	{
		string name = $"Cell {index}";
		if(text != null) { text.text = name; }
	}
}
