/**
    Player mechanics & animation implementation

@Author Steven Hoodikoff
*/
using Godot;
using System;
using System.Collections.Generic;
using System.Runtime;

/**
    Object to hold player info such as global position and health 
    (for recall ability)
*/
public struct PlayerStatus {
    public Vector2 position; // (x,y)
    public int health;
}

public class Player : KinematicBody2D
{
    [Export] public int speed = 400;
    [Export] public int dash_speed = 1000;
    [Export] public float gravity = 9.81f;
    [Export] public float jump_power = 600f;
    [Export] public float mass = 2.5f;
    [Export] public float acceleration = 20f;
    [Export] public float wall_slide_animation = 10f;
    [Export] public float max_wall_slide_speed = 120f;


    // member variables here
    public Vector2 velocity = new Vector2();
    public Vector2 position = new Vector2();
    private AnimatedSprite _animatedSprite;
    private RayCast2D groundray;
    private RayCast2D leftray;
    private RayCast2D rightray;
    private Hitbox hitbox;
    private CollisionShape2D hitbox_collision_obj;
    private Vector2 hitbox_position = new Vector2(6, 4);
    private Hurtbox hurtbox;
    private CollisionShape2D hurtbox_collision_obj;

    private Timer dash_timer; //for adjusting dash duration
    public Timer dash_cooldown; //cooldown timer for dash ability
    public float dash_cooldown_value = 1f; //literal dash cooldown
    private float dash_upward_force = 14f; //vertical boost for an upward dash
    private bool can_wall_jump = true;
    public int frame_counter; //used to count frames to measure seconds  
    public Queue<PlayerStatus> recall_statuses = new Queue<PlayerStatus>();
    private int recall_length = 2; //determines the number of seconds to store statuses (n - 1)
    public Timer recall_cooldown; //cooldown timer for recall ability
    public float recall_cooldown_value = 5f; //recall ability cooldown for player
    private float recall_animation_length = 2f; //length of recall animation
    public Light2D recall_light; //light node for visual effects of recall
    public Timer recall_duration; //duration of recall animation
    private bool recall_started = false;
    public int health = 100; //actual health value
    public Timer quick_attack_duration; //timer to keep track of quick attack duration
    public Timer heavy_attack_duration; //timer to keep track of heavy attack duration
    public Timer quick_attack_cooldown; //cooldown timer for quick attack (prevents spam)
    public Timer heavy_attack_cooldown; //cooldown timer for heavy attack
    public int quick_attack_damage = 25;
    public int heavy_attack_damage = 40;
    //heart sprites for the hud
    public AnimatedSprite heart1; 
    public AnimatedSprite heart2;
    public AnimatedSprite heart3;
    public bool is_dead = false; //flag for character death
    public Timer take_damage_timer; //timer to prevent player from taking damage every frame
    public Light2D light;
    private Vector2 target_position; //position vector for re-locating the player
    private double distance = 1; //distant to target position
    private CollisionShape2D player_collision_box;
    private const bool RIGHT = false; //definition for facing right
    private const bool LEFT = true; //definition for facing left
    


