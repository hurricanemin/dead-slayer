//Quick Scripts by Jack Wilson, Wanderlight Games 2017.
//Thank you for purchasing this product.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace QuickScripts
{
    public class QS_SpawnPoint : MonoBehaviour
    {


        public string spawnerID;
        private GameObject myItem;
        public float spawnRequestCooldown = 1;
        private float resetTimer;

        [HideInInspector]
        public QuickSpawner mySpawner;
        [HideInInspector]
        public bool occupied;

        void Start()
        {
            Invoke("CheckSpawnerLink", 1);
            Invoke("CheckIfFree", spawnRequestCooldown);
        }

        void Update()
        {
            // Check if last item has moved away from the Spawn Point
            if (myItem != null && Vector3.Distance(transform.position, myItem.transform.position) > 0.7f)
            {
                myItem = null;
            }
        }

        void FixedUpdate()
        {
            resetTimer -= Time.fixedDeltaTime;

            if (resetTimer <= 0)
                CheckIfFree();
        }

        void CheckSpawnerLink()
        {
            if (mySpawner == null)
            {
                Debug.LogError("Game Object " + gameObject.name + " could not find its matching spawner. Please check it has been included in the spawner's list of spawn points.");
            }
        }

        void CheckIfFree()
        {
            if (myItem == null)
            {
                occupied = false;
                resetTimer = spawnRequestCooldown;
            }
            else
                occupied = true;
        }

        public void SpawnItem(GameObject itemToSpawn) // Called from QuickSpawner
        {
            GameObject spawnedItem = Instantiate(itemToSpawn, this.transform.position, Quaternion.identity) as GameObject;
            myItem = spawnedItem;
            occupied = true;
        }

        #region Public Events
        public void SetRequestCooldown(float t)
        {
            spawnRequestCooldown = t;
        }
        public void SetOccupied(bool b)
        {
            occupied = b;
        }
        #endregion
    }
}
