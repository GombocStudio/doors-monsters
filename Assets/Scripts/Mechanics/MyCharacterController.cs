using UnityEngine;
using Photon.Pun;
using UnityEngine.InputSystem;


// Script responsible for handling the movement and actions of the characters in the scene.
public class MyCharacterController : MonoBehaviour
{
    // Character movement related variables
    public float _speed = 1.0f;
    private Vector2 _movement;

    private Vector3 _networkPosition;

    // Character rotation related varibles
    [Tooltip("How fast the character turns to face movement direction")]
    [Range(0.0f, 0.3f)]
    public float RotationSmoothTime = 0.12f;
    private float _rotationVelocity;
    private float _targetRotation;
    private float _rotSpeed = 5f;

    private Quaternion _networkRotation;

    // Character's current game score
    public int score;

    // Character's material
    public Material material;

    // Door control time in seconds
    public float doorControlTime;

    // Reference to character's rigidbody
    private Rigidbody _rigidbody;

    // Reference to the animator component of the character
    private Animator _anim;

    // Reference to photonview component of the character. Prevents input from local player to influence every character in the scene.
    private PhotonView _view;

    // Start is called before the first frame update
    private void Start()
    {
        // Initialize photon view reference
        _view = GetComponent<PhotonView>();

        // Initialize rigid body component reference
        _rigidbody = GetComponent<Rigidbody>();

        // Initialize animtor component reference
        _anim = GetComponent<Animator>();
    }

    // Method triggered when any of the characterActions.InputActions specified movement keys is pressed
    public void OnMove(InputAction.CallbackContext context)
    {
        // Updates movment vector if current view is from the local player
        if (_view && _view.IsMine)
            _movement = context.ReadValue<Vector2>();
    }

    //
    public void OnAttack(InputAction.CallbackContext context)
    {
        _anim.SetBool("isAttacking", context.ReadValueAsButton());
    }

    private void Update()
    {
        _anim.SetBool("isWalking", (_movement.x != 0 || _movement.y != 0));
    }

    public void FixedUpdate()
    {
        if (_view && _view.IsMine)
        {
            float speed = 0.0f;
            if (_movement != Vector2.zero)
            {
                _targetRotation = Mathf.Atan2(_movement.x, _movement.y) * Mathf.Rad2Deg;
                float rotation = Mathf.SmoothDampAngle(transform.eulerAngles.y, _targetRotation, ref _rotationVelocity, RotationSmoothTime);

                transform.rotation = Quaternion.Euler(0.0f, rotation, 0.0f);
                speed = _speed;
            }

            transform.Translate(Vector3.forward * (speed * Time.deltaTime));
        }
        else
        {
            transform.position =
                Vector3.MoveTowards(transform.position, _networkPosition, _speed);
            transform.rotation =
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
