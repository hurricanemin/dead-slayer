//Quick Scripts by Jack Wilson, Wanderlight Games 2017.
//Thank you for purchasing this product.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace QuickScripts
{

    [RequireComponent(typeof(Collider))]
    [AddComponentMenu("Quick Scripts/Quick Teleport")]
    public class QuickTeleport : MonoBehaviour
    {

        Collider myCol;
        public bool isActive = true;
        public float coolDownTime = 0.1f;
        public bool maintainInertia = true;
        public bool teleportParentsOfCollidingObject = true;
        [Space(10)]
        [Header("If more than one Destination, will choose randomly")]
        public List<GameObject> destinationNodes = new List<GameObject>();
        [Space(10)]
        [Header("Optional effects to play when teleport is used")]
        public ParticleSystem entryParticleFX;
        public bool playEntryFXAtPointOfContact = true;
        public ParticleSystem exitParticleFX;
        public bool playExitFXAtDestination = true;
        [Header("Optional audio to play when teleport is used")]
        public AudioClip entrySoundFX;
        public AudioClip exitSoundFX;
        private bool activated;
        public AudioSource entryAudioSource;
        public AudioSource exitAudioSource;
        public bool moveExitAudioToDestination;
        [Space(10)]
        [Header("Specify objects that can be teleported by including their tags here")]
        public List<string> interactableTags = new List<string>();

        #region Editor
        void OnDrawGizmos()
        {
            foreach (GameObject dest in destinationNodes)
            {
                if (dest != null)
                {
                    Gizmos.color = Color.cyan;
                    Gizmos.DrawLine(this.transform.position, dest.transform.position);
                    Gizmos.DrawRay(dest.transform.position, dest.transform.forward);
                }
            }
        }
        #endregion
        #region In-Game

        void Start()
        {
            myCol = GetComponent<Collider>();
            if (myCol == null)
            {
                Debug.Log("No Collider attached to the Teleporter! (gameobject " + this.name + ")");
            }
            else
            {
                myCol.isTrigger = true;
            }

            // Stop audio from looping
            if (entryAudioSource != null)
                entryAudioSource.loop = false;

            ErrorMessages();
        }

        void ErrorMessages()
        {
            if (destinationNodes.Count == 0)
                Debug.LogError(this.name + " | Quick Teleporter is set to Random Destination but no Random Destinations have been added.");

            if (interactableTags.Count == 0)
                Debug.LogError(this.name + " | Quick Teleporter has no interactable tags. Nothing will ever be teleported!", this.gameObject);

            if (entrySoundFX != null && entryAudioSource == null)
                Debug.LogError(this.name + " | Quick Teleporter has Entry Audio FX but no Entry Audio Source!", this.gameObject);

            if (exitSoundFX != null && exitAudioSource == null)
                Debug.LogError(this.name + " | Quick Teleporter has Exit Audio FX but no Exit Audio Source!", this.gameObject);
        }

        void OnTriggerEnter(Collider other)
        {
            if (isActive && !activated && interactableTags.Contains(other.tag.ToString()))
            {
                activated = true;
                GameObject destinationNode = SelectRandomDest();

                // Play the effects
                PlayFX(other.transform.position, destinationNode.transform.position);

                //Teleport the object
                TeleportObject(other.gameObject, destinationNode);

                // Begin the cooldown
                if (coolDownTime > 0)
                {
                    StartCoroutine(CoolDown());
                }

            }
        }

        void PlayFX(Vector3 entryPos, Vector3 exitPos)
        {
            EntryFX(entryPos);
            ExitFX(exitPos);
        }

        void EntryFX(Vector3 pos)
        {
            if (entryParticleFX != null)
            {
                if (playEntryFXAtPointOfContact)
                    entryParticleFX.transform.position = pos;

                entryParticleFX.Play();
            }

            if (entrySoundFX != null && entryAudioSource != null)
            {
                entryAudioSource.clip = entrySoundFX;
                entryAudioSource.Play();
            }
        }

        void ExitFX(Vector3 pos)
        {
            if (exitParticleFX != null)
            {
                if (playExitFXAtDestination)
                    exitParticleFX.transform.position = pos;

                exitParticleFX.Play();
            }

            if (exitSoundFX != null && exitAudioSource != null)
            {
                if (moveExitAudioToDestination)
                {
                    exitAudioSource.gameObject.transform.position = pos;
                }
                exitAudioSource.clip = exitSoundFX;
                exitAudioSource.Play();
            }
        }

        void TeleportObject(GameObject obj, GameObject destinationNode)
        {
            if (teleportParentsOfCollidingObject)
            {
                obj = obj.transform.root.gameObject;
            }

            obj.transform.position = destinationNode.transform.position;
            obj.transform.rotation = destinationNode.transform.rotation;
            if (!maintainInertia)
                obj.GetComponent<Rigidbody>().velocity = Vector3.zero;
        }

        IEnumerator CoolDown()
        {
            yield return new WaitForSecondsRealtime(coolDownTime);
            activated = false;
        }

        GameObject SelectRandomDest()
        {
            int r = Random.Range(0, destinationNodes.Count);
            return destinationNodes[r];
        }

        public void RefreshDestNodes() // Called From QSEditor_QuickTeleport
        {
            // Clear null spawns first
            for (int i = 0; i < destinationNodes.Count; i++)
            {
                GameObject dest = destinationNodes[i];
                if (dest == null)
                {
                    destinationNodes.Remove(destinationNodes[i]);
                }
            }
        }

        public void AddTag(string tag)
        {
            if (!interactableTags.Contains(tag))
                interactableTags.Add(tag);
            else
                Debug.LogWarning(this.name + " | Quick Teleport already has the " + tag + " tag assigned");
        }

        public void RemoveTag(string tag)
        {
            if (tag != null)
                interactableTags.Remove(tag);
        }

        public void RemoveLast()
        {
            interactableTags.RemoveAt(interactableTags.Count - 1);
        }

        #endregion

        #region Public Events

        public void SetCooldownTime(float t)
        {
            coolDownTime = t;
        }

        public void SetTeleportActive(bool b)
        {
            isActive = b;
        }

        public void SetMaintainInertia(bool b)
        {
            maintainInertia = b;
        }

        #endregion
    }
}
