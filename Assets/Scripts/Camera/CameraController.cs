using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{ 
    [SerializeField]
    [SerializeReference]
    private Transform target;
    [SerializeField]
    private float smoothTime = 0.4f;
    private Vector2 velocity = new Vector2(0,0);
    float z = 0;
    // Start is called before the first frame update
    void Start()
    {
        z = transform.position.z;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        Vector2 pos = Vector2.SmoothDamp(transform.position, target.position, ref velocity, smoothTime);
        transform.position = new Vector3(pos.x, pos.y, z);
    }
}
