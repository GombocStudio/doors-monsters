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
        GameObject enemy = other.gameObject;

        if (enemy == player) { return; }
        
        // Check if the attack has hit a monster
        if (enemy.CompareTag("Monster"))
        {
            if (meleeWeapon)
            {
                enemy.GetComponent<Interactable>().Interact(player);
            }
            else
            {
                //enemy.GetComponent<MonsterScript>().StunMonster();
            }
        }

        // Check if the attack has hit another player
        if (enemy.CompareTag("Player"))
        {
            if (meleeWeapon) enemy.GetComponent<MyCharacterController>().MeleeHit();
            else
            {
                //enemy.GetComponent<MyCharacterController>().DistanceHit();
            }
        }

        // Destroy weapon if it is not of type melee (projectile)
        if (meleeWeapon == false) PhotonNetwork.Destroy(this.gameObject.GetPhotonView());
    }

    private void OnCollisionEnter(Collision collision)
    {

        if (!meleeWeapon == false) PhotonNetwork.Destroy(this.gameObject.GetPhotonView());
    }
}
