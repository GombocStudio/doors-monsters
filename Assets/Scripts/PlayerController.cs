using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;
using Photon.Pun;

public class PlayerController : MonoBehaviour
{

    [Tooltip("How fast the character moves")]
    public float walkSpeed = 1.0f;
    [Tooltip("How fast the character turns to face movement direction")]
    [Range(0.0f, 0.3f)]
    public float RotationSmoothTime = 0.12f;
    [Tooltip("To test the player without Photon")]
    public bool localMode = true;

    private Vector2 _movement;
    private Animator _anim;
    private float _rotationVelocity;
    private float _targetRotation;
    private bool _attack;

    // Start is called before the first frame update
    void Start()
    {
        if (!localMode)
        {
            PhotonView photonView = this.GetComponent<PhotonView>();

            if (!photonView.IsMine)
            {
                this.GetComponent<PlayerInput>().enabled = false;
                this.enabled = false;
            }
        }
        
        _anim = GetComponent<Animator>();
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        _movement = context.ReadValue<Vector2>();
    }

    public void OnAttack(InputAction.CallbackContext context)
    {
        _anim.SetBool("isAttacking", context.ReadValueAsButton());
    }

    private void Update()
    {
        _anim.SetBool("isWalking", (_movement.x != 0 || _movement.y != 0));
    }

    private void FixedUpdate()
    {
        float speed = 0.0f;
        if (_movement != Vector2.zero)
        {
            _targetRotation = Mathf.Atan2(_movement.x, _movement.y) * Mathf.Rad2Deg;
            float rotation = Mathf.SmoothDampAngle(transform.eulerAngles.y, _targetRotation, ref _rotationVelocity, RotationSmoothTime);

            transform.rotation = Quaternion.Euler(0.0f, rotation, 0.0f);
            speed = walkSpeed;
        }

        transform.Translate(Vector3.forward * (speed * Time.deltaTime));
    }
}
