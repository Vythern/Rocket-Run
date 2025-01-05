/* * * * * * * * * * * * * * * * * * * * * * * * * * *\
|* Nate Dukes.                                       *|
|* September 19th, 2022                              *|
|* Skorch Movement, Version 1.1.                     *|
|* Starting to work on weapon implementation         *|
|* Moving everything unrelated to movement           *|
|* over to a new script.  This script shouldn't      *|
|* need editting ever again I don't think.  Wonderful*|
\* * * * * * * * * * * * * * * * * * * * * * * * * * */


using UnityEngine;
using System;

public class skorchMovement : MonoBehaviour
{
    //Essentials
    [SerializeField] private GameObject playerObject;
    [SerializeField] private GameObject playerModel; //used to determine player model rotation.  Also calculates player Y rotation.  
    [SerializeField] private Camera playerCamera; //what the player can see.  
    [SerializeField] private Camera viewModelCamera; //renders ONLY weapon models.  removes clipping through walls issue.  this is attached to the maincamera so it doesn't need moved programmatically.  .  
                                                     //put weapons on the viewModel layer to make them appear to the player, if they are not, they will clip.  pickups and props should not be on viewmodel layer.  

    private CharacterController playerController;
    LayerMask playerCollisionLayer = (1 << 6) | (1 << 7) | (1 << 0);

    //Abstract vars
    private Vector3 playerVelocity; //player moves by this amount each frame (framerate independent).  
    private Vector3 moveDirection; //This is the direction that the player wants to move frame by frame.  
    private bool grounded = true; //used to check whether player can jump.  need to replace all instances of isPlayerGrounded() with this once all issues are resolved.  

    //Acceleration
    private float minimumSpeed = 0.1f; //used to stop the player from moving incredibly small distances.  
    private float accelerationSpeed = 100f; //player's base acceleration speed.  This in addition to frictionOffset help determine how fast the player can go and how fast they slow down.  
    private float airAccelerationSpeed = 34f; //player's acceleration speed while mid air.  with a value this high, the player can gain speed by bunnyhopping.  

    //Friction
    private float frictionOffset = 94.42f; //do not set above 100 or else player will accelerate indefenitely on the current time step.  frictionOffset determines how fast the player slows down.  Lower values = faster slow down.  Default fixed timestep = 0.01.  
    private float airFrictionOffset = 98.685f; //same as normal friction, represents air friction and thus is much weaker.  dash is much stronger in air, so potentially needs slowed in air.  

    //Gravity
    private float gravityForce = 25f; //How strong is the gravity?  Higher value = faster acceleration from gravity.  
    private float terminalVelocity = -50f; //if the player's velocity.y is less than this amount, gravity is not applied.  

    //Slope stuff.  
    private float rampFactor = 3f; //Used to stop weird ramp behaviour; Also works in tandem with gravity force to limit the steepness of a slope the player can climb.  A higher number means the player can't climb as high of a slope without additional speed.  
    private float currentSlope = 0; //the fateful day, 9/30/2021, I finally fixed the fucking ramp physics.  

    //Dash vars.  
    private bool dashReady = true;
    private int dashCount = 2; //number of times the player can dash before cooldown
    private float dashCooldown = 1f; //time it takes (in seconds) for a dash to recharge.  
    private float timeSinceLastDash = 0f; //time since the last dash, used to check when the player should receive a new dash.  
    private bool crouching = false; //dash disabled while crouching.  

    //User settings, mouse.  
    float mouseSensitivity = 0.4755f; //Figure out how many inches per 360 this is.  
    bool invertY = true; //multiplies vertical sensitivity by -1 when true.  
    KeyCode levelView = KeyCode.Minus; //Underscore key?  Does it matter?  Levels character view to 0 degrees up/down.  


    //User settings, view
    private int fieldOfView = 90; //need to figure out how fov actually works.  

    //User settings, bindings.  
    KeyCode moveForward = KeyCode.W; //player forward velocity increases.  
    KeyCode moveBack = KeyCode.S; //player forward velocity decreases.  
    KeyCode moveLeft = KeyCode.A; //player sideways velocity increases.  
    KeyCode moveRight = KeyCode.D; //player forward velocity decreases.  

    KeyCode lookUp = KeyCode.UpArrow; //rotates camera up.  
    KeyCode lookDown = KeyCode.DownArrow; //rotates camera down.  
    KeyCode turnLeft = KeyCode.LeftArrow; //rotates camera left.  
    KeyCode turnRight = KeyCode.RightArrow; //rotates camera right.  

