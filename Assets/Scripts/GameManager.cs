using System.Collections;
using System.Collections.Generic;
using PhotonHashtable = ExitGames.Client.Photon.Hashtable;
using Newtonsoft.Json;
using UnityEngine;
using Photon.Pun;
using Cinemachine;
using StarterAssets;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.OnScreen;

public class GameManager : MonoBehaviourPunCallbacks
{
    TerrainGenerator terrainGenerator;

    public static TerrainStructure[,] terrain;
    // Start is called before the first frame update
    void Start()
    {
        // Initialise mobile UI if needed

#if !UNITY_IOS && !UNITY_ANDROID
                GameObject UI = FindObjectOfType<OnScreenStick>().gameObject.transform.parent.transform.parent.gameObject;

                if (UI)
                {
                    UI.gameObject.SetActive(false);
                }
#endif
        // Initialise terrain generator and generate terrain
        terrainGenerator = GetComponent<TerrainGenerator>();
        if (!terrainGenerator) { return; }

        if (PhotonNetwork.IsMasterClient)
        {
            // Generate terrain
            terrainGenerator.GenerateTerrain();

            // Get terrain data and serialize it
            JsonSerializerSettings settings = new JsonSerializerSettings() { ReferenceLoopHandling = ReferenceLoopHandling.Ignore };
            string terrainData = JsonConvert.SerializeObject(terrainGenerator.GetTerrainData(), Formatting.None, settings);

            // Add serialized terrain data to the room properties
            PhotonHashtable hash = new PhotonHashtable();
            hash.Add("td", terrainData);
            PhotonNetwork.CurrentRoom.SetCustomProperties(hash);
        }
    }

    public void SpawnPlayers()
    {
        // Check if terrain data is ready
        if (!PhotonNetwork.CurrentRoom.CustomProperties.ContainsKey("td")) { return; }

        // Get terrain data from room properties and deserialize it
        TerrainStructure[,] terrainData = JsonConvert.DeserializeObject<TerrainStructure[,]>((string)PhotonNetwork.CurrentRoom.CustomProperties["td"]);
        if (terrainData.GetLength(0) == 0 || terrainData.GetLength(1) == 0) { return; }

        terrain = terrainData;

        // Compute spawning positions from terrain data
        List<Vector3> terrainCorners = new List<Vector3>();
        terrainCorners.Add(terrainData[0, 0].position + new Vector3(0, 1, 0));
        terrainCorners.Add(terrainData[0, terrainData.GetLength(1) - 1].position + new Vector3(0, 1, 0));
        terrainCorners.Add(terrainData[terrainData.GetLength(0) - 1, 0].position + new Vector3(0, 1, 0));
        terrainCorners.Add(terrainData[terrainData.GetLength(0) - 1, terrainData.GetLength(1) - 1].position + new Vector3(0, 1, 0));

        // Compute player spawning position and ID
        for (int i = 0; i < PhotonNetwork.PlayerList.Length; i++)
        {
            if (PhotonNetwork.PlayerList[i].IsLocal && i < terrainCorners.Count)
            {
                // Get player selected character
                string character = "Character 0";
                if(PhotonNetwork.PlayerList[i].CustomProperties.ContainsKey("ch"))
                    character = (string)PhotonNetwork.PlayerList[i].CustomProperties["ch"];

                // Instantiate player instance and compute player id
                GameObject player = PhotonNetwork.Instantiate(character, terrainCorners[i], Quaternion.identity);
                if (!player) { return; }

                // Instantiate player camera
                CinemachineVirtualCamera camera = FindObjectOfType<CinemachineVirtualCamera>();
                if (camera)
                {
                    camera.LookAt = player.transform;
                    camera.Follow = player.transform;
                }


            }
        }
    }

    public override void OnRoomPropertiesUpdate(PhotonHashtable propertiesThatChanged)
    {
        base.OnRoomPropertiesUpdate(propertiesThatChanged);

        // Spawn players in terrain
        SpawnPlayers();
    }
}