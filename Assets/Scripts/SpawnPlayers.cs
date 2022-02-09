using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Cinemachine;

// Script responsible for spawning player characters in the game scene
public class SpawnPlayers : MonoBehaviour
{
    // List of different character prefabs to choose from
    [Header("List of Available Characters")]
    public List<GameObject> characterPrefabs;

    [Header("Camera")]
    public CinemachineVirtualCamera cam;

    // Max and min X and Z positions where the players should be spawned
    [Header("Spawning Area Dimensions")]
    public float minX;
    public float maxX;
    public float minZ;
    public float maxZ;

    // Start is called before the first frame update
    void Start()
    {
        // Gets the selected character from local player playePrefs
        int selectedCharacter = PlayerPrefs.GetInt("character");

        // Makes sure the selected character exits in our available characters list
        if (selectedCharacter < characterPrefabs.Count && characterPrefabs[selectedCharacter])
        {
            // Computes a random position within the spawning area and instantiates an instance of the selected character
            Vector3 randomPosition = new Vector3(Random.Range(minX, maxX), 0.5f, Random.Range(minZ, maxZ));
            GameObject go = PhotonNetwork.Instantiate(characterPrefabs[selectedCharacter].name, randomPosition, Quaternion.identity);

            // Set up player camera
            if (cam)
            {
                cam.LookAt = go.transform;
                cam.Follow = go.transform;
            }
        }
    }
}
