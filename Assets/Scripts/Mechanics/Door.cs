using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Door : Interactable
{
    // Door child mesh reference
    public MeshRenderer doorMesh;

    // Door default material
    public Material defaultMaterial;

    // Controlling character id: -1 if door is not controlled by anyone
    public int characterId = -1;

    // Control bool variable to handle open and close animations
    private bool isOpen = false;

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
        // Check if door is being controlled by any character
        if (characterId == -1) { return; }

        // If being controlled start decreasing controlled time
        controlledTime -= Time.deltaTime;

        // When controlled time is over reset door properties
        if (controlledTime <= 0) 
            ResetDoorControl();
    }

    public override void Interact(MyCharacterController cc)
    {
        // Set door player ID to the ID of the character that opened it
        if (characterId == -1)
            characterId = cc.id;

        // Play open/close door animation
        if (_anim && cc.id == characterId)
        {
            isOpen = !isOpen;

            if (isOpen) { _anim.Play("Open"); }
            else { _anim.Play("Close"); }
        }

        // Set door material to the material of the character that opened it
        if (doorMesh && cc.material)
            doorMesh.material = cc.material;

        // Set door controlled time to the control time of the character that opened it
        controlledTime = cc.doorControlTime;
    }

    public void ResetDoorControl()
    {
        // Reset door default material
        if (doorMesh && defaultMaterial)
            doorMesh.material = defaultMaterial;

        // Reset controlling character ID
        characterId = -1;
    }
}
