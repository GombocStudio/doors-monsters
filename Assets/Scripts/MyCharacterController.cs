using UnityEngine;
using Photon.Pun;
using UnityEngine.InputSystem;
using Photon.Realtime;
using ExitGames.Client.Photon;

// Script responsible for handling the movement and actions of the characters in the scene.
public class MyCharacterController : MonoBehaviourPunCallbacks, IPunObservable, IOnEventCallback
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
    public GameObject projectile;
    public float launchVel = 700f;

    public float speedUpTime = 5.0f;
    public bool isSpeedUp = false;

    private GameObject miniMap;
    public float mapOutTime = 5.0f;
    public bool isMapOut = false;

    private GameObject lightsOff;
    public float lightOutTime = 5.0f;
    public bool isLightOut = false;

    public float doublePointsTime = 5.0f;
    public int scoreMul = 1;
    public bool isDoublePoints = false;

    private float reversedControlsTime = 5.0f;
    private bool isReversedControls = false;

    public float frozenTime = 5.0f;
    public bool isFrozen = false;

    public float openDoorsTime = 5.0f;
    public bool isOpenDoors = false;
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

            PowerUpController puCntrlr = FindObjectOfType<PowerUpController>();
            miniMap = puCntrlr.miniMap;
            lightsOff = puCntrlr.lightsOff;
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
            Vector3 direction = Vector3.forward;
            transform.Translate(direction * _speed * Time.deltaTime);
        }
    }

    void Update()
    {
        //Speed
        if (isSpeedUp && speedUpTime > 0)
        {
            // If opened start decreasing controlled time
            speedUpTime -= Time.deltaTime;

            if (speedUpTime <= 0)
            {
                _speed = 3.0f;
                isSpeedUp = false;

            }

        }

        //Map
        if (isMapOut && mapOutTime > 0)
        {
            if (miniMap.activeSelf)
            {
                miniMap.SetActive(false);
            }
            // If opened start decreasing controlled time
            mapOutTime -= Time.deltaTime;

            if (mapOutTime <= 0)
            {
                miniMap.SetActive(true);
                isMapOut = false;

            }

        }

        //Lights
        if (isLightOut && lightOutTime > 0)
        {
            if (!lightsOff.activeSelf)
            {
                lightsOff.SetActive(true);
            }
            // If opened start decreasing controlled time
            lightOutTime -= Time.deltaTime;

            if (lightOutTime <= 0)
            {
                lightsOff.SetActive(false);
                isLightOut = false;

            }

        }

        //Doors
        if (isOpenDoors && openDoorsTime > 0)
        {
            // If opened start decreasing controlled time
            openDoorsTime -= Time.deltaTime;

            if (openDoorsTime <= 0)
            {
                isOpenDoors = false;

            }

        }

        //Score
        if (isDoublePoints && doublePointsTime > 0)
        {
            // If opened start decreasing controlled time
            doublePointsTime -= Time.deltaTime;

            if (doublePointsTime <= 0)
            {
                scoreMul = 1;
                isDoublePoints = false;

            }

        }

        //Reverse
        if (isReversedControls && reversedControlsTime > 0)
        {
            // If opened start decreasing controlled time
            reversedControlsTime -= Time.deltaTime;

            if (reversedControlsTime <= 0)
            {
                isReversedControls = false;

            }

        }

        //Freeze
        if (isFrozen && frozenTime > 0)
        {
            // If opened start decreasing controlled time
            frozenTime -= Time.deltaTime;

            if (frozenTime <= 0)
            {
                isFrozen = false;

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
        if (isFrozen) { _movement = Vector2.zero; }
        if (isReversedControls)
        {
            _movement = -_movement;
        }
        if (_anim) { _anim.SetBool("isWalking", (_movement.x != 0 || _movement.y != 0)); }
    }

    // Method triggered when the characterActions.InputActions specified attack key is pressed
    public void OnAttack(InputAction.CallbackContext context)
    {
        // Play animation attack
        if (_anim && !isFrozen) 
        { 
            _anim.SetBool("isAttacking", context.ReadValueAsButton());

            // Launch projectile
            GameObject bullet = Instantiate(projectile, transform.position + Vector3.up * 0.75f + Vector3.right * 0.35f,
                                              transform.rotation);

            bullet.GetComponent<Rigidbody>().AddRelativeForce(new Vector3(0, 0, launchVel));

        }

        
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


    public void OnEvent(EventData photonEvent)
    {
        byte eventCode = photonEvent.Code;

        switch (eventCode)
        {
            case 1:
                isMapOut = true;
                mapOutTime = 5.0f;
                break;
            case 2:
                isLightOut = true;
                lightOutTime = 5.0f;
                break;
            case 3:
                isReversedControls = true;
                reversedControlsTime = 5.0f;
                break;
            case 4:
                isFrozen = true;
                frozenTime = 5.0f;
                break;
            default:
                break;
        }
    }


    #endregion
}
