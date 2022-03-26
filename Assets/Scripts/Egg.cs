using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
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
        //Make object disappear
        // if (PhotonNetwork.IsMasterClient) { PhotonNetwork.Destroy(this.gameObject); }
        Destroy(this.gameObject);
            
        // Check if player that interacted with the egg is not null
        if (!player) { return; }

        // Get photon view component and check if view is mine
        PhotonView view = player.GetPhotonView();
        if (!view || !view.IsMine) { return; }

        // Increase score of the player that interacted with the egg
        if (scoreManager) { scoreManager.UpdatePlayerScore(1); }
    }
}
