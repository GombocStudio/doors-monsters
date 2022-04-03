using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

public class GUIManager : MonoBehaviour
{
    private GameManager gameManager;
    private RoundManager roundManager;
    private ScoreManager scoreManager;

    [Header("UI panels")]
    public GameObject _gamePnl;
    public GameObject _roundPnl;
    public GameObject _exitPnl;
    public GameObject _endGamePnl;

    [Header("Transition panel variables")]
    public GameObject _transitionPnl;
    public float _transitionTime;

    [Header("UI panels animators")]
    private Animator _roundPnlAnim;
    private Animator _transitionPnlAnim;

    [Header("In game UI variables")]
    public Text _roundText;
    public Text _timeTxt;
    public GameObject _pinkScoreUI;
    public GameObject _yellowScoreUI;
    public GameObject _orangeScoreUI;
    public GameObject _redScoreUI;
    private int firstRankPos = 440;
    private int distanceBtwScores = 100;

    public GameObject miniMap;
    public GameObject lightsOffPnl;
    public GameObject icePnl;

    [Header("Power up indicators")]
    public List<UIRadialIndicator> powerupIndicators;

    [Header("End game panel UI variables")]
    public Image statusImage;
    public Sprite winSprite;
    public Sprite loseSprite;
    public List<Text> rankingTexts;

    // Start is called before the first frame update
    void Start()
    {
        gameManager = FindObjectOfType<GameManager>();
        roundManager = FindObjectOfType<RoundManager>();
        scoreManager = FindObjectOfType<ScoreManager>();

        if (_roundPnl) { _roundPnlAnim = _roundPnl.GetComponent<Animator>(); }
        if (_transitionPnl) { _transitionPnlAnim = _transitionPnl.GetComponent<Animator>(); }
    }

    public void StartRoundUI()
    {
        StartCoroutine(StartRoundUICR());
    }

    public void EndRoundUI()
    {
        StartCoroutine(EndRoundUICR());
    }

    public void EndGameUI()
    {
        StartCoroutine(EndGameUICR());
    }

    public void SetTimeUI(float time)
    {
        if (!_timeTxt) { return; }

        // Compute time in minutes and seconds
        int minutes = Mathf.FloorToInt(time / 60F);
        int seconds = Mathf.FloorToInt(time - minutes * 60);
        string niceTime = string.Format("{0:00}:{1:00}", minutes, seconds);

        // Display new time
        _timeTxt.text = niceTime;
    }

    public void SetRoundUI(int numRounds, int currentRound)
    {
        if (!_roundText) { return; }

        // Set current round UI
        _roundText.text = "RONDA " + currentRound + "/" + numRounds;
    }

    public void UpdateScoreListUI(List<KeyValuePair<string, KeyValuePair<string, int>>> scoreList)
    {
        // Disable all scores
        if (_yellowScoreUI) { _yellowScoreUI.SetActive(false); }
        if (_redScoreUI) { _redScoreUI.SetActive(false); }
        if (_orangeScoreUI) { _orangeScoreUI.SetActive(false); }
        if (_redScoreUI) { _pinkScoreUI.SetActive(false); }

        // Display ordered score UI list
        int rankYPosition = firstRankPos;
        foreach (KeyValuePair<string, KeyValuePair<string, int>> pair in scoreList)
        {
            switch (pair.Key)
            {
                // Ninja
                case "Character 0":
                    if (!_yellowScoreUI) { break; }
                    _yellowScoreUI.SetActive(true);
                    _yellowScoreUI.GetComponentInChildren<Text>().text = pair.Value.Value.ToString();
                    _yellowScoreUI.transform.localPosition = new Vector2(800, rankYPosition);
                    break;

                // India
                case "Character 1":
                    if (!_redScoreUI) { break; }
                    _redScoreUI.SetActive(true);
                    _redScoreUI.GetComponentInChildren<Text>().text = pair.Value.Value.ToString();
                    _redScoreUI.transform.localPosition = new Vector2(800, rankYPosition);
                    break;

                // Punky
                case "Character 2":
                    if (!_pinkScoreUI) { break; }
                    _pinkScoreUI.SetActive(true);
                    _pinkScoreUI.GetComponentInChildren<Text>().text = pair.Value.Value.ToString();
                    _pinkScoreUI.transform.localPosition = new Vector2(800, rankYPosition);
                    break;

                // Space hunter
                case "Character 3":
                    if (!_orangeScoreUI) { break; }
                    _orangeScoreUI.SetActive(true);
                    _orangeScoreUI.GetComponentInChildren<Text>().text = pair.Value.Value.ToString();
                    _orangeScoreUI.transform.localPosition = new Vector2(800, rankYPosition);
                    break;

                default:
                    break;
            }

            // Update rank screen position
            rankYPosition -= distanceBtwScores; 
        }
    }

