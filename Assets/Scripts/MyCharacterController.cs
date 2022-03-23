using UnityEngine;
using Photon.Pun;
using UnityEngine.InputSystem;

// Script responsible for handling the movement and actions of the characters in the scene.
public class MyCharacterController : MonoBehaviourPunCallbacks, IPunObservable
{
    #region Character Variables
    // Character movement related variables
    public float _speed = 3.0f;
    private Vector2 _movement;

    // Character rotation related varibles
    [Tooltip("How fast the character turns to face movement direction")]
    [Range(0.0f, 0.3f)]
    public float RotationSmoothTime = 0.12f;
    private float _rotationVelocity;
    private float _targetRotation;

    // Lag compensation movement and rotation variables

    // Character's current game score
    public int score;

    // Character's material
    public Material material;

    // Door control time in seconds
    public float doorControlTime;
    #endregion

    // Abilities
    public float powerUpTime = 5.0f;
    public bool isPoweredUp = false;
    #region Character Components
    // Reference to the animator component of the character
    private Animator _anim;

    // Reference to photonview component of the character. Prevents input from local player to influence every character in the scene.
    private PhotonView _view;
    #endregion

    #region Unity Default Methods
    // Start is called before the first frame update
    private void Start()
    {
        // Initialize photon view reference
        _view = GetComponent<PhotonView>();

        // Initialize minimap camera to follow the player
        if (_view && _view.IsMine)
        {
            MinimapCameraController minimapCamera = FindObjectOfType<MinimapCameraController>();
            minimapCamera.playerTransform = this.transform;
        }
        // Disable player input if view is not mine
        this.GetComponent<PlayerInput>().enabled = _view.IsMine;

        // Initialize animtor component reference
        _anim = GetComponent<Animator>();
    }

    public void FixedUpdate()
    {
        if (_movement != Vector2.zero)
        {
            // Compute current smooth rotation
            _targetRotation = Mathf.Atan2(_movement.x, _movement.y) * Mathf.Rad2Deg;
            float rotation = Mathf.SmoothDampAngle(transform.eulerAngles.y, _targetRotation, ref _rotationVelocity, RotationSmoothTime);

            // Rotate character
            transform.rotation = Quaternion.Euler(0.0f, rotation, 0.0f);

            // Translate character
            transform.Translate(Vector3.forward * _speed * Time.deltaTime);
        }
    }

    void Update()
    {
        if (isPoweredUp && powerUpTime > 0)
        {
            // If opened start decreasing controlled time
            powerUpTime -= Time.deltaTime;

            // When controlled time is over reset door properties
            if (powerUpTime <= 0)
            {
                _speed = 3.0f;
                isPoweredUp = false;
                Debug.Log("normalSpeed");

            }

        }
    }

    #endregion

    #region Input System Methods
    // Method triggered when any of the characterActions.InputActions specified movement keys is pressed
    public void OnMove(InputAction.CallbackContext context)
    {
        // Updates movment vector if current view is from the local player
        _movement = context.ReadValue<Vector2>();
        if (_anim) { _anim.SetBool("isWalking", (_movement.x != 0 || _movement.y != 0)); }
    }

    // Method triggered when the characterActions.InputActions specified attack key is pressed
    public void OnAttack(InputAction.CallbackContext context)
    {
        // Play animation attack
        if (_anim) { _anim.SetBool("isAttacking", context.ReadValueAsButton()); }
    }
    #endregion

    #region Trigger and Collider Events
    private void OnTriggerEnter(Collider other)
    {
        // Check if other collider gameobject is interactable
        Interactable interactable = other.gameObject.GetComponent<Interactable>();
        if (!interactable) { return; }

        // Interact with collider gameobject
        interactable.Interact(this.gameObject);
    }
    private void OnTriggerStay(Collider other)
    {
        // Check if other collider gameobject is interactable
        Interactable interactable = other.gameObject.GetComponent<Interactable>();
        if (!interactable) { return; }

        // Interact with collider gameobject
        interactable.Interact(this.gameObject);
    }

    private void OnTriggerExit(Collider other)
    {
        // Check if other collider gameobject is interactable
        Interactable interactable = other.gameObject.GetComponent<Interactable>();
        if (!interactable) { return; }

        // Interact with collider gameobject
        interactable.Deinteract(this.gameObject);
    }
    #endregion

    #region Photon Methods
    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        /* if (stream.IsWriting)
        {
            stream.SendNext(transform.position);
            stream.SendNext(transform.rotation);
        }
        else
        {
            transform.position = (Vector3)stream.ReceiveNext();
            transform.rotation = (Quaternion)stream.ReceiveNext();
        } */
    }
    #endregion
}
