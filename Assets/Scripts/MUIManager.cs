using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MUIManager : MonoBehaviour
{
    // Network manager reference
    private MNetworkManager networkManager;

    [Header("Menu panels")]
    public GameObject _mainPnl;
    public GameObject _lobbyPnl;
    public GameObject _roomPnl;
    public GameObject _errorPnl;

    [Header("Lobby menu variables")]
    public InputField _roomNameInputField;
    public Dropdown _maxPlayersDropdown;
    public Dropdown _numRoundsDropdown;

    [Header("Room menu variables")]
    public Text _roomNameTxt;
    public Button _startGameBtn;
    public List<GameObject> characterPlatforms;
    public List<GameObject> characterPrefabs;
    public List<Text> playerNicknameTxts;

    [Header("Error popup variables")]
    public Text _errorTxt;

    [Header("Loading GIF reference")]
    public GameObject _loadingGIF;

    [Header("Scene trasition panel variables")]
    public GameObject _transitionPnl;
    public float _transitionTime;
    private Animator _transitionAnim;

    // Start is called before the first frame update
    void Start()
    {
        // Initialise network manager reference
        networkManager = FindObjectOfType<MNetworkManager>();

        // Initialise transition panel animator
        if (_transitionPnl) { _transitionAnim = _transitionPnl.GetComponent<Animator>(); }
    }

    public void EnableMain()
    {
        StartCoroutine(EnableMainCR());
    }

    public void EnableLobby()
    {
        StartCoroutine(EnableLobbyCR());
    }

    public void EnableRoom()
    {
        StartCoroutine(EnableRoomCR());
    }

    public void EnableLoadingGIF(bool value)
    {
        if (!_loadingGIF) { return; }

        _loadingGIF.SetActive(value);
    }

    public void UpdateRoomUI(string roomName, bool enablePlayBtn, List<string> characters, List<string> nicknames)
    {
        // Update room name text
        if (_roomNameTxt) { _roomNameTxt.text = roomName; }

        // Update play button state
        if (_startGameBtn) { _startGameBtn.interactable = enablePlayBtn; }

        // Reset diplayed prefabs position
        foreach (GameObject prefab in characterPrefabs)
            prefab.transform.position += new Vector3(9999.0f, 0, 0);

        // Update displayed character prefabs
        for (int i = 0; i < characters.Count; i++)
        {
            for (int j = 0; j < characterPrefabs.Count; j++)
            {
                if (characters[i] != characterPrefabs[j].name) { continue; }

                // Display character prefab
                if (i < characterPlatforms.Count && characterPlatforms[i])
                    characterPrefabs[j].transform.position = characterPlatforms[i].transform.position;

                break;
            }
        }

        foreach (Text nickname in playerNicknameTxts)
            nickname.text = "";

        // Display player nickname
        for (int i = 0; i < nicknames.Count; i++)
        {
            if (i < playerNicknameTxts.Count && playerNicknameTxts[i])
                playerNicknameTxts[i].text = nicknames[i];
        }
    }

    public void ToUpperCase(InputField inputField)
    {
        if (inputField) { inputField.text = inputField.text.ToUpper(); }
    }

    #region Button Events
    public void OnClickStartGame()
    {
        StartCoroutine(StartGameCR());
    }

    public void OnClickCreateRoom()
    {
        // Get max players an num rounds from UI
        int maxPlayers = (_maxPlayersDropdown) ? _maxPlayersDropdown.value + 2 : 4;
        int numRounds = (_numRoundsDropdown) ? _numRoundsDropdown.value + 2 : 2;

        // Create room with specified parameters from network manager
        if (networkManager) { networkManager.CreateRoom(maxPlayers, numRounds); }
    }

    public void OnClickJoinRoom()
    {
        // Get room name from UI
        string roomName = (_roomNameInputField) ? _roomNameInputField.text : "";

        if (roomName == "") { DisplayErrorMsg("Por favor, introduce el nombre de la partida a la que quieres unirte."); return; }

        // Join room with specified name
        if (networkManager) { networkManager.JoinRoom(roomName); }
    }

    public void OnClickGoBack()
    {
        if (!networkManager) { return; }

        // Disconnect from server
        if (_lobbyPnl && _lobbyPnl.activeSelf)
        {
            networkManager.DisconnectFromServerAndCloseGame();
        }
        // Leave room
        else if (_roomPnl && _roomPnl.activeSelf)
        {
            networkManager.LeaveRoom();
        }
    }

    public void OnClickCloseError()
    {
        // Hide error panel
        if (_errorPnl) { _errorPnl.SetActive(false); }
    }

    #endregion

    #region Coroutines

    IEnumerator EnableMainCR()
    {
        if (_transitionAnim) { _transitionAnim.Play("FadeOut"); }

        yield return new WaitForSeconds(_transitionTime);

        if (_transitionAnim) { _transitionAnim.Play("FadeIn"); }

        if (_mainPnl) { _mainPnl.SetActive(true); }
        if (_lobbyPnl) { _lobbyPnl.SetActive(false); }
        if (_roomPnl) { _roomPnl.SetActive(false); }
    }

    IEnumerator EnableLobbyCR()
    {
        if (_transitionAnim) { _transitionAnim.Play("FadeOut"); }

        yield return new WaitForSeconds(_transitionTime);

        if (_transitionAnim) { _transitionAnim.Play("FadeIn"); }

        if (_mainPnl) { _mainPnl.SetActive(false); }
        if (_lobbyPnl) { _lobbyPnl.SetActive(true); }
        if (_roomPnl) { _roomPnl.SetActive(false); }
    }

    IEnumerator EnableRoomCR()
    {
        if (_transitionAnim) { _transitionAnim.Play("FadeOut"); }

        yield return new WaitForSeconds(_transitionTime);

        if (_transitionAnim) { _transitionAnim.Play("FadeIn"); }

        if (_mainPnl) { _mainPnl.SetActive(false); }
        if (_lobbyPnl) { _lobbyPnl.SetActive(false); }
        if (_roomPnl) { _roomPnl.SetActive(true); }
    }

    IEnumerator StartGameCR()
    {
        if (_transitionAnim) { _transitionAnim.Play("FadeOut"); }

        yield return new WaitForSeconds(_transitionTime);

        if (networkManager) { networkManager.StartGame(); }
    }

    #endregion

    #region Error Handler

    public void HandleError(short errorCode)
    {
        switch (errorCode)
        {
            case 32766:
                OnClickCreateRoom();
                break;

            case 32758:
                DisplayErrorMsg("La partida a la que intentas unirte no existe.");
                break;

            case 32765:
                DisplayErrorMsg("La partida a la que intentas unirte está completa.");
                break;

            case 32762:
                DisplayErrorMsg("Los servidores del juego están completos.");
                break;

            case 32764:
                DisplayErrorMsg("La partida a la que intentas unirte ya está en curso.");
                break;

            default:
                DisplayErrorMsg("Ha ocurrido un error inesperado.");
                break;
        }
    }

    public void DisplayErrorMsg(string errorMsg)
    {
        // Enable error panel
        if (_errorPnl) { _errorPnl.SetActive(true); }

        // Add error message to the content of the panel
        if (_errorTxt) { _errorTxt.text = errorMsg.ToUpper(); }
    }


    #endregion
}
