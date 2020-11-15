using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Midnight;
using Midnight.Pages;

namespace VirtualHole.Client.Pages
{
	public class LoadingDisplay : MonoBehaviour, ILoadingDisplay
	{
		[SerializeField]
		private Sprite[] _spinnerSpritePool = null;

		[SerializeField]
		private bool _shouldRandomizeSpriteOnEnable = true;

		public Sprite spinnerSprite
		{
			get => _spinnerImage.sprite;
			set => _spinnerImage.sprite = value;
		}
		[Space]
		[SerializeField]
		private Image _spinnerImage = null;

		private Queue<Sprite> _spinnerSpriteQueue = new Queue<Sprite>();

		public void SetRandomSpinnerSprite()
		{
			if(_spinnerSpriteQueue.Count <= 0) {
				Sprite[] sprPool = _spinnerSpritePool;
				sprPool.Shuffle();

				foreach(Sprite spr in sprPool) { _spinnerSpriteQueue.Enqueue(spr); }
			}
			spinnerSprite = _spinnerSpriteQueue.Dequeue();
		}

		public void SetProgress(float value)
		{
			
		}

		private void OnEnable()
		{
			if(_shouldRandomizeSpriteOnEnable) { SetRandomSpinnerSprite(); }
		}
	}
}
