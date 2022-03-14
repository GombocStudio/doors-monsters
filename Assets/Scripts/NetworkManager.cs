using System;
using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.UI;
using PhotonHashtable = ExitGames.Client.Photon.Hashtable;

public class NetworkManager : MonoBehaviourPunCallbacks
{
    #region Properties

    // UI panel references
    [Header("UI Menu Panels")]
    [SerializeField] private GameObject _menuPanel;
    [SerializeField] private GameObject _lobbyPanel;
    [SerializeField] private GameObject _roomPanel;
    [SerializeField] private GameObject _errorPanel;

    // Game scene
    [Header("Game Scene")]
    [SerializeField] private string _gameSceneName;

    // Lobby UI variables
    [Header("Lobby UI Variables")]
    [SerializeField] private InputField _roomNameIF;
    [SerializeField] private Dropdown _maxPlayers;

    // Room UI variables
    [Header("Room UI Variables")]
    [SerializeField] private Text _playerList;
    [SerializeField] private Text _roomName;
    [SerializeField] private Button _startGameBtn;

    // Error UI variables
    [Header("Error UI Variables")]
    [SerializeField] private Text _error;

    private List<GameObject> _menuList;
    private GameObject _activeMenu;

    #endregion

    #region Public Methods
    /// <summary>
    /// Conecta con los servidores de Photon al pulsar
    /// en el boton "Jugar".
    /// </summary>
    public void OnClickConnect()
    {
        // If player already connected do nothing
        if (PhotonNetwork.IsConnected) { return; }

        // Connect to server
        PhotonNetwork.ConnectUsingSettings();

        // Generate random nickname
        PhotonNetwork.NickName = "Jugador_" + GenerateRandomText(2);
    }

    /// <summary>
    /// Crar una sala con un nombre aleatorio
    /// </summary>
    public void OnClickCreateRoom()
    {
        // Don't create a new room is client is already joining one
        if (PhotonNetwork.NetworkClientState == ClientState.Authenticating 
        || PhotonNetwork.NetworkClientState == ClientState.Joining
        || PhotonNetwork.NetworkClientState == ClientState.ConnectingToGameServer) 
        { return; }

        // Create room options
        RoomOptions roomOptions = new RoomOptions();

        // Set max number of players in the room
        if (!_maxPlayers) { return; }
        roomOptions.MaxPlayers = (byte) (/*_maxPlayers.value + 2*/ 4);
        // Debug.Log("Max players: " + roomOptions.MaxPlayers);

        // Create the room
        PhotonNetwork.CreateRoom(GenerateRandomText(7), roomOptions, TypedLobby.Default, null);
    }

    /// <summary>
    /// Se une a una sala con el nombre contenido en roomName.text
    /// </summary>
    public void OnClickJoinRoom()
    {
        // Don't try to join room if client is already joining
        if (PhotonNetwork.NetworkClientState == ClientState.Joining) { return; }

        // Display error message if text in input field is empty
        if (!_roomNameIF) { return; }

        if (_roomNameIF.text == "")
        {
            DisplayError("El nombre de la partida a la que pretendes unirte no puede estar vacio.");
            return;
        }

        // Join room
        PhotonNetwork.JoinRoom(_roomNameIF.text.ToLower());
    }

    /// <summary>
    /// Inicia la partida si el jugador es el creador de la sala
    /// (es el MasterClient)
    /// </summary>
    public void OnClickStartGame()
    {
        PhotonNetwork.LoadLevel(_gameSceneName);
    }

    public void OnClickGoBack()
    {
        // Go back to main menu
        if (_activeMenu == _lobbyPanel)
            PhotonNetwork.Disconnect();

        // Go back to lobby
        else if (_activeMenu == _roomPanel)
            PhotonNetwork.LeaveRoom();
    }

    public void OnClickCloseError()
    {
        // Disable error panel
        if (!_errorPanel) { return; }
        _errorPanel.SetActive(false);
    }

    public void SelectCharacter(string characterName)
    {
        PhotonHashtable hash = new PhotonHashtable();
        hash.Add("character", characterName);
        PhotonNetwork.LocalPlayer.SetCustomProperties(hash);
    }

    #endregion

    #region Private Methods

