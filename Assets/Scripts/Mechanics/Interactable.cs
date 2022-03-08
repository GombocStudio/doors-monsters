using UnityEngine;

/*All objects the player can interact with*/
public class Interactable : MonoBehaviour
{
    public virtual void Interact(MyCharacterController cc) {}
}
