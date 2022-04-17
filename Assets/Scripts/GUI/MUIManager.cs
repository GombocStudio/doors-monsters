using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MUIManager : MonoBehaviour
{
    // Network manager reference
    private MNetworkManager networkManager;

    // Available resolutions
    private Resolution[] resolutions;

    [Header("Cursor Texture")]
    public Texture2D cursorTexture;

    [Header("Menu panels")]
    public GameObject _mainPnl;
    public GameObject _lobbyPnl;
    public GameObject _roomPnl;
    public GameObject _errorPnl;
    public GameObject _settingsPnl;
    public GameObject _contactPnl;

    [Header("Lobby menu variables")]
    public InputField _nicknameInputField;
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

    [Header("Settings variables")]
    public Slider _volumeSlider;
    public Dropdown _resolutionDropdown;
    public Toggle _screenModeToggle;

    // Start is called before the first frame update
    void Start()
    {
        // Initialise network manager reference
        networkManager = FindObjectOfType<MNetworkManager>();

        // Initialise transition panel animator
        if (_transitionPnl) { _transitionAnim = _transitionPnl.GetComponent<Animator>(); }     
        
        // Set cursor texture
        if (cursorTexture) { Cursor.SetCursor(cursorTexture, Vector2.zero, CursorMode.Auto); }

        // Init settings values
        InitSettings();
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

    public void EnableSettings()
    {
        if (_settingsPnl && _volumeSlider)
        {
            _volumeSlider.value = AudioListener.volume;
        }
        StartCoroutine(EnableSettingsCR());
    }

    public void EnableLoadingGIF(bool value)
    {
        if (!_loadingGIF) { return; }

        _loadingGIF.SetActive(value);
    }

    public void UpdateNicknameInputField(string newNickname)
    {
        if (!_nicknameInputField) { return; }

        _nicknameInputField.text = newNickname;
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
        // Get local player nickname
        string nickname = (_nicknameInputField) ? _nicknameInputField.text : "";

        // Get max players an num rounds from UI
        int maxPlayers = (_maxPlayersDropdown) ? _maxPlayersDropdown.value + 2 : 4;
        int numRounds = (_numRoundsDropdown) ? _numRoundsDropdown.value + 2 : 2;

        // Create room with specified parameters from network manager
        if (networkManager) { networkManager.CreateRoom(maxPlayers, numRounds, nickname); }
    }

    public void OnClickJoinRoom()
    {
        // Get local player nickname
        string nickname = (_nicknameInputField) ? _nicknameInputField.text : "";

        // Get room name from UI
        string roomName = (_roomNameInputField) ? _roomNameInputField.text : "";

        if (roomName == "") { DisplayErrorMsg("Por favor, introduce el nombre de la sala a la que quieres unirte."); return; }

        // Join room with specified name
        if (networkManager) { networkManager.JoinRoom(roomName, nickname); }
    }

    public void OnClickGoBack()
    {
        if (_settingsPnl.activeSelf)
        {
            StartCoroutine(DisableSettingsCR());
            return;
        }

        if (_contactPnl.activeSelf)
        {
            StartCoroutine(DisableContactCR());
            return;
        }

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

    public void OnClickContactButton()
    {
        StartCoroutine(EnableContactCR());
    }

    public void OnClickCloseError()
    {
        // Hide error panel
        if (_errorPnl) { _errorPnl.SetActive(false); }
    }

    public void OnClickSettings()
    {
        EnableSettings();
    }

    public void OnClickContactLinkButton(string url)
    {
        Application.OpenURL(url);
    }

    #endregion

    #region Settings

    public void InitSettings()
    {
        // Init volume value
        if (_volumeSlider) { _volumeSlider.SetValueWithoutNotify(AudioListener.volume); }

        // Init resolution value
        if (_resolutionDropdown)
        {
            resolutions = Screen.resolutions;
            System.Array.Reverse(resolutions);

            List<string> dropOptions = new List<string>();

            foreach (var res in resolutions)
            {
                dropOptions.Add(res.width + " X " + res.height + " : " + res.refreshRate + " HZ");
            }

            _resolutionDropdown.ClearOptions();
            _resolutionDropdown.AddOptions(dropOptions);
            _resolutionDropdown.value = PlayerPrefs.GetInt("CurrentRes", 0); ;

            OnResolutionValueChanged();
        }

        // Init screen mode value
        if (_screenModeToggle) { _screenModeToggle.SetIsOnWithoutNotify(Screen.fullScreen); }
    }

    public void OnVolumeSliderValueChanged()
    {
        AudioListener.volume = _volumeSlider.value;        
    }

    public void OnResolutionValueChanged()
    {
        int currentRes = _resolutionDropdown.value;
        Resolution res = resolutions[currentRes];
        Screen.SetResolution(res.width, res.height, Screen.fullScreen, res.refreshRate);
        PlayerPrefs.SetInt("CurrentRes", currentRes);
    }

    public void OnScreenModeChanged()
    {
        if (_screenModeToggle) { Screen.fullScreen = _screenModeToggle.isOn; }
    }

    #endregion

    #region Coroutines

    IEnumerator EnableMainCR()
    {
        if (_transitionAnim) { _transitionAnim.Play("FadeOut"); }

        yield return new WaitForSeconds(_transitionTime);

        if (_transitionAnim) { _transitionAnim.Play("FadeIn"); }

        EnableLoadingGIF(false);

        if (_mainPnl) { _mainPnl.SetActive(true); }
        if (_lobbyPnl) { _lobbyPnl.SetActive(false); }
        if (_roomPnl) { _roomPnl.SetActive(false); }
        if (_settingsPnl) { _settingsPnl.SetActive(false); }
    }

    IEnumerator EnableLobbyCR()
    {
        if (_transitionAnim) { _transitionAnim.Play("FadeOut"); }

        yield return new WaitForSeconds(_transitionTime);

        if (_transitionAnim) { _transitionAnim.Play("FadeIn"); }

        EnableLoadingGIF(false);

        if (_mainPnl) { _mainPnl.SetActive(false); }
        if (_lobbyPnl) { _lobbyPnl.SetActive(true); }
        if (_roomPnl) { _roomPnl.SetActive(false); }
        if (_settingsPnl) { _settingsPnl.SetActive(false); }
    }

    IEnumerator EnableRoomCR()
    {
        if (_transitionAnim) { _transitionAnim.Play("FadeOut"); }

        yield return new WaitForSeconds(_transitionTime);

        if (_transitionAnim) { _transitionAnim.Play("FadeIn"); }

        EnableLoadingGIF(false);

        if (_mainPnl) { _mainPnl.SetActive(false); }
        if (_lobbyPnl) { _lobbyPnl.SetActive(false); }
        if (_roomPnl) { _roomPnl.SetActive(true); }
        if (_settingsPnl) { _settingsPnl.SetActive(false); }
    }


    IEnumerator EnableSettingsCR()
    {
        if (_transitionAnim) { _transitionAnim.Play("FadeOut"); }

        yield return new WaitForSeconds(_transitionTime);

        if (_transitionAnim) { _transitionAnim.Play("FadeIn"); }

        EnableLoadingGIF(false);

        if (_settingsPnl) { _settingsPnl.SetActive(true); }
    }

    IEnumerator DisableSettingsCR()
    {
        if (_transitionAnim) { _transitionAnim.Play("FadeOut"); }

        yield return new WaitForSeconds(_transitionTime);

        if (_transitionAnim) { _transitionAnim.Play("FadeIn"); }

        EnableLoadingGIF(false);

        if (_settingsPnl) { _settingsPnl.SetActive(false); }
    }

    IEnumerator EnableContactCR()
    {
        if (_transitionAnim) { _transitionAnim.Play("FadeOut"); }

        yield return new WaitForSeconds(_transitionTime);

        if (_transitionAnim) { _transitionAnim.Play("FadeIn"); }

        EnableLoadingGIF(false);

        if (_mainPnl) { _mainPnl.SetActive(false); }
        if (_lobbyPnl) { _lobbyPnl.SetActive(false); }
        if (_roomPnl) { _roomPnl.SetActive(false); }
        if (_settingsPnl) { _settingsPnl.SetActive(false); }
        if (_contactPnl) { _contactPnl.SetActive(true); }
    }

    IEnumerator DisableContactCR()
    {
        if (_transitionAnim) { _transitionAnim.Play("FadeOut"); }

        yield return new WaitForSeconds(_transitionTime);

        if (_transitionAnim) { _transitionAnim.Play("FadeIn"); }

        EnableLoadingGIF(false);

        if (_mainPnl) { _mainPnl.SetActive(false); }
        if (_lobbyPnl) { _lobbyPnl.SetActive(true); }
        if (_roomPnl) { _roomPnl.SetActive(false); }
        if (_settingsPnl) { _settingsPnl.SetActive(false); }
        if (_contactPnl) { _contactPnl.SetActive(false); }
    }

    IEnumerator StartGameCR()
    {
        if (_startGameBtn) { _startGameBtn.interactable = false; }

        if (_loadingGIF) { _loadingGIF.SetActive(true); }

        if (_transitionAnim) { _transitionAnim.Play("FadeOut"); }

        yield return new WaitForSeconds(_transitionTime);

        if (networkManager) { networkManager.StartGame(); }
    }

    #endregion

    #region Error Handler

    public void HandleError(short errorCode)
    {
        if (_loadingGIF) { _loadingGIF.SetActive(false); }

        switch (errorCode)
        {
            case 32766:
                OnClickCreateRoom();
                break;

            case 32758:
                DisplayErrorMsg("La sala a la que intentas unirte no existe.");
                break;

            case 32765:
                DisplayErrorMsg("La sala a la que intentas unirte est� completa.");
                break;

            case 32762:
                DisplayErrorMsg("Los servidores del juego est�n completos.");
                break;

            case 32764:
                DisplayErrorMsg("La sala a la que intentas unirte ya est� en curso.");
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
