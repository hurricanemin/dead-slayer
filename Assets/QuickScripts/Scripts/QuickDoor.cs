//Quick Scripts by Jack Wilson, Wanderlight Games 2017.
//Thank you for purchasing this product.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace QuickScripts
{
    // This class has an Editor script which overrides OnInspectorGUI(). See QSEditor_QuickDoor.
    [AddComponentMenu("Quick Scripts/Quick Door")]
    public class QuickDoor : MonoBehaviour
    {

        public enum doorEnum
        {
            slideUp,
            slideDown,
            slideLeft,
            slideRight,
            slideForward,
            slideBackward,
            pivot
        }
        public doorEnum doorType;

        public enum moveEnum
        {
            linear,
            smooth
        }
        public moveEnum moveType;

        public GameObject pivotPoint;
        [Range(-179, 180)]
        public float pivotAmountX;
        [Range(-179, 180)]
        public float pivotAmountY;
        [Range(-179, 180)]
        public float pivotAmountZ;

        [Space(10)]
        public bool openDoor;
        public float duration = 1;

        private MeshRenderer doorMesh;
        public bool customMoveDistance;
        public float moveDistance;
        private float doorWidth;
        private float doorDepth;
        private float doorHeight;

        public AudioClip openStartAudioClip;
        public AudioClip openStopAudioClip;
        public AudioClip closeStartAudioClip;
        public AudioClip closeStopAudioClip;
        public bool extendSoundFX;
        public AudioSource audioSource;

        public bool hasAudio;
        bool playOpenSFX;
        bool playOpenStopSFX;
        bool playCloseSFX;
        bool playCloseStopSFX;

        Vector3 startPos;
        Vector3 openPos;
        Vector3 startRot;
        float openPercent;
        bool doorIsOpen;
        bool doorIsClosed;

        //For smooth movement type
        float velocity;
        float smooth;

        void Start()
        {
            if (hasAudio && audioSource != null)
            {
                audioSource.playOnAwake = false;
            }
            else if (hasAudio && audioSource == null)
            {
                Debug.Log("You must reference an Audio Source if you want to use Audio!");
            }

            if (GetComponent<MeshRenderer>())
            {
                SetUpMeshDistance();
            }


            startPos = transform.position;
            if (pivotPoint != null)
            {
                startRot = pivotPoint.transform.eulerAngles;
            }

            //For smooth movement type:
            velocity = 0;

            doorIsClosed = true;
            playOpenSFX = true;
        }

        void SetUpMeshDistance() // Called from start if Door has a Mesh Component
        {
            doorMesh = GetComponent<MeshRenderer>();
            doorWidth = doorMesh.bounds.size.x;
            doorDepth = doorMesh.bounds.size.z;
            doorHeight = doorMesh.bounds.size.y;
        }

        void FixedUpdate()
        {
            if (openDoor)
            {
                OpenDoor();
            }
            else if (!openDoor)
            {
                CloseDoor();
            }
            smooth = duration / 10;
        }

        /// <summary>
        /// Pivots the Door on Y Axis.
        /// </summary>
        /// <param name="amount">Amount = -5 to 5.</param>
        public void OpenDoorXPivot(float amount)
        {
            float cappedAmount = Mathf.Clamp(amount, -180, 180);
            pivotAmountX = cappedAmount;
            OpenDoor();
        }

        /// <summary>
        /// Pivots the Door on Y Axis.
        /// </summary>
        /// <param name="amount">Amount = -5 to 5.</param>
        public void OpenDoorYPivot(float amount)
        {
            float cappedAmount = Mathf.Clamp(amount, -180, 180);
            pivotAmountY = cappedAmount;
            OpenDoor();
        }

        /// <summary>
        /// Pivots the Door on Y Axis.
        /// </summary>
        /// <param name="amount">Amount = -5 to 5.</param>
        public void OpenDoorZPivot(float amount)
        {
            float cappedAmount = Mathf.Clamp(amount, -180, 180);
            pivotAmountZ = cappedAmount;
            OpenDoor();
        }

        /// <summary>
        /// Opens the door using the settings in the Inspector.
        /// </summary>
        public void OpenDoor()
        {
            if (!this.isActiveAndEnabled)
                return;


            if (!doorIsOpen && hasAudio)
            {
                PlayAudioFX(1);
            }

            openDoor = true;

            if (doorType == doorEnum.pivot)
            {
                OpenPivot();
            }
            else
            {
                OpenSlide();
            }
        }

        public void CloseDoor()
        {
            if (!doorIsClosed && hasAudio)
            {
                PlayAudioFX(3);
            }
            openDoor = false;

            if (doorType == doorEnum.pivot)
            {
                ClosePivot();
            }
            else
            {
                CloseSlide();
            }
        }

        void OpenSlide()
        {
            openPos = OpenPosition();
            if (moveType == moveEnum.smooth)
            {
                openPercent = Mathf.SmoothDamp(openPercent, 1, ref velocity, smooth);
                transform.position = Vector3.Lerp(startPos, openPos, openPercent);
            }
            else if (moveType == moveEnum.linear)
            {
                transform.position = Vector3.Lerp(startPos, openPos, openPercent = Mathf.Clamp(openPercent += Time.fixedDeltaTime / duration, 0, 1));
            }

            if (transform.position == openPos && !doorIsOpen)
                DoorIsOpen();
        }

        void OpenPivot()
        {
            openPercent = Mathf.Clamp(openPercent += Time.fixedDeltaTime / duration, 0, 1);
            Vector3 endRotEuler = OpenRotation();

            if (moveType == moveEnum.smooth)
            {
                pivotPoint.transform.eulerAngles = Vector3.Lerp(pivotPoint.transform.eulerAngles, endRotEuler, Time.fixedDeltaTime / duration);

                Vector3 pivotRot = pivotPoint.transform.eulerAngles;
                if (pivotRot.x == endRotEuler.x && pivotRot.y == endRotEuler.y && pivotRot.z == endRotEuler.z)
                    DoorIsOpen();

                // pivotPoint.transform.eulerAngles = Vector3.Lerp (startRot, OpenRotation (), openPercent);
            }
            else if (moveType == moveEnum.linear)
            {

                // Lock unused values to 0
                if (pivotAmountX == 0)
                    endRotEuler.x = 0;
                if (pivotAmountY == 0)
                    endRotEuler.y = 0;
                if (pivotAmountZ == 0)
                    endRotEuler.z = 0;

                pivotPoint.transform.eulerAngles = new Vector3(
                    Mathf.LerpAngle(startRot.x, endRotEuler.x, openPercent),
                    Mathf.LerpAngle(startRot.y, endRotEuler.y, openPercent),
                    Mathf.LerpAngle(startRot.z, endRotEuler.z, openPercent));


                //	Debug.Log ("start rot " + startRot.ToString () + " end rot " + endRotEuler.ToString ());
                //	Debug.Log (pivotAmountY / Mathf.PI + "|" + (pivotAmountY / Mathf.PI) * 180);
            }
            if (openPercent > 0.98f && !doorIsOpen)
            {
                DoorIsOpen();
            }
        }

        void DoorIsOpen()
        {
            doorIsOpen = true;
            doorIsClosed = false;
            if (hasAudio && audioSource != null)
            {
                audioSource.Stop();
                PlayAudioFX(2);
            }
        }

        void CloseSlide()
        {
            openPos = OpenPosition();
            if (moveType == moveEnum.smooth)
            {
                openPercent = Mathf.SmoothDamp(openPercent, 0, ref velocity, smooth);
                transform.position = Vector3.Lerp(startPos, openPos, openPercent);
            }
            else if (moveType == moveEnum.linear)
            {
                transform.position = Vector3.Lerp(startPos, openPos, openPercent = Mathf.Clamp(openPercent -= Time.fixedDeltaTime / duration, 0, 1));
            }

            if (transform.position == startPos && !doorIsClosed)
                DoorIsClosed();
        }

        void ClosePivot()
        {
            openPercent = Mathf.Clamp(openPercent -= Time.fixedDeltaTime / duration, 0, 1);
            Vector3 endRoteuler = OpenRotation();


            if (moveType == moveEnum.smooth)
            {
                pivotPoint.transform.eulerAngles = Vector3.Lerp(pivotPoint.transform.eulerAngles, startRot, Time.fixedDeltaTime / duration);

                Vector3 pivotRot = pivotPoint.transform.eulerAngles;
                if (pivotRot.x == startRot.x && pivotRot.y == startRot.y && pivotRot.z == startRot.z)
                    DoorIsClosed();

                //	pivotPoint.transform.eulerAngles = Vector3.Lerp (startRot, OpenRotation(), openPercent);
            }
            else if (moveType == moveEnum.linear)
            {
                pivotPoint.transform.rotation = Quaternion.Euler(
                    Mathf.LerpAngle(startRot.x, endRoteuler.x, openPercent),
                    Mathf.LerpAngle(startRot.y, endRoteuler.y, openPercent),
                    Mathf.LerpAngle(startRot.z, endRoteuler.z, openPercent));
            }

            if (openPercent < 0.02f && !doorIsClosed)
            {
                DoorIsClosed();
            }
        }

        void DoorIsClosed()
        {
            doorIsOpen = false;
            doorIsClosed = true;
            if (hasAudio && audioSource != null)
            {
                audioSource.Stop();
                PlayAudioFX(4);
            }
        }

        /// <summary>
        /// Finds the open position based on the movement direction.
        /// For pivoting doors, use OpenRotation.
        /// </summary>
        /// <returns>The open position as a Vector3.</returns>
        Vector3 OpenPosition()
        {
            switch (doorType)
            {
                case doorEnum.slideUp:
                    if (!customMoveDistance)
                    {
                        moveDistance = doorHeight;
                    }

                    openPos = startPos + (transform.up * moveDistance);
                    break;
                case doorEnum.slideDown:
                    if (!customMoveDistance)
                        moveDistance = doorHeight;

                    openPos = startPos + (transform.up * -moveDistance);
                    break;
                case doorEnum.slideLeft:
                    if (!customMoveDistance)
                        moveDistance = doorWidth;

                    openPos = startPos + (transform.right * -moveDistance);
                    break;
                case doorEnum.slideRight:
                    if (!customMoveDistance)
                        moveDistance = doorWidth;

                    openPos = startPos + (transform.right * moveDistance);
                    break;
                case doorEnum.slideForward:
                    if (!customMoveDistance)
                        moveDistance = doorDepth;

                    openPos = startPos + (transform.forward * moveDistance);
                    break;
                case doorEnum.slideBackward:
                    if (!customMoveDistance)
                        moveDistance = doorDepth;

                    openPos = startPos + (transform.forward * -moveDistance);
                    break;
                case doorEnum.pivot:
                    Debug.Log("A pivot cannot use a Vector 3! Use OpenRotation instead");
                    break;
            }
            return openPos;
        }

        /// <summary>
        /// Finds the open position based on the desired rotation.
        /// For sliding doors, use OpenPosition.
        /// </summary>
        /// <returns>The open position as a Quaternion.</returns>
        Vector3 OpenRotation()
        {
            Vector3 rot = new Vector3(startRot.x + pivotAmountX,
                              startRot.y + pivotAmountY,
                              startRot.z + pivotAmountZ);

            if (moveType == moveEnum.linear && doorType == doorEnum.pivot)
            {
                if (pivotAmountX < 0 || pivotAmountY < 0 || pivotAmountZ < 0)
                {
                    // If door needs to open on a negative angle, change the start rotation to be -0.1f so the Quaternion lerps correctly on the negative 180
                    startRot = Vector3.one * -0.1f;
                }
            }
            return rot;
        }

        /// <summary>
        /// Plays the Audio Effects.
        /// </summary>
        /// <param name="audioID"><para>1 = Open Start SFX</para> <para> 2 = Open Stop SFX</para> <para> 3 = Close Start SFX</para>  4 = Close Stop SFX</param>
        void PlayAudioFX(int audioID)
        {
            if (audioSource == null)
            {
                Debug.LogError(this.name + " | Quick Door Component is trying to play audio but there is no Audio Source!", this.gameObject);
                return;
            }
            else if (!audioSource.isPlaying)
            {
                switch (audioID)
                {
                    case 1:
                        playOpenStopSFX = true;
                        if (openStartAudioClip != null && playOpenSFX)
                        {
                            audioSource.clip = openStartAudioClip;
                            audioSource.Play();
                            if (extendSoundFX)
                            {
                                audioSource.loop = true;
                            }
                            else
                            {
                                playOpenSFX = false;
                            }
                        }
                        break;
                    case 2: // Open Stop
                        playCloseSFX = true;
                        if (openStopAudioClip != null && playOpenStopSFX)
                        {
                            audioSource.clip = openStopAudioClip;
                            audioSource.loop = false;
                            audioSource.Play();
                            playOpenStopSFX = false;
                        }
                        break;
                    case 3: // Close Start
                        playCloseStopSFX = true;
                        if (closeStartAudioClip != null && playCloseSFX)
                        {
                            audioSource.clip = closeStartAudioClip;
                            audioSource.Play();
                            if (extendSoundFX)
                            {
                                audioSource.loop = true;
                            }
                            else
                            {
                                playCloseSFX = false;
                            }
                        }
                        break;
                    case 4: // Close Stop
                        playOpenSFX = true;
                        if (closeStopAudioClip != null && playCloseStopSFX)
                        {
                            audioSource.clip = closeStopAudioClip;
                            audioSource.loop = false;
                            audioSource.Play();
                            playCloseStopSFX = false;
                        }
                        break;
                }
            }
        }

        #region Public Events
        public void SwapOpenClose()
        {
            if (openDoor || doorIsOpen)
                CloseDoor();
            else if (doorIsClosed)
                OpenDoor();
        }
        public void SetDuration(float t)
        {
            duration = t;
        }
        public void SetHasAudio(bool b)
        {
            hasAudio = b;
        }
        public void SetCustomMoveDistance(float x)
        {
            customMoveDistance = true;
            moveDistance = x;
        }
        #endregion
    }
}
