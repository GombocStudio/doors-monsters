using Photon.Pun;
using System.Collections.Generic;
using Unity.AI.Navigation;
using UnityEngine;

[System.Serializable]
public struct TerrainStructure
{
    public CellType? type;

    public string prefab;

    public Vector3 position;
    public Quaternion rotation;

    public float width;
    public float depth;
}

[System.Serializable]
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
    [Header("Parent of terrain structures")]
    private GameObject terrainParent;

    [Header("Terrain grid size")]
    public int minRows;
    public int maxRows;
    public int minColumns;
    public int maxColumns;
    private int terrainRows;
    private int terrainColumns;

    [Header("Max room size")]
    public int maxRoomWidth;
    public int maxRoomDepth;

    [Header("Room spawning chance")]
    public float roomChance = 0.85f;

    [Header("Room prefabs")]
    public GameObject[] cornerRooms;
    public GameObject[] edgeRooms;
    public GameObject[] centerRooms;

    [Header("Corridor prefabs")]
    public List<GameObject> corridorOrigin;
    public GameObject corridorTile;

    [Header("Terrain data generation")]
    private TerrainDataGenerator dataGenerator;
    public TerrainStructure[,] terrainData;

    private Dictionary<string, GameObject> structureDictionary = null;

    [Header("NavMesh")]
    public NavMeshSurface navMeshSurface;

    [Header("Debug")]
    public bool usePhoton = true;

    public void GenerateTerrain()
    {
        // Initialize terrain parent
        if (!terrainParent) { terrainParent = new GameObject("TerrainParent"); }

        // Initialize terrain grid
        terrainRows = Random.Range(minRows, maxRows + 1);
        terrainColumns = Random.Range(minColumns, maxColumns + 1);

        dataGenerator = new TerrainDataGenerator();
        terrainData = dataGenerator.FromDimensions(terrainRows, terrainColumns);

        // Initialise structure dictionary
        InitStructureDictionary();

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

                // Compute terrain element neighbor data
                TerrainStructure[] neighborData = ComputeNeighborData(i, j);

                //  Compute terrain element spawning position
                ComputeSpawningPosition(ref terrainData[i, j], neighborData);
            }
        }

        /**** DISPLAY TERRAIN STRUCTURES IN WORLD ****/
        for (int i = 0; i < terrainData.GetLength(0); i++)
        {
            for (int j = 0; j < terrainData.GetLength(1); j++)
            {
                // Spawn terrain element
                SpawnStructure(terrainData[i, j]);

                // Compute terrain element neighbor data
                TerrainStructure[] neighborData = ComputeNeighborData(i, j);

                // Spawn corridor tiles that connect the terrain elements
                SpawnCorridorTiles(terrainData[i, j], neighborData);
            }
        }

        // update navMesh now that the geometry is generated        
        if (navMeshSurface) { navMeshSurface.BuildNavMesh(); };
    }

    public void DestroyTerrain()
    {
        // Destroy current terrain parent if another terrain was already spawned
        if (terrainParent)
        {
            for (int i = 0; i < terrainParent.transform.childCount; i++)
            {
                PhotonNetwork.Destroy(terrainParent.transform.GetChild(i).gameObject);
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
        if (element.prefab != "" && !terrainParent) { return; }

        GameObject structure;
        if (usePhoton)
        {
            structure = PhotonNetwork.Instantiate(element.prefab, element.position, element.rotation);
        } else
        {
            var prefab = structureDictionary[element.prefab];
            structure = Instantiate(prefab, element.position, element.rotation);
        }
        structure.transform.SetParent(terrainParent.transform);
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
            GameObject tile;
            if (usePhoton)
            {
                tile = PhotonNetwork.Instantiate(corridorTile.name, position, rotation);
            }
            else
            {                
                tile = Instantiate(corridorTile, position, rotation);
            }            
            tile.transform.SetParent(terrainParent.transform);
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
                    if (cornerRooms.Length > 0) { element.prefab = cornerRooms[Random.Range(0, cornerRooms.Length)].name; }
                    break;

                case CellType.Corridor:
                    if (corridorOrigin.Count > 2) { element.prefab = corridorOrigin[2].name; }
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
                    if (edgeRooms.Length > 0) { element.prefab = edgeRooms[Random.Range(0, edgeRooms.Length)].name; }
                    break;

                case CellType.Corridor:
                    if (corridorOrigin.Count > 1) { element.prefab = corridorOrigin[1].name; }
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
                    if (centerRooms.Length > 0) { element.prefab = centerRooms[Random.Range(0, centerRooms.Length)].name; }
                    break;

                case CellType.Corridor:
                    if (corridorOrigin.Count > 0) { element.prefab = corridorOrigin[0].name; }
                    break;

                default:
                    break;
            }

            // Compute structure rotation angle
            element.rotation = Quaternion.identity;
        }

        // Extract structure with and depth from prefab name
        if (element.prefab != "")
        {
            string[] temp = element.prefab.Split('_');

            if (temp.Length < 3) { return; }

            float.TryParse(element.prefab.Split('_')[1], out element.width); // RandomOddNumber(2, maxRoomWidth);
            float.TryParse(element.prefab.Split('_')[2], out element.depth); // RandomOddNumber(2, maxRoomDepth);
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

    public void SetTerrainData(TerrainStructure[,] td)
    {
        terrainData = td;
    }

    public void InitStructureDictionary()
    {
        structureDictionary = new Dictionary<string, GameObject>();

        // Add corner rooms to dictionary
        foreach (GameObject room in cornerRooms)
            structureDictionary.Add(room.name, room);

        // Add edge rooms to dictionary
        foreach (GameObject room in edgeRooms)
            structureDictionary.Add(room.name, room);

        // Add center rooms to dictionary
        foreach (GameObject room in centerRooms)
            structureDictionary.Add(room.name, room);

        // Add corridor origins to dictionary
        foreach (GameObject corridor in corridorOrigin)
            structureDictionary.Add(corridor.name, corridor);
    }
}