    private void UpdateRoomState()
    {
        // Update player list text content
        if (_playerList)
        {
            _playerList.text = "JUGADORES EN PARTIDA";

            // Add room players to the list
            foreach (var player in PhotonNetwork.CurrentRoom.Players.Values)
                _playerList.text += "\n" + player.NickName;
        }

        // Update start game button state
        if (_startGameBtn)
            _startGameBtn.interactable = PhotonNetwork.LocalPlayer.IsMasterClient;
    }

    private void EnableMenu(GameObject menu)
    {
        for (int i = 0; i < _menuList.Count; i++)
            _menuList[i].SetActive(_menuList[i] == menu ? true : false);

        _activeMenu = menu;
    }

    private string GenerateRandomText(int length)
    {
        string usedCharacters = "qwertyuiopasdfghjklzxcvbnm0123456789";
        string roomName = "";

        for (int i = 0; i < length; i++)
            roomName += usedCharacters[UnityEngine.Random.Range(0, usedCharacters.Length)];
        
        return roomName;
    }

    private void DisplayError(string errorMsg)
    {
        // Enable error panel
        if (_errorPanel) { _errorPanel.SetActive(true); }
            
        // Add error message to the content of the panel
        if (_error) { _error.text = errorMsg; }
    }

    #endregion

    #region Unity Callbacks

    private void Awake()
    {
        PhotonNetwork.AutomaticallySyncScene = true;
    }

    private void Start()
    {
        // Add panels to menu list
        _menuList = new List<GameObject>();
        _menuList.Add(_menuPanel);
        _menuList.Add(_lobbyPanel);
        _menuList.Add(_roomPanel);
        _activeMenu = _menuPanel;

        // Set default character selection as Character 0
        SelectCharacter("Character 0");
    }

    #endregion

    #region Photon Callbacks

    public override void OnConnectedToMaster()
    {
        Debug.Log("Connected to Master Server.");
        PhotonNetwork.JoinLobby();
        EnableMenu(_lobbyPanel);
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        Debug.LogWarning("Disconnected: " + cause);

        // Enable menu panel if possible
        if (!_menuPanel) { return; }
        EnableMenu(_menuPanel);
    }

    public override void OnJoinedLobby()
    {
        Debug.Log("Joined lobby: " + (PhotonNetwork.CurrentLobby.IsDefault ? "Default lobby" : PhotonNetwork.CurrentLobby.Name));
    }


    /*public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        String rooms = "Room List:\n";
        foreach (RoomInfo ri in roomList)
        {
            rooms += ri.Name + "\n";
        }
        Debug.Log(rooms);
    }*/

    public override void OnCreatedRoom()
    {
        base.OnCreatedRoom();
        Debug.Log("Created room " + PhotonNetwork.CurrentRoom.Name);
    }

    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        // If room already exist, try to create it again
        if (returnCode == 32766)
            PhotonNetwork.CreateRoom(GenerateRandomText(7));

        // Display create room erro message
        else
            DisplayError("Ha ocurrido un error inesperado al intentar crear la partida. Por favor, vuelve a intentarlo.");

        Debug.Log("Failed to create room " + returnCode + ": " + message);
    }

    public override void OnJoinedRoom()
    {
        Debug.Log("Connected to room " + PhotonNetwork.CurrentRoom.Name);

        if (_roomName)
            _roomName.text = "NOMBRE DE PARTIDA\n" + PhotonNetwork.CurrentRoom.Name;

        EnableMenu(_roomPanel);
        UpdateRoomState();
    }

    public override void OnLeftRoom()
    {
        // Enable lobby panel if possible
        if (!_lobbyPanel) { return; }
        EnableMenu(_lobbyPanel);
    }

    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        // Display join room error message
        switch (returnCode)
        {
            case 32758:
                DisplayError("La partida a la que intentas unirte no existe.");
                break;
            default:
                DisplayError("Ha ocurrido un error inesperado al intentar unirte a la partida. Por favor, asegurate de que el nombre introducido es correcto.");
                break;
        }

        Debug.Log("Failed to join room " + returnCode + ": " + message);        
    }

    public override void OnPlayerEnteredRoom(Photon.Realtime.Player newPlayer)
    {
        Debug.Log(newPlayer.NickName + " entered the room.");
        UpdateRoomState();
    }

    public override void OnPlayerLeftRoom(Photon.Realtime.Player otherPlayer)
    {
        Debug.Log(otherPlayer.NickName + " left the room.");
        UpdateRoomState();
    }

    #endregion
}