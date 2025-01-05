using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class bouncyGrenadePhysics : MonoBehaviour
{

    private Rigidbody grenadeBody;
    private float explosionRadius = 3f;
    private float explosionForce = 12f;


    // Start is called before the first frame update
    void Start()
    {
        grenadeBody = GetComponent<Rigidbody>();
        grenadeBody.AddRelativeForce(new Vector3(0f, 30f, 800f));
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        
    }
}
