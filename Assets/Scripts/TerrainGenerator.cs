using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct TerrainStructure
{
    public CellType? type;

    public GameObject prefab;

    public Vector3 position;
    public Quaternion rotation;

    public float width;
    public float depth;
}

public enum CellType
{
    Room,
    Corridor
}

public class TerrainDataGenerator
{
    public TerrainDataGenerator() {}

    public TerrainStructure[,] FromDimensions(int sizeRows, int sizeCols)
    {
        TerrainStructure[,] data = new TerrainStructure[sizeRows, sizeCols];
        return data;
    }
}

public class TerrainGenerator : MonoBehaviour
{
    public GameObject terrainParent;

    [Header("Terrain grid size")]
    public int terrainRows;
    public int terrainColumns;

    [Header("Max room size")]
    public int maxRoomWidth;
    public int maxRoomDepth;

    [Header("Room spawning chance")]
    public float roomChance = 0.85f;

    [Header("Room prefabs")]
    private GameObject[] cornerRooms;
    private GameObject[] edgeRooms;
    private GameObject[] centerRooms;

    [Header("Corridor prefabs")]
    public List<GameObject> corridorOrigin;
    public GameObject corridorTile;

    [Header("Terrain data generation")]
    private TerrainDataGenerator dataGenerator;
    private TerrainStructure[,] terrainData;

    private void Awake()
    {
        // Initialize terrain grid
        dataGenerator = new TerrainDataGenerator();
        terrainData = dataGenerator.FromDimensions(terrainRows, terrainColumns);

        // Initialize room and corridor lists
        cornerRooms = Resources.LoadAll<GameObject>("Prefabs/Rooms/Corner");
        edgeRooms = Resources.LoadAll<GameObject>("Prefabs/Rooms/Edge");
        centerRooms = Resources.LoadAll<GameObject>("Prefabs/Rooms/Center");
    }
    
    // Start is called before the first frame update
    void Start()
    {
        /**** FILL TERRAIN GRID WITH STRUCTURES DATA ****/
        for (int i = 0; i < terrainData.GetLength(0); i++)
        {
            for (int j = 0; j < terrainData.GetLength(1); j++)
            {
                // Compute structure type (Room or corridor)
                var cellTypeValues = typeof(CellType).GetEnumValues();

                if (Random.value < roomChance) { terrainData[i, j].type = CellType.Room; }
                else { terrainData[i, j].type = CellType.Corridor; }

                // Compute terrain structure data
                ComputeStructureData(ref terrainData[i, j], i, j);
            }
        }

        /**** DISPLAY TERRAIN STRUCTURES IN WORLD ****/
        for (int i = 0; i < terrainData.GetLength(0); i++)
        {
            for (int j = 0; j < terrainData.GetLength(1); j++)
            {
                // Compute terrain element neighbor data
                TerrainStructure[] neighborData = ComputeNeighborData(i, j);

                //  Compute terrain element spawning position
                ComputeSpawningPosition(ref terrainData[i, j], neighborData);

                // Spawn terrain element
                SpawnStructure(terrainData[i, j]);      

                // Spawn corridor tiles that connect the terrain elements
                SpawnCorridorTiles(terrainData[i, j], neighborData);
            }
        }
    }

    TerrainStructure[] ComputeNeighborData(int i, int j)
    {
        TerrainStructure[] neighborData = new TerrainStructure[4];

        // Left neighbor
        if (j != 0)
            neighborData[0] = terrainData[i, j - 1];

        // Up neighbor
        if (i != 0)
            neighborData[1] = terrainData[i - 1, j];

        // Right neighbor
        if (j != terrainColumns - 1)
            neighborData[2] = terrainData[i, j + 1];

        // Down neighbor
        if (i != terrainRows - 1)
            neighborData[3] = terrainData[i + 1, j];

        return neighborData;
    }

    private void ComputeSpawningPosition(ref TerrainStructure element, TerrainStructure[] neighborData)
    {
        if (neighborData[0].type != null)
            element.position.x = neighborData[0].position.x + maxRoomWidth;

        if (neighborData[1].type != null)
            element.position.z = neighborData[1].position.z - maxRoomDepth;
    }

    private void SpawnStructure(TerrainStructure element)
    {
        if (!element.prefab || !terrainParent) { return; }

        Instantiate(element.prefab, element.position, element.rotation, terrainParent.transform);
    }

