using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct MapStructure
{
    public int type;
    public int numConections;
    public int width;
    public int depth;

    public Vector3 position;
}

public class MapDataGenerator
{
    public MapDataGenerator() {}

    public MapStructure[,] FromDimensions(int sizeRows, int sizeCols)
    {
        MapStructure[,] data = new MapStructure[sizeRows, sizeCols];
        // stub to fill in
        return data;
    }
}

public class GenerateMap : MonoBehaviour
{
    public int mapRows;
    public int mapColumns;

    public GameObject roomPrefab;
    public GameObject corridorPrefab;

    private MapDataGenerator dataGenerator;
    private MapStructure[,] mapData;

    private void Awake()
    {
        dataGenerator = new MapDataGenerator();
        mapData = dataGenerator.FromDimensions(mapRows, mapColumns);
    }

    // Start is called before the first frame update
    void Start()
    {
        // Generate map data
        for (int i = 0; i < mapData.GetLength(0); i++)
        {
            for (int j = 0; j < mapData.GetLength(1); j++)
            {
                MapStructure structure = mapData[i, j];

                // Compute room random size
                structure.width = Random.Range(3, 4);
                structure.depth = Random.Range(3, 4);

                structure.type = Random.Range(1, 3); // 1 = Room, 2 = Corridor

                mapData[i, j] = structure;
            }
        }

        // Display map data
        for (int i = 0; i < mapData.GetLength(0); i++)
        {
            for (int j = 0; j < mapData.GetLength(1); j++)
            {
                MapStructure[] neighborData = ComputeNeighborData(i, j);

                // Compute spawning position
                Vector3 spawningPos = new Vector3(0, 0, 0);

                // Compute room spawning position
                if (neighborData[0].type != 0)
                    spawningPos.x += neighborData[0].position.x + neighborData[0].width * 0.5f + mapData[i, j].width * 0.5f;

                if (neighborData[1].type != 0)
                    spawningPos.z += neighborData[1].position.z + neighborData[1].depth * 0.5f + mapData[i, j].depth * 0.5f;

                if (mapData[i, j].type == 1) // Room
                {
                    // Spawn new room
                    GameObject roomClone = (GameObject)Instantiate(roomPrefab, spawningPos, Quaternion.AngleAxis(90, Vector3.right));
                    roomClone.transform.localScale = new Vector3(mapData[i, j].width, mapData[i, j].depth, 1);
                }
                else // Corridor
                {
                    // Compute corridor random length
                    int length = 3; // Random.Range(3, 7);
                }

                mapData[i, j].position = spawningPos;
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    MapStructure[] ComputeNeighborData(int i, int j)
    {
        MapStructure[] neighborData = new MapStructure[4];

        // Left neighbor
        if (j != 0)
            neighborData[0] = mapData[i, j - 1];

        // Up neighbor
        if (i != 0)
            neighborData[1] = mapData[i - 1, j];

        // Right neighbor
        if (j != mapColumns - 1)
            neighborData[2] = mapData[i, j + 1];

        // Down neighbor
        if (i != mapRows - 1)
            neighborData[3] = mapData[i + 1, j];

        return neighborData;
    }
}
