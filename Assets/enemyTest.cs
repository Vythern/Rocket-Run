using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class enemyTest : MonoBehaviour
{
    private CharacterController entityBody;
    LayerMask entityCollisionLayer = (1 << 6) | (1 << 7) | (1 << 0);

    private Vector3 entityVelocity;


    //Friction
    private float frictionOffset = 94.42f; //do not set above 100 or else player will accelerate indefenitely on the current time step.  frictionOffset determines how fast the player slows down.  Lower values = faster slow down.  Default fixed timestep = 0.01.  
    private float airFrictionOffset = 98.685f; //same as normal friction, represents air friction and thus is much weaker.  dash is much stronger in air, so potentially needs slowed in air.  

    //Gravity
    private float gravityForce = 25f; //How strong is the gravity?  Higher value = faster acceleration from gravity.  
    private float terminalVelocity = -50f; //if the player's velocity.y is less than this amount, gravity is not applied.  

    //Slope stuff.  
    private float rampFactor = 3f; //Used to stop weird ramp behaviour; Also works in tandem with gravity force to limit the steepness of a slope the player can climb.  A higher number means the player can't climb as high of a slope without additional speed.  
    private float currentSlope = 0; //the fateful day, 9/30/2021, I finally fixed the fucking ramp physics.  

    private float minimumSpeed = 0.1f; //used to stop the player from moving incredibly small distances.  


    void Start()
    {
        entityBody = GetComponent<CharacterController>(); //set enemy body on startup.  
    }


    private bool isEntityGrounded()
    {
        Vector3 center = new Vector3(this.transform.position.x, this.transform.position.y - 1f, this.transform.position.z);
        Vector3 size = new Vector3(0.35f, 0.1f, 0.35f);

        return Physics.CheckBox(center, size, new Quaternion(), entityCollisionLayer);
    }


    private void applyPhysics() //apply friction runs in fixedUpdate, so it uses fixedDeltaTime.  
    {
        Vector3 tempVelocity = entityVelocity;
        tempVelocity.y = 0;

        if (isEntityGrounded()) { tempVelocity *= Time.fixedDeltaTime * frictionOffset; }
        else { tempVelocity *= Time.fixedDeltaTime * airFrictionOffset; }

        entityVelocity.x = tempVelocity.x;
        entityVelocity.z = tempVelocity.z;


        //apply gravity, player falls while midair.  self explanatory.  
        if (!isEntityGrounded())
        {
            entityVelocity.y -= Time.fixedDeltaTime * gravityForce;
            if (entityVelocity.y < terminalVelocity) { entityVelocity.y = terminalVelocity; } //if player is falling faster than terminal velocity
        }


        if (isEntityGrounded())
        {
            if (currentSlope >= 31f) { entityVelocity.y -= rampFactor * Time.fixedDeltaTime * gravityForce; } //slide player down slopes they shouldn't be able to climb.  
            else //if player is on walkable slope.  
            {
                entityVelocity.y -= Time.fixedDeltaTime * gravityForce;
                if (true) //replace true with (moveDirection == Vector3.zero) if the enemy can move.  
                {
                    entityVelocity.y += Time.fixedDeltaTime * gravityForce;
                    if (entityVelocity.y > 0) { entityVelocity.y = 0; }
                }
            }
        }

        if (Mathf.Abs(entityVelocity.z) < minimumSpeed) { entityVelocity.z = 0f; }
        if (Mathf.Abs(entityVelocity.x) < minimumSpeed) { entityVelocity.x = 0f; }
    }


    public void applyKnockBack(Vector3 knockbackVector)
    {
        entityVelocity += knockbackVector;
    }

    private void FixedUpdate()
    {
        entityBody.Move(entityVelocity * Time.fixedDeltaTime);
        applyPhysics();
    }
}
