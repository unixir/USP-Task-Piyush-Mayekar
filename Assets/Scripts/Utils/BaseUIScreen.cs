using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

namespace UI
{
    public class BaseUIScreen : MonoBehaviour, IScreen
    {
        [Tooltip("If true, this panel can be overlayed while other panels are on. If false, this panel will always be on after turning other ones off")]
        public bool IsOverlayPanel;

        public CanvasGroup CanvasGroup { get; private set; }

        // public bool ForceClose;
        /// <summary>
        /// Activate screen
        /// </summary>
        public virtual void Activate(string message = null)
        {
            CanvasGroup.DOFade(1f, 0.5f).OnComplete(() =>
            {
                CanvasGroup.interactable = true;
                CanvasGroup.blocksRaycasts = true;
            });
        }

        /// <summary>
        /// Deactivate screen
        /// </summary>
        public virtual void Deactivate()
        {
            CanvasGroup.DOFade(0f, 0.5f).OnComplete(() =>
            {
                CanvasGroup.interactable = false;
                CanvasGroup.blocksRaycasts = false;
            });
        }

        public virtual void OnBackButtonPressed()
        {

        }

        public virtual void SetCanvasGroup(CanvasGroup _canvasGroup) => CanvasGroup = _canvasGroup;

        /// <summary>
        /// Check screen status
        /// </summary>
        public bool IsScreenEnabled
        {
            get { return gameObject.activeSelf; }
        }
    }
}
