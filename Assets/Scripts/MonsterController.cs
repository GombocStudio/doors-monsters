using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class MonsterController : MonoBehaviour
{
    public GameObject monsterPrefab;
    public GameObject gameManager;

    private TerrainGenerator mapGenerator;
    private System.DateTime lastSpawnedTime = System.DateTime.Now;
    
    public bool paused = false;
    public int maxMonsters = 50;
    private int activeMonsters = 0;

    // Start is called before the first frame update
    void Start()
    {
        mapGenerator = gameManager.GetComponent<TerrainGenerator>();        
        //SpawnMonster();
    }

    private TerrainStructure GetRandomRoom()
    {
        var mapData = mapGenerator.GetTerrainData();
        int rows = mapData.GetLength(0);
        int cols = mapData.GetLength(1);
        TerrainStructure? room = null;
        while (room == null)
        {
            int r = Random.Range(0, rows);
            int c = Random.Range(0, cols);            
            if (mapData[r, c].type == CellType.Room)
            {
                room = mapData[r, c];
            }
        }        
        return room.Value;
    }
    private void SpawnMonster()        
    {
        activeMonsters++;
        var room = GetRandomRoom();
        var spawnPos = room.position;
        spawnPos.x += Random.Range(-1f, 1f);        
        spawnPos.z += Random.Range(-1f, 1f);
        spawnPos.y = 8.0f;
        var monster = Instantiate(monsterPrefab, spawnPos, Quaternion.AngleAxis(0, Vector3.right));
        monster.GetComponent<MonsterScript>().mapGenerator = mapGenerator;
    }


    // Update is called once per frame
    void Update()
    {
        if (paused || activeMonsters >= maxMonsters) return;

        if (System.DateTime.Now - lastSpawnedTime > System.TimeSpan.FromMilliseconds(1000))
        {
            SpawnMonster();
            lastSpawnedTime = System.DateTime.Now;
        }
    }

    public void Destroy()
    {
        // Usar PhotonNetwork.Destroy(view) para destruir los monstruos en online
    }
}
