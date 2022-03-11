using System;
using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.UI;

public class NetworkManager : MonoBehaviourPunCallbacks
{
    #region Properties

    private TerrainGenerator _terrainGenerator;

    [SerializeField] private GameObject _menuPanel;
    [SerializeField] private GameObject _lobbyPanel;
    [SerializeField] private GameObject _roomPanel;
    [SerializeField] private string _gameSceneName;

    private Text _playerList;
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
        // TODO: si ya nos hemos conectado y hemos ido hacia atrás, para ahorrar tiempo no desconectamos
        // pero es necesario tenerlo en cuenta para saltarse el paso.
        // Si no, pues desconectamos y volveriamos a conectar luego
        PhotonNetwork.ConnectUsingSettings();
        PhotonNetwork.NickName = "Jugador_" + GenerateRandomText(2);
    }

    /// <summary>
    /// Crar una sala con un nombre aleatorio
    /// </summary>
    public void OnClickCreateRoom()
    {
        RoomOptions roomOptions = new RoomOptions();
        Dropdown playersDropdown = _lobbyPanel.transform.Find("Max Players Dropdown").GetComponent<Dropdown>();
        roomOptions.MaxPlayers = (byte) (playersDropdown.value + 2);
        Debug.Log("Max players: " + roomOptions.MaxPlayers);
        PhotonNetwork.CreateRoom(GenerateRandomText(7), roomOptions, TypedLobby.Default, null);
    }

    /// <summary>
    /// Se une a una sala con el nombre contenido en roomName.text
    /// </summary>
    public void OnClickJoinRoom()
    {
        InputField roomName = _lobbyPanel.GetComponentInChildren<InputField>();
        PhotonNetwork.JoinRoom(roomName.text.ToLower());
    }

    /// <summary>
    /// Inicia la partida si el jugador es el creador de la sala
    /// (es el MasterClient)
    /// </summary>
    public void OnClickStartGame()
    {
        if (!PhotonNetwork.IsMasterClient)
        {
            Debug.LogWarning("You are not the Master client.");
        }
        else
        {
            PhotonNetwork.LoadLevel(_gameSceneName);
        }
    }

    public void OnClickGoBack()
    {
        if (_activeMenu == _lobbyPanel)
        {
            EnableMenu(_menuPanel);
        }
        else if (_activeMenu == _roomPanel)
        {
            PhotonNetwork.LeaveRoom();
            EnableMenu(_lobbyPanel);
        }
    }

    #endregion

    #region Private Methods

    private void UpdatePlayerList()
    {
        string players = null;

        foreach (var player in PhotonNetwork.CurrentRoom.Players.Values)
        {
            players += player.NickName + "\n";
        }

        _playerList.text = players;
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

    #endregion

    #region Unity Callbacks

    private void Awake()
    {
        PhotonNetwork.AutomaticallySyncScene = true;
    }

    private void Start()
    {
        _playerList = _roomPanel.GetComponentInChildren<Text>();

        _menuList = new List<GameObject>();
        _menuList.Add(_menuPanel);
        _menuList.Add(_lobbyPanel);
        _menuList.Add(_roomPanel);
        _activeMenu = _menuPanel;
    }

    #endregion

    #region Photon Callbacks

    public override void OnConnectedToMaster()
    {
        Debug.Log("Connected to Master Server.");
        PhotonNetwork.JoinLobby();
        EnableMenu(_lobbyPanel);
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

    public override void OnDisconnected(DisconnectCause cause)
    {
        Debug.LogWarning("Disconnected: " + cause);
    }

    public override void OnCreatedRoom()
    {
        base.OnCreatedRoom();
        Debug.Log("Created room " + PhotonNetwork.CurrentRoom.Name);
    }

    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        base.OnCreateRoomFailed(returnCode, message);
        Debug.LogError("Room could not be created. Trying again...");
        PhotonNetwork.CreateRoom(GenerateRandomText(7));
    }

    public override void OnJoinedRoom()
    {
        Debug.Log("Connected to room " + PhotonNetwork.CurrentRoom.Name);

        Text roomNameText = _roomPanel.transform.Find("Room Name").GetComponent<Text>();
        roomNameText.text = "Sala: " + PhotonNetwork.CurrentRoom.Name;

        EnableMenu(_roomPanel);

        UpdatePlayerList();
    }

    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        base.OnJoinRoomFailed(returnCode, message);
        
        Debug.Log("Failed to join room: " + message);        
    }

    public override void OnPlayerEnteredRoom(Photon.Realtime.Player newPlayer)
    {
        Debug.Log(newPlayer.NickName + " entered the room.");
        UpdatePlayerList();
    }

    public override void OnPlayerLeftRoom(Photon.Realtime.Player otherPlayer)
    {
        Debug.Log(otherPlayer.NickName + " left the room.");
        UpdatePlayerList();
    }

    #endregion
}