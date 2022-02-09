using UnityEngine;
using UnityEngine.SceneManagement;


// Script responsible for loading online or local game scenes based on player's selection.
public class SelectGameMode : MonoBehaviour
{
    // Triggered when Online button is clicked. Loads ConnectToServer scene.
    public void LoadOnlineGame()
    {
        SceneManager.LoadScene("ConnectToServer");
    }

    // Triggered when Local button is pressed. // TODO Loads player selection scene in local mode.
    public void LoadLocalGame()
    {
        return;
    }
}
