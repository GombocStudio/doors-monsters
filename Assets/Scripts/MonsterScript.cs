using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonsterScript : MonoBehaviour
{
    public Vector3 targetPosition;
    public float speed;
    
    private void GenerateTargetPosition()
    {
        var random = Random.onUnitSphere;
        targetPosition = new Vector3(random.x, 0, random.z);
    }
    // Start is called before the first frame update
    void Start()
    {
        GenerateTargetPosition();
        speed = Random.Range(0.4f, 1f);
    }

    // Update is called once per frame
    void Update()
    {
        if (Vector3.Distance(transform.position, targetPosition) < 1)
        {
            GenerateTargetPosition();
        }

        transform.Translate(targetPosition * (speed / 250));
    }
}
