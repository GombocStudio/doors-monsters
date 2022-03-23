using Photon.Pun;
using StarterAssets;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PowerUp : Interactable
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
        character._speed = 6.75f;
        character.isPoweredUp = true;
        character.powerUpTime = 5.0f;
        Debug.Log("speed");

    }
}
