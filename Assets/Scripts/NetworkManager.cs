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

    [SerializeField] private GameObject _menuPanel;
    [SerializeField] private GameObject _lobbyPanel;
    [SerializeField] private GameObject _roomPanel;
    [SerializeField] private GameObject _gameModePanel;
    private InputField _name;
    private InputField _room;
    private Text _playerList;
    private const int MinNameExtension = 3;

    private bool isLocal;
    #endregion

    #region Public Methods

    public void OnClickConnect()
    {
        if (_name.text.Length > MinNameExtension)
        {
            PhotonNetwork.NickName = _name.text;
            PhotonNetwork.ConnectUsingSettings();
        }
        else
        {
            Debug.LogWarning("Name extension is invalid.");
        }
    }

    public void OnClickCreateRoom()
    {
        isLocal = false;
        if (_name.text.Length > MinNameExtension)
        {
            PhotonNetwork.NickName = _name.text;
            PhotonNetwork.CreateRoom(_name.text);
        }
        else
        {
            Debug.LogWarning("Name extension is invalid.");
        }
    }

    public void OnClickJoinRoom()
    {
        isLocal = true;
        if (_room.text.Length > MinNameExtension)
        {
            PhotonNetwork.NickName = _name.text;
            PhotonNetwork.JoinRoom(_room.text);
        }
        else
        {
            _room.gameObject.SetActive(true);
            Debug.LogWarning("Name extension is invalid.");
        }
    }

    public void OnClickMatchmaking()
    {
        //if (_name.text.Length > MinNameExtension)
        //{
            PhotonNetwork.NickName = _name.text;
            PhotonNetwork.JoinRandomOrCreateRoom(null);
        //}
        //else
        //{
        //    Debug.LogWarning("Name extension is invalid.");
       // }
    }

    public void OnClickStartGame()
    {
        if (!PhotonNetwork.IsMasterClient)
        {
            Debug.LogWarning("You are not the Master client.");
        }
        else
        {
            PhotonNetwork.LoadLevel("GameThirdPerson");
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

    private void EnableMenuPanel()
    {
        _menuPanel.SetActive(true);
        _lobbyPanel.SetActive(false);
        _roomPanel.SetActive(false);
        _gameModePanel.SetActive(false);
    }

    public void EnableLobbyPanel()
    {
        _menuPanel.SetActive(false);
        _lobbyPanel.SetActive(true);
        _room.gameObject.SetActive(false);
        _roomPanel.SetActive(false);
        _gameModePanel.SetActive(false);
    }

    private void EnableRoomPanel()
    {
        _menuPanel.SetActive(false);
        _lobbyPanel.SetActive(false);
        _roomPanel.SetActive(true);
        _gameModePanel.SetActive(false);
    } 
    
    private void EnableGameModePanel()
    {
        _menuPanel.SetActive(false);
        _lobbyPanel.SetActive(false);
        _roomPanel.SetActive(false);
        _gameModePanel.SetActive(true);
    }

    #endregion

    #region Unity Callbacks

    private void Awake()
    {
        PhotonNetwork.AutomaticallySyncScene = true;
    }

    private void Start()
    {
        _name = _menuPanel.GetComponentInChildren<InputField>();
        _room = _lobbyPanel.GetComponentInChildren<InputField>();
        _playerList = _roomPanel.GetComponentInChildren<Text>();
    }

    #endregion

    #region Photon Callbacks

    public override void OnConnectedToMaster()
    {
        base.OnConnectedToMaster();
        Debug.Log("Connected to Master Server.");
        EnableGameModePanel();
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        base.OnDisconnected(cause);
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
        Debug.LogError("Room could not be created.");
    }

    public override void OnJoinedRoom()
    {
        base.OnJoinedRoom();
        Debug.Log("Connected to room " + PhotonNetwork.CurrentRoom.Name);
        EnableRoomPanel();
        UpdatePlayerList();
    }

    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        base.OnJoinRoomFailed(returnCode, message);
        if (isLocal)
        {
            Debug.Log("Failed to join random room " + PhotonNetwork.CurrentRoom.Name);
        }
        else
        {
            Debug.Log("Failed to join random room. Creating a room.");
            PhotonNetwork.CreateRoom(null);
        }
        
    }

    public override void OnPlayerEnteredRoom(Photon.Realtime.Player newPlayer)
    {
        base.OnPlayerEnteredRoom(newPlayer);
        Debug.Log(newPlayer.NickName + " entered the room.");
        UpdatePlayerList();
    }

    public override void OnPlayerLeftRoom(Photon.Realtime.Player otherPlayer)
    {
        base.OnPlayerLeftRoom(otherPlayer);
        Debug.Log(otherPlayer.NickName + " left the room.");
        UpdatePlayerList();
    }

    #endregion
}