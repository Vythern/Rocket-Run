using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class handleWeaponInput : MonoBehaviour
{
    //weapons are rendered by depth first, then by culling mask.  
    //higher depth means higher priority, lower numbers are rendered first.  
    //in this case, the main camera is rendering everything except weapon models, at a depth of -1, always rendering, underneath of everything else.  
    //the weapon viewmodels are at depth 5, everything from -infinity to 4 renders before it, then this over top.  It only renders objects tagged "viewmodel."  


    private Camera weaponOriginCamera;


    KeyCode selectChainsaw = KeyCode.Alpha1;
    KeyCode selectBreakAction = KeyCode.Alpha2;
    KeyCode selectPumpAction = KeyCode.Alpha3;
    KeyCode selectHMG = KeyCode.Alpha4;
    KeyCode selectMinigun = KeyCode.Keypad4;
    KeyCode SelectRocketLauncher = KeyCode.Alpha5;
    KeyCode selectStickybomber = KeyCode.Keypad5;
    KeyCode selectCoilgun = KeyCode.Alpha6;
    KeyCode selectBeamgun = KeyCode.Keypad6;
    KeyCode selectGiantFuckingBlaster = KeyCode.Alpha7;
    KeyCode selectC4 = KeyCode.Alpha8;

    KeyCode selectShotguns = KeyCode.Alpha3;     //rotates between the break action and the pump action.  
    KeyCode selectExplosives = KeyCode.Alpha5;  //rotates between the rocket launcher and the stickybomb launcher.  
    KeyCode selectPlasmas = KeyCode.Alpha6;     //rotates between the beamgun and the coilgun.  


    KeyCode use = KeyCode.E; //activates select objects by raycasting to them and sendMessage(use) on usable objects.  
    KeyCode flashLight = KeyCode.L; //enables flashlight, probably not used in actual game content.  

    KeyCode primaryFire = KeyCode.Q; //primary function of current weapon.  
    KeyCode secondaryFire = KeyCode.Mouse1; //secondary function of current weapon.  
    KeyCode ability = KeyCode.F; //In Rocket quest and Skorch, the grappling hook.  
    KeyCode ability2 = KeyCode.V; //In Skorch, the teleportation orb.  
    KeyCode ability3 = KeyCode.C; //In Skorch multiplayer, the vision powerup.  
    KeyCode kick = KeyCode.G; //used to knockback trash mobs and give breathing room.  think about dodging in and out of commons in hordes in l4d2.  

    KeyCode reload = KeyCode.R; //here for testing reload functionality.  

    KeyCode console = KeyCode.Tilde; //opens console.  
    KeyCode quickSave = KeyCode.F6; //saves current level information/data.  Maybe this should be separate from the character controller?  
    KeyCode quickLoad = KeyCode.F9; //loads the most recent save.  Maybe configure another setting for most recent quick save, most recent auto save, etc.  
    KeyCode openChat = KeyCode.Slash; //opens chat box.  


    [SerializeField] private GameObject Chainsaw;               //the Chainsaw is the melee weapon of choice.  Performs similar to brutal doom's lock-on chainsaw.  
    [SerializeField] private GameObject BreakAction;            //exactly what you'd expect from a break action- high power, good kickback, and massive damage up close.  More ammo efficient than its counterpart up close, but still able to deal roughly the same damage output at any given range, just slower and less effectively.  
    [SerializeField] private GameObject PumpAction;             //The counterpart pump shotgun is a fast firing, more precise, jack of all trades shotgun for taking down lots of smaller to medium targets at a safer distance.  
    [SerializeField] private GameObject HeavyMachineGun;        //The HMG is a sustained DPS weapon good for melting medium to large single targets and finishing things off at medium to long range.  In addition, it houses a grenade launcher underbarrel.  Performs as described on the below line.  Manual reload of the underbarrel, but pretty fast.  
    [SerializeField] private GameObject Minigun;                //The ultimate in eviscerating a single target or crowds of trash, the minigun is ammo inefficient, but damn does it melt things down when you need it.  
    [SerializeField] private GameObject StickybombLauncher;     //The grenade launcher is incredibly versatile.  Fires bouncy grenades that explode on contact with an enemy, or upon remote detonation.  The alt fire controls the detonation.  It's the stickybomb launcherr and the pipe launcher.  Now it's the stickybomb launcher and the heavy machine gun is used for the grenades.  
    [SerializeField] private GameObject RocketLauncher;         //The rocket launcher is great for mobility, as well as taking out hordes of trash mobs, or dealing heavy damage to medium targets.  It doesn't have the greatest single target dps.  Alt fire remote detonates the projectile.  
    [SerializeField] private GameObject C4;                     //Just like blood's dynamite, it is a difficult to master HIGH power explosive.  Perhaps this could be the stickybomb as well?  Remote detonation removed from grenade launcher while this retains that function?  Behaves like blood's dynamite as well.  
    [SerializeField] private GameObject Beamgun;                //An LG, great pushback for floating targets.  Penetrates through multiple enemies, dealing more damage the longer it is on target, but loses damage at range.  A close range, faster firing, higher dps machinegun which is great for taking out single targets.  
    [SerializeField] private GameObject Coilgun;                //Ever need to clear a room, the Coilgun is meant for it.  Shoots a guass cannon style burst, covering a wide area.  Based off the spell lightning bolt from DND.  An easy to hit railgun shot, with more damage the closer to the center of the beam it is.  Insane pushback.  Miniature explosion on contact point.  Alt fire sends out a horizontal grid of death that shreds low to medium health enemies and deals good damage at the cost of ammo to larger ones.  
    [SerializeField] private GameObject GiantFuckingBlaster;    //Who knows what this will do?  A phaselock?  A black hole?  Tendrils like modern DOOM?  Literally just a big blaster?  


    [SerializeField] private GameObject rocketPrefab;

    private GameObject currentWeapon;
    private string currentWeaponID;    //Current weapon by number is enabled, visible, and determines what this script should do when the player presses fire, secondary fire, or tertiary fire.  
    private float cooldownEndTime = 0; //this tracks when the player's cooldown has ended.  After each attack, there is a cooldown time, which is added to the current Time to determine when their next available attack is.  


    private bool cycleExplosives = true;    //sets whether to put sticky launcher and rocket launcher in the same weapon slot.  
    private bool cyclePlasmas = true;      //sets whether to put coilgun and beamgun in the same weapon slot.  
    private bool cycleShotguns = false;      //sets whether to put coilgun and beamgun in the same weapon slot.  
    private bool cycleHeavyWeapons = true;      //sets whether to heavy machine gun and minigun in the same weapon slot.  


    /* With explosives, heavy weapons, and plasmas being cycleable, the default weapon selection keybinds are:  
     * 1.  Chainsaw.  
     * 2.  Break Action.  
     * 3.  Pump shotgun.  
     * 4.  Machine gun followed by minigun.  
     * 5.  Rocket launcher followed by stickybomb launcher.  
     * 6.  Coilgun followed by Beamgun.  
     * 7.  GFB.  
     * V.  C4.  
     */



    void Start()
    {
        weaponOriginCamera = Camera.main;
        currentWeapon = Chainsaw;
        currentWeaponID = "CH";
        Cursor.visible = false; //hides cursor from game, and also when testing.  
        currentWeapon.SetActive(true);
    }



    //I think the weapon selection system might need a re-factor.  
    //it's servicable as is, where if the cycleWeapons is enabled for the respective set, then it does so.  
    //it also makes it weird though, if you have, eg, shotguns on 2 and 3, and you have already cycled, then pressing the key for shotgun 1 might not always bring you to shotgun 1.  
    //in theory this is how it works but it's a little offputting sometimes and might make for some nasty confusion in the heat of battle.  



    //play weapon swap animation.  
    //disable currentWeapon.  
    //set current weapon.  
    //make currentWeapon active.  
    //play weapon swap animation back. 

    private bool coolingDown()
    {
        if(Time.time >= cooldownEndTime) { return false; }//Checks whether or not Time.time + cooldown time has been passed by the current time.  eg 1 second cooldown at the 10th second of the game's start translates to the 11th second ending the cooldown.  
        else { return true; }
    }

    private void handleWeaponSelection() //this method checks what weapons are active, and swaps to the relevant one if available / not already selected, 
    {
        if (Input.GetKeyDown(selectChainsaw))
        {
            if (currentWeaponID != "CH")
            {
                currentWeapon.SetActive(false);
                //play weapon swap animation.  from x to y
                Chainsaw.SetActive(true);
                currentWeapon = Chainsaw;
                currentWeaponID = "CH";
            }
        }
        if (Input.GetKeyDown(selectBreakAction))
        {
            if (currentWeaponID == "BA" && cycleShotguns) //this has to come first.  
            { //if the user has enabled weapon categories then pressing 2 while the pump action is equipped will rotate to the pump action.  
                currentWeapon.SetActive(false);
                PumpAction.SetActive(true);
                currentWeapon = PumpAction;
                currentWeaponID = "PA";
            }
            else if (currentWeaponID != "BA")
            {
                currentWeapon.SetActive(false);
                //play weapon swap animation.  from x to y
                BreakAction.SetActive(true);
                currentWeapon = BreakAction;
                currentWeaponID = "BA";
            }
        }
        if (Input.GetKeyDown(selectPumpAction))
        {
            if (currentWeaponID == "PA" && cycleShotguns)//this has to come first.  
            { //if the user has enabled weapon categories then pressing 3 while the pump action is equipped will rotate to the break action.  
                currentWeapon.SetActive(false);
                BreakAction.SetActive(true);
                currentWeapon = BreakAction;
                currentWeaponID = "BA";
            } //no need for a vice versa method as it is already done- selectPumpAction does it conveniently by breaking.  
            else if (currentWeaponID != "PA")
            {
                currentWeapon.SetActive(false);
                //play weapon swap animation.  from x to y
                PumpAction.SetActive(true);
                currentWeapon = PumpAction;
                currentWeaponID = "PA";
            }
        }
        if (Input.GetKeyDown(selectHMG))
        {
            if (currentWeaponID == "HMG" && cycleHeavyWeapons)
            {
                HeavyMachineGun.SetActive(false);
                Minigun.SetActive(true);
                currentWeapon = Minigun;
                currentWeaponID = "MG";
            }
            else if (currentWeaponID != "HMG")
            {
                currentWeapon.SetActive(false);
                //play weapon swap animation.  from x to y
                HeavyMachineGun.SetActive(true);
                currentWeapon = HeavyMachineGun;
                currentWeaponID = "HMG";
            }
        }
        if (Input.GetKeyDown(selectMinigun))
        {
            if (currentWeaponID == "MG" && cycleHeavyWeapons)
            {
                Minigun.SetActive(false);
                HeavyMachineGun.SetActive(true);
                currentWeapon = HeavyMachineGun;
                currentWeaponID = "HMG";
            }
            else if (currentWeaponID != "MG")
            {
                currentWeapon.SetActive(false);
                //play weapon swap animation.  from x to y
                Minigun.SetActive(true);
                currentWeapon = Minigun;
                currentWeaponID = "HMG";
            }
        }
        if (Input.GetKeyDown(SelectRocketLauncher))
        {
            if (currentWeaponID == "RL" && cycleExplosives)
            {
                RocketLauncher.SetActive(false);
                StickybombLauncher.SetActive(true);
                currentWeapon = StickybombLauncher;
                currentWeaponID = "SBL";
            }
            else if (currentWeaponID != "RL")
            {
                currentWeapon.SetActive(false);
                //play weapon swap animation.  from x to y
                RocketLauncher.SetActive(true);
                currentWeapon = RocketLauncher;
                currentWeaponID = "RL";
            }
        }
        if (Input.GetKeyDown(selectStickybomber))
        {
            if (currentWeaponID == "SBL" && cycleExplosives)
            {
                StickybombLauncher.SetActive(false);
                RocketLauncher.SetActive(true);
                currentWeapon = RocketLauncher;
                currentWeaponID = "RL";
            }
            else if (currentWeaponID != "SBL")
            {
                currentWeapon.SetActive(false);
                //play weapon swap animation.  from x to y
                StickybombLauncher.SetActive(true);
                currentWeapon = StickybombLauncher;
                currentWeaponID = "SBL";
            }
        }
        if (Input.GetKeyDown(selectCoilgun))
        {
            if (currentWeaponID == "CG" && cyclePlasmas)
            {
                Coilgun.SetActive(false);
                Beamgun.SetActive(true);
                currentWeapon = Beamgun;
                currentWeaponID = "BG";
            }
            else if (currentWeaponID != "CG")
            {
                currentWeapon.SetActive(false);
                //play weapon swap animation.  from x to y
                Coilgun.SetActive(true);
                currentWeapon = Coilgun;
                currentWeaponID = "CG";
            }
        }
        if (Input.GetKeyDown(selectBeamgun))
        {
            if (currentWeaponID == "BG" && cyclePlasmas)
            {
                Beamgun.SetActive(false);
                Coilgun.SetActive(true);
                currentWeapon = Coilgun;
                currentWeaponID = "CG";
            }
            else if (currentWeaponID != "BG")
            {
                currentWeapon.SetActive(false);
                //play weapon swap animation.  from x to y
                Beamgun.SetActive(true);
                currentWeapon = Beamgun;
                currentWeaponID = "BG";
            }
        }
        if (Input.GetKeyDown(selectGiantFuckingBlaster))
        {
            if (currentWeaponID != "GFB")
            {
                currentWeapon.SetActive(false);
                //play weapon swap animation.  from x to y
                GiantFuckingBlaster.SetActive(true);
                currentWeapon = GiantFuckingBlaster;
                currentWeaponID = "GFB";
            }
        }
        if (Input.GetKeyDown(selectC4))
        {
            if (currentWeaponID != "PE")
            {
                currentWeapon.SetActive(false);
                //play weapon swap animation.  from x to y
                C4.SetActive(true);
                currentWeapon = C4;
                currentWeaponID = "PE";
            }
        }
    }

    private void checkWeaponInput() //fire weapon has two versions- active, and passive.  Active is used for single shot weapons eg the coilgun primary fire railgun blast, or charged weapons such as the stickybomb launcher.  The passive is for GetKey, where weapons will continue firing automatically, eg the assault rifle.  
    {
        //we will have to decide whether or not keypressdown is relevant.  
        //incorporate time into these as well for checks.  
        //ammo, currently in an animation, etc etc.  
        if (!coolingDown())
        {
            if (Input.GetKey(primaryFire))
            {
                switch (currentWeaponID) //these getkey methods don't use getKeyDown because you can check if they pressed the button this frame within the evaluative statement.  
                {
                    case "CH": { fireChainsaw(); break; } //Vertical slash/thrust, deals continuous single target damage, draining the health of the enemy.  The chainsaw will get stuck and stall if it doesn't cut through / kill the target, but the target is mostly immobilized due to pain until it does.  
                    case "BA": { fireBA(); break; } //Fires both barrels of the break action.  High kickback force, devastating blast.  Not very accurate but covers a huge range.  Knocks the player backwards.  
                    case "PA": { firePA(); break; } //basic buck.  Shoots fast, reasonably accurately, and deals decent damage.  
                    case "HMG": { fireHMG(); break; } //Basdic assault rifle- shoots at a fast fire rate, moderate damage output, super accurate, long ranged.  
                    case "MG": { fireMinigun(); break; } //Absolute devastating DPS, less ammo efficient, and requires spin up.  
                    case "RL": { fireRocketLauncher(); break; } //Spawns a rocket projectile that explodes and deals damage on contact, knocking things backwards.  
                    case "SBL": { fireSBL(); break; } //Fires out a sticky bomb which sticks to players, enemies, and surfaces alike, but is otherwise harmless until detonated.  (sticky bomb launcher from tf2).  
                    case "CG": { fireCoilgun(); break; } //Gauss cannon piercing blast, must be charged up.  Has a good radius of damage, dealing more when centered on the target.  
                    case "BG": { fireBeamgun(); break; } //Lightning gun- knocks back targets, firing a forwards moving beam of energy, dealing more damage the longer it is on target.  Melting enemies.  Similar power level to the coilgun, but achieves the end in a different way.  Pierces enemies, and sometimes walls, dealing less damage the further it travels.  
                    case "GFB": { fireGFB(); break; } //Who knows what this thing is gonna do?  Okay, what if, it shoots an enormous glob of acid / napalm, which literally melts enemies, and then they explode on death a-la bloodsplosion krieg?  
                    case "PE": { fireC4(); break; } //tosses out the blood dynamite stick when charged.  might need separate logic for getkeydown and getkeyup.  
                    default: break;
                }
            }
            if (Input.GetKey(secondaryFire)) //these getkey methods don't use getKeyDown because you can check if they pressed the button this frame within the evaluative statement.  
            {
                switch (currentWeaponID)
                {
                    case "CH": { altFireChainsaw(); break; } //Horizontal slash in a wide area.  Can stun enemies momentarily and knock them back a very small amount.  Hits all targets in a rectangle across the screen.  
                    //case "BA": { altFireBA(); break; } //fires a single barrel of the break action, not nearly as accurate as the combat shotgun, but deals more damage still.  
                    //case "PA": { firePA(); break; } //presently no alt fire function exists for the shotgun.  A grappling hook, perhaps?  
                    case "HMG": { altFireHMG(); break; } //Fires out a bouncing grenade that detonates after a set time or upon contact with an enemy.  
                    case "MG": { fireMinigun(); break; } //pre-revs the minigun.  
                    case "RL": { altFireRocketLauncher(); break; } //detonates mid air rockets.  only an option after a certain travel time.  we'll have to decide if the rockets and stickybombs detonate at the same keypress or if they detonate specifically when the weapon is held.  
                    case "SBL": { altFireSBL(); break; } //Detonates all placed stickybombs, and mid-air ones as well.  we'll have to decide if the rockets and stickybombs detonate at the same keypress or if they detonate specifically when the weapon is held.  
                    case "CG": { altFireCoilgun(); break; } //Horizontal breach-cutter like blast.  High damage per second, low damage per hit, lines of enemies, and good sustained damage against large targets.  
                    //case "BG": { altFireBeamgun(); break; } //the beamgun doesn't currently have plans for an alt-fire.  The coilgun piercing lightning bolt that acts like 5e lightning bolt eliminates eliminates the need for the charged up alt-fire.  
                    case "GFB": { altFireGFB(); break; } //spew acid stream like a flamethrower.  
                    case "PE": { altFireC4(); break; } //detonates all armed c4 charges.  
                    default: break;
                }
            }
        }
    }

    private void fireChainsaw()
    {
        if(Input.GetKeyDown(primaryFire)) //if they are not already actively swinging.  
        {
            print("Initial chainsaw being swung");
            cooldownEndTime = Time.time + 1.5f; //long initial swing.  if it comes into contact with a target though, move to the else statement while the attack button is held.  
            //
        }
        else //else if Input.GetKey()
        {
            //grind away.  
        }
    }

    private void fireBA()
    {
        //raycast all the pellets.  

        //knockback player.  
        this.gameObject.GetComponent<moveTest>().applyKnockBack(Camera.main.transform.TransformDirection(new Vector3(0, 0, -8f))); //this weapon has serious kickback.  
        cooldownEndTime = Time.time + 0.9f;
        
        //firing break action.  
    }

    private void firePA()
    {
        
    }
    private void fireHMG()
    {

    }
    private void fireMinigun()
    {

    }
    private void fireRocketLauncher()
    {
        GameObject rocket = Object.Instantiate(rocketPrefab, (weaponOriginCamera.transform.position + weaponOriginCamera.transform.forward), weaponOriginCamera.transform.rotation);
        //spawn rocket projectile.  
        //add rocket to list of detonatables.  
        //increase cooldown time.  
        cooldownEndTime = Time.time + 0.9f;
    }
    private void fireSBL()
    {

    }
    private void fireCoilgun()
    {

    }
    private void fireBeamgun()
    {
        //FUCK ABOUTR WITH THIS SHIT LATER.  







        //we should have a single raycast hit to get the contact point of the beamgun, and then draw the beam from the edge of the gun barrel to the point that that ray hits.  
        //under the hood, it's just a spherecast originating from the camera center to the same end point.  
        /*if (Physics.SphereCast(weaponOriginCamera.transform.position, 2f, weaponOriginCamera.transform.forward, out RaycastHit sphereHit))
        {
            //this is used for the enemy damage.  
            float distance = Vector3.Distance(this.gameObject.transform.position, sphereHit.point);
            this.gameObject.GetComponent<moveTest>().applyKnockBack(Camera.main.transform.TransformDirection(new Vector3(0, 0, (-5f / distance)))); //this weapon has serious kickback.  
            print(-5f / distance);
        }*/
        if (Physics.Raycast(weaponOriginCamera.transform.position, weaponOriginCamera.transform.forward, out RaycastHit rayHit))
        {
            float distance = Vector3.Distance(this.gameObject.transform.position, rayHit.point);
            /*float force = 0f;
            if(distance >= 5) { force = -0.82f; }
            else if (distance < 5 && distance > 4) { force = -0.85f; }
            else if (distance <= 4 && distance >= 3) { force = -1.0f; }
            else if (distance <= 3 && distance >= 2) { force = -1.0f; }
            else if (distance <= 2 && distance >= 1) { force = -1.2f; }
            else if (distance <= 1 && distance >= 0) { force = -1.5f; }
            */
            if (distance <= 5)
            {
                this.gameObject.GetComponent<moveTest>().applyKnockBack(Camera.main.transform.TransformDirection(new Vector3(0, 0, -0.85f))); //this weapon has serious kickback.  
            }
            else if (distance > 5) { this.gameObject.GetComponent<moveTest>().applyKnockBack(Camera.main.transform.TransformDirection(new Vector3(0, 0, -0.80f))); } //this eventually will work as we want it.  
        }

        cooldownEndTime = Time.time + 0.033f;
    }
    private void fireGFB()
    {
        
    }
    private void fireC4()
    {

    }

    private void altFireChainsaw()
    {

    }
    private void altFireHMG()
    {

    }
    private void altFireMinigun()
    {

    }
    private void altFireRocketLauncher()
    {

    }
    private void altFireSBL()
    {

    }
    private void altFireCoilgun()
    {

    }
    private void altFireGFB()
    {

    }
    private void altFireC4()
    {

    }



    void Update()
    {

        //check for whether cooldown has ended before anything else; that way if the player is going at, say, 1 frame per second, theoretically, and they have a 0.5 second cooldown, then they are guaranteed to get the update 
        handleWeaponSelection();
        checkWeaponInput();
    }
}
