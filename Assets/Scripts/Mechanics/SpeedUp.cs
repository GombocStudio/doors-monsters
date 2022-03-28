using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpeedUp : Interactable
{

    // Start is called before the first frame update
    public override void Interact(GameObject player)
    {
        // Check if player that interacted with the door is not null
        if (!player) { return; }

        if (PhotonNetwork.IsMasterClient)
        {
            PhotonNetwork.Destroy(gameObject);
        }
        // Get photon view component
        PhotonView view = player.GetPhotonView();
        if (!view || !view.IsMine) { return; }

        MyCharacterController character = player.GetComponent<MyCharacterController>();
        character._speed = 6.75f;
        character.isSpeedUp = true;
        character.speedUpTime = 5.0f;
        Debug.Log("speed");

    }
}
