using System.Collections;
using System.Collections.Generic;
using PhotonHashtable = ExitGames.Client.Photon.Hashtable;
using Photon.Pun;
using Photon.Realtime;
using Newtonsoft.Json;
using Cinemachine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem.OnScreen;
using UnityEngine;

public class GameManager : MonoBehaviourPunCallbacks
{
    // Terrain generator reference
    TerrainGenerator terrainGenerator;

    // Round manager reference
    RoundManager roundManager;

    // Score manager reference
    ScoreManager scoreManager;

    // Monster controller reference
    MonsterController monsterController;

    // Powerup controller reference
    PowerUpController powerupController;

    // Character instance spawned by local player
    private GameObject characterInstance;

    // Variable to manage when the game has finished
    private bool isGameFinished = false;

    #region Unity Default Methods
    private void Awake()
    {
        // Syncronize scene for all players
        PhotonNetwork.AutomaticallySyncScene = true;
    }

    // Start is called before the first frame update
    void Start()
    {
        // Initialise mobile UI if needed
        #if !UNITY_IOS && !UNITY_ANDROID
        GameObject stick = FindObjectOfType<OnScreenStick>().gameObject.transform.parent.gameObject;
        GameObject button = FindObjectOfType<OnScreenButton>().gameObject;

        if (stick) { stick.gameObject.SetActive(false); }
        if (button) { button.gameObject.SetActive(false); }
        #endif

        // Initialise terrain generator reference
        terrainGenerator = FindObjectOfType<TerrainGenerator>();

        // Initialise round manager reference
        roundManager = FindObjectOfType<RoundManager>();

        // Initialise score manager reference
        scoreManager = FindObjectOfType<ScoreManager>();

        // Initialise score manager reference
        monsterController = FindObjectOfType<MonsterController>();

        // Initialise powerup controller reference
        powerupController = FindObjectOfType<PowerUpController>();

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
        PhotonNetwork.LeaveRoom();
    }

    public GameObject GetCharacterInstance()
    {
        return characterInstance;
    }

    public void UpdatePlayerOnlineScore(int playerScore)
    {
        PhotonHashtable hash = new PhotonHashtable();
        hash.Add("sc", playerScore);
        PhotonNetwork.LocalPlayer.SetCustomProperties(hash);
    }

    public void SetGameAsFinished()
    {
        isGameFinished = true;
    }

    #endregion

    #region Private Methods

    private void GenerateNewTerrain()
    {
        // Create terrain and add terrain data to room properties
        if (PhotonNetwork.IsMasterClient)
        {
            if (!terrainGenerator) { return; }

            // Destroy old terrain from past round
            terrainGenerator.DestroyTerrain();

            // Remove old terrain data from monster controller and destroy remaining monsters
            if (monsterController) 
            {
                monsterController.DestroyRemainingMonsters();
                monsterController.ResetTerrainData(); 
            }

            // Remove old terrain data from powerup controller and destroy remaining powerups
            if (powerupController)
            {
                powerupController.DestroyRemainingPowerups();
                powerupController.ResetTerrainData();
            }

            // Generate terrain
            terrainGenerator.GenerateTerrain();

            // Get terrain data and serialize it
            JsonSerializerSettings settings = new JsonSerializerSettings() { ReferenceLoopHandling = ReferenceLoopHandling.Ignore };
            string terrainData = JsonConvert.SerializeObject(terrainGenerator.GetTerrainData(), Formatting.None, settings);

            // Add serialized new terrain data to the room properties
            PhotonHashtable hash = new PhotonHashtable();
            hash.Add("td", terrainData);
            PhotonNetwork.CurrentRoom.SetCustomProperties(hash);
        }
    }

