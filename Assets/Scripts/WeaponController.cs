using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponController : MonoBehaviour
{
    // Player using the weapon 
    public MyCharacterController playerController;

    // Weapon type --> True: meele, False: distance
    public bool meeleWeapon = true;

    private void OnTriggerEnter(Collider other)
    {
        GameObject enemy = other.gameObject;

        // Check if the attack has hit a monster
        if (enemy.CompareTag("Monster"))
        {
            if (meeleWeapon)
            {
                playerController.CaptureMonster();
                Destroy(enemy); // Seguramente haya que usar un metodo de photon
            }
            else
            {
                Debug.Log("Stunt monster");
            }
        }

        // Check if the attack has hit another player
        if (enemy.CompareTag("Player"))
        {
            if (meeleWeapon) enemy.GetComponent<MyCharacterController>().MeleeHit();
            else
            {
                Destroy(this.gameObject);
                enemy.GetComponent<MyCharacterController>().DistanceHit();
            }
        }
    }
}
