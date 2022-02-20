using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct MapStructure
{
    public CellType? type;
    public int numConections;
    public int width;
    public int depth;

    public Vector3 position;
}

public enum CellType
{
    Room,
    Corridor
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

                var cellTypeValues = typeof(CellType).GetEnumValues();
                structure.type = (CellType)cellTypeValues.GetValue(Random.Range(0, cellTypeValues.Length));

                mapData[i, j] = structure;
            }
        }

        // Display map data
        for (int i = 0; i < mapData.GetLength(0); i++)
        {
            for (int j = 0; j < mapData.GetLength(1); j++)
            {                                
                Vector3 spawningPos = ComputeSpawningPosition(mapData[i, j], i, j);
                
                switch(mapData[i, j].type)
                {
                    case CellType.Room:
                        SpawnRoom(mapData[i, j], spawningPos);      
                        break;

                    case CellType.Corridor:
                        SpawnCorridor(mapData[i, j], i, j, spawningPos);
                        break;

                    default:
                        break;
                }

                mapData[i, j].position = spawningPos;
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private Vector3 ComputeSpawningPosition(MapStructure element, int row, int col)
    {
        MapStructure[] neighborData = ComputeNeighborData(row, col);
        Vector3 spawningPos = new Vector3();
        // Compute room spawning position
        if (neighborData[0].type != null)
        {
            spawningPos.x += neighborData[0].position.x + neighborData[0].width * 0.5f + element.width * 0.5f;
        }

        if (neighborData[1].type != null)
        {
            spawningPos.z += neighborData[1].position.z + neighborData[1].depth * 0.5f + element.depth * 0.5f;
        }
        return spawningPos;
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

    private void SpawnRoom(MapStructure element, Vector3 position)
    {
        GameObject roomClone = Instantiate(roomPrefab, position, Quaternion.AngleAxis(90, Vector3.right));
        roomClone.transform.localScale = new Vector3(element.width, element.depth, 1);
        roomClone.GetComponent<MeshRenderer>().material.color = Color.grey;
    }

    private void SpawnCorridor(MapStructure element, int row, int col, Vector3 position)
    {
        var elementWidth = element.width / 2;
        var elementDepth = element.depth / 2;

        System.Action<Vector3> spawnCorridor = (position) =>
        {
            Instantiate(corridorPrefab, position, Quaternion.AngleAxis(90, Vector3.right));
        };

        spawnCorridor(position);
        if (col != 0)
        {
            spawnCorridor(position - new Vector3(elementWidth, 0, 0));
        }
        if (row != 0)
        {
            spawnCorridor(position - new Vector3(0, 0, elementDepth));
        }
        if (row != mapData.GetLength(0) - 1)
        {
            spawnCorridor(position + new Vector3(0, 0, elementDepth));
        }
        if (col != mapData.GetLength(1) - 1)
        {
            spawnCorridor(position + new Vector3(elementWidth, 0, 0));
        }
    }
}