    private void SpawnPlayers()
    {
        // Get terrain data from room properties and deserialize it
        TerrainStructure[,] terrainData = terrainGenerator.GetTerrainData();

        if (terrainData.GetLength(0) == 0 || terrainData.GetLength(1) == 0) { return; }

        // Compute spawning positions from terrain data
        List<Vector3> terrainCorners = new List<Vector3>();
        terrainCorners.Add(terrainData[0, 0].position + new Vector3(0, 1, 0));
        terrainCorners.Add(terrainData[0, terrainData.GetLength(1) - 1].position + new Vector3(0, 1.5f, 0));
        terrainCorners.Add(terrainData[terrainData.GetLength(0) - 1, 0].position + new Vector3(0, 1.5f, 0));
        terrainCorners.Add(terrainData[terrainData.GetLength(0) - 1, terrainData.GetLength(1) - 1].position + new Vector3(0, 1.5f, 0));

        // Compute player spawning position and ID
        for (int i = 0; i < PhotonNetwork.PlayerList.Length; i++)
        {
            // Get player selected character
            string characterName = "Character 0";
            if (PhotonNetwork.PlayerList[i].CustomProperties.ContainsKey("ch"))
                characterName = (string)PhotonNetwork.PlayerList[i].CustomProperties["ch"];

            // Add character to the score manager
            if (scoreManager) { scoreManager.AddCharacter(characterName, PhotonNetwork.PlayerList[i].NickName); }

            if (PhotonNetwork.PlayerList[i].IsLocal && i < terrainCorners.Count)
            {
                // If character instance already exist from a past round, move it to the starting position
                if (characterInstance) 
                {
                    characterInstance.transform.position = terrainCorners[i];
                    characterInstance.transform.rotation = Quaternion.identity;
                }
                else
                {
                    // Instantiate player instance and compute player id
                    characterInstance = PhotonNetwork.Instantiate(characterName, terrainCorners[i], Quaternion.identity);
                    if (!characterInstance) { return; }

                    // Init player camera
                    CinemachineVirtualCamera camera = FindObjectOfType<CinemachineVirtualCamera>();
                    if (camera)
                    {
                        camera.LookAt = characterInstance.transform;
                        camera.Follow = characterInstance.transform;
                    }

                    // Init minimap player camera
                    MinimapCameraController minimapCamera = FindObjectOfType<MinimapCameraController>();
                    if (minimapCamera) { minimapCamera.playerTransform = characterInstance.transform; }

                    // Init player audio listener
                    characterInstance.AddComponent<AudioListener>();
                }
            }
        }
    }

    #endregion

    #region Photon Callbacks
    public override void OnRoomPropertiesUpdate(PhotonHashtable propertiesThatChanged)
    {
        base.OnRoomPropertiesUpdate(propertiesThatChanged);

        // When terrain data is available in room custom properties, trigger start game coroutine
        if (!PhotonNetwork.CurrentRoom.CustomProperties.ContainsKey("td")) { return; }
            StartCoroutine(StartGameCR());
    }

    public override void OnPlayerPropertiesUpdate(Player targetPlayer, PhotonHashtable changedProps)
    {
        base.OnPlayerPropertiesUpdate(targetPlayer, changedProps);

        string characterName = "Character 0";
        int newScore = 0;

        // Get player selected character
        if (targetPlayer.CustomProperties.ContainsKey("ch"))
            characterName = (string)targetPlayer.CustomProperties["ch"];

        // Get playe updated score
        if (targetPlayer.CustomProperties.ContainsKey("sc"))
            newScore = (int)targetPlayer.CustomProperties["sc"];

        // Update player score in other player's machines
        if (scoreManager) { scoreManager.UpdateScoreList(characterName, targetPlayer.NickName, newScore); }
    }

    public override void OnLeftRoom()
    {
        base.OnLeftRoom();

        // When aplayer leaves the room send them to the lobby
        SceneManager.LoadScene("Menu");
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        base.OnPlayerLeftRoom(otherPlayer);

        // Remove player character from the score manager
        if (otherPlayer.CustomProperties.ContainsKey("ch"))
        {
            string characterName = (string)otherPlayer.CustomProperties["ch"];
            if (scoreManager) { scoreManager.RemoveCharacter(characterName); }
        }

    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        base.OnDisconnected(cause);

        // When a player disconnects in the middle of the game we send them to try to reconnect
        SceneManager.LoadScene("Menu");
    }

    public override void OnMasterClientSwitched(Player newMasterClient)
    {
        base.OnMasterClientSwitched(newMasterClient);

        // If master client has disconnected and game is not finished, kick all players from game
        if (!isGameFinished) { LeaveGame(); }
    }

    #endregion

    #region Coroutines
    IEnumerator StartGameCR()
    {
        // Get terrain data from room properties and deserialize it
        TerrainStructure[,] terrainData = JsonConvert.DeserializeObject<TerrainStructure[,]>((string)PhotonNetwork.CurrentRoom.CustomProperties["td"]);

        // Set terrain generator local component terrain data
        if (terrainGenerator) { terrainGenerator.SetTerrainData(terrainData); }

        // Set local monster controller terrain data
        if (monsterController) { monsterController.SetTerrainData(terrainData); }

        // Set local powerup controller terrain data
        if (powerupController) { powerupController.SetTerrainData(terrainData); }

        // Spawn players in terrain
        SpawnPlayers();

        // Wait...
        yield return new WaitForSeconds(1.0f);

        // Start round from the round manager
        if (roundManager) { roundManager.StartRound(); }
    }
    #endregion
}