using UnityEngine;
using Photon.Pun;
using UnityEngine.InputSystem;


// Script responsible for handling the movement and actions of the characters in the scene.
public class MyCharacterController : MonoBehaviour
{
    // Character movement related variables
    private float _speed = 6.0f;
    private float _rotSpeed = 5f;
    private Vector3 _movement = new Vector3(0, 0, 0);

    private Vector3 _networkPosition;
    private Quaternion _networkRotation;

    // Character's current game score
    public int score;

    // Character's material
    public Material material;

    // Door control time in seconds
    public float doorControlTime;

    // Reference to character's rigidbody
    private Rigidbody _rigidbody;

    // Reference to photonview component of the character. Prevents input from local player to influence every character in the scene.
    private PhotonView _view;

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
        /* if (!_rigidbody) { return; }

        // Updates transform position of character if current view is from the local player
        if (_view && _view.IsMine)
            _rigidbody.velocity = _movement * _speed; */

        Transform playerTransform = transform;
        if (_view && _view.IsMine)
        {
            playerTransform.Rotate(Vector3.up * _movement.x * _rotSpeed);
            playerTransform.Translate(Vector3.forward * _movement.y * _speed);
        }
        else
        {
            playerTransform.position =
                Vector3.MoveTowards(transform.position, _networkPosition, _speed);
            playerTransform.rotation =
                Quaternion.RotateTowards(transform.rotation, _networkRotation, _rotSpeed);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // Check if other collider gameobject is interactable
        Interactable interactable = other.gameObject.GetComponent<Interactable>();
        if (!interactable) { return; }

        Debug.Log("Trigger enter!");

        // Interact with collider gameobject
        interactable.Interact(this.gameObject);
    }
    private void OnTriggerStay(Collider other)
    {
        // Check if other collider gameobject is interactable
        Interactable interactable = other.gameObject.GetComponent<Interactable>();
        if (!interactable) { return; }

        Debug.Log("Trigger stay!");

        // Interact with collider gameobject
        interactable.Interact(this.gameObject);
    }

    private void OnTriggerExit(Collider other)
    {
        // Check if other collider gameobject is interactable
        Interactable interactable = other.gameObject.GetComponent<Interactable>();
        if (!interactable) { return; }

        Debug.Log("Trigger exit!");

        // Interact with collider gameobject
        interactable.Deinteract(this.gameObject);
    }

    #region PhotonMethods
    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(transform.position);
            stream.SendNext(transform.rotation);
        }
        else
        {
            _networkPosition = (Vector3)stream.ReceiveNext();
            _networkRotation = (Quaternion)stream.ReceiveNext();
        }
    }
    #endregion
}
