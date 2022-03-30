using System;
using Helpers.Utilities;
using Helpers.Utilities.Editor;
using UnityEditor;
using UnityEngine;

namespace Game.PhysicsRelated.Editor
{
    [CustomEditor(typeof(RagdollManager))]
    public class RagdollManagerEditor : UnityEditor.Editor
    {
        private RagdollManager _creator;
        private SerializedProperty _ragdollJoints;
        private SerializedProperty _currentBodyWeight;

        private void OnEnable()
        {
            _creator = target as RagdollManager;
            if (_creator == null || Application.isPlaying) return;
            GenerateJoints();
            _ragdollJoints = serializedObject.FindProperty("ragdollJoints");
            _currentBodyWeight = serializedObject.FindProperty("currentBodyWeight");
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            GUI.Box(EditorGUILayout.GetControlRect(),
                "Current Body Weight: {0}".Format(_currentBodyWeight.floatValue.Round(1)));
            EditorGUI.PropertyField(EditorGUILayout.GetControlRect(), _currentBodyWeight);
            if (GUI.Button(EditorGUILayout.GetControlRect(), "Change Body Weight"))
                SetWeight(_currentBodyWeight.floatValue);
            if (GUI.Button(EditorGUILayout.GetControlRect(), "Regulate Weight Ratios")) RegulateWeight();
            if (GUI.Button(EditorGUILayout.GetControlRect(), "CREATE RAGDOLL"))
                CreateRagdoll(
                    Mathf.Approximately(_currentBodyWeight.floatValue, 0) ? 75 : _currentBodyWeight.floatValue);
            if (GUI.Button(EditorGUILayout.GetControlRect(), "Initialize Ragdoll")) GenerateJoints();
            if (serializedObject.hasModifiedProperties) serializedObject.ApplyModifiedProperties();
        }

        private void GenerateJoints()
        {
            if (Application.isPlaying) return;
            _currentBodyWeight.floatValue = 0;
            Rigidbody[] ragdollRbs = _creator.GetComponentsInChildren<Rigidbody>();
            int ragdollCount = ragdollRbs.Length;
            string[] bodyParts = Enum.GetNames(typeof(UnityEditor.BodyPart));
            int partCount = bodyParts.Length;
            UnityEditor.BodyPart currentBodyPart = UnityEditor.BodyPart.None;

            for (int i = 0; i < ragdollCount; i++)
            {
                Rigidbody currentRigidbody = ragdollRbs[i];
                currentRigidbody.angularDrag = 0;
                _currentBodyWeight.floatValue += currentRigidbody.mass;
                currentRigidbody.gameObject.RemoveComponentsIfExists<RagdollJointBase>();

                for (int j = 0; j < partCount; j++)
                {
                    if (!currentRigidbody.name.ToLower().Contains(bodyParts[j].ToLower())) continue;
                    bool isArm = bodyParts[j] == "Arm";
                    bool isLeg = bodyParts[j] == "Leg";

                    if (isArm || isLeg)
                    {
                        string newName = isArm ? "ForeArm" : "UpLeg";

                        if (name.ToLower().Contains(newName.ToLower()))
                        {
                            EditorExtensions.AddTag(newName);
                            currentRigidbody.tag = newName;
                            currentBodyPart =
                                (UnityEditor.BodyPart)Enum.Parse(typeof(UnityEditor.BodyPart), newName, true);
                            break;
                        }
                    }

                    EditorExtensions.AddTag(bodyParts[j]);
                    currentRigidbody.tag = bodyParts[j];
                    currentBodyPart = (UnityEditor.BodyPart)j;
                    break;
                }

                RagdollJointBase currentJointBase = currentRigidbody.gameObject.AddComponent<RagdollJointBase>();
                currentJointBase.Initialize();
            }

            _creator.InitializeVariables();
            Debug.Log("Ragdoll joints initialized!");
        }

        private void CreateRagdoll(float bodyWeight)
        {
            if (Application.isPlaying) return;
            Transform selfTrans = _creator.transform;
            Transform pelvis = selfTrans.FindDeepChildContain(BodyPart.Hips.ToString(), isCaseSensitive: false);
            string direction = BodyDirection.Left.ToString();
            Transform leftHips =
                selfTrans.FindDeepChildContain(direction + BodyPart.UpLeg, isCaseSensitive: false);
            Transform leftKnee = selfTrans.FindDeepChildContain(direction + BodyPart.Leg, isCaseSensitive: false);
            Transform leftArm = selfTrans.FindDeepChildContain(direction + BodyPart.Arm, isCaseSensitive: false);
            Transform leftForeArm =
                selfTrans.FindDeepChildContain(direction + BodyPart.ForeArm, isCaseSensitive: false);
            direction = BodyDirection.Right.ToString();
            Transform rightHips = selfTrans.FindDeepChildContain(direction + BodyPart.UpLeg, isCaseSensitive: false);
            Transform rightKnee = selfTrans.FindDeepChildContain(direction + BodyPart.Leg, isCaseSensitive: false);
            Transform rightArm = selfTrans.FindDeepChildContain(direction + BodyPart.Arm, isCaseSensitive: false);
            Transform rightForeArm =
                selfTrans.FindDeepChildContain(direction + BodyPart.ForeArm, isCaseSensitive: false);
            Transform middleSpine = selfTrans.FindDeepChildContain(BodyPart.Spine + "2", isCaseSensitive: false);
            Transform head = selfTrans.FindDeepChildContain(":" + BodyPart.Head, isCaseSensitive: false);
            RagdollGenerator generator = new RagdollGenerator(pelvis, leftHips, leftKnee, rightHips,
                rightKnee, leftArm, leftForeArm, rightArm, rightForeArm, middleSpine, head, bodyWeight);
            generator.CreateRagdoll(GenerateJoints);
        }

        private void RegulateWeight()
        {
            int jointCount = _ragdollJoints.arraySize;

            for (int i = 0; i < jointCount; i++)
            {
                float ratio;
                if (!(_ragdollJoints.GetArrayElementAtIndex(i).objectReferenceValue is RagdollJointBase ragdollJoint))
                    continue;

                switch (ragdollJoint.BodyPartType)
                {
                    case BodyPart.Arm:
                    case BodyPart.ForeArm:
                    case BodyPart.Head:
                        ratio = 0.0625f;
                        break;
                    case BodyPart.Hips:
                    case BodyPart.Spine:
                        ratio = 0.15625f;
                        break;
                    case BodyPart.Leg:
                    case BodyPart.UpLeg:
                        ratio = 0.09375f;
                        break;
                    default:
                        ratio = 0.0625f;
                        break;
                }

                try
                {
                    ragdollJoint.Rigidbody.mass = _currentBodyWeight.floatValue * ratio;
                }
                catch
                {
                    break;
                }
            }

            EditorUtility.SetDirty(_creator.gameObject);
        }

        public void SetWeight(float targetWeight)
        {
            if (Application.isPlaying) return;
            float ratio = targetWeight / _currentBodyWeight.floatValue;
            int jointCount = _ragdollJoints.arraySize;

            for (int i = 0; i < jointCount; i++)
            {
                if (!(_ragdollJoints.GetArrayElementAtIndex(i).objectReferenceValue is RagdollJointBase ragdollJoint))
                    continue;

                try
                {
                    ragdollJoint.Rigidbody.mass *= ratio;
                }
                catch
                {
                    //
                }
            }

            _currentBodyWeight.floatValue = targetWeight;
            EditorUtility.SetDirty(_creator.gameObject);
        }
    }
}