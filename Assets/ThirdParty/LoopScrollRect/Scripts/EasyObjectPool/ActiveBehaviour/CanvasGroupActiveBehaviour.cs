
namespace UnityEngine.UI
{
	[RequireComponent(typeof(CanvasGroup))]
	public class CanvasGroupActiveBehaviour : MonoBehaviour, IActiveBehaviour
	{
		public bool isActive 
		{
			get => _isActive;
			set {
				if(isActive == value) { return; }
				_isActive = value;
				InternalSetActive(isActive);
			} 
		}
		[SerializeField]
		private bool _isActive = true;

		private float _orgCanvasGroupAlphaValue = 0f;
		private bool _orgCanvasGroupInteractable = false;
		private bool _orgCanvaGroupBlocksRaycastValue = false;

		public CanvasGroup canvasGroup
		{
			get {
				if(_canvasGroup == null) { 
					_canvasGroup = GetComponent<CanvasGroup>();

					_orgCanvasGroupAlphaValue = canvasGroup.alpha;
					_orgCanvasGroupInteractable = canvasGroup.interactable;
					_orgCanvaGroupBlocksRaycastValue = canvasGroup.blocksRaycasts;
				}
				return _canvasGroup;
			}
		}
		private CanvasGroup _canvasGroup = null;

		public void SetActive(bool value)
		{
			if(isActive == value) { return; }
			isActive = value;
			InternalSetActive(isActive);
		}

		private void InternalSetActive(bool value)
		{
			if(value) {
				canvasGroup.alpha = _orgCanvasGroupAlphaValue;
				canvasGroup.interactable = _orgCanvasGroupInteractable;
				canvasGroup.blocksRaycasts = _orgCanvaGroupBlocksRaycastValue;
			} else {
				_orgCanvasGroupAlphaValue = canvasGroup.alpha;
				_orgCanvasGroupInteractable = canvasGroup.interactable;
				_orgCanvaGroupBlocksRaycastValue = canvasGroup.blocksRaycasts;

				canvasGroup.alpha = 0f;
				canvasGroup.interactable = false;
				canvasGroup.blocksRaycasts = false;
			}
		}

		private void Start()
		{
			InternalSetActive(isActive);	
		}
	}
}
