using UnityEngine;
using Photon.Pun;
using UnityEngine.InputSystem;


// Script responsible for handling the movement and actions of the characters in the scene.
public class CharacterController : MonoBehaviour
{
    // Reference to photonview component of the character. Prevents input from local player to influence every character in the scene.
    PhotonView view;

    // Character movement variables
    private float speed = 10.0f;
    private Vector3 movement = new Vector3(0, 0, 0);

    // Start is called before the first frame update
    private void Start()
    {
        // Initialize photon view reference
        view = GetComponent<PhotonView>();
    }

    // Method triggered when any of the characterActions.InputActions specified movement keys is pressed
    public void Move(InputAction.CallbackContext context)
    {
        // Updates movment vector if current view is from the local player
        if (view && view.IsMine)
            movement = new Vector3(context.ReadValue<Vector2>().x, 0, context.ReadValue<Vector2>().y);
    }

    public void FixedUpdate()
    {
        // Updates transform position of character if current view is from the local player
        if (view && view.IsMine)
            transform.position += movement * speed * Time.deltaTime;
    }
}
