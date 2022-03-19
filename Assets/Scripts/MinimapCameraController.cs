using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MinimapCameraController : MonoBehaviour
{
    [Tooltip("Player transform for the minimap camera to follow")]
    public Transform playerTransform;

    // Minimap camera position in the scene
    private Vector3 _cameraPosition;

    // Start is called before the first frame update
    void Start()
    {
        _cameraPosition = this.transform.position;
    }

    // Update is called once per frame
    void LateUpdate()
    {
        _cameraPosition.x = playerTransform.position.x;
        _cameraPosition.z = playerTransform.position.z;
        this.transform.position = _cameraPosition;
    }
}
