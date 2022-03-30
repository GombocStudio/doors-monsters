using UnityEngine;
using Photon.Pun;
using UnityEngine.InputSystem;
using Photon.Realtime;
using ExitGames.Client.Photon;
using UnityEngine.InputSystem.OnScreen;
using System.Collections;

// Script responsible for handling the movement and actions of the characters in the scene.
public class MyCharacterController : MonoBehaviourPunCallbacks, IPunObservable, IOnEventCallback
{
    #region Character Variables
    // Character movement related variables
    public float _speed = 3.0f;
    private Vector2 _movement;
    ////////////////////////////
    private RectTransform stick;

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

    public float projectileTimeToLive = 5.0f;
    public Transform projectileThrower;
    public float projectileSpeed = 1000.0f;
    private GameObject _projectileInstance;

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

    private GameObject iceCube;
    private GameObject iceCubePrefab;
    public float frozenTime = 5.0f;
    public bool isFrozen = false;

    public float openDoorsTime = 5.0f;
    public bool isOpenDoors = false;

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

            PowerUpController puCntrlr = FindObjectOfType<PowerUpController>();
            miniMap = puCntrlr.miniMap;
            lightsOff = puCntrlr.lightsOff;
            //iceCube = puCntrlr.iceCubePrefab;

            //iceCube.transform.SetParent(transform);
            //iceCube.transform.position = transform.position + Vector3.up * 0.8f;
        }
        // Disable player input if view is not mine
        this.GetComponent<PlayerInput>().enabled = _view.IsMine || testLocal;

        // Initialize animtor component reference
        _anim = GetComponent<Animator>();

        // Disable weapon collider
        weaponCollider.enabled = false;

        // Get Joystick (mobile)
        stick = FindObjectOfType<OnScreenStick>().gameObject.GetComponent<RectTransform>();
    }

    public void FixedUpdate()
    {
        // Mobile controls
#if UNITY_IOS || UNITY_ANDROID

        if (stick.localPosition == Vector3.zero)
        {
            _movement = Vector2.zero;
            _anim.SetBool("isWalking", false);
        }

#endif
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
            //if (!iceCube.activeSelf)
            //{
                //iceCube.SetActive(true);
            //}

            // If opened start decreasing controlled time
            frozenTime -= Time.deltaTime;

            if (frozenTime <= 0)
            {
                isFrozen = false;
                //iceCube.SetActive(false);
                PhotonNetwork.Destroy(iceCube);

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
    Vector2 lastMov;
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

        if (_movement == Vector2.zero)// && lastMov != null)
        {

            _movement = lastMov;

        }

        lastMov = _movement;
#endif

        //Debug.Log(_movement.ToString());
        //Debug.Log(stick.localPosition.ToString());

        // Power up effects
        if (isFrozen || _stunned) { _movement = Vector2.zero; }
        if (isReversedControls)
        {
            _movement = -_movement;
        }


        if (_anim) { _anim.SetBool("isWalking", (_movement.x != 0 || _movement.y != 0)); }
    }

    // Method triggered when the characterActions.InputActions specified attack key is pressed
    public void OnAttack(InputAction.CallbackContext context)
    {
        // If player is stunned don't attack
        if (_stunned || isFrozen) { return; }

        // Play animation attack
        if (_anim) 
        { 
            _anim.SetBool("isAttacking", context.ReadValueAsButton());
            /*
            // Launch projectile
            GameObject bullet = PhotonNetwork.Instantiate(projectile.name, transform.position,
                                              transform.rotation);
            Quaternion rotAdjust = projectile.transform.localRotation;
            Vector3 posAdjust = Vector3.up * 0.65f + Vector3.right * 0.4f + Vector3.forward * 0.25f ;

            PhotonView v = bullet.GetPhotonView();
            v.transform.localRotation *= rotAdjust;
            v.transform.position += posAdjust;
            bullet.GetComponent<Rigidbody>().AddRelativeForce(new Vector3(0, 0, launchVel));        
            */
        }


    }

    public void OnShoot(InputAction.CallbackContext context)
    {
        _anim.SetBool("isShooting", context.ReadValueAsButton());
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

    public void OnEvent(EventData photonEvent)
    {
        byte eventCode = photonEvent.Code;
        int data = (int)photonEvent.CustomData;

        if (data != GetInstanceID())
        {
        switch (eventCode)
            {
                case 1:
                    isMapOut = true;
                    mapOutTime = 5.0f;
                    if (miniMap.activeSelf)
                    {
                        miniMap.SetActive(false);
                    }
                    break;
                case 2:
                    isLightOut = true;
                    lightOutTime = 5.0f;
                    if (!lightsOff.activeSelf)
                    {
                        lightsOff.SetActive(true);
                    }
                    break;
                case 3:
                    isReversedControls = true;
                    reversedControlsTime = 5.0f;
                    break;
                case 4:
                    isFrozen = true;
                    frozenTime = 5.0f;
                    iceCube = PhotonNetwork.Instantiate(iceCubePrefab.name, transform.position + Vector3.up * 0.8f, transform.rotation);
                    break;
                default:
                    break;
            }
        }
        
    }
    private IEnumerator PhotonDestroyAfterTime(GameObject gameObject, float seconds)
    {
        yield return new WaitForSeconds(seconds);
        PhotonNetwork.Destroy(gameObject.GetPhotonView());
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

    #region Shoot interaction Methods

    public void InstantiateProjectile()
    {
        _projectileInstance = PhotonNetwork.Instantiate(projectile.name, projectileThrower.position, projectileThrower.rotation);
        _projectileInstance.transform.parent = projectileThrower;
        _projectileInstance.GetComponent<Rigidbody>().useGravity = false;
    }

    public void ShootProjectile()
    {
        _projectileInstance.transform.parent = null;
        PhotonView projectileView = _projectileInstance.GetPhotonView();

        RaycastHit hitInfo;
        bool hitted = Physics.BoxCast(this.GetComponent<Collider>().bounds.center, this.transform.localScale * 1.5f, this.transform.forward, out hitInfo, this.transform.rotation, 10.0f);
        if (hitted && (hitInfo.transform.CompareTag("Player") || hitInfo.transform.CompareTag("Monster")))
        {
            Vector3 direction = (hitInfo.transform.position - this.transform.position).normalized;
            projectileView.transform.rotation = Quaternion.LookRotation(-direction);
            projectileView.GetComponent<Rigidbody>().AddForce(direction * projectileSpeed);
        }
        else
        {
            Vector3 rotation = new Vector3(projectile.transform.rotation.eulerAngles.x, this.transform.rotation.eulerAngles.y + 180.0f, projectile.transform.rotation.eulerAngles.z);
            projectileView.transform.rotation = Quaternion.Euler(rotation);
            projectileView.GetComponent<Rigidbody>().AddForce(this.transform.forward * projectileSpeed);
        }

        StartCoroutine(PhotonDestroyAfterTime(_projectileInstance, projectileTimeToLive));
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
}
