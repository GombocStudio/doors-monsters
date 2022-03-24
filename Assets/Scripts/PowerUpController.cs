using Newtonsoft.Json;
using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PowerUpController : MonoBehaviour
{
    [SerializeField]
    private GameObject[] powerUps;

    public GameObject miniMap;
    public GameObject lightsOff;
    // Start is called before the first frame update
    TerrainGenerator terrainGenerator;

    // Start is called before the first frame update
    void Start()
    {
        // Initialise terrain generator and generate terrain
        //terrainGenerator = FindObjectOfType<TerrainGenerator>();
        //if (!terrainGenerator) { return; }
        StartCoroutine(Spawn());
    }

        // Update is called once per frame
    void Update()
    {
        
    }

    IEnumerator Spawn()
    {
        GameObject powerUp = powerUps[Random.Range(0, powerUps.Length)];
        yield return new WaitForSeconds(4.0f);

        //if (playing)
        //{
        //PlayPause();+
        // Check if terrain data is ready
        //if (!PhotonNetwork.CurrentRoom.CustomProperties.ContainsKey("terrainData")) { StopCoroutine(Spawn()); }

        // Get terrain data from room properties and deserialize it
        TerrainStructure[,] terrainData = GameManager.terrain;// JsonConvert.DeserializeObject<TerrainStructure[,]>((string)PhotonNetwork.CurrentRoom.CustomProperties["terrainData"]);
        if (terrainData.GetLength(0) == 0 || terrainData.GetLength(1) == 0) { StopCoroutine(Spawn()); }

        int randomRow = Random.Range(0, terrainData.GetLength(0));
        int randomColumn = Random.Range(0, terrainData.GetLength(1));

        PhotonNetwork.Instantiate(powerUp.name, terrainData[randomRow, randomColumn].position + 2*Vector3.up, Quaternion.identity);
        Debug.Log("spawned in \n" + randomRow + " " + randomColumn);
        // }
        StartCoroutine(Spawn());
    }
}
