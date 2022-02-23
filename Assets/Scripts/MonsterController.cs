using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonsterController : MonoBehaviour
{
    public GameObject monsterPrefab;
    public GameObject gameManager;

    private GenerateMap mapGenerator;
    private System.DateTime lastSpawnedTime = System.DateTime.Now;
    // Start is called before the first frame update
    void Start()
    {
        mapGenerator = gameManager.GetComponent<GenerateMap>();        
        //SpawnMonster();
    }

    private MapStructure GetRandomRoom()
    {
        var mapData = mapGenerator.mapData;
        int rows = mapData.GetLength(0);
        int cols = mapData.GetLength(1);
        MapStructure? room = null;
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
        Instantiate(monsterPrefab, room.position, Quaternion.AngleAxis(90, Vector3.right));
    }


    // Update is called once per frame
    void Update()
    {
        if (System.DateTime.Now - lastSpawnedTime > System.TimeSpan.FromMilliseconds(1000))
        {
            SpawnMonster();
            lastSpawnedTime = System.DateTime.Now;
        }
    }
}
