using System;
using Cinemachine;
using Helpers.Utilities;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Helpers.Camera
{
    public class VirtualCam : CinemachineVirtualCamera
    {
        [SerializeField] private CreaVirtualCamData creaVirtualCamData;
        [HideInInspector] [SerializeField] private CreaVirtualCamRuntimeData defaults;
        [HideInInspector] [SerializeField] private bool isInitialized;
        [HideInInspector] [SerializeField] private Transform selfTransform;
        [HideInInspector] [SerializeField] private CinemachineTransposer transposer;
        [HideInInspector] [SerializeField] private CinemachineComposer composer;
        [HideInInspector] [SerializeField] private CinemachineBasicMultiChannelPerlin perlin;

        private CreaVirtualCamRuntimeData _current;

        public CreaVirtualCamData CamData => creaVirtualCamData;
        private bool _isActive;
        public bool IsUsable => IsInitialized();

        private bool _isLookingAtCustomTarget;
        private bool _isFollowingCustomTarget;

        private void Awake()
        {
            _current = defaults;
        }

        public void UpdateLookAtPosition(Vector3 position, bool shouldBlend)
        {
            if (!IsInitialized()) return;
            if (!shouldBlend && !_isLookingAtCustomTarget) NoLookDampingTemporary();
            if (_isLookingAtCustomTarget) ResetLookAt(shouldBlend);
            LookAt.position = position;
        }

        public void SetLookAtTarget(Transform targetLookAt, bool shouldBlend)
        {
            if (!IsInitialized()) return;
            if (!shouldBlend) NoLookDampingTemporary();
            LookAt = targetLookAt;
            _current.lookAtTarget = targetLookAt;
            _isLookingAtCustomTarget = targetLookAt != null &&
                                       targetLookAt.GetInstanceID() != defaults.lookAtTarget.GetInstanceID();
        }

        public void ResetLookAt(bool shouldBlend)
        {
            SetLookAtTarget(defaults.lookAtTarget, shouldBlend);
        }

        public void LookTowards(Transform targetLookAt)
        {
            if (!IsInitialized()) return;
            LookTowardsPosition(targetLookAt.position);
        }

        public void LookTowardsPosition(Vector3 position)
        {
            if (!IsInitialized()) return;
            LookAt = null;
            _isLookingAtCustomTarget = false;
            selfTransform.rotation =
                Quaternion.LookRotation((position - selfTransform.position).normalized, Vector3.up);
        }

        public void SetFollowOffset(Vector3 newOffset, bool shouldBlend)
        {
            if (!IsInitialized()) return;
            if (!shouldBlend) NoPositionDampingTemporary();
            transposer.m_FollowOffset = newOffset;
            _current.followOffset = newOffset;
        }

        public void ResetFollowOffset(bool shouldBlend)
        {
            SetFollowOffset(defaults.followOffset, shouldBlend);
        }

        public void SetFollowTarget(Transform followTarget, bool shouldBlend)
        {
            if (!IsInitialized()) return;
            if (!shouldBlend) NoPositionDampingTemporary();
            selfTransform.position = followTarget.position + transposer.m_FollowOffset;
            Follow = followTarget;
            _isFollowingCustomTarget = defaults.hasDefaultFollowTarget
                ? followTarget != null &&
                  followTarget.GetInstanceID() != defaults.followTarget.GetInstanceID()
                : followTarget != null;
            _current.followTarget = followTarget;
        }

        public void ResetFollowTarget(bool shouldBlend)
        {
            SetFollowTarget(defaults.followTarget, shouldBlend);
        }

        public void SetPositionDamping(Vector3 positionDamping, bool isTemporary = false)
        {
            if (!IsInitialized()) return;
            bool shouldUpdateX = !float.IsNaN(positionDamping.x);
            bool shouldUpdateY = !float.IsNaN(positionDamping.y);
            bool shouldUpdateZ = !float.IsNaN(positionDamping.z);
            positionDamping.x = shouldUpdateX ? Mathf.Abs(positionDamping.x) : transposer.m_XDamping;
            positionDamping.y = shouldUpdateY ? Mathf.Abs(positionDamping.y) : transposer.m_YDamping;
            positionDamping.z = shouldUpdateZ ? Mathf.Abs(positionDamping.z) : transposer.m_ZDamping;
            transposer.m_XDamping = positionDamping.x;
            transposer.m_YDamping = positionDamping.y;
            transposer.m_ZDamping = positionDamping.z;
            if (!isTemporary) _current.positionDamping = positionDamping;
        }

        public void ResetPositionDamping()
        {
            SetPositionDamping(defaults.positionDamping);
        }

        public void SetLookDamping(Vector2 lookDamping, bool isTemporary = false)
        {
            if (!IsInitialized()) return;
            bool shouldUpdateX = !float.IsNaN(lookDamping.x);
            bool shouldUpdateY = !float.IsNaN(lookDamping.y);
            lookDamping.x = shouldUpdateX ? Mathf.Abs(lookDamping.x) : composer.m_HorizontalDamping;
            lookDamping.y = shouldUpdateY ? Mathf.Abs(lookDamping.y) : composer.m_VerticalDamping;
            composer.m_HorizontalDamping = lookDamping.x;
            composer.m_VerticalDamping = lookDamping.y;
            if (!isTemporary) _current.lookDamping = lookDamping;
        }

        public void ResetLookDamping()
        {
            if (!IsInitialized()) return;
            composer.m_HorizontalDamping = defaults.lookDamping.x;
            composer.m_VerticalDamping = defaults.lookDamping.y;
        }

        public void SetActive(bool isActive)
        {
            if (!IsInitialized()) return;
            _isActive = isActive;
            enabled = isActive;
        }

        private void NoPositionDampingTemporary()
        {
            if (Mathf.Approximately(_current.positionDamping.sqrMagnitude, 0)) return;
            SetPositionDamping(Vector3.zero, true);
            this.InvokeNextFrame(() => SetPositionDamping(_current.positionDamping), 2);
        }

        private void NoLookDampingTemporary()
        {
            if (Mathf.Approximately(_current.lookDamping.sqrMagnitude, 0)) return;
            SetLookDamping(Vector2.zero, true);
            this.InvokeNextFrame(() => SetLookDamping(_current.lookDamping), 2);
        }

        private bool IsInitialized()
        {
            if (!isInitialized)
                Debug.LogError("Camera {0} is not initialized properly!".Format(creaVirtualCamData.cameraName));
            return isInitialized;
        }

        public void Initialize()
        {
#if UNITY_EDITOR
            if (Application.isPlaying) return;
#else
            Debug.LogError("Initializing CameraController on runtime. Please initialize it from editor.");
#endif
            transposer = GetComponentInChildren<CinemachineTransposer>();
            composer = GetComponentInChildren<CinemachineComposer>();
            perlin = GetComponentInChildren<CinemachineBasicMultiChannelPerlin>();
            bool isTransposerAvailable = transposer != null;
            bool isComposerAvailable = composer != null;
            bool isPerlinAvailable = perlin != null;
            selfTransform = transform;
            Transform parent = selfTransform.parent;
            if (defaults.followTarget == null) defaults.followTarget = this.Follow;

            if (isTransposerAvailable)
            {
                defaults.positionDamping =
                    new Vector3(transposer.m_XDamping, transposer.m_YDamping, transposer.m_ZDamping);
                defaults.followOffset = transposer.m_FollowOffset;
            }
            else
            {
                transposer = this.AddCinemachineComponent<CinemachineTransposer>();
            }

            if (isComposerAvailable)
            {
                defaults.lookDamping = new Vector2(composer.m_HorizontalDamping, composer.m_VerticalDamping);
            }
            else
            {
                composer = this.AddCinemachineComponent<CinemachineComposer>();
            }

            if (isPerlinAvailable)
            {
                NoiseSettings noiseProfile = perlin.m_NoiseProfile;
                if (noiseProfile == null) Debug.LogError("There is no noise profile on virtual cam perlin!");
            }
            else
            {
                perlin = this.AddCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
            }

            isTransposerAvailable = transposer != null;
            isComposerAvailable = composer != null;
            isPerlinAvailable = perlin != null;
            defaults.hasDefaultFollowTarget = defaults.followTarget != null;
            defaults.lookAtTarget = LookAt;
            Enum.TryParse(name, true, out creaVirtualCamData.cameraType);
            parent.name = name;
            creaVirtualCamData.cameraName = name;
            isInitialized = isTransposerAvailable && isComposerAvailable && isPerlinAvailable;
            IsInitialized();
#if UNITY_EDITOR
            EditorUtility.SetDirty(gameObject);
#endif
        }

        [Serializable]
        private struct CreaVirtualCamRuntimeData
        {
            public Vector3 positionDamping;
            public Vector2 lookDamping;
            public Vector3 followOffset;
            public Transform followTarget;
            public Transform lookAtTarget;
            public bool hasDefaultFollowTarget;
        }
    }

    [Serializable]
    public struct CreaVirtualCamData
    {
        public CreaCameraType cameraType;
        [HideInInspector] public string cameraName;
    }

    public enum CameraNoiseType : byte
    {
        Explosion,
    }
}