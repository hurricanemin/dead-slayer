using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Cinemachine;
using Helpers.Utilities;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Helpers.Camera
{
    public class CameraController : MonoBehaviour
    {
        [SerializeField] private GameObject cameraPrefab;
        [SerializeField] private UnityEngine.Camera mainCamera;
        [SerializeField] private CinemachineBrain cinemachineBrain;
        [SerializeField] private List<VirtualCam> creaCams;
        [SerializeField] private bool isInitialized;

        private Coroutine _activeCoroutine;
        private VirtualCam _currentCam;

        private float _defaultBlendTime;

        public VirtualCam ActiveCamera => GetActiveCamera();

        private void Awake()
        {
            if (isInitialized)
            {
                _defaultBlendTime = cinemachineBrain.m_DefaultBlend.m_Time;
                if (GetActiveCamera() == null) ChangeActiveCamera(0, false);
                return;
            }

            Initialize();
        }

        private void Initialization()
        {
            ResetPositionDamping(CreaCameraType.DefaultVirtualCam);
        }

        /// <summary>
        /// Sets the positional damping value(s) of the active camera.
        /// </summary>
        /// <param name="positionDamping">Damping values.</param>
        public void SetPositionDamping(Vector3 positionDamping)
        {
            GetActiveCamera()?.SetPositionDamping(positionDamping);
        }

        /// <summary>
        /// Sets the positional damping value(s) of given camera type.
        /// </summary>
        /// <param name="cameraType">Target camera type.</param>
        /// <param name="positionDamping">Damping values.</param>
        public void SetPositionDamping(CreaCameraType cameraType, Vector3 positionDamping)
        {
            GetCamera(cameraType)?.SetPositionDamping(positionDamping);
        }

        /// <summary>
        /// Resets positional damping values of the active camera.
        /// </summary>
        public void ResetPositionDamping()
        {
            GetActiveCamera()?.ResetPositionDamping();
        }

        /// <summary>
        /// Resets the positional damping of given camera type.
        /// </summary>
        /// <param name="cameraType">Target camera type</param>
        public void ResetPositionDamping(CreaCameraType cameraType)
        {
            GetCamera(cameraType)?.ResetPositionDamping();
        }

        /// <summary>
        /// Sets the rotational damping value(s) of the active camera.
        /// </summary>
        /// <param name="lookDamping">Damping values.</param>
        public void SetLookDamping(Vector2 lookDamping)
        {
            GetActiveCamera()?.SetLookDamping(lookDamping);
        }

        /// <summary>
        /// Sets the rotational damping value(s) of given camera type.
        /// </summary>
        /// <param name="cameraType">Target camera type.</param>
        /// <param name="lookDamping">Damping values.</param>
        public void SetLookDamping(CreaCameraType cameraType, Vector2 lookDamping)
        {
            GetCamera(cameraType)?.SetLookDamping(lookDamping);
        }

        /// <summary>
        /// Resets the rotational damping of the active camera.
        /// </summary>
        public void ResetLookDamping()
        {
            GetActiveCamera()?.ResetLookDamping();
        }

        /// <summary>
        /// Resets the rotational damping of given camera type.
        /// </summary>
        /// <param name="cameraType">Target camera type</param>
        public void ResetLookDamping(CreaCameraType cameraType)
        {
            GetCamera(cameraType)?.ResetLookDamping();
        }

        /// <summary>
        /// Sets the look-at position of given camera type. If the camera is looking at a custom target,
        /// look at target will be reset to default too!
        /// </summary>
        /// <param name="cameraType">Target camera type</param>
        /// <param name="position">Target world-space position</param>
        /// <param name="shouldBlend">Whether to blend in or not</param>
        public void UpdateLookAtPosition(CreaCameraType cameraType, Vector3 position, bool shouldBlend = true)
        {
            GetCamera(cameraType)?.UpdateLookAtPosition(position, shouldBlend);
        }

        /// <summary>
        /// Sets look-at position of the active camera. If the camera is looking at a custom target,
        /// look at target will be reset to default too!
        /// </summary>
        /// <param name="position">Target world-space position</param>
        /// <param name="shouldBlend">Whether to blend in or not</param>
        public void UpdateLookAtPosition(Vector3 position, bool shouldBlend = true)
        {
            GetActiveCamera()?.UpdateLookAtPosition(position, shouldBlend);
        }

        /// <summary>
        /// Sets look-at target of the active camera.
        /// </summary>
        /// <param name="cameraType">Target camera type.</param>
        /// <param name="lookAtTarget">Transform of object to look at.</param>
        /// <param name="shouldBlend">Whether to blend in or not.</param>
        public void SetLookAtTarget(CreaCameraType cameraType, Transform lookAtTarget, bool shouldBlend = true)
        {
            GetCamera(cameraType)?.SetLookAtTarget(lookAtTarget, shouldBlend);
        }

        /// <summary>
        /// Sets look-at target of given camera type.
        /// </summary>
        /// <param name="lookAtTarget">Transform of object to look at.</param>
        /// <param name="shouldBlend">Whether to blend in or not.</param>
        public void SetLookAtTarget(Transform lookAtTarget, bool shouldBlend = true)
        {
            GetActiveCamera()?.SetLookAtTarget(lookAtTarget, shouldBlend);
        }

        /// <summary>
        /// Rotates the given camera towards given transforms position.
        /// </summary>
        /// <param name="cameraType">Target camera type</param>
        /// <param name="lookAtTarget">Target transform to look towards</param>
        public void LookTowards(CreaCameraType cameraType, Transform lookAtTarget)
        {
            if (lookAtTarget == null) return;
            GetCamera(cameraType)?.LookTowards(lookAtTarget);
        }

        /// <summary>
        /// Rotates the active camera towards given transforms position.
        /// </summary>
        /// <param name="lookAtTarget">Target transform to look towards</param>
        public void LookTowards(Transform lookAtTarget)
        {
            if (lookAtTarget == null) return;
            GetActiveCamera()?.LookTowards(lookAtTarget);
        }

        /// <summary>
        /// Rotates the given camera towards given position.
        /// </summary>
        /// <param name="cameraType">Target camera type</param>
        /// <param name="position">Position to look towards</param>
        public void LookTowardsPosition(CreaCameraType cameraType, Vector3 position)
        {
            GetCamera(cameraType)?.LookTowardsPosition(position);
        }

        /// <summary>
        /// Rotates the active camera towards given position.
        /// </summary>
        /// <param name="position">Position to look towards</param>
        public void LookTowardsPosition(Vector3 position)
        {
            GetActiveCamera()?.LookTowardsPosition(position);
        }

        /// <summary>
        /// Resets look at target of given camera type to default.
        /// </summary>
        /// <param name="cameraType"></param>
        /// <param name="shouldBlend">Whether to blend in or not.</param>
        public void ResetLookAt(CreaCameraType cameraType, bool shouldBlend = true)
        {
            GetCamera(cameraType)?.ResetLookAt(shouldBlend);
        }

        /// <summary>
        /// Resets look at target of the active camera.
        /// </summary>
        /// <param name="shouldBlend">Whether to blend in or not.</param>
        public void ResetLookAt(bool shouldBlend = true)
        {
            GetActiveCamera()?.ResetLookAt(shouldBlend);
        }

        /// <summary>
        /// Sets the position follow offset of the active camera.
        /// </summary>
        /// <param name="offset">New offset</param>
        /// <param name="shouldBlend">Whether to blend in or not.</param>
        public void SetFollowOffset(Vector3 offset, bool shouldBlend = true)
        {
            GetActiveCamera()?.SetFollowOffset(offset, shouldBlend);
        }

        /// <summary>
        /// Sets the position follow offset of given camera type.
        /// </summary>
        /// <param name="cameraType">Target camera type.</param>
        /// <param name="offset">New offset</param>
        /// <param name="shouldBlend">Whether to blend in or not.</param>
        public void SetFollowOffset(CreaCameraType cameraType, Vector3 offset, bool shouldBlend = true)
        {
            GetCamera(cameraType)?.SetFollowOffset(offset, shouldBlend);
        }

        /// <summary>
        /// Resets the position follow offset of given camera type.
        /// </summary>
        /// <param name="cameraType">Target camera type.</param>
        /// <param name="shouldBlend">Whether to blend in or not.</param>
        public void ResetFollowOffset(CreaCameraType cameraType, bool shouldBlend = true)
        {
            GetCamera(cameraType)?.ResetFollowOffset(shouldBlend);
        }

        /// <summary>
        /// Resets the position follow offset of the active camera.
        /// </summary>
        /// <param name="shouldBlend">Whether to blend in or not.</param>
        public void ResetFollowOffset(bool shouldBlend = true)
        {
            GetActiveCamera()?.ResetFollowOffset(shouldBlend);
        }

        /// <summary>
        /// Sets follow target of given camera type.
        /// </summary>
        /// <param name="cameraType">Target camera type.</param>
        /// <param name="followTarget">Transform to follow.</param>
        /// <param name="shouldBlend">Whether to blend in or not.</param>
        public void SetFollowTarget(CreaCameraType cameraType, Transform followTarget, bool shouldBlend = true)
        {
            GetCamera(cameraType)?.SetFollowTarget(followTarget, shouldBlend);
        }

        /// <summary>
        /// Sets follow target of the active camera.
        /// </summary>
        /// <param name="followTarget">Transform to follow.</param>
        /// <param name="shouldBlend">Whether to blend in or not.</param>
        public void SetFollowTarget(Transform followTarget, bool shouldBlend = true)
        {
            GetActiveCamera()?.SetFollowTarget(followTarget, shouldBlend);
        }

        /// <summary>
        /// Resets follow target of given camera type.
        /// </summary>
        /// <param name="cameraType">Target camera type.</param>
        /// <param name="shouldBlend">Whether to blend in or not.</param>
        public void ResetFollowTarget(CreaCameraType cameraType, bool shouldBlend = true)
        {
            GetCamera(cameraType)?.ResetFollowTarget(shouldBlend);
        }

        /// <summary>
        /// Resets follow target of the active camera.
        /// </summary>
        /// <param name="shouldBlend">Whether to blend in or not.</param>
        public void ResetFollowTarget(bool shouldBlend = true)
        {
            GetActiveCamera()?.ResetFollowTarget(shouldBlend);
        }

        /// <summary>
        /// Changes the active camera to given camera type. Fires the event given when the transition is done.
        /// </summary>
        /// <param name="cameraType">Target camera type.</param>
        /// <param name="shouldBlend">Whether to make the transition blended in or instant.</param>
        /// <param name="onComplete">Event to fire when transition ends.</param>
        public void ChangeActiveCamera(CreaCameraType cameraType, bool shouldBlend = true,
            Action<CreaCameraType, TransitionResult> onComplete = null)
        {
            VirtualCam targetCamera = creaCams.FirstOrDefault(x => x.CamData.cameraType == cameraType);

            if (targetCamera == null || !targetCamera.IsUsable)
            {
                Debug.LogError("Couldn't shift to camera {0}!".Format(cameraType));
                onComplete?.Invoke(cameraType, TransitionResult.Incomplete);
                return;
            }

            _currentCam = GetActiveCamera();
            bool isPreviousCameraAvailable = _currentCam != null;

            switch (isPreviousCameraAvailable)
            {
                case true when _currentCam.CamData.cameraType == targetCamera.CamData.cameraType:
                    onComplete?.Invoke(targetCamera.CamData.cameraType, TransitionResult.Incomplete);
                    return;
                case false:
                {
                    SetPositionDamping(targetCamera.CamData.cameraType, Vector3.zero);
                    _currentCam = targetCamera;
                    _currentCam.SetActive(true);
                    this.InvokeNextFrame(Initialization, 30);
                    return;
                }
            }

            cinemachineBrain.m_DefaultBlend.m_Time = shouldBlend ? _defaultBlendTime : 0;
            float blendTime = cinemachineBrain.m_DefaultBlend.m_Time;
            this.InvokeNextFrame(() =>
            {
                if (shouldBlend)
                {
                    CinemachineBlenderSettings blenderSettings = cinemachineBrain.m_CustomBlends;
                    bool isSettingsExist = blenderSettings != null;
                    CinemachineBlenderSettings.CustomBlend blend = new CinemachineBlenderSettings.CustomBlend();

                    if (isSettingsExist)
                    {
                        CinemachineBlenderSettings.CustomBlend[] blends = blenderSettings.m_CustomBlends;
                        blend = blends.FirstOrDefault(x =>
                            x.m_From == _currentCam.CamData.cameraName && x.m_To == targetCamera.CamData.cameraName);
                    }

                    blendTime = blend.m_From == null || blend.m_To == null
                        ? cinemachineBrain.m_DefaultBlend.m_Time
                        : blend.m_Blend.m_Time;
                }

                creaCams.ForEach(x => x.SetActive(false));
                targetCamera.SetActive(true);
                Debug.Log("Camera changed to {0} from {1}!".Format(_currentCam.CamData.cameraType,
                    targetCamera.CamData.cameraType));
                _currentCam = targetCamera;

                if (_activeCoroutine != null)
                {
                    StopCoroutine(_activeCoroutine);
                    onComplete?.Invoke(cameraType, TransitionResult.Overriden);
                }

                _activeCoroutine = this.InvokeWithDelay(blendTime,
                    () => { onComplete?.Invoke(cameraType, TransitionResult.Complete); });
            });
        }

        /// <returns>Active camera. (CanBeNull)</returns>
        private VirtualCam GetActiveCamera()
        {
            if (!IsInitialized()) return null;
            ICinemachineCamera activeCinemachineCamera = cinemachineBrain.ActiveVirtualCamera;
            return cinemachineBrain.ActiveVirtualCamera != null
                ? creaCams.FirstOrDefault(x => x.name == activeCinemachineCamera.Name)
                : null;
        }

        /// <param name="cameraType">Type to return.</param>
        /// <returns>Given camera type if exist. Otherwise null.</returns>
        public VirtualCam GetCamera(CreaCameraType cameraType)
        {
            VirtualCam selectedCamera = creaCams.FirstOrDefault(cam => cam.CamData.cameraType == cameraType);
            if (selectedCamera == null) Debug.LogError("Couldn't find camera {0}!".Format(cameraType));
            return selectedCamera;
        }

        private bool IsInitialized()
        {
            if (!isInitialized) Debug.LogError("Camera controller is not initialized properly!");
            return isInitialized;
        }

        /// <summary>
        /// Initializes the controller.
        /// </summary>
        public void Initialize()
        {
            bool isPrefabAvailable =
                cameraPrefab != null && cameraPrefab.GetComponentInChildren<VirtualCam>() != null;

            if (!isPrefabAvailable)
            {
#if UNITY_EDITOR
                string assetPath = Path.Combine("Assets/Scripts/Helpers/Camera", "DefaultVirtualCam.prefab");
                GameObject contentsRoot = (GameObject)AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(assetPath);
                cameraPrefab = contentsRoot;
                isPrefabAvailable =
                    cameraPrefab != null && cameraPrefab.GetComponentInChildren<VirtualCam>() != null;

                if (!isPrefabAvailable)
                {
                    Debug.LogError("Insert \"DefaultVirtualCam\" prefab before using the manager!");
                    cameraPrefab = null;
                }
#else
                Debug.LogError("Insert \"DefaultVirtualCam\" prefab before using the manager!");
                    cameraPrefab = null;
#endif
            }

            Transform controllerTransform = this.transform;
            Transform cameraParent =
                controllerTransform.FindDeepChild("Cameras") ?? new GameObject("Cameras").transform;
            creaCams = cameraParent.GetComponentsInChildren<VirtualCam>(true).ToList();
            creaCams.ForEach(x => x.SetActive(false));
            creaCams.FirstOrDefault(x => x.CamData.cameraType == 0)?.SetActive(true);
            mainCamera = UnityEngine.Camera.main;
            CreaCameraType[] cameraTypes = Enum.GetValues(typeof(CreaCameraType)) as CreaCameraType[];

            if (isPrefabAvailable && creaCams.Count < cameraTypes.Length)
            {
                int camTypeCount = cameraTypes.Length;
                cameraParent.parent = controllerTransform;

                for (int i = 0; i < camTypeCount; i++)
                {
                    VirtualCam currentCam = creaCams.FirstOrDefault(x => x.CamData.cameraType == cameraTypes[i]);
                    if (currentCam != null) continue;
#if UNITY_EDITOR
                    currentCam =
                        ((GameObject)PrefabUtility.InstantiatePrefab(cameraPrefab, cameraParent))
                        .GetComponentInChildren<VirtualCam>(true);
#else
                    currentCam =
                        Instantiate(cameraPrefab, cameraParent).GetComponentInChildren<CreaVirtualCam>(true);
#endif
                    currentCam.name = cameraTypes[i].ToString();
                    currentCam.Initialize();
                    creaCams.Add(currentCam);
                }

                creaCams.ForEach(x => x.SetActive(false));
                creaCams.FirstOrDefault(x => x.CamData.cameraType == 0)?.SetActive(true);
            }

            if (mainCamera == null) Debug.LogError("Couldn't find any camera on scene!");
            else
            {
                cinemachineBrain = mainCamera.GetComponent<CinemachineBrain>();

                if (cinemachineBrain == null)
                {
                    cinemachineBrain = mainCamera.gameObject.AddComponent<CinemachineBrain>();
#if UNITY_EDITOR
                    EditorUtility.SetDirty(mainCamera.gameObject);
#endif
                }
            }

            CheckConsistency();
            isInitialized = creaCams.Count > 0 && mainCamera != null && cinemachineBrain != null && isPrefabAvailable;
            IsInitialized();
#if UNITY_EDITOR
            EditorUtility.SetDirty(this.gameObject);
#endif
        }

        private void CheckConsistency()
        {
            bool whileLock = true;

            while (whileLock)
            {
                int camCount = creaCams.Count;
                bool repeatLock = false;

                for (int i = 0; i < camCount - 1; i++)
                {
                    if (creaCams[i].CamData.cameraType != creaCams[i + 1].CamData.cameraType) continue;
                    Debug.LogError(
                        "There was a duplicate of {0} camera! This won't be included in camera list. Please use one camera for each camera type."
                            .Format(creaCams[i].CamData.cameraType));
                    creaCams.RemoveAt(i + 1);
                    repeatLock = true;
                    break;
                }

                whileLock = repeatLock;
            }
        }
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(CameraController))]
    public class CreaCameraControllerEditor : Editor
    {
        private CameraController cameraController;
        private bool _isEditable;
        private SerializedProperty _cameraPrefab;
        private SerializedProperty _mainCamera;
        private SerializedProperty _cinemachineBrain;
        private SerializedProperty _creaCams;
        private SerializedProperty _isInitialized;

        private void OnEnable()
        {
            cameraController = (CameraController)target;
            cameraController.Initialize();
            _cameraPrefab = serializedObject.FindProperty("cameraPrefab");
            _mainCamera = serializedObject.FindProperty("mainCamera");
            _cinemachineBrain = serializedObject.FindProperty("cinemachineBrain");
            _creaCams = serializedObject.FindProperty("creaCams");
            _isInitialized = serializedObject.FindProperty("isInitialized");
        }

        public override void OnInspectorGUI()
        {
            if (GUILayout.Button("Toggle Edit")) _isEditable = !_isEditable;
            GUI.enabled = _isEditable;
            EditorGUILayout.PropertyField(_cameraPrefab);
            EditorGUILayout.PropertyField(_mainCamera);
            EditorGUILayout.PropertyField(_cinemachineBrain);
            EditorGUILayout.PropertyField(_creaCams, true);

            if (serializedObject.hasModifiedProperties)
            {
                serializedObject.ApplyModifiedProperties();
                cameraController.Initialize();
            }

            if (!_isInitialized.boolValue)
            {
                EditorGUILayout.TextArea("Camera controller isn't initialized properly!",
                    new GUIStyle
                    {
                        alignment = TextAnchor.MiddleCenter,
                        fontSize = 18, fontStyle = FontStyle.Bold,
                        normal = new GUIStyleState { textColor = Color.red },
                    });
            }
            else
            {
                EditorGUILayout.TextArea("Initialized",
                    new GUIStyle
                    {
                        alignment = TextAnchor.MiddleCenter,
                        fontSize = 18, fontStyle = FontStyle.Bold,
                        normal = new GUIStyleState { textColor = Color.green },
                    });
            }
        }
    }
#endif

    public enum CreaCameraType : byte
    {
        DefaultVirtualCam = 0, // Keep it as default.
        ScanRequest = 1,
        InfiniteStack = 2,
        ProgressMap = 3,
    }

    public enum TransitionResult : byte
    {
        Incomplete,
        Overriden,
        Complete,
    }
}