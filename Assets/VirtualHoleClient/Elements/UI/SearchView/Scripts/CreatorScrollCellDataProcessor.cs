using UnityEngine;
using UnityEngine.UI;

namespace VirtualHole.Client.UI
{
	[CreateAssetMenu(menuName = "VirtualHole/UI/Creator Scroll Cell Data Processor")]
	public class CreatorScrollCellDataProcessor : ScrollCellDataProcessor<CreatorButton, CreatorScrollCellData>
	{
		public override void ProcessData(CreatorButton instance, CreatorScrollCellData data)
		{
			instance.creatorAvatarImage.sprite = data.creatorDTO.avatarSprite;
			instance.creatorNameText.SetText(data.creatorDTO.raw.universalName, false);

			if(data.onClick != null) {
				instance.button.onClick.RemoveAllListeners();
				instance.button.onClick.AddListener(() => data.onClick());
			}
		}
	}
}
