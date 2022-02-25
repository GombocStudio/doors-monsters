using UnityEngine;
using Photon.Pun;
using UnityEngine.InputSystem;


// Script responsible for handling the movement and actions of the characters in the scene.
public class CharacterController : MonoBehaviour
{
    // Reference to photonview component of the character. Prevents input from local player to influence every character in the scene.
    PhotonView _view;

    // Character movement variables
    private float _speed = 6.0f;
    private Vector3 _movement = new Vector3(0, 0, 0);

    private Rigidbody _rigidbody;

    // Start is called before the first frame update
    private void Start()
    {
        // Initialize photon view reference
        _view = GetComponent<PhotonView>();

        // Initialize rigid body component reference
        _rigidbody = GetComponent<Rigidbody>();
    }

    // Method triggered when any of the characterActions.InputActions specified movement keys is pressed
    public void Move(InputAction.CallbackContext context)
    {
        // Updates movment vector if current view is from the local player
        if (_view && _view.IsMine)
            _movement = new Vector3(context.ReadValue<Vector2>().x, 0, context.ReadValue<Vector2>().y);
    }

    public void FixedUpdate()
    {
        // Check if character has a rigidbody component assign to it
        if (!_rigidbody) { return; }

        // Updates transform position of character if current view is from the local player
        if (_view && _view.IsMine)
            _rigidbody.velocity = _movement * _speed;
    }
}
