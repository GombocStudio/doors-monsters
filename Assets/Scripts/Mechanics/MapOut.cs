using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapOut : Interactable
{
    private GameObject miniMap;
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

        // Hide other characters mini map
        HideMiniMap();
    }

    public const byte HideMiniMapEventCode = 1;

    private void HideMiniMap()
    {
        // You would have to set the Receivers to All in order to receive this event on the local client as well
        RaiseEventOptions raiseEventOptions = new RaiseEventOptions();
        PhotonNetwork.RaiseEvent(HideMiniMapEventCode, null, raiseEventOptions, SendOptions.SendReliable);
    }
}
