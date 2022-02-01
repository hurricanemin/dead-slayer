//Quick Scripts by Jack Wilson, Wanderlight Games 2017.
//Thank you for purchasing this product.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
using QuickScripts;

[CustomEditor (typeof(QuickSpawner))]
[CanEditMultipleObjects]
public class QSEditor_QuickSpawner : Editor {

	[SerializeField]
	QuickSpawner _quickSpawner;
	public bool showHelp;
	static string helpText = "Quick Tips:\n" +
	                         "1. This script allows you to spawn one or more objects at multiple Spawn Points. It is NOT the Spawn Point, but " +
	                         "instead the 'central brain' that does the spawning and designates where objects will spawn. " +
	                         "\n\n2. You can click 'Create Spawn Point' to quickly make a new Spawn Point." +
	                         "\n\n3. ALL Spawn Points must have a Spawner ID that matches their associated Quick Spawner's ID, otherwise they will not link. " +
	                         "By default this will set itself automatically so there should be no need to edit it." +
	                         "\n\n4. If you need to delete a Spawn Point, you can use the 'Refresh Spawn Points' to automatically re-organise the list of Spawn Points. " +
	                         "\n\n5. Seconds Between Waves means how many seconds pass until a spawn is called. This means a message is sent to the available Spawn Points " +
	                         "to say 'It's time to spawn something!'.\nYou can add (or subtract) time from this with 'Add Time Between Waves'. " +
	                         "This will let you make spawn waves happen faster or slower over time. You can put a negative number in that field to reduce time between waves. " +
	                         "It will not go below 0.5 seconds." +
	                         "\n\n6. If you click the dropdown arrow next to 'Objects To Spawn' and change 'Size' 0 to 1 or more, you can drag and drop objects into these slots. " +
	                         "These are the objects the Quick Spawner will spawn at the Spawn Points.\nIf you add multiple objects to the list of Objects To Spawn, the Quick Spawner will spawn random selections from that list. To set up chance, " +
	                         "just add more or less of the same object." +
	                         "\n\nNOTE: The Quick Spawner will Instantiate objects. Please keep this in mind if building to a platform that runs on a lower processor. " +
	                         "Object pooling may be added in the future." +
	                         "\n\nFor more information on how to set up the Quick Spawner, see the User Guide.";

	void OnEnable(){
		_quickSpawner = (MonoBehaviour)target as QuickSpawner;
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

		DrawDefaultInspector ();

		if (GUILayout.Button ("Create Spawn Point"))
		{
			CreateSpawnPoint ();
		}

		if (GUILayout.Button ("Refresh Spawn Points"))
		{
			_quickSpawner = (MonoBehaviour)target as QuickSpawner; 
			_quickSpawner.RefreshSpawnPoints ();
		}

        if (GUI.changed)
        {
            EditorUtility.SetDirty(_quickSpawner);
            serializedObject.ApplyModifiedProperties();
        }
    }

	void CreateSpawnPoint()
	{
		// Set up required variables
		_quickSpawner = (MonoBehaviour)target as QuickSpawner; 
		string id = _quickSpawner.spawnerID;

		// Create the prefab
		GameObject spawnPoint = new GameObject("spawn point");

		// Select the prefab and update the components
		Selection.activeObject = spawnPoint;
		spawnPoint.AddComponent<QuickGizmo> ();
		SetSpawnPointGizmo (spawnPoint.GetComponent<QuickGizmo> ());
		spawnPoint.AddComponent<QS_SpawnPoint> ();
		spawnPoint.GetComponent<QS_SpawnPoint> ().spawnerID = id;

		// Move the prefab to the Editor's camera position
		var sceneView = SceneView.lastActiveSceneView;
		if (sceneView != null)
		{
			spawnPoint.transform.position = sceneView.camera.ViewportToWorldPoint (new Vector3 (0.5f, 0.5f, 10f));
		}

		// Child the prefab to its relevant Spawner and add it to the list of Spawn Points
		spawnPoint.transform.parent = _quickSpawner.gameObject.transform;
		_quickSpawner.RefreshSpawnPoints ();
	}

	void SetSpawnPointGizmo(QuickGizmo spGizmo)
	{
		Color spColor = new Color32 (77, 147, 217, 255);
		spGizmo.gizmoColor = spColor;
		spGizmo.gizmoColor.a = 0.4f;
		spGizmo.gizmoType = QuickGizmo.currentGizmoType.cube;
		spGizmo.gizmoRadius = 0.5f;
	}

	void OpenUserGuide()
	{
		System.Diagnostics.Process.Start (Application.dataPath + "/QuickScripts/QuickScriptsUserGuide.pdf");
	}
}
