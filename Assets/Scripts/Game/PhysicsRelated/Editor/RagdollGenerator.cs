using System;
using System.Collections;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Game.PhysicsRelated.Editor
{
    public class RagdollGenerator
    {
        private readonly Transform pelvis;
        private readonly Transform leftHips;
        private readonly Transform leftKnee;
        private readonly Transform rightHips;
        private readonly Transform rightKnee;
        private readonly Transform leftArm;
        private readonly Transform leftElbow;
        private readonly Transform rightArm;
        private readonly Transform rightElbow;
        private readonly Transform middleSpine;
        private readonly Transform head;
        private readonly float totalMass;
        private readonly Vector3 right = Vector3.right;
        private readonly Vector3 up = Vector3.up;
        private readonly Vector3 forward = Vector3.forward;
        private Vector3 worldRight = Vector3.right;
        private Vector3 worldUp = Vector3.up;
        private Vector3 worldForward = Vector3.forward;
        private ArrayList bones;
        private BoneInfo rootBone;

        public RagdollGenerator(Transform pelvis, Transform leftHips, Transform leftKnee,
            Transform rightHips, Transform rightKnee, Transform leftArm, Transform leftElbow,
            Transform rightArm, Transform rightElbow, Transform middleSpine, Transform head, float totalMass)
        {
            this.pelvis = pelvis;
            this.leftHips = leftHips;
            this.leftKnee = leftKnee;
            this.rightHips = rightHips;
            this.rightKnee = rightKnee;
            this.leftArm = leftArm;
            this.leftElbow = leftElbow;
            this.rightArm = rightArm;
            this.rightElbow = rightElbow;
            this.middleSpine = middleSpine;
            this.head = head;
            this.totalMass = totalMass;
        }

        private void CheckConsistency()
        {
            PrepareBones();
            Hashtable hashtable = new Hashtable();

            foreach (BoneInfo bone in bones)
            {
                if (!(bool)(Object)bone.anchor) continue;
                if (hashtable[bone.anchor] != null) return;
                hashtable[bone.anchor] = bone;
            }

            foreach (BoneInfo bone in bones)
            {
                if (bone.anchor == null) return;
            }
        }

        private void PrepareBones()
        {
            if ((bool)(Object)pelvis)
            {
                worldRight = pelvis.TransformDirection(right);
                worldUp = pelvis.TransformDirection(up);
                worldForward = pelvis.TransformDirection(forward);
            }

            bones = new ArrayList();
            rootBone = new BoneInfo { name = "Pelvis", anchor = pelvis, parent = null, density = 2.5f };
            bones.Add(rootBone);
            AddMirroredJoint("Hips", leftHips, rightHips, "Pelvis", worldRight, worldForward, -20f,
                70f, 30f, typeof(CapsuleCollider), 0.3f, 1.5f);
            AddMirroredJoint("Knee", leftKnee, rightKnee, "Hips", worldRight, worldForward, -80f,
                0.0f, 0.0f, typeof(CapsuleCollider), 0.25f, 1.5f);
            AddJoint("Middle Spine", middleSpine, "Pelvis", worldRight, worldForward, -20f, 20f, 10f,
                null, 1f, 2.5f);
            AddMirroredJoint("Arm", leftArm, rightArm, "Middle Spine", worldUp, worldForward, -70f,
                10f, 50f, typeof(CapsuleCollider), 0.25f, 1f);
            AddMirroredJoint("Elbow", leftElbow, rightElbow, "Arm", worldForward, worldUp, -90f,
                0.0f, 0.0f, typeof(CapsuleCollider), 0.2f, 1f);
            AddJoint("Head", head, "Middle Spine", worldRight, worldForward, -40f, 25f, 25f,
                null, 1f, 1f);
        }

        public void CreateRagdoll(Action initializerAction)
        {
            CheckConsistency();
            Cleanup();
            BuildCapsules();
            AddBreastColliders();
            AddHeadCollider();
            BuildBodies();
            BuildJoints();
            CalculateMass();
            Debug.Log("Ragdoll created!");
            initializerAction?.Invoke();
        }

        private BoneInfo FindBone(string name)
        {
            foreach (BoneInfo bone in bones)
            {
                if (bone.name == name)
                    return bone;
            }

            return null;
        }

        private void AddMirroredJoint(
            string name,
            Transform leftAnchor,
            Transform rightAnchor,
            string parent,
            Vector3 worldTwistAxis,
            Vector3 worldSwingAxis,
            float minLimit,
            float maxLimit,
            float swingLimit,
            Type colliderType,
            float radiusScale,
            float density)
        {
            AddJoint("Left " + name, leftAnchor, parent, worldTwistAxis, worldSwingAxis, minLimit, maxLimit,
                swingLimit, colliderType, radiusScale, density);
            AddJoint("Right " + name, rightAnchor, parent, worldTwistAxis, worldSwingAxis, minLimit, maxLimit,
                swingLimit, colliderType, radiusScale, density);
        }

        private void AddJoint(
            string name,
            Transform anchor,
            string parent,
            Vector3 worldTwistAxis,
            Vector3 worldSwingAxis,
            float minLimit,
            float maxLimit,
            float swingLimit,
            Type colliderType,
            float radiusScale,
            float density)
        {
            BoneInfo boneInfo = new BoneInfo
            {
                name = name,
                anchor = anchor,
                axis = worldTwistAxis,
                normalAxis = worldSwingAxis,
                minLimit = minLimit,
                maxLimit = maxLimit,
                swingLimit = swingLimit,
                density = density,
                colliderType = colliderType,
                radiusScale = radiusScale
            };
            if (FindBone(parent) != null)
                boneInfo.parent = FindBone(parent);
            else if (name.StartsWith("Left"))
                boneInfo.parent = FindBone("Left " + parent);
            else if (name.StartsWith("Right"))
                boneInfo.parent = FindBone("Right " + parent);
            boneInfo.parent.children.Add(boneInfo);
            bones.Add(boneInfo);
        }

        private void BuildCapsules()
        {
            foreach (BoneInfo bone in bones)
            {
                if (bone.colliderType == typeof(CapsuleCollider))
                {
                    int direction;
                    float distance;
                    if (bone.children.Count == 1)
                    {
                        Vector3 position = ((BoneInfo)bone.children[0]).anchor.position;
                        CalculateDirection(bone.anchor.InverseTransformPoint(position), out direction, out distance);
                    }
                    else
                    {
                        Vector3 anchorPosition = bone.anchor.position;
                        Vector3 position = anchorPosition - bone.parent.anchor.position + anchorPosition;
                        CalculateDirection(bone.anchor.InverseTransformPoint(position), out direction, out distance);
                        if (bone.anchor.GetComponentsInChildren(typeof(Transform)).Length > 1)
                        {
                            Bounds bounds = new Bounds();

                            foreach (var component in bone.anchor.GetComponentsInChildren(typeof(Transform)))
                            {
                                var componentsInChild = (Transform)component;
                                bounds.Encapsulate(bone.anchor.InverseTransformPoint(componentsInChild.position));
                            }

                            distance = distance <= 0.0 ? bounds.min[direction] : bounds.max[direction];
                        }
                    }

                    CapsuleCollider capsuleCollider = Undo.AddComponent<CapsuleCollider>(bone.anchor.gameObject);
                    capsuleCollider.direction = direction;
                    Vector3 zero = Vector3.zero;
                    zero[direction] = distance * 0.5f;
                    capsuleCollider.center = zero;
                    capsuleCollider.height = Mathf.Abs(distance);
                    capsuleCollider.radius = Mathf.Abs(0.055f);
                }
            }
        }

        private void Cleanup()
        {
            foreach (BoneInfo bone in bones)
            {
                if ((bool)(Object)bone.anchor)
                {
                    foreach (Component componentsInChild in bone.anchor.GetComponentsInChildren(typeof(Joint)))
                        Undo.DestroyObjectImmediate(componentsInChild);
                    foreach (Component componentsInChild in bone.anchor.GetComponentsInChildren(typeof(Rigidbody)))
                        Undo.DestroyObjectImmediate(componentsInChild);
                    foreach (Component componentsInChild in bone.anchor.GetComponentsInChildren(typeof(Collider)))
                        Undo.DestroyObjectImmediate(componentsInChild);
                }
            }
        }

        private void BuildBodies()
        {
            foreach (BoneInfo bone in bones)
            {
                Undo.AddComponent<Rigidbody>(bone.anchor.gameObject);
                bone.anchor.GetComponent<Rigidbody>().mass = bone.density;
            }
        }

        private void BuildJoints()
        {
            foreach (BoneInfo bone in bones)
            {
                if (bone.parent != null)
                {
                    CharacterJoint characterJoint = Undo.AddComponent<CharacterJoint>(bone.anchor.gameObject);
                    characterJoint.axis = CalculateDirectionAxis(bone.anchor.InverseTransformDirection(bone.axis));
                    characterJoint.swingAxis =
                        CalculateDirectionAxis(bone.anchor.InverseTransformDirection(bone.normalAxis));
                    characterJoint.anchor = Vector3.zero;
                    characterJoint.connectedBody = bone.parent.anchor.GetComponent<Rigidbody>();
                    characterJoint.enablePreprocessing = false;
                    SoftJointLimit softJointLimit = new SoftJointLimit
                        { contactDistance = 0.0f, limit = bone.minLimit };
                    characterJoint.lowTwistLimit = softJointLimit;
                    softJointLimit.limit = bone.maxLimit;
                    characterJoint.highTwistLimit = softJointLimit;
                    softJointLimit.limit = bone.swingLimit;
                    characterJoint.swing1Limit = softJointLimit;
                    softJointLimit.limit = 0.0f;
                    characterJoint.swing2Limit = softJointLimit;
                }
            }
        }

        private static void CalculateMassRecurse(BoneInfo bone)
        {
            float mass = bone.anchor.GetComponent<Rigidbody>().mass;
            foreach (BoneInfo child in bone.children)
            {
                CalculateMassRecurse(child);
                mass += child.summedMass;
            }

            bone.summedMass = mass;
        }

        private void CalculateMass()
        {
            CalculateMassRecurse(rootBone);
            float num = totalMass / rootBone.summedMass;
            foreach (BoneInfo bone in bones)
                bone.anchor.GetComponent<Rigidbody>().mass *= num;
            CalculateMassRecurse(rootBone);
        }

        private static void CalculateDirection(Vector3 point, out int direction, out float distance)
        {
            direction = 0;
            if (Mathf.Abs(point[1]) > (double)Mathf.Abs(point[0]))
                direction = 1;
            if (Mathf.Abs(point[2]) > (double)Mathf.Abs(point[direction]))
                direction = 2;
            distance = point[direction];
        }

        private static Vector3 CalculateDirectionAxis(Vector3 point)
        {
            CalculateDirection(point, out int direction, out float distance);
            Vector3 zero = Vector3.zero;
            zero[direction] = distance <= 0.0 ? -1f : 1f;
            return zero;
        }

        private static int SmallestComponent(Vector3 point)
        {
            int index = 0;
            if (Mathf.Abs(point[1]) < (double)Mathf.Abs(point[0]))
                index = 1;
            if (Mathf.Abs(point[2]) < (double)Mathf.Abs(point[index]))
                index = 2;
            return index;
        }

        private static int LargestComponent(Vector3 point)
        {
            int index = 0;
            if (Mathf.Abs(point[1]) > (double)Mathf.Abs(point[0]))
                index = 1;
            if (Mathf.Abs(point[2]) > (double)Mathf.Abs(point[index]))
                index = 2;
            return index;
        }

        private Bounds Clip(Bounds bounds, Transform relativeTo, Transform clipTransform, bool below)
        {
            int index = LargestComponent(bounds.size);
            if (Vector3.Dot(worldUp, relativeTo.TransformPoint(bounds.max)) >
                (double)Vector3.Dot(worldUp, relativeTo.TransformPoint(bounds.min)) == below)
            {
                Vector3 min = bounds.min;
                min[index] = relativeTo.InverseTransformPoint(clipTransform.position)[index];
                bounds.min = min;
            }
            else
            {
                Vector3 max = bounds.max;
                max[index] = relativeTo.InverseTransformPoint(clipTransform.position)[index];
                bounds.max = max;
            }

            return bounds;
        }

        private Bounds GetBreastBounds(Transform relativeTo)
        {
            Bounds bounds = new Bounds();
            bounds.Encapsulate(relativeTo.InverseTransformPoint(leftHips.position));
            bounds.Encapsulate(relativeTo.InverseTransformPoint(rightHips.position));
            bounds.Encapsulate(relativeTo.InverseTransformPoint(leftArm.position));
            bounds.Encapsulate(relativeTo.InverseTransformPoint(rightArm.position));
            Vector3 size = bounds.size;
            size[SmallestComponent(bounds.size)] = size[LargestComponent(bounds.size)] / 2f;
            bounds.size = size;
            return bounds;
        }

        private void AddBreastColliders()
        {
            if (middleSpine != null &&
                pelvis != null)
            {
                CapsuleCollider pelvisCapsule = Undo.AddComponent<CapsuleCollider>(pelvis.gameObject);
                pelvisCapsule.center = Vector3.up * 0.05f;
                pelvisCapsule.radius = 0.1f;
                pelvisCapsule.height = 0.2f;
                pelvisCapsule.direction = 1;
                CapsuleCollider spineCapsule = Undo.AddComponent<CapsuleCollider>(middleSpine.gameObject);
                spineCapsule.center = Vector3.up * 0.03f;
                spineCapsule.radius = 0.135f;
                spineCapsule.height = 0.285f;
                spineCapsule.direction = 1;
                // Bounds bounds1 = Clip(GetBreastBounds(_pelvis), _pelvis, _middleSpine, false);
                // BoxCollider boxCollider1 = Undo.AddComponent<BoxCollider>(_pelvis.gameObject);
                // boxCollider1.center = bounds1.center;
                // boxCollider1.size = bounds1.size * 0.9f;
                // Bounds bounds2 = Clip(GetBreastBounds(_middleSpine), _middleSpine, _middleSpine,
                //     true);
                // BoxCollider boxCollider2 = Undo.AddComponent<BoxCollider>(_middleSpine.gameObject);
                // boxCollider2.center = bounds2.center;
                // boxCollider2.size = bounds2.size * 0.9f;
            }
            else
            {
                Bounds bounds = new Bounds();
                bounds.Encapsulate(pelvis.InverseTransformPoint(leftHips.position));
                bounds.Encapsulate(pelvis.InverseTransformPoint(rightHips.position));
                bounds.Encapsulate(pelvis.InverseTransformPoint(leftArm.position));
                bounds.Encapsulate(pelvis.InverseTransformPoint(rightArm.position));
                Vector3 size = bounds.size;
                size[SmallestComponent(bounds.size)] = size[LargestComponent(bounds.size)] / 2f;
                BoxCollider boxCollider = Undo.AddComponent<BoxCollider>(pelvis.gameObject);
                boxCollider.center = bounds.center;
                boxCollider.size = size * 0.9f;
            }
        }

        private void AddHeadCollider()
        {
            if ((bool)(Object)head.GetComponent<Collider>())
                Object.Destroy(head.GetComponent<Collider>());
            float num = Vector3.Distance(leftArm.transform.position, rightArm.transform.position) * 0.4f;
            SphereCollider sphereCollider = Undo.AddComponent<SphereCollider>(head.gameObject);
            sphereCollider.radius = num;
            Vector3 zero = Vector3.zero;
            CalculateDirection(head.InverseTransformPoint(pelvis.position), out int direction, out float distance);
            zero[direction] = distance <= 0.0 ? num : -num;
            sphereCollider.center = zero;
        }

        private class BoneInfo
        {
            public string name;
            public Transform anchor;
            public BoneInfo parent;
            public float minLimit;
            public float maxLimit;
            public float swingLimit;
            public Vector3 axis;
            public Vector3 normalAxis;
            public float radiusScale;
            public Type colliderType;
            public readonly ArrayList children = new ArrayList();
            public float density;
            public float summedMass;
        }
    }
}