    private void DisplayFinalPlayerScores()
    {
        // Disable game UI and enable end game panel
        // if (_exitPnl) { _exitPnl.SetActive(false); }
        if (_gamePnl) { _gamePnl.SetActive(false); }
        if (_endGamePnl) { _endGamePnl.SetActive(true); }

        if (!scoreManager) { return; }

        // Get my local score and sorted score list from score manager
        int myScore = scoreManager.GetMyScore();
        List<KeyValuePair<string, KeyValuePair<string, int>>> scoreList = scoreManager.GetScoreList();

        // Check if local player has the highest score
        if (statusImage && scoreList.Count > 0)
        { 
            // Local player has the highest score, set "Has ganado" title text
            if (myScore == scoreList[0].Value.Value)
            {
                statusImage.sprite = winSprite;
            }
            // Local player doesn't have the highest score, set "Has perdido" title text
            else
            {
                statusImage.sprite = loseSprite;
            }
        }

        // Display sorted score list in end game panel
        for (int i = 0; i < scoreList.Count; i++)
        {
            if (i >= rankingTexts.Count) { return; }

            if (rankingTexts[i])
            {
                rankingTexts[i].enabled = true;
                rankingTexts[i].text = scoreList[i].Value.Key + "      " + scoreList[i].Value.Value.ToString();
            }
        }
    }

    public void EnableMinimap(bool value)
    {
        if (!miniMap) { return; }

        miniMap.SetActive(value);
    }

    public void EnableLightsOff(bool value)
    {
        if (!lightsOffPnl) { return; }

        lightsOffPnl.SetActive(value);
    }

    public void EnableIcePanel(bool value)
    {
        if (!icePnl) { return; }

        icePnl.SetActive(value);
    }

    public void ActivatePowerupIndicator(int index, float time)
    {
        if (index >= 0 && index < powerupIndicators.Count)
        {
            if (!powerupIndicators[index]) { return; }

            powerupIndicators[index].ActivateXSeconds(time);
        }
    }

    #region Button Events
    
    public void OnClickContinue()
    {
        StartCoroutine(LeaveGameUICR());
    }

    public void DisplayHideExitPanel()
    {
        if (_exitPnl) { _exitPnl.SetActive(!_exitPnl.activeSelf); }
    }

    public void OnClickKeepPlaying()
    {
        if (_exitPnl) { _exitPnl.SetActive(false); }
    }

    public void OnClickLeaveGame()
    {
        StartCoroutine(LeaveGameUICR());
    }

    #endregion

    #region Coroutines
    IEnumerator StartRoundUICR()
    {
        // Start fade out transition
        if (_transitionPnlAnim) { _transitionPnlAnim.Play("FadeIn"); }

        // Wait until fade out is completely done
        yield return new WaitForSeconds(_transitionTime);

        // Play start round UI animation
        if (_roundPnlAnim) { _roundPnlAnim.Play("StartRound"); }

        //Play start round sound
        FindObjectOfType<AudioManager>().Play("StartGame");

        yield return new WaitForSeconds(4.25f);

        // After round counter UI animation is done activate player input and start round timer
        if (gameManager) { gameManager.EnablePlayerInput(true); }
        if (roundManager) { roundManager.StartTimer(); }
    }

    IEnumerator EndRoundUICR()
    {
        // Disable player input so players can't move
        if (gameManager) { gameManager.EnablePlayerInput(false); }

        // Play start round UI animation
        if (_roundPnlAnim) { _roundPnlAnim.Play("EndRound"); }

        // Wait...
        yield return new WaitForSeconds(2.0f);

        // Start fade out transition
        if (_transitionPnlAnim) { _transitionPnlAnim.Play("FadeOut"); }

        // Wait until fade out is completely done
        yield return new WaitForSeconds(_transitionTime);

        // Start new round (Instantiate characters and terrain)
        if (gameManager) { gameManager.StartGame(); }
    }

    IEnumerator EndGameUICR()
    {
        // Disable player input so players can't move
        if (gameManager) 
        {
            gameManager.SetGameAsFinished();
            gameManager.EnablePlayerInput(false); 
        }

        // Play start round UI animation
        if (_roundPnlAnim) { _roundPnlAnim.Play("EndRound"); }

        // Wait...
        yield return new WaitForSeconds(2.0f);

        // Start fade out transition
        if (_transitionPnlAnim) { _transitionPnlAnim.Play("FadeOut"); }

        // Wait until fade out is completely done
        yield return new WaitForSeconds(_transitionTime);

        // Enable end game panel where player scores are displayed
        DisplayFinalPlayerScores();

        // Start fade in transition
        if (_transitionPnlAnim) { _transitionPnlAnim.Play("FadeIn"); }
    }

    IEnumerator LeaveGameUICR()
    {
        // Start fade out transition
        if (_transitionPnlAnim) { _transitionPnlAnim.Play("FadeOut"); }

        // Wait until fade out is completely done
        yield return new WaitForSeconds(_transitionTime);

        // Leave game and load menu scene
        if (gameManager) { gameManager.LeaveGame(); }
    }

    #endregion
}
