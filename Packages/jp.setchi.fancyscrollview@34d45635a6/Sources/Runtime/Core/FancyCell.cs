/*
 * FancyScrollView (https://github.com/setchi/FancyScrollView)
 * Copyright (c) 2020 setchi
 * Licensed under MIT (https://github.com/setchi/FancyScrollView/blob/master/LICENSE)
 */

using UnityEngine;

namespace FancyScrollView
{
    /// <summary>
    /// <see cref="FancyScrollView{TItemData, TContext}"/> のセルを実装するための抽象基底クラス.
    /// <see cref="FancyCell{TItemData, TContext}.Context"/> が不要な場合は
    /// 代わりに <see cref="FancyCell{TItemData}"/> を使用します.
    /// </summary>
    /// <typeparam name="TItemData">アイテムのデータ型.</typeparam>
    /// <typeparam name="TContext"><see cref="Context"/> の型.</typeparam>
    public abstract class FancyCell<TItemData, TContext> : MonoBehaviour where TContext : class, new()
    {
		protected enum Visibility
		{
			None,
			GameObjectActive,
			CanvasGroupAlpha
		};

        /// <summary>
        /// このセルで表示しているデータのインデックス.
        /// </summary>
        public int Index { get; set; } = -1;

        /// <summary>
        /// このセルの可視状態.
        /// </summary>
        public virtual bool IsVisible
		{
			get {
				if(VisibilityHandling == Visibility.CanvasGroupAlpha) {
					return CanvasGroup.alpha > 0f;
				} else {
					return gameObject.activeSelf;
				}
			}
		}

		protected Visibility VisibilityHandling 
		{
			get {
				if(visibilityHandling == Visibility.None) {
					visibilityHandling = CanvasGroup != null ? Visibility.CanvasGroupAlpha : Visibility.GameObjectActive;
				}
				return visibilityHandling;
			} 
		}
		
		private Visibility visibilityHandling = Visibility.None;

		protected CanvasGroup CanvasGroup 
		{ 
			get {
				if(canvasGroup == null) { canvasGroup = GetComponent<CanvasGroup>(); }
				return canvasGroup;
			}
		}

		private CanvasGroup canvasGroup = null;

        /// <summary>
        /// <see cref="FancyScrollView{TItemData, TContext}.Context"/> の参照.
        /// セルとスクロールビュー間で同じインスタンスが共有されます. 情報の受け渡しや状態の保持に使用します.
        /// </summary>
        protected TContext Context { get; private set; }

        /// <summary>
        /// <see cref="Context"/> をセットします.
        /// </summary>
        /// <param name="context">コンテキスト.</param>
        public virtual void SetContext(TContext context) => Context = context;

        /// <summary>
        /// 初期化を行います.
        /// </summary>
        public virtual void Initialize() { }

		/// <summary>
		/// このセルの可視状態を設定します.
		/// </summary>
		/// <param name="visible">可視状態なら <c>true</c>, 非可視状態なら <c>false</c>.</param>
		public virtual void SetVisible(bool visible)
		{
			if(VisibilityHandling == Visibility.CanvasGroupAlpha) {
				CanvasGroup.alpha = visible ? 1f : 0f;
				CanvasGroup.interactable = visible ? true : false;
				CanvasGroup.blocksRaycasts = visible ? true : false;
			} else {
				gameObject.SetActive(visible);
			}
		}

        /// <summary>
        /// アイテムデータに基づいてこのセルの表示内容を更新します.
        /// </summary>
        /// <param name="itemData">アイテムデータ.</param>
        public abstract void UpdateContent(TItemData itemData);

        /// <summary>
        /// <c>0.0f</c> ~ <c>1.0f</c> の値に基づいてこのセルのスクロール位置を更新します.
        /// </summary>
        /// <param name="position">ビューポート範囲の正規化されたスクロール位置.</param>
        public abstract void UpdatePosition(float position);
    }

    /// <summary>
    /// <see cref="FancyScrollView{TItemData}"/> のセルを実装するための抽象基底クラス.
    /// </summary>
    /// <typeparam name="TItemData">アイテムのデータ型.</typeparam>
    /// <seealso cref="FancyCell{TItemData, TContext}"/>
    public abstract class FancyCell<TItemData> : FancyCell<TItemData, NullContext>
    {
        /// <inheritdoc/>
        public sealed override void SetContext(NullContext context) => base.SetContext(context);
    }
}
