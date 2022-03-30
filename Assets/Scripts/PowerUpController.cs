using Newtonsoft.Json;
using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PowerUpController : MonoBehaviour
{
    private GameObject powerupsParent;

    [SerializeField]
    private GameObject[] powerUps;

    public GameObject miniMap;
    public GameObject lightsOff;
    public GameObject iceCubePrefab;
    // Start is called before the first frame update
    TerrainGenerator terrainGenerator;

    private TerrainStructure[,] terrainData;
    private bool terrainDataReady = false;

    // Start is called before the first frame update
    void Start()
    {
        // Disable this component in all players but the master clients
        this.enabled = PhotonNetwork.IsMasterClient;

        // Initialize powerup parent game object
        powerupsParent = new GameObject("PowerupsParent");

        // Start spawning coroutine
        StartCoroutine(Spawn());
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    IEnumerator Spawn()
    {
        yield return new WaitForSeconds(4.0f);

        if (terrainDataReady)
        {
            GameObject powerUp = powerUps[Random.Range(0, powerUps.Length)];

            // Get terrain data from room properties and deserialize it
            if (terrainData.GetLength(0) == 0 || terrainData.GetLength(1) == 0) { StopCoroutine(Spawn()); }

            int randomRow = Random.Range(0, terrainData.GetLength(0));
            int randomColumn = Random.Range(0, terrainData.GetLength(1));

            TerrainStructure structure = terrainData[randomRow, randomColumn];

            var spawnPos = structure.position;
            spawnPos.x += Random.Range(-structure.width * 0.4f, structure.width * 0.4f);
            spawnPos.z += Random.Range(-structure.depth * 0.4f, structure.depth * 0.4f);
            spawnPos.y = 2; // powerUp.transform.position.y;

            GameObject powerupClone = PhotonNetwork.Instantiate(powerUp.name, spawnPos, Quaternion.identity);

            // Set spawned monster as child of monstersParent
            if (powerupsParent) { powerupClone.transform.SetParent(powerupsParent.transform); }

            Debug.Log("spawned in \n" + randomRow + " " + randomColumn);
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
        // Destroy remaining monsters before starting a new round
        if (powerupsParent)
        {
            // Set active powerups to 0
            // activeMonsters = 0;

            for (int i = 0; i < powerupsParent.transform.childCount; i++)
            {
                PhotonNetwork.Destroy(powerupsParent.transform.GetChild(i).gameObject);
            }
        }
    }
}