    KeyCode jump = KeyCode.Space; //player jumps, vertical velocity increases.  
    KeyCode dash = KeyCode.LeftShift;
    KeyCode crouch = KeyCode.X; //Sep 19th, 2022.  Let's nip this one in the bud so we never have to open this script again.  
                                //crouch puts the player's size to half, maintaining the position they were at.  Cuts move speed into half.  




    private void controlMouse()
    {

        float mouseX = (mouseSensitivity * Input.GetAxisRaw("Mouse Y"));
        float mouseY = (mouseSensitivity * Input.GetAxisRaw("Mouse X")); //Why the hell does mouseY correspond to Mouse X and vice versa?  Fuck if I know.  
        if (Input.GetKey(lookUp)) { mouseX -= 180f * Time.deltaTime; }
        if (Input.GetKey(lookDown)) { mouseX += 180f * Time.deltaTime; }
        if (Input.GetKey(turnLeft)) { mouseY -= 180f * Time.deltaTime; }
        if (Input.GetKey(turnRight)) { mouseY += 180f * Time.deltaTime; }

        playerModel.transform.eulerAngles += new Vector3(0, mouseY, 0);
        playerCamera.transform.eulerAngles += new Vector3(mouseX, mouseY, 0);

        //Somewhere in the editor or one of these scripts, the vertical axis is inverted, and I have no clue why or how, but it's the default.  
        //In other words, invert y is true by default in code.  
    }

    private void handleInput() //handles player's primary input.  Separated from primary mouse input since they require distinct calculations and considerations.  Mouse inputs can still be bound to primary input Keycodes.  
    {
        moveDirection = Vector3.zero;

        if (Input.GetKey(moveForward)) { moveDirection += playerModel.transform.forward; }
        if (Input.GetKey(moveBack)) { moveDirection -= playerModel.transform.forward; }
        if (Input.GetKey(moveLeft)) { moveDirection -= playerModel.transform.right; }
        if (Input.GetKey(moveRight)) { moveDirection += playerModel.transform.right; }


        if (Input.GetKeyDown(dash) && crouching == false)
        {
            if (moveDirection != Vector3.zero)
            {
                if (dashCount > 0)
                {
                    if (isPlayerGrounded()) { playerVelocity += moveDirection.normalized * 50f; }
                    else { playerVelocity += moveDirection.normalized * 25f; } //airdash is weaker, but ends up being roughly the same since acceleration in the air is different.  
                    dashCount--;
                    timeSinceLastDash = Time.time;
                }
            }
        }

        if (Input.GetKey(jump) && isPlayerGrounded() && playerVelocity.y < 10f) //stop player from gaining massive speed.  disabled while crouching.  
        { if (!crouching) { playerVelocity.y = 10f; } }

        if (Input.GetKeyDown(crouch))
        {
            crouching = true;
            playerController.height = 1f;
            accelerationSpeed = 33.33f;
            //if standing still, player doesn't crouch...  Why is this?  

            if (grounded) { playerVelocity.y -= 2f; } //solves the player 'floating' when not moving as they press crouch.  also serves as an "animation" to get down lower.  


            //air acceleration speed stays the same.  Doesn't influence your momentum in the air basically.  
            //Big idea:  Since this can make it so that you have to quickly and with good timing, uncrouch / crouch between jumps, what if 
            //there was a speed boost for doing this in between jumps?  A nice bit of skill needed for increased speed perhaps?  
        }
        if (Input.GetKeyUp(crouch))
        {
            crouching = false;
            playerController.height = 2f;
            accelerationSpeed = 100f;
            if (isPlayerGrounded()) { playerController.Move(new Vector3(0, 0.5f, 0)); }
            //player has an issue where they don't return to standing when uncrouching (supposing that they are NOT moving currently). 
            //the move call returns them to the default standing position, and as far as I can tell it works perfectly.  
            //there could be some errors related to this but none that I found during my short time of testing at the moment.  
        }

        moveDirection = moveDirection.normalized; //no sr50 strafes.  

        if (isPlayerGrounded()) { playerVelocity += moveDirection * accelerationSpeed * Time.deltaTime; }
        else { playerVelocity += moveDirection * airAccelerationSpeed * Time.deltaTime; }


        playerController.Move(playerVelocity * Time.deltaTime);
    }