    private void SpawnCorridorTiles(TerrainStructure element, TerrainStructure[] neighborData)
    {
        if (!corridorTile || !terrainParent) { return; }

        float elementWidth = element.width * 0.5f;
        float elementDepth = element.depth * 0.5f;

        float neighborWidth = neighborData[0].width * 0.5f;
        float neighborDepth = neighborData[1].depth * 0.5f;

        System.Action<Vector3, Quaternion> spawnCorridor = (position, rotation) =>
        {
            Instantiate(corridorTile, position, rotation, terrainParent.transform);
        };

        if (neighborData[0].type != null)
        {
            int numTiles = (int)(maxRoomWidth - elementWidth - neighborWidth);
            for (int i = 0; i < numTiles; i++)
            {
                Vector3 spawnPos = element.position - new Vector3(elementWidth + 0.5f, 0, 0) - new Vector3(1, 0, 0) * i;
                spawnCorridor(spawnPos, Quaternion.AngleAxis(90, Vector3.up));
            }
        }

        if (neighborData[1].type != null)
        {
            int numTiles = (int)(maxRoomDepth - elementDepth - neighborDepth);
            for (int i = 0; i < numTiles; i++)
            {
                Vector3 spawnPos = element.position + new Vector3(0, 0, elementDepth + 0.5f) + new Vector3(0, 0, 1) * i;
                spawnCorridor(spawnPos, Quaternion.identity);
            }
        }
    }

    private void ComputeStructureData(ref TerrainStructure element, int row, int col)
    {
        /**** COMPUTE CORNER STRUCTURE ****/
        if ((row == 0 && col == 0) || (row == 0 && col == terrainColumns - 1)
        || (row == terrainRows - 1 && col == 0) || (row == terrainRows - 1 && col == terrainColumns - 1))
        {
            // Select corner structure prefab
            switch (element.type)
            {
                case CellType.Room:
                    if (cornerRooms.Length > 0) { element.prefab = cornerRooms[Random.Range(0, cornerRooms.Length)]; }
                    break;

                case CellType.Corridor:
                    if (corridorOrigin.Count > 2) { element.prefab = corridorOrigin[2]; }
                    break;

                default:
                    break;
            }

            // Compute structure rotation angle
            float angle = (90 * ((row != 0) ? 1 : 0) + 90 * ((col != 0) ? 1 : 0)) * ((row != 0 && col == 0) ? -1 : 1);
            element.rotation = Quaternion.AngleAxis(angle, Vector3.up);
        }

        /**** COMPUTE EDGE STRUCTURE ****/
        else if (row == 0 || row == terrainRows - 1 || col == 0 || col == terrainColumns - 1)
        {
            // Select edge structure prefab
            switch (element.type)
            {
                case CellType.Room:
                    if (edgeRooms.Length > 0) { element.prefab = edgeRooms[Random.Range(0, edgeRooms.Length)]; }
                    break;

                case CellType.Corridor:
                    if (corridorOrigin.Count > 1) { element.prefab = corridorOrigin[1]; }
                    break;

                default:
                    break;
            }

            // Compute structure rotation angle
            float angle = 90 * ((col != 0) ? 1 : -1) * ((row != 0) ? 1 : 0) + 90 * ((row == terrainRows - 1) ? 1 : 0);
            element.rotation = Quaternion.AngleAxis(angle, Vector3.up);
        }

        /**** COMPUTE CENTER STRUCTURE ****/
        else
        {
            // Select center structure prefab
            switch (element.type)
            {
                case CellType.Room:
                    if (centerRooms.Length > 0) { element.prefab = centerRooms[Random.Range(0, centerRooms.Length)]; }
                    break;

                case CellType.Corridor:
                    if (corridorOrigin.Count > 0) { element.prefab = corridorOrigin[0]; }
                    break;

                default:
                    break;
            }

            // Compute structure rotation angle
            element.rotation = Quaternion.identity;
        }

        // Extract structure with and depth from prefab name
        if (element.prefab)
        {
            string[] temp = element.prefab.name.Split('_');

            if (temp.Length < 3) { return; }

            float.TryParse(element.prefab.name.Split('_')[1], out element.width); // RandomOddNumber(2, maxRoomWidth);
            float.TryParse(element.prefab.name.Split('_')[2], out element.depth); // RandomOddNumber(2, maxRoomDepth);
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

    public TerrainStructure[,] GetTerrainData()
    {
        return terrainData;
    }
}
