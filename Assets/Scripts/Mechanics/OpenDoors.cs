using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OpenDoors : Interactable
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

        // Give local player ability to open every door
        MyCharacterController cc = player.GetComponent<MyCharacterController>();
        if (!cc) { return; }

        cc.isOpenDoors = true;
        cc.openDoorsTime = 5.0f;
    }
}
