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
        return data;
    }
}

public class GenerateMap : MonoBehaviour
{
    public int mapRows;
    public int mapColumns;

    public int maxRoomWidth;
    public int maxRoomDepth;

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

                var cellTypeValues = typeof(CellType).GetEnumValues();

                if (Random.value <= 0.85f) { structure.type = CellType.Room; }
                else { structure.type = CellType.Corridor; }

                // Compute map structure size
                switch (structure.type)
                {
                    case CellType.Room:
                        structure.width = RandomOddNumber(2, maxRoomWidth);
                        structure.depth = RandomOddNumber(2, maxRoomDepth);
                        break;

                    case CellType.Corridor:
                        structure.width = 1;
                        structure.depth = 1;
                        break;

                    default:
                        break;
                }

                mapData[i, j] = structure;
            }
        }

        // Display map data
        for (int i = 0; i < mapData.GetLength(0); i++)
        {
            for (int j = 0; j < mapData.GetLength(1); j++)
            {
                // Compute map element neighbor data
                MapStructure[] neighborData = ComputeNeighborData(i, j);

                //  Compute map element spawning position
                mapData[i, j].position = ComputeSpawningPosition(mapData[i, j], neighborData);

                // Spawn map element
                switch (mapData[i, j].type)
                {
                    case CellType.Room:
                        SpawnRoom(mapData[i, j]);      
                        break;

                    case CellType.Corridor:
                        SpawnCorridorOrigin(mapData[i, j]);
                        break;

                    default:
                        break;
                }

                // Spawn corridor tiles that connect the map elements
                SpawnCorridorTiles(mapData[i, j], neighborData);
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private Vector3 ComputeSpawningPosition(MapStructure element, MapStructure[] neighborData)
    {
        // Compute room spawning position
        Vector3 spawningPos = new Vector3();
        if (neighborData[0].type != null)
        {
            spawningPos.x = neighborData[0].position.x + (maxRoomWidth - 1);
        }

        if (neighborData[1].type != null)
        {
            spawningPos.z = neighborData[1].position.z - (maxRoomDepth - 1);
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

    private void SpawnRoom(MapStructure element)
    {
        GameObject roomClone = Instantiate(roomPrefab, element.position, Quaternion.AngleAxis(90, Vector3.right));
        roomClone.transform.localScale = new Vector3(element.width, element.depth, 1);
        roomClone.GetComponent<MeshRenderer>().material.color = Color.grey;
    }

    private void SpawnCorridorOrigin(MapStructure element)
    {
        Instantiate(corridorPrefab, element.position, Quaternion.AngleAxis(90, Vector3.right));
    }

    private void SpawnCorridorTiles(MapStructure element, MapStructure[] neighborData)
    {
        float elementWidth = element.width * 0.5f;
        float elementDepth = element.depth * 0.5f;

        float neighborWidth = neighborData[0].width * 0.5f;
        float neighborDepth = neighborData[1].depth * 0.5f;

        System.Action<Vector3> spawnCorridor = (position) =>
        {
            Instantiate(corridorPrefab, position, Quaternion.AngleAxis(90, Vector3.right));
        };

        if (neighborData[0].type != null)
        {
            int numTiles = (int)(maxRoomWidth - elementWidth - neighborWidth) - 1;
            for (int i = 0; i < numTiles; i++)
            {
                spawnCorridor(element.position - new Vector3(elementWidth + 0.5f, 0, 0) - new Vector3(1, 0, 0) * i);
            }
        }

        if (neighborData[1].type != null)
        {
            int numTiles = (int)(maxRoomDepth - elementDepth - neighborDepth) - 1;
            for (int i = 0; i < numTiles; i++)
            {
                spawnCorridor(element.position + new Vector3(0, 0, elementDepth + 0.5f) + new Vector3(0, 0, 1) * i);
            }
        }
    }

    private int RandomOddNumber(int minInclusive, int maxExclusive)
    {
        int rand = Random.Range(2, maxRoomWidth);
        while (rand % 2 == 0)
        {
            rand = Random.Range(minInclusive, maxExclusive);
        }

        return rand;
    }

    public MapStructure[,] GetMapData()
    {
        return mapData;
    }
}
