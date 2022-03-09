using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using Photon.Pun;
using StarterAssets;
using UnityEngine;
using UnityEngine.InputSystem;

public class GameManager : MonoBehaviour
{

    // Start is called before the first frame update
    void Start()
    {
        // GameObject player = PhotonNetwork.Instantiate("Player", new Vector3(0, 1, 0), Quaternion.identity, 0);
        GameObject player = PhotonNetwork.Instantiate("Character 0", new Vector3(0, 1, 0), Quaternion.identity, 0);

        CinemachineVirtualCamera cam = FindObjectOfType<CinemachineVirtualCamera>();

        // Gets the selected character from local player playePrefs
        //int selectedCharacter = PlayerPrefs.GetInt("character");

        // Makes sure the selected character exits in our available characters list
        // if (selectedCharacter < characterPrefabs.Count && characterPrefabs[selectedCharacter])
        //{
        // Computes a random position within the spawning area and instantiates an instance of the selected character
        //Vector3 randomPosition = new Vector3(Random.Range(minX, maxX), 0.5f, Random.Range(minZ, maxZ));
        //GameObject go = PhotonNetwork.Instantiate(player.name, randomPosition, Quaternion.identity);
        if (cam)
        {
            cam.LookAt = player.transform;
            cam.Follow = player.transform;
        }

#if UNITY_IOS || UNITY_ANDROID
            UICanvasControllerInput UI = FindObjectOfType<UICanvasControllerInput>();
            UI.gameObject.SetActive(true);
            UI.starterAssetsInputs = player.GetComponent<StarterAssetsInputs>();

            MobileDisableAutoSwitchControls mobile = UI.gameObject.GetComponent<MobileDisableAutoSwitchControls>();
            mobile.playerInput = player.GetComponent<PlayerInput>();
#endif

    }
}