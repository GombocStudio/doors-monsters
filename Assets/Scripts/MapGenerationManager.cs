using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapGenerationManager : MonoBehaviour
{
    public GameObject gameManager;
    // Temporary class to test the map generation scene
    void Start()
    {
        gameManager.GetComponent<TerrainGenerator>().GenerateTerrain();        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
