using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMovement : MonoBehaviour
{
    public Transform target;

    private Vector3 zero = Vector3.zero;
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 targetPosition = target.position;
        targetPosition.z = transform.position.z;

        transform.position = targetPosition;
    }
}
