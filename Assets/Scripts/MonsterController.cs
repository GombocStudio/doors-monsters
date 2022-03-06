using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonsterController : MonoBehaviour
{
    public GameObject monsterPrefab;
    public GameObject gameManager;

    private TerrainGenerator mapGenerator;
    private System.DateTime lastSpawnedTime = System.DateTime.Now;
    
    public bool paused = false;
    // Start is called before the first frame update
    void Start()
    {
        mapGenerator = gameManager.GetComponent<TerrainGenerator>();        
        //SpawnMonster();
    }

    private TerrainStructure GetRandomRoom()
    {
        var mapData = mapGenerator.terrainData;
        int rows = mapData.GetLength(0);
        int cols = mapData.GetLength(1);
        TerrainStructure? room = null;
        while (room == null)
        {
            int r = Random.Range(0, rows);
            int c = Random.Range(0, cols);
            Debug.LogFormat("{0} - {1}", r, c);
            if (mapData[r, c].type == CellType.Room)
            {
                room = mapData[r, c];
            }
        }

        Debug.LogFormat("Found room: {0}", room);
        return room.Value;
    }
    private void SpawnMonster()        
    {
        var room = GetRandomRoom();
        var spawnPos = room.position;
        spawnPos.x += Random.Range(-1f, 1f);        
        spawnPos.z += Random.Range(-1f, 1f);
        var monster = Instantiate(monsterPrefab, spawnPos, Quaternion.AngleAxis(0, Vector3.right));
        monster.transform.localScale = new Vector3(0.3f, 0.3f, 0.3f);
        monster.transform.Translate(new Vector3(0, 0.5f));
    }


    // Update is called once per frame
    void Update()
    {
        if (paused) return;

        if (System.DateTime.Now - lastSpawnedTime > System.TimeSpan.FromMilliseconds(1000))
        {
            SpawnMonster();
            lastSpawnedTime = System.DateTime.Now;
        }
    }
}
