//Quick Scripts by Jack Wilson, Wanderlight Games 2017.
//Thank you for purchasing this product.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
using QuickScripts;

[CustomEditor (typeof(QuickGizmo))]
[CanEditMultipleObjects]
public class QSEditor_QuickGizmo : Editor {

	[SerializeField]
	QuickGizmo _quickGizmo;
	public bool showHelp;
		static string helpText = "Quick Tips:\n1. Gizmos are only seen in the editor. They are useful for locating empty game objects in the scene, " +
	                          "defining collider boundaries and drawing lines between objects." +
	                          "\n\n2. If the Gizmo is not showing, check that its color's alpha channel is above zero." +
	                          "\n\nFor more information on how to use Quick Gizmos, see the User Guide.";

	public bool specifyCollider;

	SerializedProperty gizmoType;
	SerializedProperty gizmoMesh;
	SerializedProperty gizmoColor;
	SerializedProperty gizmoRadius;
	SerializedProperty gizmoTarget;
	SerializedProperty gizmoCustomCollider;
	SerializedProperty hideBool;


	void OnEnable()
	{
		_quickGizmo = (MonoBehaviour)target as QuickGizmo;
		gizmoType = serializedObject.FindProperty ("gizmoType");
		gizmoMesh = serializedObject.FindProperty ("mesh");
		gizmoColor = serializedObject.FindProperty ("gizmoColor");
		gizmoRadius = serializedObject.FindProperty ("gizmoRadius");
		gizmoTarget = serializedObject.FindProperty ("target");
		gizmoCustomCollider = serializedObject.FindProperty ("chosenCollider");
		hideBool = serializedObject.FindProperty ("hideWhenNotSelected");
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

		EditorGUILayout.PropertyField (gizmoType);
		EditorGUILayout.PropertyField (gizmoColor);

		if (gizmoType.enumValueIndex == 2 || gizmoType.enumValueIndex == 5) // Gizmo is a Mesh
		{
			EditorGUILayout.BeginFadeGroup (1);
			EditorGUILayout.PropertyField (gizmoMesh);
			EditorGUILayout.EndFadeGroup ();
		} 
		if (gizmoType.enumValueIndex == 6) // Gizmo is a Line
		{
			EditorGUILayout.BeginFadeGroup (1);
			EditorGUILayout.PropertyField (gizmoTarget);
			EditorGUILayout.EndFadeGroup ();
		}
		if (gizmoType.enumValueIndex == 7) // Gizmo is a Collider
		{
			if (_quickGizmo.GetComponent<Collider> () == null) // Has no Collider Component
			{
				EditorGUILayout.BeginFadeGroup (1);
				EditorGUILayout.HelpBox ("There is no collider on this game object!", MessageType.Error);
				EditorGUILayout.EndFadeGroup ();
			} 

			if (gizmoType.enumValueIndex == 7 && _quickGizmo.GetComponent<CapsuleCollider> ()) // Has a Capsule Collider
			{
				EditorGUILayout.BeginFadeGroup (1);
				EditorGUILayout.HelpBox ("Unity cannot draw a capsule mesh. " +
					"If you want to draw a capsule, use type Mesh or Wireframe Mesh and select the capsule mesh.", MessageType.Error);
				EditorGUILayout.EndFadeGroup();
			}

			specifyCollider = (bool)EditorGUILayout.Toggle ("Use Specific Collider", specifyCollider);
			if (specifyCollider)
				EditorGUILayout.PropertyField (gizmoCustomCollider);
			
			_quickGizmo.UpdateGizmoCollider ();
		}

		// Show Radius Option unless Gizmo is a Mesh, Wireframe Mesh, Line or Collider
		if (gizmoType.enumValueIndex != 2 && gizmoType.enumValueIndex != 5 && gizmoType.enumValueIndex != 6 && gizmoType.enumValueIndex != 7)
		{
			EditorGUILayout.BeginFadeGroup (1);
			EditorGUILayout.PropertyField (gizmoRadius);
			EditorGUILayout.EndFadeGroup ();
		}

		EditorGUILayout.PropertyField (hideBool);

		if (GUI.changed)
		{
			EditorUtility.SetDirty (_quickGizmo);
			serializedObject.ApplyModifiedProperties ();
		}
	}

	void OpenUserGuide()
	{
		System.Diagnostics.Process.Start (Application.dataPath + "/QuickScripts/QuickScriptsUserGuide.pdf");
	}
}
