using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class Door : Interactable
{
    // Door child mesh reference
    public MeshRenderer doorMesh;
    public MeshRenderer edgeMesh;

    // Door default texture
    public Texture2D defaultTexture;

    // Controlling character id: -1 if door is not controlled by anyone
    public int characterId = -1;

    // Control bool variable to handle open and close animations
    private bool isOpen = true;

    // Time left until door is released from the control of the controlling character
    private float controlledTime;

    // Reference to the door's animator component
    private Animator _anim;

    public void Start()
    {
        // Initialize door's animator component
        _anim = GetComponent<Animator>();
    }

    public void Update()
    {
        // Check if door is opened
        if (!isOpen && controlledTime > 0)
        {
            // If opened start decreasing controlled time
            controlledTime -= Time.deltaTime;

            // When controlled time is over reset door properties
            if (controlledTime <= 0)
                ResetDoorControl();
        }
    }

    public override void Interact(GameObject player)
    {
        /**** SET DOOR ID ****/
        // Check if player that interacted with the door is not null
        if (!player) { return; }

        // Get photon view component
        PhotonView view = player.GetPhotonView();
        if (!view) { return; }

        // Get character component from player
        MyCharacterController cc = player.GetComponent<MyCharacterController>();
        if (!cc) { return; }

        // Set door player ID to the ID of the character that opened it
        if (characterId < 0)
            characterId = view.ViewID;

        if (characterId != view.ViewID && !cc.isOpenDoors) { return; }

        /**** PLAY OPEN ANIMATION ****/
        // Play open door animation
        if (_anim && !isOpen)
        {
            isOpen = true;
            _anim.Play("Open");

            //Play opening door sound
            FindObjectOfType<AudioManager>().Play("DoorOpen");
        }

        /**** SET DOOR TEXTURE ****/
        if (characterId == view.ViewID)
        {
            // Set door material to the material of the character that opened it
            if (!doorMesh || !edgeMesh || !cc.doorTexture) { return; }

            if (doorMesh.material.mainTexture != cc.doorTexture)
            {
                Color pixel_colour = cc.doorTexture.GetPixel(1, 1);
                doorMesh.material.mainTexture = cc.doorTexture;
                edgeMesh.material.color = pixel_colour;

                edgeMesh.material.EnableKeyword("_EMISSION");
                edgeMesh.material.SetColor("_EmissionColor", pixel_colour);
            }
        }
    }

    public override void Deinteract(GameObject player)
    {
        /**** PLAY CLOSE ANIMATION ****/
        // Check if player that interacted with the door is not null
        if (!player) { return; }

        // Get photon view component
        PhotonView view = player.GetPhotonView();
        if (!view) { return; }

        // Get character component from player
        MyCharacterController cc = player.GetComponent<MyCharacterController>();
        if (!cc) { return; }

        // Check if same player that contols the door is the one deinteracting
        if (characterId != view.ViewID && !cc.isOpenDoors) { return; }

        // Play close door animation
        if (_anim && isOpen)
        {
            isOpen = false;
            _anim.Play("Close");

            //Play closing door sound
            FindObjectOfType<AudioManager>().Play("DoorClose");
        }

        /**** SET DOOR CONTROLLED TIME ****/
        if (characterId == view.ViewID)
        {
            // Set door controlled time to the control time of the character that interacted with it
            controlledTime = cc.doorControlTime;
        }
    }

    public void ResetDoorControl()
    {
        /**** RESET DOOR TEXTURE ****/
        // Reset door default texture
        if (doorMesh && edgeMesh && defaultTexture)
        {
            doorMesh.material.mainTexture = defaultTexture;
            edgeMesh.material.color = Color.white;
            edgeMesh.material.DisableKeyword("_EMISSION");
        }

        /**** PLAY OPEN ANIMATION ****/
        // Play open door animation
        if (_anim && !isOpen)
        {
            isOpen = true;
            _anim.Play("Open");

            //Play opening door sound
            FindObjectOfType<AudioManager>().Play("DoorOpen");
        }

        /**** RESET DOOR ID ****/
        // Reset controlling character ID
        characterId = -1;
    }
}
