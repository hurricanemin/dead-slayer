//Quick Scripts by Jack Wilson, Wanderlight Games 2017.
//Thank you for purchasing this product.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
using QuickScripts;

[CustomEditor (typeof(QuickTeleport))]
[CanEditMultipleObjects]
public class QSEditor_QuickTeleport : Editor {

	[SerializeField]
	QuickTeleport _quickTeleport;

    private static readonly string[] _dontIncludeMe = new string[]
   { "m_Script",
      "interactableTags"
   };

    public bool showTags;
	string selectedTag;
    string tags;
    public bool showHelp;
	static string helpText = "Quick Tips:\n1. Click 'Create Destination Node' to make a point to teleport to." +
	                         "\n\n2. Add more than one destination node to create a teleporter that sends to random destinations." +
	                         "\n\n3. Remember to fill out the list of interactable tags for what game objects can be teleported." +
	                         "\n\n4. Teleported objects will face the same way as the node they teleport to. The direction the node is facing is indicated by the small " +
	                         "line protruding from it." +
	                         "\n\n5. If Cool Down Time is set to 0, the teleport will only trigger once. Default is 0.1." +
	                         "\n\n6. Create an Audio Source and assign it to the Entry and Exit Audio Source fields if you want to play audio when the teleport is used." +
	                         "\n\n7. Tick 'Move Exit Audio To Destination' to play the Exit Audio where at the Destination. " +
	                         "\nNOTE: This will move the game object that contains the Audio Source component." +
	                         "\n\nFor more information, see the User Guide.";


	void OnEnable()
	{
		_quickTeleport = (MonoBehaviour)target as QuickTeleport;
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


        if (_quickTeleport.interactableTags.Count == 0)
        {
            EditorGUILayout.BeginFadeGroup(1);
            EditorGUILayout.HelpBox("There are no interactable tags. Nothing will be able to use this trigger. \n Tick 'Edit Tags' to make changes.", MessageType.Error);
            EditorGUILayout.EndFadeGroup();
        }

        EditorGUILayout.Space ();
        EditorGUILayout.LabelField("Interactable Tags:", EditorStyles.boldLabel);
        // Update Tags
        EditorGUILayout.LabelField(tags, EditorStyles.helpBox);
        tags = "";
        foreach (string t in _quickTeleport.interactableTags)
        {
            tags += t + "\n";
        }
		showTags = (bool)EditorGUILayout.Toggle ("Edit Tags", showTags);
		if (showTags)
		{
			EditorGUILayout.BeginFadeGroup(1);
			selectedTag = EditorGUILayout.TagField ("Interactable Tag", selectedTag);
			if (GUILayout.Button ("Add Tag"))
			{
				_quickTeleport.AddTag (selectedTag);
			}
			if (GUILayout.Button ("Remove Tag"))
			{
				_quickTeleport.RemoveTag (selectedTag);
			}if (GUILayout.Button ("Remove Last"))
			{
				_quickTeleport.RemoveLast ();
			}
            EditorGUILayout.EndFadeGroup();
		}
        GUILayout.Space (10);
        //Draw Rest of Script
        DrawPropertiesExcluding(serializedObject, _dontIncludeMe);
        GUILayout.Space(10);

        if (GUILayout.Button ("Create Destination Node"))
		{
			CreateDestNode ();
		}
		if (GUILayout.Button ("Refresh Destination Nodes"))
		{
			_quickTeleport = (MonoBehaviour)target as QuickTeleport; 
			_quickTeleport.RefreshDestNodes ();
		}

        if (GUI.changed)
        {
            EditorUtility.SetDirty(_quickTeleport);
            serializedObject.ApplyModifiedProperties();
        }

    }

	void CreateDestNode()
	{
		// Set up required variables
			_quickTeleport = (MonoBehaviour)target as QuickTeleport; 

		// Create the destination node
		GameObject teleDest = new GameObject("teleport destination");

		// Select the prefab and update the components
		Selection.activeObject = teleDest;
		teleDest.AddComponent<QuickGizmo> ();
		SetGizmo (teleDest.GetComponent<QuickGizmo> ());
		

		// Move the prefab to the Editor's camera position
		var sceneView = SceneView.lastActiveSceneView;
		if (sceneView != null)
		{
			teleDest.transform.position = sceneView.camera.ViewportToWorldPoint (new Vector3 (0.5f, 0.5f, 10f));
		}

		// Child the prefab to its relevant Spawner and add it to the list of Spawn Points
		teleDest.transform.parent = _quickTeleport.transform;
		_quickTeleport.destinationNodes.Add (teleDest);
	}

	void SetGizmo(QuickGizmo gizmo)
	{
		Color newColor = Color.blue;
		gizmo.gizmoColor = newColor;
		gizmo.gizmoColor.a = 0.4f;
		gizmo.gizmoType = QuickGizmo.currentGizmoType.sphere;
        gizmo.gizmoRadius = 0.3f;
	}


	void OpenUserGuide()
	{
		System.Diagnostics.Process.Start (Application.dataPath + "/QuickScripts/QuickScriptsUserGuide.pdf");
	}
}
