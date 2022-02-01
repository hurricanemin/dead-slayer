//Quick Scripts by Jack Wilson, Wanderlight Games 2017.
//Thank you for purchasing this product.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
using QuickScripts;

[CustomEditor (typeof (QuickPendulum))]
[CanEditMultipleObjects]
public class QSEditor_QuickPendulum : Editor {

	//QuickPendulum _quickPendulum;
	static string helpText = "Quick Tips:" +
		"\n1. The pendulum will use this game object's transform position as the pivot point and it will override any rotation applied to this transform." +
		"\n\n2. Child other game objects to this one for the pendulum effect." +
		"\n\nFor more information on how to use the Quick Pendulum, see the User Guide.";
	public bool showHelp;

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
		DrawDefaultInspector ();

		if (GUI.changed)
		{
			serializedObject.ApplyModifiedProperties ();
		}
	}

	void OpenUserGuide()
	{
		System.Diagnostics.Process.Start (Application.dataPath + "/QuickScripts/QuickScriptsUserGuide.pdf");
	}
}
