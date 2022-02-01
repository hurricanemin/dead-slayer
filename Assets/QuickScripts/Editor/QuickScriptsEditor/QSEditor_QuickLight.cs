//Quick Scripts by Jack Wilson, Wanderlight Games 2017.
//Thank you for purchasing this product.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
using QuickScripts;

[CustomEditor (typeof (QuickLight))]
[CanEditMultipleObjects]
public class QSEditor_QuickLight : Editor {

	[SerializeField]
	QuickLight _quickLight;

	public bool showHelp;
	static string helpText = "Quick Tips:\n" +
	                         "1. Adjust the intensity setting on the main Light component to set the maximum intensity for the Light." +
	                         "\n\n2. Tick 'Override Color' to set a custom color animation. The gradient will automatically match the duration of the light animation and will loop." +
	                         "\n\n3. You can change the gradient mode in the gradient window. Experiment with Blend and Fixed to see what works best for you." +
	                         "\n\n4. Lights with the Quick Light component can't be baked because they are animated. Keep them set to Realtime." +
	                         "\n\nFor more information, see the User Guide.";

	SerializedProperty lightType;
	SerializedProperty lightSpeed;
	SerializedProperty lightCustomAnim; // "lightOverTime"
	SerializedProperty overrideColorBool;
	SerializedProperty lightGradient;

	void OnEnable()
	{
		_quickLight = (MonoBehaviour)target as QuickLight;

		lightType = serializedObject.FindProperty ("lightType");
		lightSpeed = serializedObject.FindProperty ("speed");
		lightCustomAnim = serializedObject.FindProperty ("lightOverTime");
		overrideColorBool = serializedObject.FindProperty ("overrideColor");
		lightGradient = serializedObject.FindProperty ("colorGradient");
	}

		public override void OnInspectorGUI ()
	{
		showHelp = (bool)EditorGUILayout.Toggle ("Show Help", showHelp);
		if (showHelp)
		{
			EditorGUILayout.BeginFadeGroup (1);
			EditorGUILayout.HelpBox (helpText, MessageType.None);
			if (GUILayout.Button ("User Guide"))
				OpenUserGuide ();
			EditorGUILayout.EndFadeGroup ();
		}

		EditorGUILayout.PropertyField (lightType);
		if (lightType.enumValueIndex == 3) // If Strobe
		{
			EditorGUILayout.BeginFadeGroup (1);
			EditorGUILayout.HelpBox ("Warning: Those who have a history of epilepsy may be affected by this setting. Use with caution.", MessageType.Warning);
			EditorGUILayout.EndFadeGroup ();

		}
		if (lightType.enumValueIndex == 11) // Custom Light
		{
			EditorGUILayout.BeginFadeGroup (1);
			EditorGUILayout.HelpBox ("'Light Over Time' is an Animation Curve that represents the intensity of the Light over time.", MessageType.None);
			EditorGUILayout.PropertyField (lightCustomAnim);
			EditorGUILayout.EndFadeGroup ();
		}
		EditorGUILayout.PropertyField (lightSpeed);

		EditorGUILayout.Space ();
		EditorGUILayout.PropertyField (overrideColorBool);
		if (overrideColorBool.boolValue == true)
		{
			EditorGUILayout.BeginFadeGroup (1);
			EditorGUILayout.PropertyField (lightGradient);
			EditorGUILayout.EndFadeGroup ();
		}

		if (GUI.changed)
		{
			EditorUtility.SetDirty (_quickLight);
			serializedObject.ApplyModifiedProperties ();
		}
	}

	void OpenUserGuide()
	{
		System.Diagnostics.Process.Start (Application.dataPath + "/QuickScripts/QuickScriptsUserGuide.pdf");
	}
}
