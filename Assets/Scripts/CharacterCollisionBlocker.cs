using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterCollisionBlocker : MonoBehaviour
{
    public CapsuleCollider characterCollider;
    public CapsuleCollider characterBlockerCollider;

    // Start is called before the first frame update
    void Start()
    {
        Physics.IgnoreCollision(characterCollider, characterBlockerCollider);
    }
}
