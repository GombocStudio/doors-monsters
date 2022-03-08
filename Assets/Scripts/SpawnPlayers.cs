using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Cinemachine;
using StarterAssets;
using UnityEngine.InputSystem;

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
            cam.LookAt = go.transform;
            cam.Follow = go.transform;

            UICanvasControllerInput UI = FindObjectOfType<UICanvasControllerInput>();

#if UNITY_IOS || UNITY_ANDROID
    UI.starterAssetsInputs = go.GetComponent<StarterAssetsInputs>();

    MobileDisableAutoSwitchControls mobile = UI.gameObject.GetComponent<MobileDisableAutoSwitchControls>();
    mobile.playerInput = go.GetComponent<PlayerInput>();
#endif

#if !(UNITY_IOS || UNITY_ANDROID)
    UI.gameObject.SetActive(false);
#endif

        }
    }
}
