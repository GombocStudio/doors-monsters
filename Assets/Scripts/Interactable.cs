using UnityEngine;
using Photon.Pun;

/*All objects the player can interact with*/
public class Interactable : MonoBehaviour
{
    public virtual void Interact(GameObject player) {}

    public virtual void Deinteract(GameObject player) {}
}
