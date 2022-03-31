using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{

    // Start is called before the first frame update
    private void OnCollisionEnter(Collision collision)
    {
        PhotonNetwork.Destroy(gameObject);
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}
