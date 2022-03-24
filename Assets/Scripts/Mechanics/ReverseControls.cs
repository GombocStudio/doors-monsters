using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using StarterAssets;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReverseControls : Interactable
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
        Reverse();
    }

    public const byte ReverseEventCode = 3;

    private void Reverse()
    {
        RaiseEventOptions raiseEventOptions = new RaiseEventOptions();// { Receivers = ReceiverGroup.Others }; // You would have to set the Receivers to All in order to receive this event on the local client as well
        PhotonNetwork.RaiseEvent(ReverseEventCode, null, raiseEventOptions, SendOptions.SendReliable);
    }
}
