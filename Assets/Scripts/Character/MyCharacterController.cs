using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using ExitGames.Client.Photon;
using UnityEngine.InputSystem;
using System;
using UnityEngine.InputSystem.OnScreen;
using System.Collections;

// Script responsible for handling the movement and actions of the characters in the scene.
public class MyCharacterController : MonoBehaviourPunCallbacks, IOnEventCallback
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
    public Texture2D doorTexture;

    [Header("Melee attack")]

    // Melee weapon collider
    public Collider weaponCollider;

    // Abilities
    [Header("Long distance attack")]
    public GameObject projectile;

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

    // Layer mask for distance attack boxcast
    private int layerMask = 0;

    public float projectileTimeToLive = 5.0f;
    public Transform projectileThrower;
    public float projectileSpeed = 1000.0f;
    private GameObject _projectileInstance;

    [Header("Powerup variables")]
    private float speedUpTime = 5.0f;
    private bool isSpeedUp = false;

    private float doublePointsTime = 5.0f;
    public int scoreMul = 1;
    private bool isDoublePoints = false;

    private float openDoorsTime = 5.0f;
    public bool isOpenDoors = false;

    private float mapOutTime = 5.0f;
    private bool isMapOut = false;

    private float lightOutTime = 5.0f;
    private bool isLightOut = false;

    private float reversedControlsTime = 5.0f;
    private bool isReversedControls = false;

    public GameObject iceCubePrefab;
    private float frozenTime = 5.0f;
    private bool isFrozen = false;

    // Character sounds
    [Header("Character Sounds")]
    public Sound[] sounds;

    #endregion

    #region Character Components

    // Reference to in game score manager
    private ScoreManager scoreManager;

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

    // Reference to camera shake component
    private CinemachineShake cinemachineShake;

    #endregion

    #region Unity Default Methods
    private void Awake()
    {
        // Init character sound sources
        foreach (Sound s in sounds)
        {
            s.source = gameObject.AddComponent<AudioSource>();
            s.source.clip = s.clip;

            s.source.volume = s.volume;
            s.source.pitch = s.pitch;
            s.source.loop = s.loop;

            s.source.spatialBlend = s.spatialBlend;
            s.source.maxDistance = s.maxDistance;
            s.source.minDistance = s.minDistance;
        }
    }

    // Start is called before the first frame update
    private void Start()
    {
        // Initialize in game score manager reference
        scoreManager = FindObjectOfType<ScoreManager>();

        // Initialize photon view component reference
        // _view = GetComponent<PhotonView>();

        // Initialize in game ui manager reference
        uiManager = FindObjectOfType<GUIManager>();

        // Initialize rigid body component reference
        _rb = GetComponent<Rigidbody>();

        // Initialize animator component reference
        _anim = GetComponent<Animator>();

        // Initialize cinemachine shake component reference
        cinemachineShake = FindObjectOfType<CinemachineShake>();

        // Disable weapon collider
        weaponCollider.enabled = false;

        // Get Joystick (mobile)
#if UNITY_IOS || UNITY_ANDROID
        stick = FindObjectOfType<OnScreenStick>().gameObject.GetComponent<RectTransform>();
#endif

        // Set layer mask for player and monster layers
        layerMask = (1 << 6) | (1 << 9);
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

        // Check stunned
        if (_stunned && Time.time > _timeStunned)
        {
            _stunned = false;
        }

        // Check invencible
        if (_invencible && Time.time > _timeInvencible)
        {
            _invencible = false;
            if (_anim) { _anim.SetBool("isInvincible", false); }
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
        if (_movement.x < -0.5) { _movement.x = -1.0f; }
        if (_movement.x > 0.5) { _movement.x = 1.0f; }
        if (_movement.y < -0.5) { _movement.y = -1.0f; }
        if (_movement.y > 0.5) { _movement.y = 1.0f; }

        _movement.Normalize();

        if (_movement == Vector2.zero) { _movement = lastMov; }

        lastMov = _movement;
        #endif

        // Power up effects
        if (isFrozen || _stunned) { _movement = Vector2.zero; }
        if (isReversedControls) { _movement = -_movement; }

        // Play walk animation when needed
        if (_anim) { _anim.SetBool("isWalking", (_movement.x != 0 || _movement.y != 0)); }
    }

    // Method triggered when the characterActions.InputActions specified attack key is pressed
    public void OnAttack(InputAction.CallbackContext context)
    {
        // Play animation attack
        if (_anim && !isFrozen && !_stunned)
        {
            _anim.SetBool("isAttacking", context.ReadValueAsButton());
        }
    }

    // Method triggered when the characterActions.InputActions specified shoot key is pressed
    public void OnShoot(InputAction.CallbackContext context)
    {
        // Play animation shoot
        if (_anim && !isFrozen && !_stunned)
        {
            _anim.SetBool("isShooting", context.ReadValueAsButton());

            // Play attack sound (short range)
            Sound s = Array.Find(sounds, sound => sound.name == "LongRangeAttack");
            if (s != null) { s.source.Play(); }
        }
    }

    #endregion

    #region Trigger and Collider Events
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Monster")) { return; }

        // Check if other collider gameobject is interactable
        Interactable interactable = other.gameObject.GetComponent<Interactable>();
        if (!interactable) { return; }

        // Play pick up powerup sound
        if (other.gameObject.CompareTag("PowerUp"))
        {
            Sound s = Array.Find(sounds, sound => sound.name == "CatchPowerup");
            if (s != null && s.source != null) { s.source.Play(); }
            else { Debug.Log("Character attack sound not found!"); }
        }

        // Interact with collider gameobject
        interactable.Interact(this.gameObject);
    }

    private void OnTriggerStay(Collider other)
    {
        // Just for doors
        if (!other.gameObject.CompareTag("Door")) { return; }

        // Check if other collider gameobject is interactable
        Interactable interactable = other.gameObject.GetComponent<Interactable>();
        if (!interactable) { return; }

        // Interact with collider gameobject
        interactable.Interact(this.gameObject);
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("Monster")) { return; }

        // Check if other collider gameobject is interactable
        Interactable interactable = other.gameObject.GetComponent<Interactable>();
        if (!interactable) { return; }

        // Interact with collider gameobject
        interactable.Deinteract(this.gameObject);
    }
    #endregion

    #region Photon Methods
    public void OnEvent(EventData photonEvent)
    {
        // Get event code and check if is an event sent by a power up
        byte eventCode = photonEvent.Code;
        if (eventCode != 0 || photonEvent.CustomData == null) { return; }

        // Get event data
        object[] data = (object[])photonEvent.CustomData;

        string powerupName = (string)data[0];
        float duration = (float)data[1];

        ActivatePowerupEffect(powerupName, duration);
    }

    #endregion

    #region Powerup Methods
    public void ActivatePowerupEffect(string powerupName, float duration)
    {
        if (!uiManager) { return; }
        int indicatorIndex = -1;

        switch (powerupName)
        {
            case "OpenDoors":
                isOpenDoors = true;
                openDoorsTime = duration;
                indicatorIndex = 0;
                break;

            case "SpeedUp":
                isSpeedUp = true;
                speedUpTime = duration;
                _speed = 5;
                indicatorIndex = 1;
                break;

            case "DoublePoints":
                isDoublePoints = true;
                doublePointsTime = duration;
                scoreMul = 2;
                indicatorIndex = 2;
                break;

            case "Freeze":
                isFrozen = true;
                frozenTime = duration;
                indicatorIndex = 3;

                //  Spawn ice cube mesh
                if (iceCubePrefab) { iceCubePrefab.transform.localScale = new Vector3(1, 1, 1); }

                // Show ice panel
                uiManager.EnableIcePanel(true);
                break;

            case "LightsOut":
                isLightOut = true;
                lightOutTime = duration;
                indicatorIndex = 4;

                // Show lights off panel
                uiManager.EnableLightsOff(true);
                break;

            // Hide minimap
            case "MapOut":
                isMapOut = true;
                mapOutTime = duration;
                indicatorIndex = 5;

                // Hide minimap ui
                uiManager.EnableMinimap(false);
                break;

            // Reverse controls
            case "Reverse":
                isReversedControls = true;
                reversedControlsTime = duration;
                indicatorIndex = 6;
                break;

            default:
                break;
        }

        // Activate power up indicator
        uiManager.ActivatePowerupIndicator(indicatorIndex, duration);
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
            // Stun character
            // _stunned = true;
            // _timeStunned = Time.time + stunTime;

            // Make character invincible
            _invencible = true;
            _timeInvencible = Time.time + invencibleTime;
            if (_anim) { _anim.SetBool("isInvincible", true); }

            // Decrease player score
            if (scoreManager) { scoreManager.UpdatePlayerScore(this.gameObject, -50); }
            Debug.Log("Soltar monstruos");

            // Play receive damage sound
            Sound s = Array.Find(sounds, sound => sound.name == "ReceiveDamage");
            if (s != null && s.source != null) { s.source.Play(); }
            else { Debug.Log("Character attack sound not found!"); }

            // Shake camera
            if (cinemachineShake) { cinemachineShake.ShakeCamera(2, 0.15f); }
            else { Debug.LogWarning("CinemachineShake component not found!"); }
        }
    }

    #endregion

    #region Shoot interaction Methods
    public void InstantiateProjectile()
    {
        _projectileInstance = PhotonNetwork.Instantiate(projectile.name, projectileThrower.position, projectileThrower.rotation);
        _projectileInstance.transform.parent = projectileThrower;
        _projectileInstance.GetComponent<Rigidbody>().useGravity = false;
        _projectileInstance.GetComponent<WeaponController>().player = this.gameObject;
    }

    public void ShootProjectile()
    {
        RaycastHit hitInfo;
        bool hitted = Physics.BoxCast(this.GetComponent<Collider>().bounds.center, this.transform.localScale * 1.5f, this.transform.forward, out hitInfo, this.transform.rotation, 100.0f, layerMask);
        /*
        if (hitInfo.transform.CompareTag("Player"))
        {
            hitInfo.transform.GetComponent<MyCharacterController>().DistanceHit();
        }
        else if (hitInfo.transform.CompareTag("Monster"))
        {
            hitInfo.transform.GetComponent<MonsterScript>().StunMonster();
        }
        */
        if (_projectileInstance == null) { return; }

        _projectileInstance.transform.parent = null;
        PhotonView projectileView = _projectileInstance.GetPhotonView();

        if (hitted)
        {
            Vector3 enemyPos = hitInfo.transform.position;
            enemyPos.y = 0.1f;
            Vector3 direction = (enemyPos - this.transform.position).normalized;
            projectileView.transform.rotation = Quaternion.LookRotation(-direction);
            projectileView.GetComponent<Rigidbody>().AddForce(direction * projectileSpeed);
        }
        else
        {
            Vector3 rotation = new Vector3(projectile.transform.rotation.eulerAngles.x, this.transform.rotation.eulerAngles.y + 180.0f, projectile.transform.rotation.eulerAngles.z);
            projectileView.transform.rotation = Quaternion.Euler(rotation);
            projectileView.GetComponent<Rigidbody>().AddForce(this.transform.forward * projectileSpeed);
        }

        //StartCoroutine(PhotonDestroyAfterTime(_projectileInstance, projectileTimeToLive));
    }

    public void DistanceHit()
    {
        Debug.Log("Golpeado con arma a distancia");
        if (!_stunned)
        {
            _stunned = true;
            _timeStunned = Time.time + stunTime;
        }
    }

    #endregion

    #region Sound Methods

    public void PlayShortRangeAttackSound()
    {
        // Play attack sound (short range)
        Sound s = Array.Find(sounds, sound => sound.name == "ShortRangeAttack");
        if (s != null && s.source != null) { s.source.Play(); }
        else { Debug.Log("Character attack sound not found!"); }
    }

    #endregion
}
