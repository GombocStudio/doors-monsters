using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

public class ScoreManager : MonoBehaviour
{
    private Dictionary<string, int> _scoreDictionary = new Dictionary<string, int>();
    private int myScore = 0;

    private GameManager gameManager;
    private GUIManager uiManager;

    private void Start()
    {
        // Initialise game manager reference
        gameManager = FindObjectOfType<GameManager>();

        // Initialise ui manager reference
        uiManager = FindObjectOfType<GUIManager>();
    }

    public void AddCharacter(string characterName)
    {
        if (!_scoreDictionary.ContainsKey(characterName))
        {
            // Add character to the score dictionary
            _scoreDictionary.Add(characterName, 0);

            // Update score list UI
            if (uiManager) { uiManager.UpdateScoreListUI(_scoreDictionary); }
        }
    }

    public void RemoveCharacter(string characterName)
    {
        if (_scoreDictionary.ContainsKey(characterName))
        {
            // Remove character from score dictionary
            _scoreDictionary.Remove(characterName);

            // Update socore list UI
            if (uiManager) { uiManager.UpdateScoreListUI(_scoreDictionary); }
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

    public void UpdateScoreList(string characterName, int newScore)
    {
        // Update character score in score dictionary
        if (_scoreDictionary.ContainsKey(characterName))
            _scoreDictionary[characterName] = newScore;

        // Update UI score list
        if (uiManager) { uiManager.UpdateScoreListUI(_scoreDictionary); }
    }
}