    // Called when the node enters the scene tree for the first time.
    public override void _Ready() {
        // Get player sprite
        _animatedSprite = GetNode<AnimatedSprite>("AnimatedSprite");

        // Get player collision box
        player_collision_box = GetNode<CollisionShape2D>("PlayerBox");

        // Get the groundray (used to detect if the player is on the ground) 
        groundray = GetNode<RayCast2D>("GroundRay"); 

        // Get left/right rays (to detect if player is colliding on the left or right)
        leftray = GetNode<RayCast2D>("LeftRay"); 
        rightray = GetNode<RayCast2D>("RightRay"); 

        // Dash duration timer
        dash_timer = GetNode<Timer>("Abilities/DashDuration");

        // Dash cooldown timer
        dash_cooldown = GetNode<Timer>("Abilities/DashCooldown");
        dash_cooldown.WaitTime = dash_cooldown_value; //set cooldown for dash
        dash_cooldown.OneShot = true; 

        // Recall duration timer
        recall_duration = GetNode<Timer>("Abilities/RecallDuration");
        recall_duration.WaitTime = recall_animation_length;
        recall_duration.OneShot = true;

        // Recall cooldown
        recall_cooldown = GetNode<Timer>("Abilities/RecallCooldown");
        recall_cooldown.WaitTime = recall_cooldown_value; //cooldown for recall
        recall_cooldown.OneShot = true;
        
        // Recall External Nodes for visual effects
        recall_light = GetNode<Light2D>("Abilities/Recall_Light"); 

        // Health bar hearts
        HUD hud = (HUD)this.GetParent().GetNode("HUD"); //(HUD)GetNode("HUD");
        
        Panel health_bar = hud.GetChild<Panel>(1);
        heart1 = health_bar.GetChild<AnimatedSprite>(0);
        heart2 = health_bar.GetChild<AnimatedSprite>(1);
        heart3 = health_bar.GetChild<AnimatedSprite>(2);

        // Attack animation timers
        quick_attack_duration = GetNode<Timer>("Attack/QuickAttackDuration");
        heavy_attack_duration = GetNode<Timer>("Attack/HeavyAttackDuration");

        // Attack cooldown timers
        quick_attack_cooldown = GetNode<Timer>("Attack/QuickAttackCooldown");
        heavy_attack_cooldown = GetNode<Timer>("Attack/HeavyAttackCooldown");

        // Received Damage Timer
        take_damage_timer = GetNode<Timer>("Attack/TakeDamageTimer");

        // Player hitbox
        hitbox = GetNode<Hitbox>("Hitbox");
        hitbox_collision_obj = hitbox.GetChild<CollisionShape2D>(0);
        hitbox_collision_obj.Disabled = true;
        

        // Player hurtbox
        hurtbox = GetNode<Hurtbox>("Hurtbox");
        hurtbox_collision_obj = hurtbox.GetChild<CollisionShape2D>(0);

        light = GetNode<Light2D>("Light2D");
        
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(float delta) {
        
    }

    public override void _PhysicsProcess(float delta) {

        // Check Wall jump/sliding status 
        CheckWall(delta);
        
        // Get user input
        GetInput(delta);
        
        // adjust character animations
        CheckAnimations(delta);
        
        // check Recall ability 
        ProcessRecall();

        // check health
        UpdateHealth();
        
        if (hitbox_collision_obj.Disabled) {light.Enabled = false;}
        else {GD.Print("hitbox enable");}
    }

    public void GetInput(float delta) {
        //relative speed refers to the character speed in this frame.
        //speed is a constant for default walking speed, but relative can
        //change depending on if the character is dashing/rolling
        int relative_speed = speed; 
        
        //check if recall is active
        if (CheckRecallActive()) {return;}

        recall_started = false; //reset recall flag because recall is over
        player_collision_box.Disabled = false; //enable player collision box
        hurtbox_collision_obj.Disabled = false; //enable player hurtbox

        //WALKING LOGIC--------------------------   
        if (Input.IsActionPressed("right")) {velocity.x += 1;}
        else if (Input.IsActionPressed("left")) { velocity.x -= 1;}
        
        //JUMP LOGIC------------------------------
        if (Input.IsActionJustPressed("jump")) {
            if (groundray.IsColliding()) { //if player is on the ground
                velocity.y = -jump_power; // negative y-values move player up
            }
            else if (IsOnWall() && can_wall_jump) { //if player is on a wall
                velocity.y = -jump_power;
                can_wall_jump = false; //only allow 1 jump once the player hits the wall
            }
        }
        
        if (!IsOnWall()) {can_wall_jump = true;} //reset wall-jump flag when player leaves wall

        // DASH LOGIC-----------------------------
        // if dash is activated, set flag and start timer
        if (Input.IsActionJustPressed("dash") && dash_cooldown.IsStopped()) {
            dash_timer.Start(); //start dash duration timer (how long dash lasts)
            dash_cooldown.Start(); //start dash cooldown
        }
        if (!dash_timer.IsStopped()) { //while dashing is activated
            relative_speed = dash_speed;
            if (Input.IsActionPressed("up")) {velocity.y -= dash_upward_force;} //apply upward momentum if moving up
        }
        
        /*
        ---RECALL LOGIC---------------------------
        1) check if recall key is pressed
        2) ensure there's at least 1 recall point in the queue
        3) ensure recall cooldown is not active  
        */
        if (Input.IsActionJustPressed("recall") && recall_statuses.Count >= 1
            && recall_cooldown.IsStopped()) {
            recall_cooldown.Start(); //begin ability cooldown timer
            recall_duration.Start(); //begin animation timer
            PlayerStatus status = recall_statuses.Peek(); //get oldest status
            target_position = status.position; //save position from status to target position 
            health = status.health; //set player health to what it was in that status
        }

        /*
        ---Quick/Heavy Attack Logic----------------------
        */
        hitbox_collision_obj.Disabled = true; //disable hitbox by default
        if (quick_attack_cooldown.IsStopped() && heavy_attack_cooldown.IsStopped()) {

            //Check if Quick attack is activated
            if (Input.IsActionJustPressed("quick_attack")) { 
                hitbox_collision_obj.Disabled = false; //enable hitbox
                hitbox.setDamage(quick_attack_damage); //set damage to preset quick attack value 
                quick_attack_cooldown.Start(); //begin cooldown for quick attack
                quick_attack_duration.Start(); //timer represents the duration of the attack
            }
            //Check if Heavy attack is activated (same logic as above)
            else if (Input.IsActionJustPressed("heavy_attack")) { 
                hitbox_collision_obj.Disabled = false;
                hitbox.setDamage(heavy_attack_damage);
                heavy_attack_cooldown.Start();
                heavy_attack_duration.Start();
            }
        }
        
        //adjust horizontal speed
        velocity.x *= relative_speed;
        // Move character
        velocity = MoveAndSlide(velocity);
        velocity.x = 0; //stop movement when finished  
    }

    public void CheckAnimations(float delta) {
        //priority of animations goes TOP DOWN (e.g., recall is highest, then attacking, etc.)


        //check if Recall was just recently activated (recall_duration is started by GetInput()) 
        if (Input.IsActionJustPressed("recall") && !recall_duration.IsStopped()) {
            _animatedSprite.Play("recall");
            recall_light.Enabled = true;
        }

        //check if Recall animation is active 
        if (!recall_duration.IsStopped()) {return;} //play no other animations while recall is active
        else {recall_light.Enabled = false;} //disable Light effect if recall is over

        // Attack animation checks (duration timer is started by GetInput())
        if (Input.IsActionJustPressed("quick_attack") && !quick_attack_duration.IsStopped()) {
            _animatedSprite.Play("quick_attack");
            return; //if attack animation is active, refrain from movement animations            

        } 
        if (Input.IsActionJustPressed("heavy_attack") && !heavy_attack_duration.IsStopped()) {
            _animatedSprite.Play("heavy_attack");
            return;
        }

        //player is moving to the right
        if (Input.IsActionPressed("right")) { FlipPlayer(RIGHT);}
        //player is moving to the left
        else if (Input.IsActionPressed("left")) {FlipPlayer(LEFT);}

        //if player is not on the ground, play jumping animation
        if (!groundray.IsColliding()) {
            //check if player is on a wall to their left
            if (leftray.IsColliding()) {
                _animatedSprite.Stop();
                _animatedSprite.Animation = "wall_sliding";
                _animatedSprite.Frame = 0;
                FlipPlayer(LEFT);
            } 
            //check if player is on a wall to their left
            else if (rightray.IsColliding()) {
                _animatedSprite.Stop();
                _animatedSprite.Animation = "wall_sliding";
                _animatedSprite.Frame = 1;
                FlipPlayer(RIGHT);
            } else { //else, player is not on the ground and not on a wall: play Jump
                _animatedSprite.Play("jump");
            }
            if (!dash_timer.IsStopped()) { //if dash is active, play dash
                _animatedSprite.Play("dash");
            }
        } else { //player is on the ground
            
            
            //check direction again to see if running animation needs to be played
            //player is moving to the right
            if (Input.IsActionPressed("right")) {
                _animatedSprite.Play("run");
            }
            //player is moving to the left
            else if (Input.IsActionPressed("left")) {
                _animatedSprite.Play("run");
            }
            //no movement is happening, play idle animation
            else {
                _animatedSprite.Play("idle");
            }
        }


    }


    /**
        Adjusts the queue of player position vectors in the past 2 seconds.
        When the Recall ability is activated, it will use the vector
        at the head of the queue and move to that location. 
    */
    public void ProcessRecall() {
        frame_counter++;
        if (frame_counter == 60) { //activate on the 60th (1 second) frame
            frame_counter = 0; //reset counter
            //save current info about player (position, health)
            PlayerStatus new_status;
            new_status.position = this.GlobalPosition;
            new_status.health = health;

            if (recall_statuses.Count >= recall_length) {recall_statuses.Dequeue();} //remove oldest status
            recall_statuses.Enqueue(new_status); //add latest status
        }
    }

    /**
    Checks state of Recall ability and updates player position if active 
    @return bool : true if Recall active, false if Recall inactive
    */
    private bool CheckRecallActive() {
        /* During recall animation:
        1) Calculate distance from current position to target position
        2) Use the distance to scale speed toward target (farther = faster, closer = slower) 
        1) MoveAndSlide the player on the X axis from current position to previous position
        2) MoveAndSlide the player on the Y axis from current position to previous position
        3) Disable Player collision box & hurtbox so that it can move through objects & doesn't take damage
        */
        if (!recall_duration.IsStopped()) {
            //reset distance value if it wasn't already reset and this is the first iteration of recalling
            if (distance > 0 && !recall_started) {distance = 0;}

            //calculate distance to target (distance function = sqrt((x2-x1)^2 + (y2-y1)^2)
            double current_distance = Math.Sqrt(Math.Pow(target_position.x - this.Position.x, 2) + Math.Pow(target_position.y - this.Position.y, 2));
            //if recall hasn't begun, use current distance because it is essentialy the initial distance to the target
            //if recall has begun, use distance, which will be the value of the first current_distance (initial distance)
            distance = recall_started ? distance : current_distance; 
            //convert to a percentage (e.g. if curr_dist=14 & dist=20, progress=0.3 or 30% because the goal is distance of 0 to target)
            double progress_to_target = 1 - (current_distance / distance);
            //need to move 3x faster than default speed and subtract so that early in recall state = higher speed, later = slower
            //e.g. if progress is 0.3, then speed = 3*speed - (3*speed*0.3) = 3*speed - speed = 2*speed
            //     if progress is 0.8, then speed = 3*speed - (3*speed*0.8) = 3*speed - 2.4*speed = 0.6*speed 
            int relative_speed = Convert.ToInt32(3*speed - (3*speed * progress_to_target));

            //Calculate velocity needed to get player to new location
            velocity.x = target_position.x - this.Position.x > 0 ? relative_speed : relative_speed*-1;
            velocity.y = target_position.y - this.Position.y > 0 ? relative_speed : relative_speed*-1;
            
            //Keep collision box and hurtbox disabled so player can go through walls if needed
            // and they won't take damage from enemies.
            player_collision_box.Disabled = true;
            hurtbox_collision_obj.Disabled = true;
            recall_started = true; //set recall flag to true so that distance will not use current_distance

            // Move character
            velocity = MoveAndSlide(velocity); 
            velocity.x = 0;
            return true; //recall active
        }

        return false; //recall inactive
    }

    public void CheckWall(float delta) {
        //if on wall and moving toward the wall
        if (IsOnWall() && (Input.IsActionPressed("right")||Input.IsActionPressed("left"))) {
            if (velocity.y >= 0) { //if moving down, slow the descent
                velocity.y = Math.Min(velocity.y + wall_slide_animation, max_wall_slide_speed);
            } else {
                // Apply normal gravity when not moving toward wall
                velocity.y += gravity*mass;
            }
        } else {velocity.y += gravity*mass;} //? idk y
    }

    /**
    Helper function to update the visual health bar based on health value
    */
    public void UpdateHealth() {
        //need to check each heart state so that when player regains health,
        // all hearts are updated.

        if (health < 68) {heart3.Frame = 2;}      // Heart 3 = empty
        else if (health < 84) {heart3.Frame = 1;} // Heart 3 = half
        else {heart3.Frame = 0;}                  // Heart 3 = full
        if (health < 37) {heart2.Frame = 2;}      // Heart 2 = empty
        else if (health < 52) {heart2.Frame = 1;} // Heart 2 = half
        else {heart2.Frame = 0;}                  // Heart 2 = full
        if (health <= 0) {heart1.Frame = 2;}      // Heart 1 = empty
        else if (health < 20) {heart1.Frame = 1;} // Heart 1 = half
        else {heart1.Frame = 0;}                  // Heart 1 = full
    }

    /**
    Flips player sprite and moves hitbox to the side the player is facing
    */
    private void FlipPlayer(bool flipToLeft) {
        //if we are flipping to left, the position of the hitbox needs to be moved to the left side
        if (flipToLeft) {hitbox.Position = new Vector2(-12, 4);} 
        else            {hitbox.Position = new Vector2(6, 4);} //else the player is flipping right, so move to the right

        _animatedSprite.FlipH = flipToLeft; //flip player sprite: false = face right, true = face left
    }

    private void _on_Hurtbox_area_entered(Area2D area) {
        CollisionShape2D col = area.GetChild<CollisionShape2D>(0);
        GD.Print(col.Name);
        // EXIT IF:
        // 1) if not a hitbox (in which case area is a hurtbox)
        // 2) if incoming hitbox is not enabled
        if (!(area is Hitbox) || col.Disabled) {return;} 
        
        //Convert to hitbox type to get damage data
        Hitbox incoming_hitbox = (Hitbox) area;
        int incoming_damage = incoming_hitbox.getDamage();
        GD.Print(col.Name + " deals " + incoming_damage.ToString());
        
        health -= incoming_damage;
        UpdateHealth(); //update health on HUD 
        GD.Print("player hit " + incoming_damage.ToString() + " (" + health.ToString() + " HP)");    
        

        //check if character died
        if (health <= 0 && !is_dead) {
            
        } else { //character not dead, begin cooldown timer
            //start cooldown timer for taking damage (player is immune for this duration)
            take_damage_timer.Start();
        }
    }

    private void Die() {
        is_dead = true;
        // QueueFree();
        GD.Print("player dead");
    }

}
