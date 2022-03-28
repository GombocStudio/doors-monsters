using System.Collections;
using System.Collections.Generic;

using Photon.Pun;
using Photon.Realtime;
using PhotonHashtable = ExitGames.Client.Photon.Hashtable;

using UnityEngine;

public class MNetworkManager : MonoBehaviourPunCallbacks
{
    #region Variables
    // UI manager class reference
    private MUIManager uiManager;

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
        uiManager = FindObjectOfType<MUIManager>();

        // Connect to server on start game
        if (!PhotonNetwork.IsConnected) 
        { 
            PhotonNetwork.ConnectUsingSettings();

            // Generate random nickname for player
            PhotonNetwork.LocalPlayer.NickName = "JUGADOR_" + GenerateRandomText(2);
        }
        else
        {
            OnJoinedLobby();
        }

        // Update nickname input field UI in lobby
        if (uiManager) { uiManager.UpdateNicknameInputField(PhotonNetwork.LocalPlayer.NickName); }
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
        PhotonNetwork.CurrentRoom.IsOpen = false;
        PhotonNetwork.LoadLevel(_gameSceneName);
    }

    public void DisconnectFromServerAndCloseGame()
    {
        // Do nothing if client is busy
        if (isBusy) { return; }

        // Disconnect from server and close application window
        PhotonNetwork.Disconnect();
    }

    public void CreateRoom(int maxPlayers, int numRounds, string nickname)
    {
        // Do nothing if client is busy or already connected to the game server
        if (isBusy || PhotonNetwork.NetworkingClient.Server == ServerConnection.GameServer) { return; }

        // Set local player nickame
        if (nickname != "") { PhotonNetwork.LocalPlayer.NickName = nickname; }

        // Create and set room options
        RoomOptions roomOptions = new RoomOptions();
        roomOptions.MaxPlayers = (byte)(maxPlayers);

        PhotonHashtable hash = new PhotonHashtable();
        hash.Add("nr", numRounds);
        roomOptions.CustomRoomProperties = hash;

        // Create room with specified options
        PhotonNetwork.CreateRoom(GenerateRandomText(5), roomOptions, TypedLobby.Default, null);
    }

    public void JoinRoom(string roomName, string nickname)
    {
        // Do nothing if client is busy or already connected to the game server
        if (isBusy || PhotonNetwork.NetworkingClient.Server == ServerConnection.GameServer) { return; }

        // Set local player nickame
        if (nickname != "") { PhotonNetwork.LocalPlayer.NickName = nickname; }

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
        string usedCharacters = "qwertyuipasdfghjklzxcvbnm123456789"; // Don't include o and 0 because they're very similar in the used font
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
        foreach (var player in PhotonNetwork.CurrentRoom.Players.Values)
        {
            if (!player.CustomProperties.ContainsKey("ch")) { continue; }
            characterNames.Remove((string)player.CustomProperties["ch"]);
        }

        // Select random character from the remaining character names in the list
        PhotonHashtable hash = new PhotonHashtable();
        hash.Add("ch", characterNames[Random.Range(0, characterNames.Count)]);
        PhotonNetwork.LocalPlayer.SetCustomProperties(hash);
    }

    private void UpdateRoomState()
    {
        if (uiManager)
        {
            string roomName = "NOMBRE DE PARTIDA\n" + PhotonNetwork.CurrentRoom.Name;
            bool enablePlayBtn = PhotonNetwork.LocalPlayer.IsMasterClient;

            List<string> characters = new List<string>();
            List<string> nicknames = new List<string>();

            foreach (var player in PhotonNetwork.CurrentRoom.Players.Values)
            {
                // Get room players selected characters
                if (player.CustomProperties.ContainsKey("ch"))
                    characters.Add((string)player.CustomProperties["ch"]);

                // Get room players nicknames
                if (player.NickName != "")
                    nicknames.Add(player.NickName);
            }

            uiManager.UpdateRoomUI(roomName, enablePlayBtn, characters, nicknames);
        }
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

        if (cause == DisconnectCause.DisconnectByClientLogic || cause == DisconnectCause.DisconnectByServerLogic)
            Application.Quit();
    }

    public override void OnJoinedLobby()
    {
        // Enable lobby panel
        if (uiManager) { uiManager.EnableLobby(); }
    }

    public override void OnJoinedRoom()
    {
        Debug.Log("Connected to room " + PhotonNetwork.CurrentRoom.Name);

        // Select random character for player that joined the room
        SelectRandomCharacter();

        // Enable room menu panel
        if (uiManager) { uiManager.EnableRoom(); }
    }

    public override void OnPlayerEnteredRoom(Photon.Realtime.Player newPlayer)
    {
        Debug.Log(newPlayer.NickName + " entered the room.");

        // Update current room UI
        UpdateRoomState();
    }

    public override void OnPlayerLeftRoom(Photon.Realtime.Player otherPlayer)
    {
        Debug.Log(otherPlayer.NickName + " left the room.");

        // Update current room UI
        UpdateRoomState();
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

    public override void OnPlayerPropertiesUpdate(Player targetPlayer, PhotonHashtable changedProps)
    {
        base.OnPlayerPropertiesUpdate(targetPlayer, changedProps);

        // Update current room UI
        UpdateRoomState();
    }
    #endregion
}