//Quick Scripts by Jack Wilson, Wanderlight Games 2017.
//Thank you for purchasing this product.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace QuickScripts
{
    public class QS_MoverNode : MonoBehaviour
    {

        public string moverID;
        public GameObject nextNode;
        public float waitTime;
        [Header("Override speed changes speed of moving to this node")]
        public bool overrideSpeed;
        [Range(0, 20)]
        public float moveSpeed;
        Color gizmoColor;

        public void DrawLineToNode(GameObject target, Color color)
        {
            nextNode = target;
            gizmoColor = color;
            OnDrawGizmos();
        }

        void OnDrawGizmos()
        {
            if (nextNode)
            {
                Gizmos.color = gizmoColor;
                Gizmos.DrawLine(transform.position, nextNode.transform.position);
            }
        }
    }
}
