using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.UI;
using UnityEngine.SceneManagement;


// Script responsible for allowing the players create/join rooms, leave the default lobby, and disconnect from the master server.
public class CreateAndJoinRooms : MonoBehaviourPunCallbacks
{
    // Reference to the roomCode input field
    [Header("UI References")]
    public InputField joinInput;


    /*** CREATE ROOM METHODS ***/

    // This method tries to create a new room with a 4 digits random name and a specific max ammount of players
    public void CreateRoom()
    {
        // Stablish 4 digits room code
        string roomCode = Random.Range(0, 9).ToString() + Random.Range(0, 9).ToString() + Random.Range(0, 9).ToString() + Random.Range(0, 9).ToString();

        // Stablish room options (Max players, time the room will still be in the server while empty, etc...)
        RoomOptions options = new RoomOptions();
        options.MaxPlayers = 4;
        options.EmptyRoomTtl = 300000;

        // Create new room with the corresponding parameters
        PhotonNetwork.CreateRoom(roomCode, options);
    }

    // If room couldn't be created because another room with the same name already exists, we simply try to create it again
    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        CreateRoom();
    }


    /*** JOIN ROOM METHODS ***/

    // Method linked to the OnClick event of the Join button. Tries to make the local player join the room which name is specified in joinInput.text
    public void JoinRoom()
    {
        if (joinInput && joinInput.text != "")
            PhotonNetwork.JoinRoom(joinInput.text);
    }

    // Method is called when local player successfully joined a room
    public override void OnJoinedRoom()
    {
        PhotonNetwork.LoadLevel("Room");
    }

    // Prints error code and message in case the process of joining a room failed
    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        Debug.Log("Code: " + returnCode + " Message: " + message);
    }


    /*** LEAVE LOBBY METHODS ***/

    // Method linked to the OnClick method of the Leave button. Makes the local player disconnect from the master server.
    public void LeaveLobby()
    {
        PhotonNetwork.Disconnect();
    }

    // Triggered when the local player disconnects from the master server. Debugs cause of the disconnection and loads the mainManu scene.
    public override void OnDisconnected(DisconnectCause cause)
    {
        Debug.Log("Cause of disconnection: " + cause);
        SceneManager.LoadScene("MainMenu");
    }
}
