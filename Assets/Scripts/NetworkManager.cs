using System.Collections;
using System.Collections.Generic;

using Photon.Pun;
using Photon.Realtime;
using PhotonHashtable = ExitGames.Client.Photon.Hashtable;

using UnityEngine.UI;
using UnityEngine;

public class NetworkManager : MonoBehaviourPunCallbacks
{
    #region Variables
    // UI manager class reference
    private UIManager uiManager;

    // Online client state variable
    private bool isBusy = false;

    [Header("Game Scene")]
    [SerializeField] private string _gameSceneName;
    #endregion

    #region Unity Methods
    private void Awake()
    {
        // Syncronize scene for all players
        PhotonNetwork.AutomaticallySyncScene = true;
    }

    private void Start()
    {
        // Initialise ui manager reference
        uiManager = FindObjectOfType<UIManager>();

        // Connect to server on start game
        if (!PhotonNetwork.IsConnected) { PhotonNetwork.ConnectUsingSettings(); }

        // Generate random nickname for player
        PhotonNetwork.NickName = "JUGADOR_" + GenerateRandomText(2);
    }

    private void Update()
    {
        // Check if client is busy connecting to the server or joining a game
        isBusy =
        PhotonNetwork.NetworkClientState == ClientState.Authenticating
        || PhotonNetwork.NetworkClientState == ClientState.ConnectingToGameServer
        || PhotonNetwork.NetworkClientState == ClientState.ConnectingToMasterServer
        || PhotonNetwork.NetworkClientState == ClientState.ConnectingToNameServer
        || PhotonNetwork.NetworkClientState == ClientState.Disconnecting
        || PhotonNetwork.NetworkClientState == ClientState.DisconnectingFromGameServer
        || PhotonNetwork.NetworkClientState == ClientState.DisconnectingFromMasterServer
        || PhotonNetwork.NetworkClientState == ClientState.DisconnectingFromNameServer
        || PhotonNetwork.NetworkClientState == ClientState.Joining
        || PhotonNetwork.NetworkClientState == ClientState.JoiningLobby
        || PhotonNetwork.NetworkClientState == ClientState.Leaving;

        // Enable loading gif if client is busy
        if (uiManager) { uiManager.EnableLoadingGIF(isBusy); }
    }
    #endregion

    #region Public Methods
    public void StartGame()
    {
        PhotonNetwork.LoadLevel(_gameSceneName);
    }

    public void DisconnectFromServerAndCloseGame()
    {
        // Do nothing if client is busy
        if (isBusy) { return; }

        // Disconnect from server and close application window
        PhotonNetwork.Disconnect();
        Application.Quit();
    }

    public void CreateRoom(int maxPlayers, int numRounds)
    {
        // Do nothing if client is busy or already connected to the game server
        if (isBusy || PhotonNetwork.NetworkingClient.Server == ServerConnection.GameServer) { return; }

        // Create and set room options
        RoomOptions roomOptions = new RoomOptions();
        roomOptions.MaxPlayers = (byte)(maxPlayers);
        // roomOptions.CustomRoomProperties.Add("nr", numRounds);

        // Create room with specified options
        PhotonNetwork.CreateRoom(GenerateRandomText(5), roomOptions, TypedLobby.Default, null);
    }

    public void JoinRoom(string roomName)
    {
        // Do nothing if client is busy or already connected to the game server
        if (isBusy || PhotonNetwork.NetworkingClient.Server == ServerConnection.GameServer) { return; }

        // Join room called roomName
        PhotonNetwork.JoinRoom(roomName);
    }

    public void LeaveRoom()
    {
        // Do nothing if client is busy or already connected to the master server
        if (isBusy || PhotonNetwork.NetworkingClient.Server == ServerConnection.MasterServer) { return; }

        // Leave current room
        PhotonNetwork.LeaveRoom();
    }
    #endregion

    #region Private Methods
    private string GenerateRandomText(int length)
    {
        string usedCharacters = "qwertyuiopasdfghjklzxcvbnm0123456789";
        string randomTxt = "";

        for (int i = 0; i < length; i++)
            randomTxt += usedCharacters[UnityEngine.Random.Range(0, usedCharacters.Length)];
        
        return randomTxt.ToUpper();
    }

    private void SelectRandomCharacter()
    {
        // Initialise list with all character names in it
        List<string> characterNames = new List<string>();
        characterNames.Add("Character 0");
        characterNames.Add("Character 1");
        characterNames.Add("Character 2");
        characterNames.Add("Character 3");

        // Remove from the list the characters that have already been assigned to a player
        foreach (var player in PhotonNetwork.PlayerList)
        {
            if (!player.CustomProperties.ContainsKey("character")) { continue; }
            characterNames.Remove((string)player.CustomProperties["character"]);
        }

        // Select random character from the remaining character names in the list
        PhotonHashtable hash = new PhotonHashtable();
        hash.Add("character", characterNames[Random.Range(0, characterNames.Count)]);
        PhotonNetwork.LocalPlayer.SetCustomProperties(hash);
    }
    #endregion

    #region Photon Callbacks
    public override void OnConnectedToMaster()
    {
        // Join lobby
        PhotonNetwork.JoinLobby();
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        Debug.LogWarning("Disconnected: " + cause);        
    }

    public override void OnJoinedLobby()
    {
        // Enable lobby panel
        if (uiManager) { uiManager.EnableLobby(); }
    }

    public override void OnJoinedRoom()
    {
        Debug.Log("Connected to room " + PhotonNetwork.CurrentRoom.Name);

        if (uiManager) 
        { 
            // Enable room menu panel
            uiManager.EnableRoom();

            // Update room ui elements
            string roomName = "NOMBRE DE PARTIDA\n" + PhotonNetwork.CurrentRoom.Name;
            bool enablePlayBtn = PhotonNetwork.LocalPlayer.IsMasterClient;
            uiManager.UpdateRoomUI(roomName, enablePlayBtn);
        }

        // Select random character for player that joined the room
        SelectRandomCharacter();
    }

    public override void OnPlayerEnteredRoom(Photon.Realtime.Player newPlayer)
    {
        Debug.Log(newPlayer.NickName + " entered the room.");
        if (uiManager)
        {
            string roomName = "NOMBRE DE PARTIDA\n" + PhotonNetwork.CurrentRoom.Name;
            bool enablePlayBtn = PhotonNetwork.LocalPlayer.IsMasterClient;

            uiManager.UpdateRoomUI(roomName, enablePlayBtn);
        }
    }

    public override void OnPlayerLeftRoom(Photon.Realtime.Player otherPlayer)
    {
        Debug.Log(otherPlayer.NickName + " left the room.");
        if (uiManager)
        {
            string roomName = "NOMBRE DE PARTIDA\n" + PhotonNetwork.CurrentRoom.Name;
            bool enablePlayBtn = PhotonNetwork.LocalPlayer.IsMasterClient;

            uiManager.UpdateRoomUI(roomName, enablePlayBtn);
        }
    }

    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        Debug.Log("Failed to create room " + returnCode + ": " + message);

        if (uiManager) { uiManager.HandleError(returnCode); }
    }

    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        Debug.Log("Failed to join room " + returnCode + ": " + message);

        if (uiManager) { uiManager.HandleError(returnCode); }
    }
    #endregion

}