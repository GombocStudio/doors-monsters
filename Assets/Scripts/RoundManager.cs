using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class RoundManager : MonoBehaviour
{
    private GUIManager uiManager;

    public int _currentRound = 0;
    public int _numRounds = 0;

    private bool _startTimer = false;
    private float _currentRoundTime = 0;
    private float _roundDuration = 5;

    // Start is called before the first frame update
    void Start()
    {
        // Initialise ui manager reference
        uiManager = FindObjectOfType<GUIManager>();
    }

    // Update is called once per frame
    void Update()
    {
        // If round has started activate time counter
        if (_startTimer)
        {
            // Increase current round time by deltaTime
            _currentRoundTime += Time.deltaTime;

            // Display remaining round time in the UI
            if (uiManager) { uiManager.SetTimeUI(_roundDuration - _currentRoundTime); }

            // End round when current round time is equal to round duration
            if (_currentRoundTime >= _roundDuration)
            {
                // Stop timer
                _startTimer = false;

                if (uiManager)
                {
                    // Set timer UI to 00:00
                    uiManager.SetTimeUI(0);

                    // End game if we just finished the final round
                    if (_currentRound >= _numRounds)
                        uiManager.EndGameUI();

                    // If not start a new round
                    else
                        uiManager.EndRoundUI();
                }
            }
        }
    }

    public void StartTimer()
    {
        _startTimer = true;
        _currentRoundTime = 0;
    }

    public void StartRound()
    {
        // Increase current round number
        _currentRound++;

        if (uiManager) 
        {
            // Set current time ui to round duration
            uiManager.SetTimeUI(_roundDuration);

            // Set round UI text
            uiManager.SetRoundUI(_numRounds, _currentRound);

            // Start round UI counter animation
            uiManager.StartRoundUI();
        }
    }

    public void SetNumberOfRounds(int numRounds)
    {
        _numRounds = numRounds;
    }
}
