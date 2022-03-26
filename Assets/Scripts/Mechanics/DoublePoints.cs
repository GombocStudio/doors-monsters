using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoublePoints : Interactable
{

    // Start is called before the first frame update
    public override void Interact(GameObject player)
    {
        // Check if player that interacted with the door is not null
        if (!player) { return; }

        // Get photon view component
        PhotonView view = player.GetPhotonView();
        if (!view) { return; }
        Destroy(gameObject);
        MyCharacterController character = player.GetComponent<MyCharacterController>();
        character.isDoublePoints = true;
        character.doublePointsTime = 5.0f;
        character.scoreMul = 2;
        Debug.Log("doble");

    }
}
