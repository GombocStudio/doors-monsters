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

        // Get photon view component
        PhotonView view = player.GetPhotonView();
        if (!view) { return; }
        Destroy(gameObject);

        HideMiniMap();

    }

    public const byte HideMiniMapEventCode = 1;

    private void HideMiniMap()
    {
        //object[] content = new object[] { true }; // Array contains the target position and the IDs of the selected units
        RaiseEventOptions raiseEventOptions = new RaiseEventOptions();// { Receivers = ReceiverGroup.Others }; // You would have to set the Receivers to All in order to receive this event on the local client as well
        PhotonNetwork.RaiseEvent(HideMiniMapEventCode, null, raiseEventOptions, SendOptions.SendReliable);
    }
}
