//Quick Scripts by Jack Wilson, Wanderlight Games 2017.
//Thank you for purchasing this product.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
using QuickScripts;

[CustomEditor (typeof(QuickTrigger))]
[CanEditMultipleObjects]
public class QSEditor_QuickTrigger : Editor {

    [SerializeField]
	QuickTrigger _quickTrigger;
    
    private static readonly string[] _dontIncludeMe = new string[] 
    { "m_Script",
      "interactableTags"
    };

	public bool showHelp;
	static string helpText = "Quick Tips:\n1.You must add tags to the list of interactable tags to define what game objects will activate this trigger. To do this, " +
	                         "scroll down and tick 'Edit Tags'. Then, select a tag from the dropdown field and press Add Tag.\n\n" +
	                         "Your current interactable tags are stored and listed under 'Interactable Tags'.\n" +
	                         "When you're done adding or removing tags, it's recommended you untick 'Edit Tags'." +
	                         "\n\n2. Any object interacting with a trigger must have a Rigidbody, this is required by Unity." +
	                         "\n\n3. To set up events to happen when something enters, stays within, or exits the Trigger follow these steps:" +
	                         "\n- Click the + sign underneath TriggerEnter, TriggerStay or TriggerExit to add an event." +
	                         "\n- An empty object field will appear. You can drag and drop any object from the Hierarchy window into this field." +
	                         "\n- You can now use the dropdown menu for this object to access any Component on that object and edit most public variables or call public functions " +
	                         "within those Components. These edits or calls will happen when the tigger is entered, stayed within or exited depending how you set it up." +
	                         "\n\n4. To set up input requirements follow these examples: " +
	                         "\n(If you want the player to press a button etc to make the event happen. " +
	                         "This feature only applies to Trigger Stay events)" +
	                         "\n- Key Names are written in lower case. ie 'E' Key would be e. Some are abbreviated ie 'left ctrl'. Consult Unity's Online API for all key names." +
	                         "\n- Axis input names are taken from Unity's Input Manager. Make sure they match case, for example 'Fire1'. " +
	                         "To find Axis names go to Edit/Project Settings/Input." +
	                         "\n- Mouse input: \n0 = Left Click\n1= Right Click\n2 = Middle Click" +
	                         "\n\nFor more information on how to use Quick Trigger, see the User Guide.";
	bool showTags;
	string selectedTag;
    string tags;

	void OnEnable()
	{
		_quickTrigger = (MonoBehaviour)target as QuickTrigger;
       
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

		if (_quickTrigger.interactableTags.Count == 0)
		{
			EditorGUILayout.BeginFadeGroup (1);
			EditorGUILayout.HelpBox ("There are no interactable tags. Nothing will be able to use this trigger. \n Tick 'Edit Tags' to make changes.", MessageType.Error);
            EditorGUILayout.EndFadeGroup();
        }

        EditorGUILayout.LabelField("Interactable Tags:", EditorStyles.boldLabel);
        // Update Tags
        EditorGUILayout.LabelField(tags, EditorStyles.helpBox);
        tags = "";
        foreach (string t in _quickTrigger.interactableTags)
        {
            tags += t + "\n";
        }
        showTags = (bool)EditorGUILayout.Toggle ("Edit Tags", showTags);
		if (showTags)
		{
			EditorGUILayout.BeginFadeGroup(1);
			selectedTag = EditorGUILayout.TagField ("Saved Tags", selectedTag);
			if (GUILayout.Button ("Add Tag"))
			{
				_quickTrigger.AddTag (selectedTag);
			}
			if (GUILayout.Button ("Remove Tag"))
			{
				_quickTrigger.RemoveTag (selectedTag);
			}if (GUILayout.Button ("Remove Last"))
			{
				_quickTrigger.RemoveLast ();
			}
			EditorGUILayout.EndFadeGroup();
		}

        GUILayout.Space(20);
        EditorGUILayout.LabelField("TRIGGER EVENTS", EditorStyles.boldLabel);

        DrawPropertiesExcluding(serializedObject, _dontIncludeMe);

		if (GUI.changed)
		{
			EditorUtility.SetDirty (_quickTrigger);
			serializedObject.ApplyModifiedProperties ();
		}
	}

	void OpenUserGuide()
	{
		System.Diagnostics.Process.Start (Application.dataPath + "/QuickScripts/QuickScriptsUserGuide.pdf");
	}
}
