using System;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Helpers.Utilities.ScreenUtils
{
    public static class ScreenUtilities
    {
        public static Action OnScreenResolutionChanged;
        public static Vector2 ScreenRatios = new Vector2(1, 1);
#if UNITY_EDITOR
        private static int _screenWidth;
        private static int _screenHeight;
        private static UnityEngine.Camera _mainCamera;

        [InitializeOnLoadMethod]
        private static void OnLoad()
        {
            _mainCamera = UnityEngine.Camera.main;

            if (_mainCamera == null)
            {
                EditorApplication.update -= Update;
                return;
            }

            _screenHeight = _mainCamera.pixelHeight;
            _screenWidth = _mainCamera.pixelWidth;
            EditorApplication.update -= Update;
            EditorApplication.update += Update;
        }

        private static void Update()
        {
            if (!Mathf.Approximately(_screenHeight, _mainCamera.pixelHeight) ||
                !Mathf.Approximately(_screenWidth, _mainCamera.pixelWidth))
            {
                ScreenRatios = new Vector2((float)_mainCamera.pixelWidth / _screenWidth,
                    (float)_mainCamera.pixelHeight / _screenHeight);
                OnScreenResolutionChanged?.Invoke();
            }

            _screenHeight = _mainCamera.pixelHeight;
            _screenWidth = _mainCamera.pixelWidth;
        }
#endif
    }
}