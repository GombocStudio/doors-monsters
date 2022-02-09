using UnityEngine;
using Photon.Pun;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Hashtable = ExitGames.Client.Photon.Hashtable;
using Photon.Realtime;


// Script reponsible for handling the character selection of the players and loading the game scene whe all players are ready.
public class PrepareAndLoadGame : MonoBehaviourPunCallbacks
{
    // References to needed UI elements in the scene
    [Header("UI References")]
    public Text roomCodeText;
    public Text playersText;
    public Toggle readyToggle;

    // Start is called before the first frame update
    private void Start()
    {
        // Set content of roomCode text 
        if (roomCodeText)
            roomCodeText.text = "ROOM CODE: " + PhotonNetwork.CurrentRoom.Name;

        // Update ammount of players text
        UpdatePlayersText();

        // Update is ready status of local player
        UpdateIsReady();

        // Select character 0 by default
        SelectCharacter("Character 0");
    }

    // Update is called once per frame
    private void Update()
    {
        // Travel through the room's player list and check if all of them are ready
        if (PhotonNetwork.CurrentRoom != null)
        {
            bool loadGame = true;
            for (int i = 0; i < PhotonNetwork.CurrentRoom.PlayerCount; i++)
            {
                Hashtable hash = PhotonNetwork.PlayerList[i].CustomProperties;

                if (!hash.ContainsKey("isReady"))
                {
                    loadGame = false;
                }
                else if (!(bool)hash["isReady"])
                {
                    loadGame = false;
                }
            }

            // In case all players in the room are ready, load the Game scene.
            if (loadGame)
                SceneManager.LoadScene("Game");
        }
    }

    // Method triggered when the value of ready checkbox changes. Updates the is ready status of the local player.
    public void UpdateIsReady()
    {
        // Update local player is ready status
        if (readyToggle)
        {
            bool isReady = readyToggle.isOn;

            Hashtable hash = new Hashtable();
            hash.Add("isReady", isReady);
            PhotonNetwork.LocalPlayer.SetCustomProperties(hash);
        }
    }

    // Method called when local player checks a character checkbox. Sets in player preferences which character the local player has selected.
    public void SelectCharacter(string name)
    {
        if (name.Equals("Character 0"))
        {
            PlayerPrefs.SetInt("character", 0);
        }
        else if (name.Equals("Character 1"))
        {
            PlayerPrefs.SetInt("character", 1);
        }
    }

    // Called when number of players in the room changes. Updates the players text content.
    public void UpdatePlayersText()
    {
        if (playersText)
            playersText.text = "PLAYERS: " + PhotonNetwork.CurrentRoom.PlayerCount + "/" + PhotonNetwork.CurrentRoom.MaxPlayers;
    }

    // Triggered when a new player enters the room.
    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        UpdatePlayersText();
    }

    // Triggered when a player leaves the room.
    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        UpdatePlayersText();
    }

    // Method called when Leave button is clicked. Allows the local player to leave the room.
    public void LeaveRoom()
    {
        PhotonNetwork.LeaveRoom();
    }

    // Method called when the local player leaves the room. Loads the Lobby scene.
    public override void OnLeftRoom()
    {
        SceneManager.LoadScene("Lobby");
    }
}
