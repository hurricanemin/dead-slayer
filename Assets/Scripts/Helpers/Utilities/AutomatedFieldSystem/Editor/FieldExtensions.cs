using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using Helpers.Utilities.AutomatedFieldSystem.CustomAttributes;
using Helpers.Utilities.Editor;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace Helpers.Utilities.AutomatedFieldSystem.Editor
{
    public static class FieldExtensions
    {
        [InitializeOnLoadMethod]
        private static void RegisterEvent()
        {
            Selection.selectionChanged += CheckClass;
        }

        [InitializeOnEnterPlayMode]
        private static void UnRegisterEvent()
        {
            Selection.selectionChanged -= CheckClass;
        }

        private static void CheckClass()
        {
            GameObject[] gameObjects = Selection.gameObjects;

            foreach (var gameObject in gameObjects)
            {
                Component[] componentsOnGo = gameObject.GetComponents<Component>();

                foreach (var component in componentsOnGo)
                {
                    if (component.IsComponentPartOfAPrefabInstance()) continue;
                    Type componentType = component.GetType();
                    List<Type> typesToSearch = componentType.FindParentClasses();
                    typesToSearch.Add(componentType);
                    bool hasAutomatedFields = false;

                    foreach (var type in typesToSearch)
                    {
                        hasAutomatedFields = type
                            .GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public).Any(x =>
                                x.CustomAttributes.FirstOrDefault(y => y.AttributeType == typeof(AutomatedField)) !=
                                null &&
                                x.IsSerialized());
                        if (hasAutomatedFields) break;
                    }

                    if (hasAutomatedFields)
                    {
                        component.SetVariables();
                        component.CheckVariables();
                    }

                    List<MethodInfo> methods = new List<MethodInfo>();

                    foreach (var type in typesToSearch)
                    {
                        MethodInfo[] foundMethods =
                            type.GetMethods(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)
                                .Where(x => x.CustomAttributes.FirstOrDefault(y =>
                                                y.AttributeType == typeof(FieldInitializer)) !=
                                            null).ToArray();

                        foreach (MethodInfo foundMethod in foundMethods)
                        {
                            bool isUnique = true;

                            foreach (var method in methods)
                            {
                                if (method.Name != foundMethod.Name) continue;
                                isUnique = false;
                                break;
                            }

                            if (isUnique) methods.Add(foundMethod);
                        }
                    }

                    foreach (var method in methods) method?.Invoke(component, new object[] { });
                }
            }
        }

        public static void SetVariables(this Component target)
        {
            Type indicatorType = target.GetType();
            List<FieldInfo> fieldInfos = new List<FieldInfo>();
            SerializedObject serializedObject = new SerializedObject(target);
            List<Type> classesToSearch = indicatorType.FindParentClasses();
            classesToSearch.Add(indicatorType);

            foreach (Type type in classesToSearch)
            {
                FieldInfo[] fields =
                    type.GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
                foreach (var field in fields)
                    if (fieldInfos.FirstOrDefault(x => x.Name == field.Name) == null)
                        fieldInfos.Add(field);
            }

            FieldInfo[] serializedFields = fieldInfos.Where(x =>
                x.CustomAttributes.FirstOrDefault(y => y.AttributeType == typeof(AutomatedField)) != null &&
                x.IsSerialized()).ToArray();

            foreach (var field in serializedFields)
            {
                AutomatedField automatedField =
                    (AutomatedField)Attribute.GetCustomAttribute(field, typeof(AutomatedField));
                string fieldName = string.IsNullOrEmpty(automatedField.NameOverride)
                    ? field.Name
                    : automatedField.NameOverride;
                SerializedProperty serializedProperty = serializedObject.FindProperty(field.Name);
                if (serializedProperty == null) continue;
                Transform targetTransform = target.FindTargetTransform(automatedField, fieldName);

                if (serializedProperty.isArray)
                {
                    Object[] objects = Array.Empty<Object>();
                    Type elementType = field.FieldType.GetElementType();

                    if (elementType == null)
                    {
                        Type[] genericTypes = field.FieldType.GetGenericArguments();
                        if (genericTypes.Length > 0) elementType = genericTypes.First();
                        if (elementType == null) continue;
                    }

                    switch (automatedField.SearchType)
                    {
                        case SearchType.ByName:
                            if (targetTransform == null) continue;

                            switch (automatedField.SearchIn)
                            {
                                default:
                                case SearchIn.Children:
                                case SearchIn.BaseParentsChildren:
                                    objects = targetTransform.GetComponentsInChildren(elementType)
                                        .ConvertToObjectArray();
                                    break;
                                case SearchIn.Parent:
                                case SearchIn.Root:
                                    objects = targetTransform.GetComponents(elementType).ConvertToObjectArray();
                                    break;
                                case SearchIn.CurrentScene:
                                    if (!target.IsObjectFromScene()) break;
                                    GameObject[] sceneObjects = target.gameObject.scene.GetRootGameObjects();
                                    List<Object> foundObjects = new List<Object>();
                                    foreach (var gameObject in sceneObjects)
                                        foundObjects.AddRange(gameObject.GetComponentsInChildren(elementType));
                                    objects = foundObjects.ToArray();
                                    break;
                            }

                            break;
                        default:
                        case SearchType.FirstEncounter:
                            switch (automatedField.SearchIn)
                            {
                                default:
                                case SearchIn.Children:
                                case SearchIn.BaseParentsChildren:
                                    objects = targetTransform.GetComponentsInChildren(elementType)
                                        .ConvertToObjectArray();
                                    break;
                                case SearchIn.Parent:
                                case SearchIn.Root:
                                    objects = targetTransform.GetComponents(elementType).ConvertToObjectArray();
                                    break;
                                case SearchIn.CurrentScene:
                                    if (!target.IsObjectFromScene()) break;
                                    GameObject[] sceneObjects = target.gameObject.scene.GetRootGameObjects();
                                    List<Object> foundObjects = new List<Object>();
                                    foreach (var gameObject in sceneObjects)
                                        foundObjects.AddRange(gameObject.GetComponentsInChildren(elementType));
                                    objects = foundObjects.ToArray();
                                    break;
                            }

                            break;
                    }

                    serializedProperty.InitializeArray(objects);
                }
                else
                {
                    if (serializedProperty.objectReferenceValue != null)
                    {
                        if (automatedField.SearchType != SearchType.ByName) continue;
                        if (string.Equals(serializedProperty.objectReferenceValue.name, fieldName,
                                StringComparison.CurrentCultureIgnoreCase)) continue;
                    }

                    Object foundObject = serializedProperty.objectReferenceValue;

                    switch (automatedField.SearchType)
                    {
                        case SearchType.ByName:
                            if (targetTransform == null) continue;

                            switch (automatedField.SearchIn)
                            {
                                default:
                                case SearchIn.Children:
                                case SearchIn.Parent:
                                case SearchIn.Root:
                                case SearchIn.BaseParentsChildren:
                                    foundObject = targetTransform.GetComponent(field.FieldType);
                                    break;
                                case SearchIn.CurrentScene:
                                    if (!target.IsObjectFromScene()) break;
                                    GameObject[] sceneObjects = target.gameObject.scene.GetRootGameObjects();

                                    foreach (var gameObject in sceneObjects)
                                    {
                                        if (string.Equals(gameObject.name, fieldName,
                                                StringComparison.CurrentCultureIgnoreCase))
                                        {
                                            foundObject = gameObject.GetComponent(field.FieldType);
                                            if (foundObject != null) break;
                                        }

                                        Transform found = gameObject.transform.FindDeepChild(fieldName);
                                        if (found == null) continue;
                                        foundObject = found.GetComponent(field.FieldType);
                                        if (foundObject != null) break;
                                    }

                                    GameObject foundGo = sceneObjects.FirstOrDefault(x =>
                                        x.name.ToLower(CultureInfo.InvariantCulture).ToLower() ==
                                        fieldName.ToLower(CultureInfo.InvariantCulture));
                                    if (foundGo == null) continue;
                                    foundGo.GetComponent(field.FieldType);
                                    foundObject = foundGo;
                                    break;
                            }

                            break;
                        default:
                        case SearchType.FirstEncounter:
                            switch (automatedField.SearchIn)
                            {
                                default:
                                case SearchIn.Children:
                                case SearchIn.BaseParentsChildren:
                                    foundObject = targetTransform.GetComponentInChildren(field.FieldType);
                                    break;
                                case SearchIn.Parent:
                                case SearchIn.Root:
                                    foundObject = targetTransform.GetComponent(field.FieldType);
                                    break;
                                case SearchIn.CurrentScene:
                                    if (!target.IsObjectFromScene()) break;
                                    GameObject[] sceneObjects = targetTransform.gameObject.scene.GetRootGameObjects();

                                    foreach (var gameObject in sceneObjects)
                                    {
                                        foundObject = gameObject.GetComponentInChildren(field.FieldType);
                                        if (foundObject != null) break;
                                    }

                                    break;
                            }

                            break;
                    }

                    serializedProperty.objectReferenceValue = foundObject;
                }
            }

            serializedObject.ApplyModifiedProperties();
        }

        private static Transform FindTargetTransform(this Component target, AutomatedField automatedField,
            string fieldName)
        {
            Transform targetTransform = target.transform;

            switch (automatedField.SearchType)
            {
                case SearchType.ByName:
                    switch (automatedField.SearchIn)
                    {
                        case SearchIn.Children:
                            targetTransform = targetTransform.FindDeepChild(fieldName, isCaseSensitive: false);
                            break;
                        case SearchIn.Parent:
                            targetTransform = targetTransform.FindParent(fieldName, isCaseSensitive: false);
                            break;
                        case SearchIn.BaseParentsChildren:
                            targetTransform = targetTransform.ReturnBaseParent()
                                .FindDeepChild(fieldName, isCaseSensitive: false);
                            break;
                        case SearchIn.CurrentScene:
                        case SearchIn.Root:
                        default:
                            break;
                    }

                    break;
                default:
                case SearchType.FirstEncounter:
                    switch (automatedField.SearchIn)
                    {
                        default:
                        case SearchIn.Root:
                        case SearchIn.Children:
                        case SearchIn.Parent:
                        case SearchIn.CurrentScene:
                            break;
                        case SearchIn.BaseParentsChildren:
                            targetTransform = targetTransform.ReturnBaseParent();
                            break;
                    }

                    break;
            }

            return targetTransform;
        }

        private static void InitializeArray(this SerializedProperty serializedProperty, Object[] elements)
        {
            if (elements == null) return;
            int objectCount = elements.Length;
            if (objectCount == 0) return;
            serializedProperty.ClearArray();
            for (int i = 0; i < objectCount; i++) serializedProperty.InsertArrayElementAtIndex(i);
            for (int i = 0; i < objectCount; i++)
                serializedProperty.GetArrayElementAtIndex(i).objectReferenceValue = elements[i];
        }

        private static Object[] ConvertToObjectArray(this Component[] components)
        {
            int componentCount = components.Length;
            Object[] objects = new Object[componentCount];
            for (int i = 0; i < componentCount; i++) objects[i] = components[i];
            return objects;
        }

        public static bool IsSerialized(this FieldInfo fieldInfo)
        {
            return !fieldInfo.IsNotSerialized && fieldInfo.IsPublic ||
                   fieldInfo.CustomAttributes.FirstOrDefault(
                       y => y.AttributeType == typeof(SerializeField)) != null;
        }

        private static bool CheckVariables(this Component component)
        {
            List<string> missingFields = component.FindAllMissingFields();
            if (missingFields.Count == 0) return true;
            string baseText = "Missing fields on {0}: ".Format(component.name);
            foreach (var missingField in missingFields) baseText += ", {0}".Format(missingField);
            Debug.LogError(baseText);
            return false;
        }

        public static List<string> FindAllMissingFields(this Component component)
        {
            List<string> missingFields = new List<string>();
            var so = new SerializedObject(component);
            SerializedProperty serializedProperty = so.GetIterator();

            while (serializedProperty.NextVisible(true))
            {
                if (serializedProperty.propertyType != SerializedPropertyType.ObjectReference) continue;
                if (serializedProperty.exposedReferenceValue != null ||
                    serializedProperty.objectReferenceValue != null) continue;
                if (!IsException(component, serializedProperty)) missingFields.Add(serializedProperty.name);
            }

            FieldInfo[] fieldInfos = component.GetType().GetFields(BindingFlags.Instance | BindingFlags.NonPublic)
                .Where(x => x.Name == "areReferencesSet").ToArray();
            if (fieldInfos.Length > 0) fieldInfos[0].SetValue(component, missingFields.Count == 0);
            return missingFields;
        }

        private static bool IsException(Component component, SerializedProperty serializedProperty)
        {
            switch (component)
            {
                case Image _:
                case Text _ when serializedProperty.name == "m_Material":
                case EventSystem _:
                    return true;
                default:
                    return false;
            }
        }
    }
}