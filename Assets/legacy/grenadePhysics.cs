using UnityEngine;

public class grenadePhysics : MonoBehaviour
{
    private Rigidbody grenadeBody;
    private float explosionRadius = 3f;
    private float explosionForce = 12f;

    LayerMask overlapLayer   = (1 << 10) | (1 << 12); 
    //overlap layer detects all things which can be affected by the explosion, enemies, physics objects, and the player characters.  
    //layer 12 accounts for damageable objects, which includes enemies and physics objects.  

    LayerMask wallEnemyLayer = (1 << 0) | ( 1 << 12); //y'know I have no clue why this works, since it's only shifting a 1 to the 0 spot, so it should only collide with layer[0]?  idk man
    //freakin layermasks and shit are weird.  Nonetheless, it collides with the enemy layer somehow, and the default.  Of course, we wanted the default layer collision, but I guess it hits enemy too.  

    //distance also doesn't seem to be subtracting the necessary amount for radius properly.  Might need to increase the radius of the explosion too, and adjust values accordingly.  
    //worry about balancing when we have enemies to fight with.  


    void Start()
    {
        grenadeBody = GetComponent<Rigidbody>();
        grenadeBody.AddRelativeForce(new Vector3(0f, 30f, 800f));
    }



    private void OnCollisionEnter(Collision collision)
    {
        grenadeBody.constraints = RigidbodyConstraints.FreezeAll;
        if (collision.gameObject.tag == "destroyable") { collision.gameObject.SendMessage("takeDamage", 70f); }
        //if grenade directly hits enemy/physics props, deals additional damage. 

        Collider[] collisionPoints = Physics.OverlapSphere(transform.position, explosionRadius, overlapLayer);
        //gets all damageable objects in a radius
        for (int i = 0; i < collisionPoints.Length; i++)
        { //for each object in a 1.5 radius, make an array of raycasts towards the collision point.  
            if (collisionPoints[i].gameObject.tag == "PlayerCharacter")
            {
                float distanceFromTarget = Vector3.Distance(collisionPoints[i].ClosestPointOnBounds(this.gameObject.transform.position), this.gameObject.transform.position);
                if(distanceFromTarget <= 1f) { distanceFromTarget = 1f; }
                Vector3 forceDirection =  (collisionPoints[i].transform.position - this.gameObject.transform.position).normalized;
                forceDirection *= explosionForce / distanceFromTarget;
                collisionPoints[i].gameObject.SendMessage("knockback", forceDirection);
            }
            Ray explosionRay = new Ray(transform.position, collisionPoints[i].transform.position - transform.position);
            
            RaycastHit[] explosionHits = Physics.RaycastAll(explosionRay, wallEnemyLayer);
            System.Array.Sort(explosionHits, delegate (RaycastHit x, RaycastHit y) { return x.distance.CompareTo(y.distance); });
            //sort the array of hits received by the raycast by distance.  
            //for every hit, check whether or not the hit is the target object.  If not, next iteration.  If it's a wall, then stop loop.  
            for (int j = 0; j < explosionHits.Length; j++)
            {
                if (explosionHits[j].collider.gameObject == collisionPoints[i].gameObject)
                {
                    if (explosionHits[j].collider.gameObject.tag != "PlayerCharacter")
                    {
                        explosionHits[j].collider.gameObject.SendMessage("takeDamage", 350f);
                    }
                    else
                    {
                        //explosionHits[j].collider.gameObject.SendMessage("takeDamage", explosionDamage * 0.10 / distance);
                        //modify these values to account for distance?  maybe not.  - 100 for every unit of distance?  
                    }
                    j = 100;
                }
                else
                {
                    if(explosionHits[j].collider.gameObject.tag == "Enemy" && explosionHits[j].collider.gameObject != collisionPoints[i].gameObject)
                    {
                        //do nothing.  
                    }
                    if (explosionHits[j].collider.gameObject.tag != "Enemy")
                    {
                        j = 100;
                    }
                }
            }
        }
        Destroy(this.gameObject);
    }
}

//check if the overlap layers are hitting the right things, eg, print the layer of the gameobjects.  