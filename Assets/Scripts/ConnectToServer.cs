using Photon.Pun;
using UnityEngine.SceneManagement;


// Script responsible for connecting the local player to the master server and the default lobby.
public class ConnectToServer : MonoBehaviourPunCallbacks
{
    // Start is called before the first frame update
    void Start()
    {
        // Connects local player to the master server using the settings specified in PhotonServerSettings
        PhotonNetwork.ConnectUsingSettings();
    }

    // Triggered when the local player succesfully connected to the master server. Loads the Lobby scene.
    public override void OnConnectedToMaster()
    {
        SceneManager.LoadScene("Lobby");
    }
}
