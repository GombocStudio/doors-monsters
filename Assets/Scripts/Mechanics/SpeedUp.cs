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

        // Destroy power up instance
        if (PhotonNetwork.IsMasterClient)
            PhotonNetwork.Destroy(gameObject);

        // Get photon view component
        PhotonView view = player.GetPhotonView();
        if (!view || !view.IsMine) { return; }

        // Increase local player speed
        MyCharacterController cc = player.GetComponent<MyCharacterController>();
        if (!cc) { return; }

        cc._speed = 6.75f;
        cc.isSpeedUp = true;
        cc.speedUpTime = 5.0f;
    }
}
