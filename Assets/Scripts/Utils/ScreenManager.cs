using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UI
{
    public class ScreenManager : Singleton<ScreenManager>
    {
        private static ScreenManager _instance;

        public List<BaseUIScreen> screens;
        public List<BaseUIScreen> activeScreens;

        public BaseUIScreen startingScreen;

        void Awake()
        {
            screens.ForEach(screen =>
            {
                if (!screen.TryGetComponent<CanvasGroup>(out var canvasGroup))
                {
                    canvasGroup = screen.gameObject.AddComponent<CanvasGroup>();
                    canvasGroup.alpha = 0;
                    canvasGroup.interactable = false;
                    canvasGroup.blocksRaycasts = false;
                    screen.SetCanvasGroup(canvasGroup);
                }
                else
                {
                    screen.SetCanvasGroup(canvasGroup);
                    canvasGroup.alpha = 0;
                    canvasGroup.interactable = false;
                    canvasGroup.blocksRaycasts = false;
                }
            });
        }

        private void Start()
        {
            if (startingScreen != null)
            {
                activeScreens.Add(startingScreen);
                startingScreen.Activate();
            }
        }

        public void ActivateScreen(int screenIndex, string message = null)
        {
            if (screenIndex < 0 || screenIndex >= screens.Count)
            {
                Debug.LogError($"ScreenManager: ActivateScreen - Invalid screen index {screenIndex}");
                return;
            }
            //Deactivate other panels which are not overlay, if this is not an overlay panel
            var deactivateOthers = !screens[screenIndex].IsOverlayPanel;
            Debug.Log($"ScreenManager: ActivateScreen - Index: {screenIndex}, Message: {message}, DeactivateOthers: {deactivateOthers}");
            //Only deactivate others if they are not an overlay panel.
            if (deactivateOthers)
            {
                for (int i = activeScreens.Count - 1; i >= 0; i--)
                {
                    if (!screens[i].IsOverlayPanel)
                    {
                        activeScreens[i].Deactivate();
                        RemoveScreen(activeScreens[i]);
                    }
                }
            }

            BaseUIScreen screen = screens[screenIndex];
            if (!activeScreens.Contains(screen))
            {
                activeScreens.Add(screen);
            }
            screen.Activate(message);
            // AudioSource.PlayClipAtPoint(PopUpSound, transform.position, 1f);
        }

        /// <summary>
        /// Activate screen
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public void ActivateScreen<T>(string message = null) where T : IScreen
        {
            int indexOf = screens.FindIndex(t => t.GetType().Name == typeof(T).Name);
            ActivateScreen(indexOf, message);
            // AudioSource.PlayClipAtPoint(PopUpSound, transform.position, 1f);
        }

        /// <summary>
        /// Deactivate Screen
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public void DeactivateScreen<T>() where T : IScreen
        {
            IScreen screen = GetScreen<T>();

            RemoveScreen((BaseUIScreen)screen);
            screen.Deactivate();
        }

        public IScreen GetScreen<T>() where T : IScreen
        {
            return screens.Find(t => t.GetType().Name == typeof(T).Name);
        }

        public void DeactivateScreen(int screenIndex)
        {
            IScreen screen = screens[screenIndex];
            RemoveScreen((BaseUIScreen)screen);
            screen.Deactivate();
        }

        /// <summary>
        /// Deactivate all active screens regardless if they are overlay or not.
        /// </summary>
        public void DeactivateAllScreens()
        {
            activeScreens.ForEach(t => t.Deactivate());
        }

        public void RemoveScreen(BaseUIScreen screen)
        {
            if (activeScreens.Contains(screen))
            {
                activeScreens.Remove(screen);
            }
        }

        public bool IsScreenActive<T>() where T : IScreen
        {
            return activeScreens.Exists(screen => screen.GetType() == typeof(T));
        }

        internal BaseUIScreen GetActiveScreen()
        {
            if (activeScreens.Count > 0)
            {
                return activeScreens[activeScreens.Count - 1];
            }
            return null;
        }
    }
}

