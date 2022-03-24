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

    // Abilities
    public float powerUpTime = 5.0f;
    public bool isPoweredUp = false;

    // Allow using the player without photon for testing
    public bool testLocal = false;

    // Melee weapon collider
    public Collider weaponCollider;

    // Time the player is stunned after melee attack (in secons)
    public float stunTime = 2.0f;

    // Time the player is invencible when stunned (in seconds)
    public float invencibleTime = 3.0f;

    // Is player stunned
    private bool _stunned = false;

    // Is player invencible
    private bool _invencible = false;

    // When the player will stop being stunned
    private float _timeStunned = 0.0f;

    // When the player will stop being invencible
    private float _timeInvencible = 0.0f;
    #endregion

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
        if (_view && _view.IsMine && !testLocal)
        {
            MinimapCameraController minimapCamera = FindObjectOfType<MinimapCameraController>();
            minimapCamera.playerTransform = this.transform;
        }
        // Disable player input if view is not mine
        this.GetComponent<PlayerInput>().enabled = _view.IsMine || testLocal;

        // Initialize animtor component reference
        _anim = GetComponent<Animator>();

        // Disable weapon collider
        weaponCollider.enabled = false;
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

        if (_stunned)
        {
            _stunned = Time.time <= _timeStunned;
        }

        if (_invencible)
        {
            _invencible = Time.time <= _timeInvencible;
        }
    }

    #endregion

    #region Input System Methods
    // Method triggered when any of the characterActions.InputActions specified movement keys is pressed
    public void OnMove(InputAction.CallbackContext context)
    {
        // If player is stunned don't move
        if (_stunned) { return; }

        // Updates movment vector if current view is from the local player
        _movement = context.ReadValue<Vector2>();
        if (_anim) { _anim.SetBool("isWalking", (_movement.x != 0 || _movement.y != 0)); }
    }

    // Method triggered when the characterActions.InputActions specified attack key is pressed
    public void OnAttack(InputAction.CallbackContext context)
    {
        // If player is stunned don't attack
        if (_stunned) { return; }

        // Play animation attack
        if (_anim) { _anim.SetBool("isAttacking", context.ReadValueAsButton()); }
    }
    #endregion

    #region Trigger and Collider Events
    private void OnTriggerEnter(Collider other)
    {
        // Check if other collider gameobject is interactable
        Interactable interactable = other.gameObject.GetComponent<Interactable>();
        if (interactable)
        {
            // Interact with collider gameobject
            interactable.Interact(this.gameObject);
        }
        
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

    #region Melee interaction Methods

    public void EnableMeleeCollider()
    {
        weaponCollider.enabled = true;
    }

    public void DisableMeleeCollider()
    {
        weaponCollider.enabled = false;
    }

    // When the player is hit by a melee attack, this function is called
    public void MeleeHit()
    {
        if (!_invencible)
        {
            _stunned = true;
            _invencible = true;
            _timeStunned = Time.time + stunTime;
            _timeInvencible = Time.time + invencibleTime;

            Debug.Log("MeleeHit(): Reducir score y soltar monstruos");
        }
    }

    // When the player capture a monster, thsi function is called
    public void CaptureMonster()
    {
        Debug.Log("CaptureMonster(): Modificar score");
    }

    #endregion
}
