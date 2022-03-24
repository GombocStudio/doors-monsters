using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeleeWeapon : MonoBehaviour
{
    // Player using the weapon 
    public MyCharacterController playerController;

    private void OnTriggerEnter(Collider other)
    {
        GameObject enemy = other.gameObject;

        // Check if the attack has hit a monster
        if (enemy.CompareTag("Monster"))
        {
            playerController.CaptureMonster();
            Destroy(enemy); // Seguramente haya que usar un metodo de photon
            //enemy.GetComponent<MonsterController>().Destroy();
        }

        // Check if the attack has hit another player
        if (enemy.CompareTag("Player"))
        {
            enemy.GetComponent<MyCharacterController>().MeleeHit();
        }
    }
}
