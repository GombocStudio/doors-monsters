using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Egg : Interactable
{
    ScoreManager scoreManager;

    private void Start()
    {
        scoreManager = FindObjectOfType<ScoreManager>();
    }

    public override void Interact(GameObject player)
    {
        // Increase score of the player that interacted with the egg
        if (scoreManager) { scoreManager.UpdatePlayerScore(player, 1); }

        //Make object disappear
        // PhotonNetwork.Destroy(this.gameObject);
        Destroy(this.gameObject);
    }
}
