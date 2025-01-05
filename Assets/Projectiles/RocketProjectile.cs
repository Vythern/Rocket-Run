using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class RocketProjectile : MonoBehaviour
{
    private Rigidbody rocketBody;
    private float explosionRadius = 5f;
    private float explosionForce = 5f;

    private bool hitAlready = false;

    //private LayerMask wallsAndEnemies = (1 << 0) | (1 << 10) | (1 << 11);


    //private GameObject playerObject;
    

    void Start()
    {
        //the rocket needs to get the player's velocity.  
        //reference the moveTest / movement script and obtain it.  
        //GameObject player = GameObject.FindGameObjectWithTag("Player");
        //moveTest movementScript = player.GetComponent<moveTest>();
        //Vector3 playerVelocity = movementScript.returnVelocity();
        
        rocketBody = GetComponent<Rigidbody>();
        //rocketBody.velocity = playerVelocity;
        rocketBody.AddRelativeForce(new Vector3(0f, 30f, 3000f));

    }

    //how come at around ~2 distance from the player, the force is reversed?  what equation are we using that is causing this?  

    private void OnCollisionEnter(Collision collision) //when rocket encounters a collider.  
    {
        if(hitAlready == false)
        {
            rocketBody.constraints = RigidbodyConstraints.FreezeAll;

            Collider[] colliders = Physics.OverlapSphere(transform.position, explosionRadius);
            List<Collider> collisionPoints = new List<Collider>();
            collisionPoints.AddRange(colliders);

            //sometimes this is called twice?  
            //Seems to be if there are two collision points on a single frame, then it will do bad things, and run twice.  
            //the source of our double explosive force issue.  
            //keep this in mind for all future explosion based weapons.  
            //The solution is to stop the code from running again if it has run once already.  
            //either a bool, disabling the object such that it doesn't have any way to run again
            //etc.  


            for (int i = 0; i < collisionPoints.Count; i++)
            { //for each object in a 1.5 radius, make an array of raycasts towards the collision point.  
                if (collisionPoints[i].gameObject.layer.Equals(10))
                {
                    float distanceFromTarget = Vector3.Distance(collisionPoints[i].ClosestPointOnBounds(this.gameObject.transform.position), this.gameObject.transform.position);
                    float currentForce = explosionForce - distanceFromTarget;

                    //print("Current force:  " + currentForce);
                    Vector3 forceDirection = (collisionPoints[i].transform.position - this.gameObject.transform.position).normalized; //transform the direction force is applied. 
                                                                                                                                      //print("Force of explosion after being made relative" + forceDirection);
                    forceDirection *= currentForce * 6;
                    forceDirection.y *= 0.66f;
                    //print("Force of explosion after multiplication" + forceDirection);
                    if (forceDirection.y > 30f) { forceDirection.y = 30f; }
                    collisionPoints[i].gameObject.GetComponent<enemyTest>().applyKnockBack(forceDirection);
                    //print(collisionPoints[i].name);
                }

                if (collisionPoints[i].gameObject.layer.Equals(3)) //if collider can take knockback.  
                {
                    float distanceFromTarget = Vector3.Distance(collisionPoints[i].ClosestPointOnBounds(this.gameObject.transform.position), this.gameObject.transform.position);
                    float currentForce = explosionForce - distanceFromTarget;

                    //print("Current force:  " + currentForce);
                    Vector3 forceDirection = (collisionPoints[i].transform.position - this.gameObject.transform.position).normalized; //transform the direction force is applied. 
                                                                                                                                      //print("Force of explosion after being made relative" + forceDirection);
                    forceDirection *= currentForce * 6;
                    forceDirection.y *= 0.66f;
                    //print("Force of explosion after multiplication" + forceDirection);
                    if (forceDirection.y > 30f) { forceDirection.y = 30f; }
                    collisionPoints[i].gameObject.GetComponent<moveTest>().applyKnockBack(forceDirection);
                    //print(collisionPoints[i].name);
                }
            }
        }
        hitAlready = true;
        Destroy(this.gameObject);
    }
}