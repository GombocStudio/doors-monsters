using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UI;
using UnityEngine;

public class GUIManager : MonoBehaviour
{
    private GameManager gameManager;
    private RoundManager roundManager;

    [Header("UI panels")]
    public GameObject _gamePnl;
    public GameObject _roundPnl;
    public GameObject _errorPnl;
    public GameObject _endGamePnl;

    [Header("Transition panel variables")]
    public GameObject _transitionPnl;
    public float _transitionTime;

    [Header("UI panels animators")]
    private Animator _roundPnlAnim;
    private Animator _transitionPnlAnim;
        
    [Header("In game UI variables")]
    public Text _timeTxt;
    public GameObject _pinkScoreUI;
    public GameObject _yellowScoreUI;
    public GameObject _orangeScoreUI;
    public GameObject _redScoreUI;
    private int firstRankPos = 440;
    private int distanceBtwScores = 100;


    // Start is called before the first frame update
    void Start()
    {
        gameManager = FindObjectOfType<GameManager>();
        roundManager = FindObjectOfType<RoundManager>();

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

    public void UpdateScoreListUI(Dictionary<string, int> scoreDictionary)
    {
        // Disable all scores
        if (_yellowScoreUI) { _yellowScoreUI.SetActive(false); }
        if (_redScoreUI) { _redScoreUI.SetActive(false); }
        if (_orangeScoreUI) { _orangeScoreUI.SetActive(false); }
        if (_redScoreUI) { _pinkScoreUI.SetActive(false); }

        // Move score dictionary into a list and sort it
        List<KeyValuePair<string, int>> scoreList = new List<KeyValuePair<string, int>>(scoreDictionary);
        scoreList = scoreList.OrderByDescending(x => x.Value).ToList();

        // Display ordered score UI list
        int rankYPosition = firstRankPos;
        foreach (KeyValuePair<string, int> pair in scoreList)
        {
            switch (pair.Key)
            {
                // Ninja
                case "Character 0":
                    if (!_yellowScoreUI) { break; }
                    _yellowScoreUI.SetActive(true);
                    _yellowScoreUI.GetComponentInChildren<Text>().text = pair.Value.ToString();
                    _yellowScoreUI.transform.localPosition = new Vector2(800, rankYPosition);
                    break;

                // India
                case "Character 1":
                    if (!_redScoreUI) { break; }
                    _redScoreUI.SetActive(true);
                    _redScoreUI.GetComponentInChildren<Text>().text = pair.Value.ToString();
                    _redScoreUI.transform.localPosition = new Vector2(800, rankYPosition);
                    break;

                // Punky
                case "Character 2":
                    if (!_pinkScoreUI) { break; }
                    _pinkScoreUI.SetActive(true);
                    _pinkScoreUI.GetComponentInChildren<Text>().text = pair.Value.ToString();
                    _pinkScoreUI.transform.localPosition = new Vector2(800, rankYPosition);
                    break;

                // Space hunter
                case "Character 3":
                    if (!_orangeScoreUI) { break; }
                    _orangeScoreUI.SetActive(true);
                    _orangeScoreUI.GetComponentInChildren<Text>().text = pair.Value.ToString();
                    _orangeScoreUI.transform.localPosition = new Vector2(800, rankYPosition);
                    break;

                default:
                    break;
            }

            // Update rank screen position
            rankYPosition -= distanceBtwScores; 
        }
    }

    #region Button Events
    
    public void OnClickContinue()
    {
        StartCoroutine(LeaveGameUICR());
    }

    #endregion

    #region Coroutines

    IEnumerator StartRoundUICR()
    {
        // Play start round UI animation
        if (_roundPnlAnim) { _roundPnlAnim.Play("StartRound"); }

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

        // Wait util new round terrain and characters have been spawned
        yield return new WaitForSeconds(0.5f);

        // Start fade in transition
        if (_transitionPnlAnim) { _transitionPnlAnim.Play("FadeIn"); }
    }

    IEnumerator EndGameUICR()
    {
        // Disable player input so players can't move
        if (gameManager) { gameManager.EnablePlayerInput(false); }

        // Play start round UI animation
        if (_roundPnlAnim) { _roundPnlAnim.Play("EndRound"); }

        // Wait...
        yield return new WaitForSeconds(3.0f);

        // Start fade out transition
        if (_transitionPnlAnim) { _transitionPnlAnim.Play("FadeOut"); }

        // Wait until fade out is completely done
        yield return new WaitForSeconds(_transitionTime);

       // Enable end game panel where player scores are displayed
       if (_gamePnl) { _gamePnl.SetActive(false); }
       if (_endGamePnl) { _endGamePnl.SetActive(true); }

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
