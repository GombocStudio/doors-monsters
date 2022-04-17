using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class MonsterController : MonoBehaviour
{
    private GameObject monstersParent;

    public GameObject[] monsterPrefabs;
    public bool paused = false;
    public int maxMonsters = 10;
    private int activeMonsters = 0;

    private System.DateTime lastSpawnedTime = System.DateTime.Now;        
    private bool usePhoton = true;

    private TerrainStructure[,] terrainData;
    private bool terrainDataReady = false;

    // Start is called before the first frame update
    void Start() 
    {
        // Disable this component in all players but the master clients
        this.enabled = PhotonNetwork.IsMasterClient;

        if (PhotonNetwork.IsMasterClient)
        {
            // Initialize monster parent game object
            monstersParent = new GameObject("MonstersParent");
        }
    }

    private TerrainStructure GetRandomRoom()
    {
        int rows = terrainData.GetLength(0);
        int cols = terrainData.GetLength(1);
        TerrainStructure? room = null;
        while (room == null)
        {
            int r = Random.Range(0, rows);
            int c = Random.Range(0, cols);
            if (terrainData[r, c].type == CellType.Room)
            {
                room = terrainData[r, c];
            }
        }
        return room.Value;
    }
    private void SpawnMonster()        
    {
        // Increase number of active monsters
        activeMonsters++;

        // Select random monster to spawn
        GameObject monsterClone;
        GameObject monsterPrefab = monsterPrefabs[Random.Range(0, monsterPrefabs.Length)];

        // Compute spawning room and position
        var room = GetRandomRoom();
        var spawnPos = room.position;
        spawnPos.x += Random.Range(-room.width * 0.4f, room.width * 0.4f);
        spawnPos.z += Random.Range(-room.depth * 0.4f, room.depth * 0.4f);
        spawnPos.y = monsterPrefab.transform.position.y;

        // Only spawn if client is master client
        if (usePhoton)
        {
            monsterClone = PhotonNetwork.Instantiate(monsterPrefab.name, spawnPos, monsterPrefab.transform.localRotation);

        } else
        {
            monsterClone = Instantiate(monsterPrefab, spawnPos, Quaternion.AngleAxis(0, Vector3.right));
        }

        if (monsterClone)
        {
            // Set spawned monster as child of monstersParent
            if (monstersParent) { monsterClone.transform.SetParent(monstersParent.transform); }

           // Set spawned monster controller
            MonsterScript ss = monsterClone.GetComponent<MonsterScript>();
            if (ss) { ss.SetController(this); }
        }
    }


    // Update is called once per frame
    void Update()
    {
        if (terrainDataReady && monstersParent && !paused)
        {
            if (monstersParent.transform.childCount < maxMonsters)
            {
                if (System.DateTime.Now - lastSpawnedTime > System.TimeSpan.FromMilliseconds(1000))
                {
                    SpawnMonster();
                    lastSpawnedTime = System.DateTime.Now;
                }
            }
        }
    }

    public void MonsterCollision(MonsterScript monster)
    {
        PhotonNetwork.Destroy(monster.gameObject);
        activeMonsters--;
    }

    public TerrainStructure[,] GetTerrainData()
    {
        return terrainData;
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

    public void DestroyRemainingMonsters()
    {
        // Destroy remaining monsters before starting a new round
        if (monstersParent)
        {
            // Set active monsters to 0
            activeMonsters = 0;

            for (int i = 0; i < monstersParent.transform.childCount; i++)
            {
                PhotonNetwork.Destroy(monstersParent.transform.GetChild(i).gameObject);
            }
        }
    }
}
