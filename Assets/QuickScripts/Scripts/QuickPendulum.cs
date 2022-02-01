//Quick Scripts by Jack Wilson, Wanderlight Games 2017.
//Thank you for purchasing this product.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace QuickScripts
{

    [AddComponentMenu("Quick Scripts/Quick Pendulum")]
    public class QuickPendulum : MonoBehaviour
    {

        public float angleX = 90;
        public float angleY = 0;
        public float angleZ = 0;
        public float speed = 1;
        [Range(0, 3)]
        public float offset = 0;

        private Vector3 startRotation;
        void Start()
        {
            startRotation = transform.eulerAngles;

        }

        void Update()
        {
            if (angleX != 0)
                startRotation.x = 1;
            if (angleY != 0)
                startRotation.y = 1;
            if (angleZ != 0)
                startRotation.z = 1;
        }

        void FixedUpdate()
        {
            float t = (Mathf.Sin(offset + Time.fixedTime * speed));
            transform.rotation = Quaternion.Euler(startRotation.x * angleX * t, startRotation.y * angleY * t, startRotation.z * angleZ * t);
        }

        #region Public Events

        public void SetAngleX(float x)
        {
            angleX = x;
        }
        public void SetAngleY(float y)
        {
            angleY = y;
        }
        public void SetAngleZ(float z)
        {
            angleZ = z;
        }
        public void SetSpeed(float t)
        {
            speed = t;
        }
        public void SetOffset(float o)
        {
            offset = Mathf.Clamp(o, 0, 3);
        }

        #endregion
    }
}
