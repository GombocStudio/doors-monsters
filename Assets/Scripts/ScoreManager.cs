using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Photon.Pun;
using UnityEngine;

public class ScoreManager : MonoBehaviour
{
    private Dictionary<string, KeyValuePair<string, int>> _scoreDictionary = new Dictionary<string, KeyValuePair<string, int>>();
    List<KeyValuePair<string, KeyValuePair<string, int>>> _scoreList;

    private GameManager gameManager;
    private GUIManager uiManager;

    private int myScore = 0;

    private void Start()
    {
        // Initialise game manager reference
        gameManager = FindObjectOfType<GameManager>();

        // Initialise ui manager reference
        uiManager = FindObjectOfType<GUIManager>();
    }

    public void AddCharacter(string characterName, string playerNickname)
    {
        if (!_scoreDictionary.ContainsKey(characterName))
        {
            // Add character to the score dictionary
            _scoreDictionary.Add(characterName, new KeyValuePair<string, int>(playerNickname, 0));

            // Move score dictionary into the score list and sort it
            _scoreList = new List<KeyValuePair<string, KeyValuePair<string, int>>>(_scoreDictionary);
            _scoreList = _scoreList.OrderByDescending(x => x.Value.Value).ToList();

            // Update score list UI
            if (uiManager) { uiManager.UpdateScoreListUI(_scoreList); }
        }
    }

    public void RemoveCharacter(string characterName)
    {
        if (_scoreDictionary.ContainsKey(characterName))
        {
            // Remove character from score dictionary
            _scoreDictionary.Remove(characterName);

            // Move score dictionary into the score list and sort it
            _scoreList = new List<KeyValuePair<string, KeyValuePair<string, int>>>(_scoreDictionary);
            _scoreList = _scoreList.OrderByDescending(x => x.Value.Value).ToList();

            // Update socore list UI
            if (uiManager) { uiManager.UpdateScoreListUI(_scoreList); }
        }
    }

    public void UpdatePlayerScore(GameObject player, int scoreToAdd)
    {
        // Check if player that interacted with the egg is not null
        if (!player) { return; }

        // Get photon view component and check if view is mine
        PhotonView view = player.GetPhotonView();
        if (!view || !view.IsMine) { return; }

        // Update local player score
        myScore += scoreToAdd;

        // Update player socre in other player's machines
        if (gameManager) { gameManager.UpdatePlayerOnlineScore(myScore); }
    }

    public void UpdateScoreList(string characterName, string playerNickname, int newScore)
    {
        // Update character score in score dictionary
        if (_scoreDictionary.ContainsKey(characterName))
            _scoreDictionary[characterName] = new KeyValuePair<string, int>(playerNickname, newScore);

        // Move score dictionary into the score list and sort it
        _scoreList = new List<KeyValuePair<string, KeyValuePair<string, int>>>(_scoreDictionary);
        _scoreList = _scoreList.OrderByDescending(x => x.Value.Value).ToList();

        // Update UI score list
        if (uiManager) { uiManager.UpdateScoreListUI(_scoreList); }
    }

    public int GetMyScore()
    {
        return myScore;
    }

    public List<KeyValuePair<string, KeyValuePair<string, int>>> GetScoreList()
    {
        return _scoreList;
    }
}

