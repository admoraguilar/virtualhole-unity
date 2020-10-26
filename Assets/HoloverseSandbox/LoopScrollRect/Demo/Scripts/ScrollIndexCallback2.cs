using UnityEngine;
using UnityEngine.UI;

public class ScrollIndexCallback2 : MonoBehaviour, ILoopScrollIndexReceiver
{
	private static float[] randomWidths = new float[3] { 100, 150, 50 };

	public Text text;
	public LayoutElement element;

	void ILoopScrollIndexReceiver.ScrollCellIndex(int index)
	{
		string name = $"Cell {index}";
		if(text != null) { text.text = name; }

		element.preferredWidth = randomWidths[Mathf.Abs(index) % 3];
		gameObject.name = name;
	}
}
