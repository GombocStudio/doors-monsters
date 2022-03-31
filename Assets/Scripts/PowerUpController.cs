using Newtonsoft.Json;
using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PowerUpController : MonoBehaviour
{
    private GameObject powerupsParent;

    [SerializeField]
    private GameObject[] powerupPrefabs;
    public int maxPowerups = 5;
    public float spawnTime = 10.0f;

    private TerrainStructure[,] terrainData;
    private bool terrainDataReady = false;

    // Start is called before the first frame update
    void Start()
    {
        // Disable this component in all players but the master clients
        this.enabled = PhotonNetwork.IsMasterClient;

        if (PhotonNetwork.IsMasterClient)
        {
            // Initialize powerup parent game object
            powerupsParent = new GameObject("PowerupsParent");

            // Start spawning coroutine
            StartCoroutine(Spawn());
        }
    }

    IEnumerator Spawn()
    {
        yield return new WaitForSeconds(spawnTime);

        if (terrainDataReady && powerupsParent)
        {
            if (powerupsParent.transform.childCount < maxPowerups)
            {
                // Select random power up
                GameObject powerupPrefab = powerupPrefabs[Random.Range(0, powerupPrefabs.Length)];

                // Get terrain data from room properties and deserialize it
                if (terrainData.GetLength(0) == 0 || terrainData.GetLength(1) == 0) { StopCoroutine(Spawn()); }

                int randomRow = Random.Range(0, terrainData.GetLength(0));
                int randomColumn = Random.Range(0, terrainData.GetLength(1));

                TerrainStructure structure = terrainData[randomRow, randomColumn];

                var spawnPos = structure.position;
                spawnPos.x += Random.Range(-structure.width * 0.4f, structure.width * 0.4f);
                spawnPos.z += Random.Range(-structure.depth * 0.4f, structure.depth * 0.4f);
                spawnPos.y = powerupPrefab.transform.position.y;

                GameObject powerupClone = PhotonNetwork.Instantiate(powerupPrefab.name, spawnPos, Quaternion.identity);

                // Set spawned powerup as child of powerupsParent
                if (powerupClone) { powerupClone.transform.SetParent(powerupsParent.transform); }
            }
        }

        StartCoroutine(Spawn());
    }

    public void SetTerrainData(TerrainStructure[,] td)
    {
        terrainData = td;
        terrainDataReady = true;
    }

    public void ResetTerrainData()
    {
        terrainData = new TerrainStructure[0, 0];
        terrainDataReady = false;
    }

    public void DestroyRemainingPowerups()
    {
        // Destroy remaining powerups before starting a new round
        if (powerupsParent)
        {
            for (int i = 0; i < powerupsParent.transform.childCount; i++)
            {
                PhotonNetwork.Destroy(powerupsParent.transform.GetChild(i).gameObject);
            }
        }
    }
}
