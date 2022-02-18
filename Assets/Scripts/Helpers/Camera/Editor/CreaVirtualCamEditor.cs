using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Cinemachine;
using Cinemachine.Editor;
using Cinemachine.Utility;
using UnityEditor;
using UnityEngine;

namespace Helpers.Camera.Editor
{
    [CustomEditor(typeof(VirtualCam))]
    [CanEditMultipleObjects]
    public class CreaVirtualCamEditor : CinemachineVirtualCameraBaseEditor<CinemachineVirtualCamera>
    {
        private VirtualCam virtualCam;
        private SerializedProperty _isInitialized;

        protected override void OnEnable()
        {
            base.OnEnable();
            IsPrefab = Target.gameObject.scene.name == null;
            UpdateStaticData();
            UpdateStageDataTypeMatchesForMultiSelection();
            Undo.undoRedoPerformed += ResetTargetOnUndo;
            virtualCam = (VirtualCam)target;
            virtualCam.Initialize();
            _isInitialized = serializedObject.FindProperty("isInitialized");
        }

        public override void OnInspectorGUI()
        {
            BeginInspector();
            DrawHeaderInInspector();
            DrawPropertyInInspector(FindProperty(x => x.m_Priority));
            DrawTargetsInInspector(FindProperty(x => x.m_Follow), FindProperty(x => x.m_LookAt));
            DrawRemainingPropertiesInInspector();
            DrawPipelineInInspector();
            DrawExtensionsWidgetInInspector();
            if (serializedObject.hasModifiedProperties) virtualCam.Initialize();

            if (!_isInitialized.boolValue)
            {
                EditorGUILayout.TextArea("Camera is not initialized!",
                    new GUIStyle
                    {
                        alignment = TextAnchor.MiddleCenter,
                        fontSize = 18, fontStyle = FontStyle.Bold,
                        normal = new GUIStyleState { textColor = Color.red },
                    });
            }
            else
            {
                EditorGUILayout.TextArea("Camera is properly initialized!",
                    new GUIStyle
                    {
                        alignment = TextAnchor.MiddleCenter,
                        fontSize = 18, fontStyle = FontStyle.Bold,
                        normal = new GUIStyleState { textColor = Color.green },
                    });
            }
        }

        private struct StageData
        {
            private string ExpandedKey
            {
                get { return "CNMCN_Core_Vcam_Expanded_" + Name; }
            }

            public bool IsExpanded
            {
                get => EditorPrefs.GetBool(ExpandedKey, false);
                set => EditorPrefs.SetBool(ExpandedKey, value);
            }

            public string Name;
            public Type[] types; // first entry is null
            public GUIContent[] PopupOptions;
        }

        private static StageData[] _sStageData = null;
        private bool[] m_hasSameStageDataTypes = new bool[Enum.GetValues(typeof(CinemachineCore.Stage)).Length];

        // Instance data - call UpdateInstanceData() to refresh this
        private int[] _mStageState = null;
        private bool[] _mStageError = null;
        private CinemachineComponentBase[] _mComponents;
        private UnityEditor.Editor[] _mComponentEditors = Array.Empty<UnityEditor.Editor>();
        private bool IsPrefab { get; set; }

        private void ResetTargetOnUndo()
        {
            UpdateInstanceData();
            ResetTarget();
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            Undo.undoRedoPerformed -= ResetTargetOnUndo;
            // Must destroy editors or we get exceptions
            if (_mComponentEditors == null) return;
            foreach (UnityEditor.Editor e in _mComponentEditors)
                if (e != null)
                    DestroyImmediate(e);
        }

        private Vector3 mPreviousPosition;

        private void OnSceneGUI()
        {
            if (!Target.UserIsDragging)
                mPreviousPosition = Target.transform.position;
            if (Selection.Contains(Target.gameObject) && Tools.current == Tool.Move
                                                      && Event.current.type == EventType.MouseDrag)
            {
                // User might be dragging our position handle
                Target.UserIsDragging = true;
                Vector3 delta = Target.transform.position - mPreviousPosition;
                if (!delta.AlmostZero())
                {
                    OnPositionDragged(delta);
                    mPreviousPosition = Target.transform.position;
                }
            }
            else if (GUIUtility.hotControl == 0 && Target.UserIsDragging)
            {
                // We're not dragging anything now, but we were
                InspectorUtility.RepaintGameView();
                Target.UserIsDragging = false;
            }
        }

