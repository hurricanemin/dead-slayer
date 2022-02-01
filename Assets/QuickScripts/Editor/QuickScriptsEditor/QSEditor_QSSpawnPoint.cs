//Quick Scripts by Jack Wilson, Wanderlight Games 2017.
//Thank you for purchasing this product.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
using QuickScripts;

[CustomEditor (typeof(QS_SpawnPoint))]
[CanEditMultipleObjects]
public class QSEditor_QSSpawnPoint : Editor {

	QS_SpawnPoint _qsSpawnPoint;
	public bool showHelp;
	static string helpText = "Quick Tips:\n" +
	                         "1. The QS_SpawnPoint is an empty transform which is used as the position the objects spawn to, from the associated QuickSpawner." +
	                         "\n\n2. Do not change the Spawner ID or else it will break the Spawner." +
	                         "\n\n3. Spawn Request Cooldown is the number of seconds until this Spawn Point is available to be spawned to again. " +
	                         "The countdown only begins when the Spawn Point is clear of whichever object it last spawned. This is useful for Quick Spawners that " +
	                         "have more than one Spawn Point." +
	                         "\n\nFor Example: If this Spawn Point spawns in health packs, and the player collects the health pack, this Spawn Point won't be able to spawn " +
	                         "another health pack until after the Spawn Request Cooldown time has passed. " +
	                         "\nNOTE: It won't immediately respawn once the timer has run out. The Quick Spawner which this Spawn Point is connected to " +
	                         "determines the time between spawn waves." +
	                         "\n\nFor more information on how to use Quick Spawner, see the User Guide.";
	

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