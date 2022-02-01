//Quick Scripts by Jack Wilson, Wanderlight Games 2017.
//Thank you for purchasing this product.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
using QuickScripts;

[CustomEditor (typeof(QuickMover))]
[CanEditMultipleObjects]
public class QSEditor_QuickMover : Editor {

	[SerializeField]
	QuickMover _quickMover;
	public bool showHelp;
	static string helpText = "Quick Tips: \n1. Make sure the Mover ID here matches the one on the mover nodes." +
	                         "\n\n" + "2. If you need to delete or replace nodes, click Rebuild to fix the path." +
	                         "\n\n" + "Note: It may reverse the order of your existing nodes. In which case, just tick 'reverse'." +
	                         "\n\n" + "For more detail on how to set up and use the Quick Mover, please consult the User Guide.";

	void OnEnable(){
		_quickMover = (MonoBehaviour)target as QuickMover;
	}

	public override void OnInspectorGUI()
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
		if (_quickMover.autoReturn && _quickMover.loop)
		{
			EditorGUILayout.BeginFadeGroup (1);
			EditorGUILayout.HelpBox ("Warning: You cannot have a Mover that is set to Loop and Auto Return. It must be one or the other.", MessageType.Error);
			EditorGUILayout.EndFadeGroup ();
		}
		DrawDefaultInspector ();

		if (GUILayout.Button ("Create Waypoint Node"))
		{
			CreateMoverNode ();
			// Instantly create a second one, setting the first as Start Pos
			if (_quickMover.nodeContainer.transform.childCount == 1)
				CreateMoverNode ();
		}
		if (GUILayout.Button ("Rebuild List"))
		{
			_quickMover.RefreshNodeList ();
		}

		if (GUI.changed)
		{
			EditorUtility.SetDirty (_quickMover);
			serializedObject.ApplyModifiedProperties ();
		}
	}

	void CreateMoverNode()
	{
		// Set up required variables
		_quickMover = (MonoBehaviour)target as QuickMover;
		string id = _quickMover.moverID;


		// Create the node parent game object
		if (GameObject.Find (_quickMover.name + " Mover Nodes") == null)
		{
			GameObject destGroup = new GameObject (_quickMover.name + " Mover Nodes");
			_quickMover.nodeContainer = destGroup;
		}
		// Create the prefab
		GameObject moverNode = new GameObject(id + " node");
		moverNode.transform.parent = GameObject.Find (_quickMover.name + " Mover Nodes").transform;
		moverNode.transform.SetAsLastSibling ();

		// Check if this is the start pos node

		// Select the prefab and update the components
		Selection.activeObject = moverNode;
		moverNode.AddComponent<QuickGizmo> ();
		SetGizmo (moverNode.GetComponent<QuickGizmo> ());
		moverNode.AddComponent<QS_MoverNode> ();
		moverNode.GetComponent<QS_MoverNode> ().moverID = id;

		// Move the prefab to the Editor's camera position
		var sceneView = SceneView.lastActiveSceneView;
		if (sceneView != null)
		{
			moverNode.transform.position = sceneView.camera.ViewportToWorldPoint (new Vector3 (0.5f, 0.5f, 10f));
		}


		Debug.Log (_quickMover.nodeContainer.name);
		if (_quickMover.nodeContainer.transform.childCount == 1)
		{
			moverNode.name = (id + " Start Pos");
			moverNode.transform.SetAsFirstSibling ();
		} 
	}

	void SetGizmo(QuickGizmo gizmo)
	{
		Color newColor = Color.cyan;
		gizmo.gizmoColor = newColor;
		gizmo.gizmoColor.a = 0.4f;
		gizmo.gizmoType = QuickGizmo.currentGizmoType.sphere;
		gizmo.gizmoRadius = 0.4f;
	}

	void OpenUserGuide()
	{
		System.Diagnostics.Process.Start (Application.dataPath + "/QuickScripts/QuickScriptsUserGuide.pdf");
	}
}
