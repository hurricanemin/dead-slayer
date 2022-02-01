//Quick Scripts by Jack Wilson, Wanderlight Games 2017.
//Thank you for purchasing this product.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace QuickScripts
{

    // This class has an Editor script which overrides OnInspectorGUI(). See QSEditor_QuickMover.
    [AddComponentMenu("Quick Scripts/Quick Mover")]
    public class QuickMover : MonoBehaviour
    {

        GameObject moverObject;

        public string moverID;
        [Range(0, 20)]
        public float moveSpeed = 2;
        public enum speedMovementType
        {
            linear,
            smoothed
        }
        public speedMovementType moveType;
        private float smoothSpeed;
        private float storedSpeed;
        [Space(10)]
        public bool isMoving = true;
        public bool reverseDirection;
        public bool loop;
        public bool autoReturn;

        public enum rotationEnum
        {
            byTurnSpeed,
            nodeToNode,
        }
        [Header("Rotation Options")]
        public bool faceForward;
        public bool stayLevel;
        public rotationEnum rotationType;
        public float turnSpeed = 1;
        private float turnPercent;

        [Header("The below fields will fill automatically")]
        public GameObject nodeContainer;
        public List<QS_MoverNode> moverNodes = new List<QS_MoverNode>();

        private QS_MoverNode destinationNode;
        private QS_MoverNode previousNode;
        private float distanceToNode;
        private int prevNodeInt;
        private Quaternion prevFacingRot;
        Color drawLineColor;

        bool isAtDest;
        bool newDestSet;
        bool wait;
        bool playMode;
        float waitTimer;

        // For QS version 1.1.0
        [Header("Optional different start position")]
        public QS_MoverNode newStartNode;
        int startNodeInt;


        #region Editor
        public void OnInspectorGUI()
        {
            if (GUILayout.Button("Refresh List"))
            {
                RefreshNodeList();
            }

        }
        void OnDrawGizmos()
        {
            // Update ID
            if (moverID == null)
                moverID = this.transform.name + transform.GetInstanceID();


            //	 Add nodes to list
            foreach (QS_MoverNode node in GameObject.FindObjectsOfType(typeof(QS_MoverNode)))
            {
                if (node.moverID == moverID && !moverNodes.Contains(node))
                {
                    moverNodes.Add(node);
                }
                if (node.name == moverID + " Start Pos" && moverNodes.IndexOf(node) != 0)
                {
                    moverNodes.Remove(node);
                    moverNodes.Insert(0, node);
                }
            }
            if (reverseDirection)
            {
                drawLineColor = Color.yellow;
            }
            else
            {
                drawLineColor = Color.cyan;
            }

            // Specify which node is next and tells each node which node to draw a line to
            foreach (QS_MoverNode node in moverNodes)
            {
                if (node == null)
                {
                    moverNodes.Remove(node);
                }
                else
                {
                    int nextNodeValue = moverNodes.IndexOf(node) + 1;
                    if (nextNodeValue < moverNodes.Count)
                    {
                        node.DrawLineToNode(moverNodes[nextNodeValue].gameObject, drawLineColor);
                    }
                    else if (loop)
                    {
                        node.DrawLineToNode(moverNodes[0].gameObject, drawLineColor);
                    }
                    else
                    {
                        node.nextNode = null;
                    }

                    // Put the first node on the platform's current pos so it starts there
                    if (!playMode && node.transform.GetSiblingIndex() == 0)
                        node.transform.position = this.transform.position;
                }
            }
        }

        public void RefreshNodeList()
        {
            moverNodes.Clear();
            {
                foreach (Transform t in nodeContainer.transform)
                {
                    QS_MoverNode node = t.GetComponent<QS_MoverNode>();
                    if (!moverNodes.Contains(node))
                    {
                        moverNodes.Add(node);
                    }
                }
            }
        }
        #endregion

        #region In-Game
        void Start()
        {
            playMode = true;
            storedSpeed = moveSpeed;
            moverObject = this.gameObject;

            if (newStartNode != null)
                startNodeInt = moverNodes.IndexOf(newStartNode) + 1;

            if (!reverseDirection) // if standard direction of movement
            {
                destinationNode = moverNodes[startNodeInt];
            }
            else if (reverseDirection) // if reverse direction
            {
                destinationNode = moverNodes[moverNodes.Count - startNodeInt];
            }

            if (startNodeInt > 1)
                moverObject.transform.position = moverNodes[startNodeInt - 1].transform.position;

            wait = false;

        }

        void Update()
        {
            if (distanceToNode < 0.2f)
            {
                isAtDest = true;
                WaitCheck();
            }
            else
            {
                isAtDest = false;
                newDestSet = false;
            }

        }

        void FixedUpdate()
        {
            //Move the platform to the next node
            if (destinationNode != null)
                distanceToNode = Vector3.Distance(moverObject.transform.position, destinationNode.transform.position);

            if (!isAtDest && isMoving)
            {
                MovePlatform();
            }
            else
            {
                //WaitCheck ();
            }

            if (faceForward)
                RotatePlatform();

        }

        void MovePlatform() // Called from FixedUpdate
        {
            Vector3 direction = (moverObject.transform.position - destinationNode.transform.position).normalized;

            if (moveType == speedMovementType.linear)
            {
                moverObject.transform.position -= direction * Time.fixedDeltaTime * moveSpeed;
            }
            else if (moveType == speedMovementType.smoothed)
            {
                Vector3 pos = moverObject.transform.position;
                Vector3 rearPos;
                float distanceToRear;

                GetPrevNode();

                rearPos = moverNodes[prevNodeInt].transform.position;
                distanceToRear = Vector3.Distance(rearPos, pos);

                if (distanceToNode > distanceToRear)
                {
                    smoothSpeed = distanceToRear + (Time.fixedDeltaTime * moveSpeed);
                }
                else if (distanceToNode < distanceToRear)
                {
                    smoothSpeed = distanceToNode + (Time.fixedDeltaTime * moveSpeed);
                }

                moverObject.transform.position -= direction * Time.fixedDeltaTime * smoothSpeed;
            }
        }

        void RotatePlatform() // Called from Fixed Update
        {
            // Standard rotation
            Quaternion newRotation;
            Quaternion lookAtRotation;

            if (distanceToNode > 0.1f)
            {
                newRotation = Quaternion.LookRotation(destinationNode.transform.position - transform.position);

            }

            else
                newRotation = transform.rotation;


            if (previousNode == null)
                previousNode = moverNodes[0];

            if (rotationType == rotationEnum.byTurnSpeed)
                lookAtRotation = Quaternion.Lerp(transform.rotation, newRotation, RotationPercent());
            else
                lookAtRotation = Quaternion.Lerp(prevFacingRot, newRotation, RotationPercent());

            // Lock axis to stay level
            if (stayLevel)
            {
                // Rotate the platform on Y only
                // Use our own unchanged x and y rotation to stay level,
                // And use the slerped y to rotate around that axis, we also take the new W to prevent gimble shenanigans.
                transform.rotation = new Quaternion(transform.rotation.x, lookAtRotation.y, transform.rotation.z, lookAtRotation.w);
            }
            else
            {
                //transform.rotation = Quaternion.Slerp (transform.rotation, newRotation, Time.fixedDeltaTime * turnSpeed);
                transform.rotation = lookAtRotation;
            }
        }

        float RotationPercent()
        {
            switch (rotationType)
            {
                //Rotation with Turn Speed
                case rotationEnum.byTurnSpeed:
                    return turnSpeed * Time.fixedDeltaTime;

                // Rotation over distance
                case rotationEnum.nodeToNode:
                    float distancePrevNodeToNextNode = Vector3.Distance(previousNode.transform.position, destinationNode.transform.position);
                    float distancePrevNodeToPlatform = Vector3.Distance(transform.position, previousNode.transform.position);
                    float t = (distancePrevNodeToPlatform / distancePrevNodeToNextNode);
                    return t;
            }

            return 0;
        }

        void GetPrevNode() // Called at end of Next Node
        {
            if (reverseDirection)
            {
                prevNodeInt = moverNodes.IndexOf(destinationNode) + 1;
                if (prevNodeInt > moverNodes.Count - 1 && loop)
                {
                    prevNodeInt = 0;
                }
            }
            else if (!reverseDirection)
            {
                prevNodeInt = moverNodes.IndexOf(destinationNode) - 1;
                if (prevNodeInt < 0 && loop)
                    prevNodeInt = moverNodes.Count - 1;
            }
            previousNode = moverNodes[prevNodeInt];
        }

        void WaitCheck()
        {
            if (!wait)
            {
                if (destinationNode != null)
                {
                    waitTimer = destinationNode.waitTime;
                    wait = true;
                }
            }

            waitTimer -= Time.fixedDeltaTime;

            if (waitTimer <= 0)
            {
                wait = false;
                NextNode();
            }
        }

        void NextNode() // Called when platform reaches its destination node
        {
            if (!wait && isAtDest && !newDestSet)
            {
                int currentNode = moverNodes.IndexOf(destinationNode);
                prevFacingRot = transform.rotation;
                //turnPercent = 0;

                if (!loop)
                {

                    if (autoReturn)
                    {

                        if (currentNode + 1 == moverNodes.Count)
                        {
                            reverseDirection = true;
                        }
                        else if (currentNode - 1 == -1)
                        {
                            reverseDirection = false;
                        }
                    }
                    if (!reverseDirection && currentNode < moverNodes.Count - 1) // Standard direction && if node is not the last
                    {
                        destinationNode = moverNodes[currentNode + 1];
                    }
                    else if (reverseDirection && currentNode > 0) // Reverse direction && current node is not the first
                    {
                        destinationNode = moverNodes[currentNode - 1];
                    }

                }
                else if (loop)
                {

                    if (!reverseDirection)
                    {

                        //Ternary Operator
                        // thisVariable = depending if this statement is true ? will equal this if true : or this if false
                        destinationNode = currentNode + 1 == moverNodes.Count ? moverNodes[0] : moverNodes[currentNode + 1];
                    }
                    else if (reverseDirection)
                    {
                        destinationNode = currentNode - 1 == -1 ? moverNodes[moverNodes.Count - 1] : moverNodes[currentNode - 1];
                    }
                }

                // Apply speed from destination node
                if (destinationNode != null && destinationNode.overrideSpeed)
                {
                    moveSpeed = destinationNode.moveSpeed;
                }
                else
                {
                    if (moveSpeed != storedSpeed)
                        storedSpeed = moveSpeed;
                    else
                        moveSpeed = storedSpeed;

                    smoothSpeed = storedSpeed;
                }
                newDestSet = true;
            }
            GetPrevNode();
        }
        #endregion

        #region Public Events

        public void SetMoving(bool b)
        {
            isMoving = b;
        }

        public void SetMoveSpeed(float speed)
        {
            moveSpeed = Mathf.Clamp(speed, 0, 20);
        }

        public void SetTurnSpeed(float speed)
        {
            turnSpeed = speed;
        }

        public void SetLooping(bool b)
        {
            loop = b;
        }

        public void SetReverse(bool b)
        {
            reverseDirection = b;
        }

        public void SetAutoReturn(bool b)
        {
            autoReturn = b;
        }

        #endregion
    }
}
