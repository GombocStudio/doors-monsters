using System.Collections;
using System.Collections.Generic;
using PhotonHashtable = ExitGames.Client.Photon.Hashtable;
using Photon.Pun;
using Photon.Realtime;
using Newtonsoft.Json;
using Cinemachine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine;

using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviourPunCallbacks
{
    // Terrain generator reference
    TerrainGenerator terrainGenerator;

    // Round manager reference
    RoundManager roundManager;

    // Character instance spawned by local player
    private GameObject characterInstance;

    #region Unity Default Methods
    // Start is called before the first frame update
    void Start()
    {
        // Initialise terrain generator reference
        terrainGenerator = FindObjectOfType<TerrainGenerator>();

        // Initialise round manager reference
        roundManager = FindObjectOfType<RoundManager>();

        // Set round manager number of rounds
        if (PhotonNetwork.CurrentRoom.CustomProperties.ContainsKey("nr"))
            roundManager.SetNumberOfRounds((int)PhotonNetwork.CurrentRoom.CustomProperties["nr"]);

        // Start game
        StartGame();
    }

    #endregion

    #region Public methods

    public void StartGame()
    {
        // Generate game terrain
        GenerateNewTerrain();
    }

    public void EnablePlayerInput(bool value)
    {
        if (characterInstance)
        {
            PlayerInput playerInput = characterInstance.GetComponent<PlayerInput>();
            if (playerInput) { playerInput.enabled = value; }
        }
    }

    public void LeaveGame()
    {
        // Leave current room
        PhotonNetwork.Disconnect();
    }

    #endregion

    #region Private Methods

    private void GenerateNewTerrain()
    {
        // Create terrain and add terrain data to room properties
        if (PhotonNetwork.IsMasterClient)
        {
            if (!terrainGenerator) { return; }

            // Generate terrain
            terrainGenerator.GenerateTerrain();

            // Get terrain data and serialize it
            JsonSerializerSettings settings = new JsonSerializerSettings() { ReferenceLoopHandling = ReferenceLoopHandling.Ignore };
            string terrainData = JsonConvert.SerializeObject(terrainGenerator.GetTerrainData(), Formatting.None, settings);

            // Reset room custom properties in case old terrain data already exists
            // PhotonHashtable hash = new PhotonHashtable();
            // PhotonNetwork.CurrentRoom.SetCustomProperties(hash);

            // Add serialized new terrain data to the room properties
            PhotonHashtable hash = new PhotonHashtable();
            hash.Add("td", terrainData);
            PhotonNetwork.CurrentRoom.SetCustomProperties(hash);
        }
    }

    private void SpawnPlayers()
    {
        // Get terrain data from room properties and deserialize it
        TerrainStructure[,] terrainData = JsonConvert.DeserializeObject<TerrainStructure[,]>((string)PhotonNetwork.CurrentRoom.CustomProperties["td"]);
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
                // Get player selected character
                string characterName = "Character 0";
                if(PhotonNetwork.PlayerList[i].CustomProperties.ContainsKey("ch"))
                    characterName = (string)PhotonNetwork.PlayerList[i].CustomProperties["ch"];

                // Destroy character instance if it was already spawned in a past round
                if (characterInstance) { PhotonNetwork.Destroy(characterInstance); }

                // Instantiate player instance and compute player id
                characterInstance = PhotonNetwork.Instantiate(characterName, terrainCorners[i], Quaternion.identity);
                if (!characterInstance) { return; }

                // Instantiate player camera
                CinemachineVirtualCamera camera = FindObjectOfType<CinemachineVirtualCamera>();
                if (camera)
                {
                    camera.LookAt = characterInstance.transform;
                    camera.Follow = characterInstance.transform;
                }

                // Initialise mobile UI if needed
                // InitMobileUI(player);
            }
        }
    }

    #endregion

    #region Photon Callbacks
    public override void OnRoomPropertiesUpdate(PhotonHashtable propertiesThatChanged)
    {
        base.OnRoomPropertiesUpdate(propertiesThatChanged);

        Debug.Log("Room properties updated!");

        foreach (string key in propertiesThatChanged.Keys)
            Debug.Log(key);

        // When terrain data is available in room custom properties, trigger start game coroutine
        if (!PhotonNetwork.CurrentRoom.CustomProperties.ContainsKey("td")) { return; }
            StartCoroutine(StartGameCR());
    }

    public override void OnLeftRoom()
    {
        base.OnLeftRoom();
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        base.OnPlayerLeftRoom(otherPlayer);
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        base.OnDisconnected(cause);

        // Load menu scene
        SceneManager.LoadScene("MenuPhoton");
    }

    #endregion

    #region Coroutines
    IEnumerator StartGameCR()
    {
        // Spawn players in terrain
        SpawnPlayers();

        // Wait...
        yield return new WaitForSeconds(1.0f);

        // Start round from the round manager
        if (roundManager) { roundManager.StartRound(); }
    }
    #endregion

    /* public void InitMobileUI(GameObject player)
{
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
} */
}