    private void applyPhysics() //apply friction runs in fixedUpdate, so it uses fixedDeltaTime.  
    {
        //Slow the player's horizontal speed.  
        Vector3 tempVelocity = playerVelocity;
        tempVelocity.y = 0;

        if (isPlayerGrounded()) { tempVelocity *= Time.fixedDeltaTime * frictionOffset; }
        else { tempVelocity *= Time.fixedDeltaTime * airFrictionOffset; }

        playerVelocity.x = tempVelocity.x;
        playerVelocity.z = tempVelocity.z;


        //apply gravity, player falls while midair.  self explanatory.  
        if (!isPlayerGrounded())
        {
            playerVelocity.y -= Time.fixedDeltaTime * gravityForce;
            if (playerVelocity.y < terminalVelocity) { playerVelocity.y = terminalVelocity; } //if player is falling faster than terminal velocity
        }


        if (isPlayerGrounded())
        {
            if (currentSlope >= 31f) { playerVelocity.y -= rampFactor * Time.fixedDeltaTime * gravityForce; } //slide player down slopes they shouldn't be able to climb.  
            else //if player is on walkable slope.  
            {
                playerVelocity.y -= Time.fixedDeltaTime * gravityForce;
                if (moveDirection == Vector3.zero)
                {
                    playerVelocity.y += Time.fixedDeltaTime * gravityForce;
                    if (playerVelocity.y > 0) { playerVelocity.y = 0; }
                }
            }
        }

        if (Mathf.Abs(playerVelocity.z) < minimumSpeed) { playerVelocity.z = 0f; }
        if (Mathf.Abs(playerVelocity.x) < minimumSpeed) { playerVelocity.x = 0f; }
    }


    //I don't even remember how I figured this out, must have been some stroke of genius.  
    //In any case I figured it out once and don't understand it, and at this point am too lazy to figure out why I ended up with this code again.  
    //dealing with the ramps in this game delayed development of the game by like a full year, for real. 
    //Part of that is probably because I had never worked with physics before and never built a game this scale
    //so yeah, it was a very valuable year of learning more about game dev, but still a delay.  


    //Writing this another year after the last line of text, It's actually because I only worked on it for like a week at a time with month long breaks.  

    private void OnControllerColliderHit(ControllerColliderHit collision) //called whenever in collision with a game object that can collide with the player layer.  
    {

        //this works perfectly, it is the other stuff that doesn't work.  
        //Version 1.0:  10/6/2021.  
        //If errors occur, drastically limit the angles at which newton's second law affects the player.  Step offset should be set to 0, or else ramps get REALLY FUCKY.  
        //That's the literal worst case scenario though- Works fine right now, but ofc I might find something incredibly minor to delay the game for again.  
        currentSlope = Vector3.Angle(collision.normal, Vector3.up);
        Vector3 collisionDifference = collision.normal * Vector3.Dot(playerVelocity, collision.normal);
        playerVelocity -= collisionDifference;
    }

    private bool isPlayerGrounded()
    {
        //Version 1.0:  10/6/2021.  
        //there are some very odd circumstances here and there which can cause the player to be listed as not grounded, but for them to occur you'd likely have to specifically design the geometry for it to happen. 
        //for example, if the player stands above a hole with width and length of 0.36f and is perfectly centered over top of it such that the checkBox is small enough to miss but the player controller is also able to get stuck in the hole (no thanks to unity's capsule collider and lack of cylinder collider).  

        Vector3 center = new Vector3(this.transform.position.x, this.transform.position.y - 1f, this.transform.position.z);
        Vector3 size = new Vector3(0.35f, 0.1f, 0.35f);

        return Physics.CheckBox(center, size, new Quaternion(), playerCollisionLayer);
    }


    void Start()
    {
        //Application.targetFrameRate = 30; //application should run at high FPS for the most part.  

        playerObject = this.gameObject;
        playerController = playerObject.GetComponent<CharacterController>();
        //playerCamera = playerObject.GetComponentInChildren<Camera>();
        //viewModelCamera = playerObject.GetComponentInChildren<Camera>();
    }

    private void FixedUpdate()
    {
        applyPhysics(); //physics includes gravity + friction, does not include newton's second law physics.  
    }

    void Update()
    {

        if (dashCount < 2)
        {
            if (Time.time - timeSinceLastDash >= dashCooldown)
            {
                dashCount++;
                timeSinceLastDash = Time.time; //time since last dash is reset so that cooldown doesn't instantly charge to full.  
            }
        }

        controlMouse();
        handleInput();
    }
}