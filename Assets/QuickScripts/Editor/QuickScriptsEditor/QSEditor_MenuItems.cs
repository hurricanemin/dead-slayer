//Quick Scripts by Jack Wilson, Wanderlight Games 2017.
//Thank you for purchasing this product.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
using QuickScripts;

public class QSEditor_MenuItems : Editor {

	static GameObject recentGO;

	[MenuItem ("Tools/Quick Scripts Presets/Quick Trigger")]								// Trigger
	static void CreateTrigger(MenuCommand quickTrigger)
	{
		CreateGameObject (quickTrigger, "Quick Trigger", true, typeof(QuickTrigger));
		AddGizmo (Color.green, 0.4f, 1, 0);
	}

	[MenuItem ("Tools/Quick Scripts Presets/Quick Spawner")]								// Spawner
	static void CreateSpawner(MenuCommand quickSpawner)
	{
		CreateGameObject (quickSpawner, "Quick Spawner", false, typeof(QuickSpawner));
		AddGizmo (Color.cyan, 0.4f, 1, 0);
		recentGO.GetComponent<QuickSpawner> ().SetSpawnerID ();
	}

	[MenuItem ("Tools/Quick Scripts Presets/Quick Teleport")]								// Teleport
	static void CreateTeleport(MenuCommand quickTeleport)
	{
		CreateGameObject (quickTeleport, "Quick Teleport", true, typeof(QuickTeleport));
		AddGizmo (Color.blue, 0.4f, 1, 2);
	}

	[MenuItem ("Tools/Quick Scripts Presets/Quick Light")]									// Light
	static void CreateLight(MenuCommand quickLight)
	{
		CreateGameObject (quickLight, "Quick Light", false, typeof(QuickLight));
	}

	[MenuItem ("Tools/Quick Scripts Presets/Quick Pendulum")]								// Pendulum
	static void CreatePendulum(MenuCommand quickPendulum)
	{
		CreateGameObject (quickPendulum, "Quick Pendulum", false, typeof(QuickPendulum));
		AddGizmo (Color.white, 0.5f, 0.3f, 1);
		// Make sub object
		GameObject go = new GameObject ();
		go.transform.parent = recentGO.transform;
		go.transform.localPosition = new Vector3 (0, -5, 0);;
		go.name = "Object";
	}

	[MenuItem ("Tools/Quick Scripts Presets/Quick Door")]									// Door
	static void CreateDoor(MenuCommand quickDoor)
	{
		//CreateGameObject (quickDoor, "Quick Door", true, typeof(QuickDoor));

		// Make pivot object
		GameObject go = new GameObject ();
		go.name = "Door Pivot";
		recentGO = go;
		AddGizmo (Color.cyan, 1f, 0.2f, 4);

		var sceneView = SceneView.lastActiveSceneView;
		if (sceneView != null)
		{
			go.transform.position = sceneView.camera.ViewportToWorldPoint (new Vector3 (0.5f, 0.5f, 10f));
		}

		// Make door container
		GameObject go2 = GameObject.CreatePrimitive(PrimitiveType.Cube);
		go2.transform.localScale = new Vector3 (2f, 4, 0.2f);
		go2.transform.SetParent (go.transform);
		go2.transform.localPosition = new Vector3 (1, 0, 0);
		go2.name = "Door";
		go2.AddComponent<QuickDoor> ();

		GameObjectUtility.SetParentAndAlign (go, quickDoor.context as GameObject);
		Undo.RegisterCreatedObjectUndo (go, "Create " + go.name);
		Selection.activeObject = go;
	}


	static void CreateGameObject(MenuCommand menuCommand, string name, bool hasCollider, System.Type type)
	{
		GameObject go = new GameObject ();
		recentGO = go;

		if (hasCollider)
			AddCollider (0, true);
		
		go.AddComponent (type);
		go.name = name;

		// Move the prefab to the Editor's camera position
		var sceneView = SceneView.lastActiveSceneView;
		if (sceneView != null)
		{
			go.transform.position = sceneView.camera.ViewportToWorldPoint (new Vector3 (0.5f, 0.5f, 10f));
		}

		GameObjectUtility.SetParentAndAlign (go, menuCommand.context as GameObject);
		Undo.RegisterCreatedObjectUndo (go, "Create " + go.name);
		Selection.activeObject = go;
	}

	/// <summary>
	/// Adds a Collider.
	/// </summary>
	/// <param name="colliderType"><para>0 = Box</para> <para>1= Sphere</para> <para>2 = Capsule.</para></param>
	/// <param name="go">Game Object.</param>
	/// <param name="isTrigger">If set to <c>true</c> is trigger.</param>
	static void AddCollider(int colliderType, bool isTrigger)
	{
		
		switch (colliderType)
		{
		case 0:
			recentGO.AddComponent<BoxCollider> ();
			break;
		case 1:
			recentGO.AddComponent<SphereCollider> ();
			break;
		case 2:
			recentGO.AddComponent <CapsuleCollider> ();
			break;
		}

		recentGO.GetComponent<Collider> ().isTrigger = isTrigger;

	}

	/// <summary>
	/// Adds the gizmo.
	/// </summary>
	/// <param name="gizmoColor">Gizmo color.</param>
	/// <param name="transparency">Transparency.</param>
	/// <param name="type">Type. 0 = Cube | 1 = Sphere | 2 = Collider | 3 = Wire Cube | 4 = Wire Sphere </param>
	static void AddGizmo (Color gizmoColor, float transparency, float radius, int type)
	{
		recentGO.AddComponent<QuickGizmo> ();
		QuickGizmo gizmo = recentGO.GetComponent<QuickGizmo> ();
		UnityEditorInternal.ComponentUtility.MoveComponentUp (gizmo);

		switch (type)
		{
		case 0:
			gizmo.gizmoType = QuickGizmo.currentGizmoType.cube;
			break;
		case 1:
			gizmo.gizmoType = QuickGizmo.currentGizmoType.sphere;
			break;
		case 2: 
			gizmo.gizmoType = QuickGizmo.currentGizmoType.collider;
			break;
		case 3:
			gizmo.gizmoType = QuickGizmo.currentGizmoType.wireframeCube;
			break;
		case 4:
			gizmo.gizmoType = QuickGizmo.currentGizmoType.wireframeSphere;
			break;
		}

		gizmo.gizmoColor = gizmoColor;
		gizmo.gizmoColor.a = transparency;
		gizmo.gizmoRadius = radius;

	}
}
