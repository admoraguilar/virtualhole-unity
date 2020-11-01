using UnityEngine;
using UnityEngine.UI;

namespace VirtualHole.Client.UI
{
	[CreateAssetMenu(menuName = "VirtualHole/UI/Creator Scroll Cell Data Processor")]
	public class CreatorScrollCellDataProcessor : LoopScrollCellDataProcessor<CreatorButton, CreatorScrollCellData>
	{
		public override void ProcessData(CreatorButton instance, CreatorScrollCellData data)
		{
			instance.creatorAvatarImage.sprite = data.creatorAvatar;
			instance.creatorNameText.text = data.creatorName;

			if(data.onClick != null) {
				instance.button.onClick.RemoveAllListeners();
				instance.button.onClick.AddListener(() => data.onClick());
			}
		}
	}
}
