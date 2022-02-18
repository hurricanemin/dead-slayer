using System;
using Game.Character.Controller;
using Helpers.Utilities.AutomatedFieldSystem.CustomAttributes;
using UnityEngine;
using CharacterController = Game.Character.Controller.CharacterController;

public class TestSceneController : MonoBehaviour
{
    [SerializeField] [AutomatedField(SearchIn.CurrentScene, SearchType.FirstEncounter)]
    private GameCharacter playerChar;

    private CharacterController _characterController;

    private void Awake()
    {
        _characterController = new CharacterController();
        _characterController.PossessCharacter(playerChar);
    }
}