        private void OnPositionDragged(Vector3 delta)
        {
            if (_mComponentEditors == null) return;
            foreach (UnityEditor.Editor e in _mComponentEditors)
            {
                if (e == null) continue;
                MethodInfo mi = e.GetType().GetMethod("OnVcamPositionDragged"
                    , BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
                if (mi != null && e.target != null)
                {
                    mi.Invoke(e, new object[] { delta });
                }
            }
        }

        private void DrawPipelineInInspector()
        {
            UpdateInstanceData();
            foreach (CinemachineCore.Stage stage in Enum.GetValues(typeof(CinemachineCore.Stage)))
            {
                int index = (int)stage;

                // Skip pipeline stages that have no implementations
                if (index < 0 || _sStageData[index].PopupOptions.Length <= 1)
                    continue;

                const float indentOffset = 4;

                GUIStyle stageBoxStyle = GUI.skin.box;
                EditorGUILayout.BeginVertical(stageBoxStyle);
                Rect rect = EditorGUILayout.GetControlRect(true);

                // Don't use PrefixLabel() because it will link the enabled status of field and label
                GUIContent label = new GUIContent(InspectorUtility.NicifyClassName(stage.ToString()));
                if (_mStageError[index])
                    label.image = EditorGUIUtility.IconContent("console.warnicon.sml").image;
                float labelWidth = EditorGUIUtility.labelWidth - (indentOffset + EditorGUI.indentLevel * 15);
                Rect r = rect;
                r.width = labelWidth;
                EditorGUI.LabelField(r, label);
                r = rect;
                r.width -= labelWidth;
                r.x += labelWidth;

                EditorGUI.BeginChangeCheck();
                GUI.enabled = !StageIsLocked(stage);
                EditorGUI.showMixedValue = !m_hasSameStageDataTypes[index];
                int newSelection = EditorGUI.Popup(r, _mStageState[index], _sStageData[index].PopupOptions);
                EditorGUI.showMixedValue = false;
                GUI.enabled = true;
                Type type = _sStageData[index].types[newSelection];
                if (EditorGUI.EndChangeCheck())
                {
                    SetPipelineStage(stage, type);
                    if (newSelection != 0)
                        _sStageData[index].IsExpanded = true;
                    UpdateInstanceData(); // because we changed it
                    ResetTarget(); // to allow multi-selection correctly adjust every target 

                    return;
                }

                if (type != null)
                {
                    Rect stageRect = new Rect(
                        rect.x - indentOffset, rect.y, rect.width + indentOffset, rect.height);
                    _sStageData[index].IsExpanded = EditorGUI.Foldout(
                        stageRect, _sStageData[index].IsExpanded, GUIContent.none, true);
                    if (_sStageData[index].IsExpanded)
                    {
                        // Make the editor for that stage
                        UnityEditor.Editor e = GetEditorForPipelineStage(stage);
                        if (e != null)
                        {
                            ++EditorGUI.indentLevel;
                            EditorGUILayout.Separator();
                            e.OnInspectorGUI();

                            EditorGUILayout.Separator();
                            --EditorGUI.indentLevel;
                        }
                    }
                }

                EditorGUILayout.EndVertical();
            }
        }

        private bool StageIsLocked(CinemachineCore.Stage stage)
        {
            if (IsPrefab) return true;
            CinemachineCore.Stage[] locked = Target.m_LockStageInInspector;
            if (locked == null) return false;
            for (int i = 0; i < locked.Length; ++i)
                if (locked[i] == stage)
                    return true;
            return false;
        }

        private UnityEditor.Editor GetEditorForPipelineStage(CinemachineCore.Stage stage)
        {
            if (_mComponentEditors == null) return null;
            foreach (UnityEditor.Editor e in _mComponentEditors)
            {
                if (e == null) continue;
                CinemachineComponentBase c = e.target as CinemachineComponentBase;
                if (c != null && c.Stage == stage) return e;
            }

            return null;
        }

        /// <summary>
        /// Register with CinemachineVirtualCamera to create the pipeline in an undo-friendly manner
        /// </summary>
        [InitializeOnLoad]
        private class CreatePipelineWithUndo
        {
            static CreatePipelineWithUndo()
            {
                CinemachineVirtualCamera.CreatePipelineOverride =
                    (CinemachineVirtualCamera vCam, string name, CinemachineComponentBase[] copyFrom) =>
                    {
                        // Create a new pipeline
                        GameObject go = InspectorUtility.CreateGameObject(name);
                        Undo.RegisterCreatedObjectUndo(go, "created pipeline");
                        bool partOfPrefab = PrefabUtility.IsPartOfAnyPrefab(vCam.gameObject);
                        if (!partOfPrefab)
                            Undo.SetTransformParent(go.transform, vCam.transform, "parenting pipeline");
                        Undo.AddComponent<CinemachinePipeline>(go);

                        // If copying, transfer the components
                        if (copyFrom != null)
                        {
                            foreach (Component c in copyFrom)
                            {
                                Component copy = Undo.AddComponent(go, c.GetType());
                                Undo.RecordObject(copy, "copying pipeline");
                                ReflectionHelpers.CopyFields(c, copy);
                            }
                        }

                        return go.transform;
                    };
                CinemachineVirtualCamera.DestroyPipelineOverride = Undo.DestroyObjectImmediate;
            }
        }

        private void SetPipelineStage(CinemachineCore.Stage stage, Type type)
        {
            Undo.SetCurrentGroupName("Cinemachine pipeline change");

            // Get the existing components
            for (int j = 0; j < targets.Length; j++)
            {
                var vCam = targets[j] as CinemachineVirtualCamera;
                Transform owner = vCam.GetComponentOwner();
                if (owner == null) continue; // maybe it's a prefab
                CinemachineComponentBase[] components = owner.GetComponents<CinemachineComponentBase>() ??
                                                        Array.Empty<CinemachineComponentBase>();
                // Find an appropriate insertion point
                int numComponents = components.Length;
                int insertPoint = 0;
                for (insertPoint = 0; insertPoint < numComponents; ++insertPoint)
                    if (components[insertPoint].Stage >= stage)
                        break;

                // Remove the existing components at that stage
                for (int i = numComponents - 1; i >= 0; --i)
                {
                    if (components[i].Stage != stage) continue;
                    Undo.DestroyObjectImmediate(components[i]);
                    components[i] = null;
                    --numComponents;
                    if (i < insertPoint) --insertPoint;
                }

                // Add the new stage
                if (type == null) continue;
                MonoBehaviour b = Undo.AddComponent(owner.gameObject, type) as MonoBehaviour;
                while (numComponents-- > insertPoint) UnityEditorInternal.ComponentUtility.MoveComponentDown(b);
            }
        }

        // This code dynamically discovers eligible classes and builds the menu
        // data for the various component pipeline stages.
        private static void UpdateStaticData()
        {
            if (_sStageData != null)
                return;
            _sStageData = new StageData[Enum.GetValues(typeof(CinemachineCore.Stage)).Length];

            var stageTypes = new List<Type>[Enum.GetValues(typeof(CinemachineCore.Stage)).Length];
            for (int i = 0; i < stageTypes.Length; ++i)
            {
                _sStageData[i].Name = ((CinemachineCore.Stage)i).ToString();
                stageTypes[i] = new List<Type>();
            }

            // Get all ICinemachineComponents
            var allTypes
                = ReflectionHelpers.GetTypesInAllDependentAssemblies(
                    (Type t) => typeof(CinemachineComponentBase).IsAssignableFrom(t) && !t.IsAbstract);

            // Create a temp game object so we can instance behaviours
            GameObject go = new GameObject("Cinemachine Temp Object");
            go.hideFlags = HideFlags.DontSaveInBuild | HideFlags.DontSaveInEditor;
            foreach (Type t in allTypes)
            {
                MonoBehaviour b = go.AddComponent(t) as MonoBehaviour;
                CinemachineComponentBase c = b != null ? (CinemachineComponentBase)b : null;
                if (c != null)
                {
                    CinemachineCore.Stage stage = c.Stage;
                    stageTypes[(int)stage].Add(t);
                }
            }

            DestroyImmediate(go);

            // Create the static lists
            for (int i = 0; i < stageTypes.Length; ++i)
            {
                stageTypes[i].Insert(0, null); // first item is "none"
                _sStageData[i].types = stageTypes[i].ToArray();
                GUIContent[] names = new GUIContent[_sStageData[i].types.Length];
                for (int n = 0; n < names.Length; ++n)
                {
                    if (n == 0)
                    {
                        bool useSimple
                            = (i == (int)CinemachineCore.Stage.Aim)
                              || (i == (int)CinemachineCore.Stage.Body);
                        names[n] = new GUIContent((useSimple) ? "Do nothing" : "none");
                    }
                    else
                        names[n] = new GUIContent(InspectorUtility.NicifyClassName(_sStageData[i].types[n].Name));
                }

                _sStageData[i].PopupOptions = names;
            }
        }

        private void GetPipelineTypes(CinemachineVirtualCamera vCam, ref Type[] types)
        {
            for (int i = 0; i < types.Length; ++i) types[i] = null;
            if (vCam == null) return;
            var components = vCam.GetComponentPipeline();
            for (int j = 0; j < components.Length; ++j) types[(int)components[j].Stage] = components[j].GetType();
        }

        // scratch buffers for pipeline types
        private Type[] _mPipelineTypeCache0 = new Type[Enum.GetValues(typeof(CinemachineCore.Stage)).Length];
        private Type[] _mPipelineTypeCacheN = new Type[Enum.GetValues(typeof(CinemachineCore.Stage)).Length];

        private void UpdateStageDataTypeMatchesForMultiSelection()
        {
            for (int i = 0; i < m_hasSameStageDataTypes.Length; ++i)
                m_hasSameStageDataTypes[i] = true;

            if (targets.Length > 1)
            {
                GetPipelineTypes(serializedObject.targetObjects[0] as CinemachineVirtualCamera,
                    ref _mPipelineTypeCache0);
                for (int i = 1; i < targets.Length; ++i)
                {
                    GetPipelineTypes(serializedObject.targetObjects[i] as CinemachineVirtualCamera,
                        ref _mPipelineTypeCacheN);
                    for (int j = 0; j < _mPipelineTypeCache0.Length; ++j)
                        if (_mPipelineTypeCache0[j] != _mPipelineTypeCacheN[j])
                            m_hasSameStageDataTypes[j] = false;
                }
            }
        }

        private void UpdateInstanceData()
        {
            // Invalidate the target's cache - this is to support Undo
            for (int i = 0; i < targets.Length; i++)
            {
                var cam = targets[i] as CinemachineVirtualCamera;
                if (cam != null)
                    cam.InvalidateComponentPipeline();
            }

            UpdateStageDataTypeMatchesForMultiSelection();
            UpdateComponentEditors();
            UpdateStageState(_mComponents);
        }

        // This code dynamically builds editors for the pipeline components.
        // Expansion state is cached statically to preserve foldout state.
        private void UpdateComponentEditors()
        {
            if (Target == null)
            {
                _mComponents = Array.Empty<CinemachineComponentBase>();
                return;
            }

            CinemachineComponentBase[] components = Target.GetComponentPipeline();
            int numComponents = components != null ? components.Length : 0;
            if (_mComponents == null || _mComponents.Length != numComponents)
                _mComponents = new CinemachineComponentBase[numComponents];
            bool dirty = (numComponents == 0);
            for (int i = 0; i < numComponents; ++i)
            {
                if (_mComponents[i] == null || components[i] != _mComponents[i])
                {
                    dirty = true;
                    _mComponents[i] = components[i];
                }
            }

            if (dirty)
            {
                // Destroy the subeditors
                if (_mComponentEditors != null)
                    foreach (UnityEditor.Editor e in _mComponentEditors)
                        if (e != null)
                            DestroyImmediate(e);

                // Create new editors
                _mComponentEditors = new UnityEditor.Editor[numComponents];
                for (int i = 0; i < numComponents; ++i)
                {
                    List<MonoBehaviour> behaviours = new List<MonoBehaviour>();
                    for (int j = 0; j < targets.Length; j++)
                    {
                        var cinemachineVirtualCamera = targets[j] as CinemachineVirtualCamera;
                        if (cinemachineVirtualCamera == null)
                            continue;

                        var behaviour =
                            cinemachineVirtualCamera.GetCinemachineComponent(components[i].Stage) as MonoBehaviour;
                        if (behaviour != null)
                            behaviours.Add(behaviour);
                    }

                    var behaviourArray = behaviours.ToArray();
                    if (behaviourArray.Length > 0 && m_hasSameStageDataTypes[(int)components[i].Stage])
                        CreateCachedEditor(behaviourArray, null, ref _mComponentEditors[i]);
                }
            }
        }

        private void UpdateStageState(CinemachineComponentBase[] components)
        {
            _mStageState = new int[Enum.GetValues(typeof(CinemachineCore.Stage)).Length];
            _mStageError = new bool[Enum.GetValues(typeof(CinemachineCore.Stage)).Length];
            foreach (var c in components)
            {
                CinemachineCore.Stage stage = c.Stage;
                int index = 0;
                for (index = _sStageData[(int)stage].types.Length - 1; index > 0; --index)
                    if (_sStageData[(int)stage].types[index] == c.GetType())
                        break;
                _mStageState[(int)stage] = index;
                _mStageError[(int)stage] = c == null || !c.IsValid;
            }
        }

        // Because the cinemachine components are attached to hidden objects, their
        // gizmos don't get drawn by default.  We have to do it explicitly.
        [InitializeOnLoad]
        private static class CollectGizmoDrawers
        {
            static CollectGizmoDrawers()
            {
                MGizmoDrawers = new Dictionary<Type, MethodInfo>();
                string definedIn = typeof(CinemachineComponentBase).Assembly.GetName().Name;
                Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
                foreach (Assembly assembly in assemblies)
                {
                    // Note that we have to call GetName().Name.  Just GetName() will not work.
                    if ((!assembly.GlobalAssemblyCache)
                        && ((assembly.GetName().Name == definedIn)
                            || assembly.GetReferencedAssemblies().Any(a => a.Name == definedIn)))
                    {
                        try
                        {
                            foreach (var type in assembly.GetTypes())
                            {
                                try
                                {
                                    bool added = false;
                                    foreach (var method in type.GetMethods(
                                        BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static))
                                    {
                                        if (added)
                                            break;
                                        if (!method.IsStatic)
                                            continue;
                                        var attributes =
                                            method.GetCustomAttributes(typeof(DrawGizmo), true) as DrawGizmo[];
                                        foreach (var a in attributes)
                                        {
                                            if (!typeof(CinemachineComponentBase).IsAssignableFrom(a.drawnType) ||
                                                a.drawnType.IsAbstract) continue;
                                            MGizmoDrawers.Add(a.drawnType, method);
                                            added = true;
                                            break;
                                        }
                                    }
                                }
                                catch (Exception)
                                {
                                    //
                                } // Just skip uncooperative types
                            }
                        }
                        catch (Exception)
                        {
                            //
                        } // Just skip uncooperative assemblies
                    }
                }
            }

            public static readonly Dictionary<Type, MethodInfo> MGizmoDrawers;
        }

        [DrawGizmo(GizmoType.Active | GizmoType.InSelectionHierarchy, typeof(CinemachineVirtualCamera))]
        internal static void DrawVirtualCameraGizmos(CinemachineVirtualCamera vCam, GizmoType selectionType)
        {
            var pipeline = vCam.GetComponentPipeline();
            if (pipeline == null) return;

            foreach (var c in pipeline)
            {
                if (c == null) continue;
                if (!CollectGizmoDrawers.MGizmoDrawers.TryGetValue(c.GetType(), out var method)) continue;
                if (method != null)
                    method.Invoke(null, new object[] { c, selectionType });
            }
        }
    }
}