using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponController : MonoBehaviour
{
    // Player using the weapon 
    public GameObject player;

    // Weapon type --> True: meele, False: distance
    public bool meleeWeapon = true;

    private void OnTriggerEnter(Collider other)
    {
        // If registered hit is with player parent do nothing
        GameObject enemy = other.gameObject;
        if (enemy == player) { return; }
        
        // Check if the attack has hit a monster
        if (enemy.CompareTag("Monster"))
        {
            if (meleeWeapon)
            {
                // Check if other collider gameobject is interactable
                Interactable interactable = enemy.GetComponent<Interactable>();
                if (!interactable) { return; }

                // Interact with collider gameobject
                interactable.Interact(player);
            }
            else
            {
                enemy.GetComponent<MonsterScript>().StunMonster();
            }
        }

        // Check if the attack has hit another player
        if (enemy.CompareTag("Player"))
        {
            if (meleeWeapon) 
            {
                MyCharacterController cc = enemy.GetComponent<MyCharacterController>();
                if (cc) { cc.MeleeHit(); }
            }
            else
            {
                enemy.GetComponent<MyCharacterController>().DistanceHit();
            }
        }

        if (enemy.CompareTag("Door")) { return; }

        // Destroy weapon if it is not of type melee (projectile)
        if (meleeWeapon == false) PhotonNetwork.Destroy(this.gameObject.GetPhotonView());
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (!meleeWeapon == false) PhotonNetwork.Destroy(this.gameObject.GetPhotonView());
    }
}
