using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Freeze : Interactable
{
    public float effectDuration = 5.0f;

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

        // Freeze other characters
        FreezeControls();
    }

    private void FreezeControls()
    {
        // You would have to set the Receivers to All in order to receive this event on the local client as well
        RaiseEventOptions raiseEventOptions = new RaiseEventOptions();
        object[] content = new object[] { "Freeze", effectDuration };
        PhotonNetwork.RaiseEvent(0, content, raiseEventOptions, SendOptions.SendReliable);
    }
}
