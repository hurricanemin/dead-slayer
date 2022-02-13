using UnityEditor;
using UnityEditor.Experimental.SceneManagement;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Helpers.Utilities.Editor
{
    public static class EditorExtensions
    {
        #region Animation

        public static void UpdateSkinnedMeshRendererBones(this SkinnedMeshRenderer skinnedMeshRenderer,
            Transform newRootBone)
        {
            string rootName = "";
            if (skinnedMeshRenderer.rootBone != null) rootName = skinnedMeshRenderer.rootBone.name;
            Transform newRoot = null;
            Transform[] bones = skinnedMeshRenderer.bones;
            Transform[] newBones = new Transform[bones.Length];
            Transform[] existingBones = newRootBone.GetComponentsInChildren<Transform>(true);

            for (int i = 0; i < bones.Length; i++)
            {
                if (bones[i] == null)
                {
                    Debug.LogError("Bone index of {0} doesn't exits!".Format(i));
                    return;
                }

                bool found = false;

                foreach (var newBone in existingBones)
                {
                    if (newBone.name == rootName) newRoot = newBone;
                    if (newBone.name != bones[i].name) continue;
                    newBones[i] = newBone;
                    found = true;
                }

                if (found) continue;
                Debug.LogError("Couldn't find bone {0} on target rig!".Format(bones[i].name));
                return;
            }

            if (newRoot == null)
            {
                Debug.LogError("Couldn't find root bone {0} on target rig!".Format(skinnedMeshRenderer.rootBone));
                return;
            }

            skinnedMeshRenderer.bones = newBones;
            skinnedMeshRenderer.rootBone = newRoot;
        }

        #endregion

        public static bool IsObjectFromPrefabStage(this GameObject gameObject)
        {
            try
            {
                if (gameObject.scene.name == null) return false;
                PrefabStage prefabStage = PrefabStageUtility.GetCurrentPrefabStage();
                if (prefabStage == null) return false;
                bool isSameObject = prefabStage.prefabContentsRoot.GetInstanceID() == gameObject.GetInstanceID();
                if (!isSameObject) return false;
            }
            catch
            {
                return false;
            }

            return true;
        }

        public static bool IsObjectFromScene(this GameObject gameObject)
        {
            try
            {
                string sceneName = gameObject.scene.name;
                bool isFromScene = !gameObject.IsObjectFromPrefabStage();
                if (!isFromScene) return false;
                isFromScene = sceneName != null;
                if (!isFromScene) return false;
                Transform mainParent = gameObject.transform.ReturnBaseParent();
                bool isChild = mainParent.gameObject.GetInstanceID() != gameObject.GetInstanceID();
                return isChild ? mainParent.IsObjectFromScene() : sceneName != gameObject.name;
            }
            catch
            {
                return false;
            }
        }

        public static bool IsObjectFromScene(this Component component)
        {
            try
            {
                GameObject gameObject = component.gameObject;
                return IsObjectFromScene(gameObject);
            }
            catch
            {
                return false;
            }
        }

        public static bool IsComponentPartOfAPrefabInstance(this Component component)
        {
            bool isInAScene = component.IsObjectFromScene();
            bool isInsidePrefab = component.IsComponentInsideOfAPrefab();
            return isInAScene && isInsidePrefab;
        }

        public static bool IsComponentPartOfAPrefab(this Component component)
        {
            try
            {
                bool isInAScene = component.IsObjectFromScene();
                bool isInsidePrefab = component.IsComponentInsideOfAPrefab();
                return !isInAScene && isInsidePrefab;
            }
            catch
            {
                return false;
            }
        }

        public static bool IsComponentInsideOfAPrefab(this Component component)
        {
            try
            {
                bool isRegular = PrefabUtility.GetPrefabAssetType(component) == PrefabAssetType.Regular;
                return isRegular;
            }
            catch
            {
                return false;
            }
        }

        public static void AddTag(string tag)
        {
            if (string.IsNullOrEmpty(tag)) return;
            Object[] asset = AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset");
            if (asset == null || asset.Length <= 0) return;
            SerializedObject so = new SerializedObject(asset[0]);
            SerializedProperty tags = so.FindProperty("tags");

            for (int i = 0; i < tags.arraySize; ++i)
            {
                if (tags.GetArrayElementAtIndex(i).stringValue != tag) continue;
                return;
            }

            tags.InsertArrayElementAtIndex(tags.arraySize);
            tags.GetArrayElementAtIndex(tags.arraySize - 1).stringValue = tag;
            so.ApplyModifiedProperties();
            so.Update();
        }
    }
}