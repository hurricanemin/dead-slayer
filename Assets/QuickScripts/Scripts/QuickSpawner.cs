//Quick Scripts by Jack Wilson, Wanderlight Games 2017.
//Thank you for purchasing this product.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace QuickScripts
{

    // This class has an Editor script which overrides OnInspectorGUI(). See QSEditor_QuickSpawner.
    [AddComponentMenu("Quick Scripts/Quick Spawner")]
    public class QuickSpawner : MonoBehaviour
    {

        public string spawnerID;
        public bool isActive = true;
        public bool randomSpawnPoint;
        public bool autoRespawn = true;
        public float secondsBetweenSpawnWaves = 5;
        public float addSecondsPerWave = 0;
        public bool startSpawned = true;
        [Space(10)]
        [Header("If more than one object in Objects To Spawn list, will choose object randomly")]
        public List<GameObject> objectsToSpawn = new List<GameObject>();
        public List<QS_SpawnPoint> spawnPoints = new List<QS_SpawnPoint>();

        private float respawnTimer;
        private int availablePoints;
        private bool firstWave;


        #region Editor

        public void SetSpawnerID() // Called from MenuItems
        {
            spawnerID = "Spawner" + this.GetInstanceID().ToString();
        }

        void OnDrawGizmos()
        {
            foreach (QS_SpawnPoint spawn in spawnPoints)
            {
                if (spawn != null)
                {
                    Gizmos.color = Color.blue;
                    Gizmos.DrawLine(transform.position, spawn.transform.position);
                }
            }
        }

        public void RefreshSpawnPoints() // Called From QSEditor_QuickSpawner
        {
            // Clear null spawns first
            for (int i = 0; i < spawnPoints.Count; i++)
            {
                QS_SpawnPoint sp = spawnPoints[i];
                if (sp == null || sp.spawnerID != spawnerID)
                {
                    spawnPoints.Remove(spawnPoints[i]);
                }
            }

            // Reassign SpawnerID
            SetSpawnerID();

            // Search scene and add spawn points
            foreach (QS_SpawnPoint sp in QS_SpawnPoint.FindObjectsOfType(typeof(QS_SpawnPoint)))
            {
                if (sp.spawnerID == spawnerID && !spawnPoints.Contains(sp))
                {
                    spawnPoints.Add(sp);
                }
            }
        }
        #endregion

        #region In-Game

        void Start()
        {
            foreach (QS_SpawnPoint sp in spawnPoints)
            {
                if (sp != null)
                    sp.mySpawner = GetComponent<QuickSpawner>();
            }

            respawnTimer = secondsBetweenSpawnWaves;

            if (startSpawned)
            {
                firstWave = true;
                RespawnItem();
            }
        }

        void FixedUpdate()
        {
            respawnTimer -= Time.fixedDeltaTime;

            if (respawnTimer <= 0 && isActive && autoRespawn)
                RespawnItem();
        }

        /// <summary>
        /// Forces the spawn call. isActive must be true for this to run.
        /// </summary>
        public void ForceSpawnCall() // Can be accessed publicly to force a spawn
        {
            RespawnItem();
        }

        public void RespawnItem() // Starts the Spawn process. The next step is to find out if it's spawning to a random spawn point or ALL spawn points
        {
            // Check if User has not allocated any objects to spawn
            if (objectsToSpawn.Count == 0)
            {
                Debug.LogError(this.name + " | Quick Spawner has no object to spawn or a null object slot.", this.gameObject);
                return;
            }


            if (autoRespawn && isActive)
            {
                if (randomSpawnPoint && !firstWave) // Make sure to spawn to all, even if random
                {
                    RestockSpawnPoint();
                }
                else
                {
                    RestockAllSpawnPoints();
                }
            }

            // Update the time between spawn waves IF it is above zero

            if (secondsBetweenSpawnWaves + addSecondsPerWave > 0.5f)
            {
                secondsBetweenSpawnWaves += addSecondsPerWave;
                respawnTimer = secondsBetweenSpawnWaves;
            }
            else
            {
                secondsBetweenSpawnWaves = 0.5f;
                respawnTimer = 0.5f;
            }
        }

        void RestockSpawnPoint() // Immediately spawn to one free spawn point, used for random selection of spawn points
        {


            // Check if any points are free before continuing
            availablePoints = 0;
            for (int i = 0; i < spawnPoints.Count; i++)
            {
                QS_SpawnPoint node = spawnPoints[i].GetComponent<QS_SpawnPoint>();

                if (!node.occupied)
                    availablePoints++;

                if (availablePoints == 0)
                    return;
            }

            foreach (Object item in objectsToSpawn)
            {
                if (item == null)
                {
                    Debug.LogError(this.name + " | Quick Spawner has no object to spawn or a null object slot.", this.gameObject);
                    return;
                }
            }

            int r = Random.Range(0, spawnPoints.Count);
            QS_SpawnPoint chosenSpawn = spawnPoints[r];

            if (!chosenSpawn.occupied)
                chosenSpawn.SpawnItem(SelectRandomItem());
            else
                RestockSpawnPoint();

        }

        void RestockAllSpawnPoints() // Immediately spawn to all spawn points that are free
        {
            for (int i = 0; i < spawnPoints.Count; i++)
            //foreach (QS_SpawnPoint spawn in spawnPoints)
            {
                QS_SpawnPoint sp = spawnPoints[i];
                if (!sp.occupied)
                {
                    sp.SpawnItem(SelectRandomItem());
                }
            }

            // No longer first wave, used if Start Spawned and Random Spawn Point are both ticked
            firstWave = false;
        }

        GameObject SelectRandomItem()
        {
            int r = Random.Range(0, objectsToSpawn.Count);
            return objectsToSpawn[r];
        }
        #endregion

        #region Public Events
        public void SetSpawnerActive(bool b)
        {
            isActive = b;
        }
        public void SetSecondsBetweenWaves(float t)
        {
            secondsBetweenSpawnWaves = t;
        }
        public void SetAddSecondsPerWave(float t)
        {
            addSecondsPerWave = t;
        }
        public void SetRandomSpawnPoint(bool b)
        {
            randomSpawnPoint = b;
        }
        public void SetAutoRespawn(bool b)
        {
            autoRespawn = b;
        }
        public void SetStartSpawned(bool b)
        {
            startSpawned = b;
        }

        #endregion
    }
}
