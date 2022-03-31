using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using ExitGames.Client.Photon;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.OnScreen;

// Script responsible for handling the movement and actions of the characters in the scene.
public class MyCharacterController : MonoBehaviourPunCallbacks, IPunObservable, IOnEventCallback
{
#region Character Variables
    // Character movement related variables
    public float _speed = 3.0f;
    private Vector2 _movement;
    Vector2 lastMov;

    // Character rotation related varibles
    [Tooltip("How fast the character turns to face movement direction")]
    [Range(0.0f, 0.3f)]
    public float RotationSmoothTime = 0.12f;
    private float _rotationVelocity;
    private float _targetRotation;

    // Door control time in seconds
    public float doorControlTime;

    // Character's door texture
    public Texture doorTexture;

    // Abilities
    [Header("Long distance attack")]
    public GameObject projectile;
    public float launchVel = 700f;

    [Header("Powerup variables")]
    public float speedUpTime = 5.0f;
    public bool isSpeedUp = false;

    public float mapOutTime = 5.0f;
    public bool isMapOut = false;

    public float lightOutTime = 5.0f;
    public bool isLightOut = false;

    public float doublePointsTime = 5.0f;
    public int scoreMul = 1;
    public bool isDoublePoints = false;

    private float reversedControlsTime = 5.0f;
    private bool isReversedControls = false;

    private GameObject iceCubeInstance;
    public GameObject iceCubePrefab;
    public float frozenTime = 5.0f;
    public bool isFrozen = false;

    public float openDoorsTime = 5.0f;
    public bool isOpenDoors = false;

#endregion

#region Character Components

    // Reference to in game ui manager
    private GUIManager uiManager;

    // Reference to photonview component of the character
    private PhotonView _view;

    // Reference to the rigid body component of the character;
    private Rigidbody _rb;

    // Reference to the animator component of the character
    private Animator _anim;

    // Reference to stick for mobile controls
    private RectTransform stick;

    #endregion

#region Unity Default Methods
    // Start is called before the first frame update
    private void Start()
    {
        // Initialize photon view component reference
        // _view = GetComponent<PhotonView>();

        // Initialize in game ui manager reference
        uiManager = FindObjectOfType<GUIManager>();

        // Initialize rigid body component reference
        _rb = GetComponent<Rigidbody>();

        // Initialize animator component reference
        _anim = GetComponent<Animator>();

        // Get Joystick (mobile)
        #if UNITY_IOS || UNITY_ANDROID
        stick = FindObjectOfType<OnScreenStick>().gameObject.GetComponent<RectTransform>();
        #endif
    }

    private void Update()
    {
        //Speed
        if (isSpeedUp && speedUpTime > 0)
        {
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
            mapOutTime -= Time.deltaTime;

            if (mapOutTime <= 0)
            {
                if (uiManager) { uiManager.EnableMinimap(true); }
                isMapOut = false;
            }
        }

        //Lights
        if (isLightOut && lightOutTime > 0)
        {
            lightOutTime -= Time.deltaTime;

            if (lightOutTime <= 0)
            {
                if (uiManager) { uiManager.EnableLightsOff(false); }
                isLightOut = false;
            }
        }

        //Doors
        if (isOpenDoors && openDoorsTime > 0)
        {
            openDoorsTime -= Time.deltaTime;

            if (openDoorsTime <= 0)
                isOpenDoors = false;
        }

        //Score
        if (isDoublePoints && doublePointsTime > 0)
        {
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
            reversedControlsTime -= Time.deltaTime;

            if (reversedControlsTime <= 0)
                isReversedControls = false;
        }

        //Freeze
        if (isFrozen && frozenTime > 0)
        {
            frozenTime -= Time.deltaTime;

            if (frozenTime <= 0)
            {
                isFrozen = false;
                if (iceCubePrefab) { iceCubePrefab.transform.localScale = new Vector3(0, 0, 0); };
                if (uiManager) { uiManager.EnableIcePanel(false); };
            }
        }
    }


