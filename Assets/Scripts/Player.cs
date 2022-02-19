using System;
using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;
using UnityEngine.InputSystem;
using Hashtable = ExitGames.Client.Photon.Hashtable;
using Random = UnityEngine.Random;

public class Player : MonoBehaviourPunCallbacks, IPunObservable
{
    private Vector2 _movement;
    private float _rotSpeed = 5f;
    private float _speed = .2f;

    private Vector3 _networkPosition;
    private Quaternion _networkRotation;

    public override void OnEnable()
    {
        base.OnEnable();
        if (photonView.IsMine)
        {
            PhotonNetwork.LocalPlayer.CustomProperties.Add("PlayerCreated", Time.time);
            PhotonNetwork.LocalPlayer.CustomProperties.Add("LastColorChange", Time.time);
        }
    }

    public void Move(InputAction.CallbackContext context)
    {
        _movement = context.action.ReadValue<Vector2>();
    }

    public void Fire(InputAction.CallbackContext context)
    {
        if (photonView.IsMine && context.started)
        {
            Color newPlayerColor = new Color(Random.Range(0f, 1f), 
                Random.Range(0f, 1f), Random.Range(0f, 1f));
            photonView.RPC("SetPlayerColor", RpcTarget.All, 
                newPlayerColor.r, newPlayerColor.g, newPlayerColor.b);
            
            Hashtable updateProperty = new Hashtable();
            updateProperty.Add("LastColorChange",Time.time);
            PhotonNetwork.LocalPlayer.SetCustomProperties(updateProperty);
        }
    }

    [PunRPC]
    private void SetPlayerColor(float r, float g, float b)
    {
        Color newPlayerColor = new Color(r, g, b);
        GetComponent<Renderer>().material.color = newPlayerColor;
    }

    private void FixedUpdate()
    {
        Transform playerTransform = transform;
        if (photonView.IsMine)
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

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(transform.position);
            stream.SendNext(transform.rotation);
        }
        else
        {
            _networkPosition = (Vector3) stream.ReceiveNext();
            _networkRotation = (Quaternion) stream.ReceiveNext();
        }
    }

    public override void OnPlayerPropertiesUpdate(Photon.Realtime.Player targetPlayer, Hashtable changedProps)
    {
        base.OnPlayerPropertiesUpdate(targetPlayer, changedProps);
        foreach (var prop in changedProps)
        {
            Debug.Log("Player " + targetPlayer.NickName + " " + prop.Key + " changed to " + prop.Value + ".");
        }
    }

    public override void OnPlayerLeftRoom(Photon.Realtime.Player otherPlayer)
    {
        base.OnPlayerLeftRoom(otherPlayer);
        Debug.LogWarning(otherPlayer.NickName + " left the game.");
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(_networkPosition, .1f);
    }
}