using System.Collections;
using System.Collections.Generic;
using PhotonHashtable = ExitGames.Client.Photon.Hashtable;
using Newtonsoft.Json;
using UnityEngine;
using Photon.Pun;


public class GameManager : MonoBehaviourPunCallbacks
{
    TerrainGenerator terrainGenerator;

    // Start is called before the first frame update
    void Start()
    {
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
            hash.Add("terrainData", terrainData);
            PhotonNetwork.CurrentRoom.SetCustomProperties(hash);
        }

        // Initialise mobile UI if needed
#if UNITY_IOS || UNITY_ANDROID
            UICanvasControllerInput UI = FindObjectOfType<UICanvasControllerInput>();

            if (UI)
            {
                UI.gameObject.SetActive(true);
                UI.starterAssetsInputs = player.GetComponent<StarterAssetsInputs>();

                MobileDisableAutoSwitchControls mobile = UI.gameObject.GetComponent<MobileDisableAutoSwitchControls>();
                mobile.playerInput = player.GetComponent<PlayerInput>();
            }
#endif
    }

    public void SpawnPlayers()
    {
        // Check if terrain data is ready
        if (!PhotonNetwork.CurrentRoom.CustomProperties.ContainsKey("terrainData")) { return; }

        // Get terrain data from room properties and deserialize it
        TerrainStructure[,] terrainData = JsonConvert.DeserializeObject<TerrainStructure[,]>((string)PhotonNetwork.CurrentRoom.CustomProperties["terrainData"]);
        if (terrainData.GetLength(0) == 0 || terrainData.GetLength(1) == 0) { return; }

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
                // Instantiate player instance and compute player id
                GameObject player = PhotonNetwork.Instantiate("Character", terrainCorners[i], Quaternion.identity);
                if (!player) { return; }

                // Instantiate player camera
                GameObject camera = new GameObject("Camera");
                camera.AddComponent<Camera>();

                camera.transform.SetParent(player.transform);

                camera.transform.localPosition = new Vector3(0, 14, -16);
                camera.transform.rotation = Quaternion.AngleAxis(60, Vector3.right);
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