    public void FixedUpdate()
    {
        // Mobile controls
        #if UNITY_IOS || UNITY_ANDROID
        if (stick.localPosition == Vector3.zero)
        {
            _movement = Vector2.zero;
            if (_anim) { _anim.SetBool("isWalking", false); }
        }
        #endif

        if (_movement != Vector2.zero)
        {
            // Compute current smooth rotation
            _targetRotation = Mathf.Atan2(_movement.x, _movement.y) * Mathf.Rad2Deg;
            float rotation = Mathf.SmoothDampAngle(transform.eulerAngles.y, _targetRotation, ref _rotationVelocity, RotationSmoothTime);

            // Rotate character
            transform.rotation = Quaternion.Euler(0.0f, rotation, 0.0f);
        }

        // Set character's velocity
        if (_rb) { _rb.velocity = new Vector3(_movement.x, 0, _movement.y) * _speed; }
    }
#endregion

#region Input System Methods
    // Method triggered when any of the characterActions.InputActions specified movement keys is pressed
    public void OnMove(InputAction.CallbackContext context)
    {
        // Updates movment vector if current view is from the local player
        _movement = context.ReadValue<Vector2>();

        // Mobile controls
        #if UNITY_IOS || UNITY_ANDROID
        if (_movement.x < -0.3) { _movement.x = -1.0f; }
        if (_movement.x > 0.3) { _movement.x = 1.0f; }
        if (_movement.y < -0.3) { _movement.y = -1.0f; }
        if (_movement.y > 0.3) { _movement.y = 1.0f; }

        _movement.Normalize();

        if (_movement == Vector2.zero) { _movement = lastMov; }

        lastMov = _movement;
        #endif

        // Power up effects
        if (isFrozen) { _movement = Vector2.zero; }
        if (isReversedControls) { _movement = -_movement; }

        // Play walk animation when needed
        if (_anim) { _anim.SetBool("isWalking", (_movement.x != 0 || _movement.y != 0)); }
    }

    // Method triggered when the characterActions.InputActions specified attack key is pressed
    public void OnAttack(InputAction.CallbackContext context)
    {
        // Play animation attack
        if (_anim && !isFrozen)
        {
            _anim.SetBool("isAttacking", context.ReadValueAsButton());

            // Distance attack if needed
            // LaunchProjectile();
        }
    }

    public void LaunchProjectile()
    {
        GameObject bullet = PhotonNetwork.Instantiate(projectile.name, transform.position, transform.rotation);
        if (!bullet) { return; }

        Quaternion rotAdjust = projectile.transform.localRotation;
        Vector3 posAdjust = Vector3.up * 0.65f + Vector3.right * 0.4f + Vector3.forward * 0.25f;

        PhotonView v = bullet.GetPhotonView();
        v.transform.localRotation *= rotAdjust;
        v.transform.position += posAdjust;
        bullet.GetComponent<Rigidbody>().AddRelativeForce(new Vector3(0, 0, launchVel));
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
        if (other.gameObject.tag != "Door") { return; }

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
            // Hide minimap
            case 1:
                isMapOut = true;
                mapOutTime = 5.0f;
                if (uiManager) { uiManager.EnableMinimap(false); }
                break;

            // Turn off lights
            case 2:
                isLightOut = true;
                lightOutTime = 5.0f;
                if (uiManager) { uiManager.EnableLightsOff(true); };
                break;

            // Reverse controls
            case 3:
                isReversedControls = true;
                reversedControlsTime = 5.0f;
                break;

            // Freeze
            case 4:
                isFrozen = true;
                frozenTime = 5.0f;

                //  Spawn ice cube mesh
                if (iceCubePrefab) { iceCubePrefab.transform.localScale = new Vector3(1, 1, 1); };
                if (uiManager) { uiManager.EnableIcePanel(true); };
                break;

            default:
                break;
        }
    }

#endregion
}
