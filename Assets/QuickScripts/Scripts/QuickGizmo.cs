//Quick Scripts by Jack Wilson, Wanderlight Games 2017.
//Thank you for purchasing this product.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace QuickScripts
{

    [AddComponentMenu("Quick Scripts/Quick Gizmo")]
    public class QuickGizmo : MonoBehaviour
    {

        public Color gizmoColor = new Color(0, 1, 0.17f, 0.5f);
        public enum currentGizmoType
        {
            cube,               // 0
            sphere,             // 1
            mesh,               // 2
            wireframeCube,      // 3
            wireframeSphere,    // 4
            wireframeMesh,      // 5
            line,               // 6
            collider,           // 7
        }
        public currentGizmoType gizmoType;
        public Mesh mesh;
        public float gizmoRadius = 1;
        public bool hideWhenNotSelected;
        [Header("Include target game object here:")]
        public GameObject target;
        public Collider chosenCollider;
        Vector3 gizmoScale;

        void OnDrawGizmos()
        {
            if (!hideWhenNotSelected)
            {
                DisplayGizmos();
            }
        }

        void OnDrawGizmosSelected()
        {
            if (hideWhenNotSelected)
            {
                DisplayGizmos();
            }
        }

        void DisplayGizmos()
        {
            // Set rotation and color for the gizmo
            Gizmos.color = gizmoColor;
            Matrix4x4 rotationMatrix = Matrix4x4.TRS(transform.position, transform.rotation, transform.localScale);

            if (gizmoType != currentGizmoType.line && gizmoType != currentGizmoType.collider)
                Gizmos.matrix *= rotationMatrix;

            switch (gizmoType)
            {
                case currentGizmoType.sphere:
                    Gizmos.DrawSphere(Vector3.zero, gizmoRadius);
                    break;
                case currentGizmoType.cube:
                    Gizmos.DrawCube(Vector3.zero, Vector3.one * gizmoRadius);
                    break;
                case currentGizmoType.mesh:
                    Gizmos.DrawMesh(mesh);
                    break;
                case currentGizmoType.wireframeCube:
                    Gizmos.DrawWireCube(Vector3.zero, Vector3.one * gizmoRadius);
                    break;
                case currentGizmoType.wireframeSphere:
                    Gizmos.DrawWireSphere(Vector3.zero, gizmoRadius);
                    break;
                case currentGizmoType.wireframeMesh:
                    Gizmos.DrawWireMesh(mesh);
                    break;
                case currentGizmoType.line:
                    if (target != null)
                        Gizmos.DrawLine(transform.position, target.transform.position);
                    break;
                case currentGizmoType.collider:
                    if (chosenCollider != null)
                        DrawCollider(chosenCollider);
                    else if (chosenCollider == null && GetComponent<Collider>())
                        DrawCollider(GetComponent<Collider>());
                    else if (GetComponent<Collider>() == null)
                        Debug.LogError(gameObject.name + " | Quick Gizmo does not detect a Collider on this object! " +
                        "Please attach one if you want to use a Collider-type Gizmo.");

                    UpdateGizmoCollider();
                    break;
            }
        }

        public void UpdateGizmoCollider() // Called from QSEditor_QuickGizmo and DisplayGizmos() (above)
        {
            Collider col;
            if (chosenCollider != null)
                col = chosenCollider;
            else
                col = GetComponent<Collider>();

            Quaternion savedRotation = transform.rotation;
            transform.rotation = Quaternion.Euler(Vector3.zero);
            gizmoScale = col.bounds.size;
            transform.rotation = savedRotation;
        }

        void DrawCollider(Collider col)
        {
            //Set up Matrix of Collider
            Vector3 colPos = col.bounds.center;
            Matrix4x4 colliderMatrix = Matrix4x4.TRS(colPos, transform.rotation, transform.localScale);
            Gizmos.matrix *= colliderMatrix;

            if (GetComponent<BoxCollider>())
            {
                Gizmos.DrawCube(Vector3.zero, gizmoScale);
            }
            else if (GetComponent<SphereCollider>())
            {
                Gizmos.DrawSphere(Vector3.zero, col.bounds.extents.x);
            }
            else if (GetComponent<CapsuleCollider>())
            {
                Gizmos.DrawWireCube(Vector3.zero, gizmoScale);
            }
        }
    }
}
