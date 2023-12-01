using Godot;
using System;
using System.Collections.Generic;


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
    [Export] public bool right = true;
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
<<<<<<< Updated upstream:Player.cs

    private Timer dash_timer; //for adjusting dash duration
    private Timer dash_cooldown; //for adjusting dash duration
    private float dash_cooldown_value = 1f; //literal dash cooldown
=======
    private Hitbox hitbox;
    private CollisionShape2D hitbox_collision_obj;
    private Hurtbox hurtbox;

    private Timer dash_timer; //for adjusting dash duration
    public Timer dash_cooldown; //cooldown timer for dash ability
    public float dash_cooldown_value = 1f; //literal dash cooldown
    private float dash_upward_force = 14f; //vertical boost for an upward dash
>>>>>>> Stashed changes:Final Project/Player.cs
    private bool can_wall_jump = true;
    public int frame_counter; //used to count frames to measure seconds  
    public Queue<PlayerStatus> recall_statuses = new Queue<PlayerStatus>();
    private int recall_length = 2; //determines the number of seconds to store statuses (n - 1)
<<<<<<< Updated upstream:Player.cs
    private Timer recall_cooldown; //cooldown for recall ability
    private float recall_cooldown_value = 5f; //recall ability cooldown for player
    private float recall_animation = 3f; 
    public int health = 100; //actual health value
<<<<<<< Updated upstream:Player.cs
    public int health_tick = 60; //delete this
    private Timer quick_attack_timer;
    private Timer heavy_attack_timer;
=======
    private Timer quick_attack_timer; //cooldown timer for quick attack (prevents spam)
    private Timer heavy_attack_timer; //cooldown timer for heavy attack
>>>>>>> Stashed changes:Final Project/Player.cs
=======
    public Timer recall_cooldown; //cooldown timer for recall ability
    public float recall_cooldown_value = 5f; //recall ability cooldown for player
    private float recall_animation = 3f; 
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
>>>>>>> Stashed changes:Final Project/Player.cs

    // Called when the node enters the scene tree for the first time.
    public override void _Ready() {
        // Get player sprite
        _animatedSprite = GetNode<AnimatedSprite>("AnimatedSprite");

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

<<<<<<< Updated upstream:Player.cs
        // Health bar
        Camera2D cam = GetNode<Camera2D>("Camera2D");
        health_label = cam.GetChild<Label>(0);
=======
        // Recall cooldown
        recall_cooldown = GetNode<Timer>("Abilities/RecallCooldown");
        recall_cooldown.WaitTime = recall_cooldown_value; //cooldown for recall
        recall_cooldown.OneShot = true;

        // Health bar hearts
        HUD hud = (HUD)GetNode("/root/HUD");
        Panel health_bar = hud.GetChild<Panel>(1);
        heart1 = health_bar.GetChild<AnimatedSprite>(0);
        heart2 = health_bar.GetChild<AnimatedSprite>(1);
        heart3 = health_bar.GetChild<AnimatedSprite>(2);
>>>>>>> Stashed changes:Final Project/Player.cs

        // Attack animation timers
        quick_attack_duration = GetNode<Timer>("Attack/QuickAttackDuration");
        heavy_attack_duration = GetNode<Timer>("Attack/HeavyAttackDuration");

<<<<<<< Updated upstream:Player.cs
        // Recall cooldown
        recall_cooldown = GetNode<Timer>("RecallCooldown");
        recall_cooldown.WaitTime = recall_cooldown_value; //cooldown for recall
        recall_cooldown.OneShot = true;
=======
        // Attack cooldown timers
        quick_attack_cooldown = GetNode<Timer>("Attack/QuickAttackCooldown");
        heavy_attack_cooldown = GetNode<Timer>("Attack/HeavyAttackCooldown");

        // Received Damage Timer
        take_damage_timer = GetNode<Timer>("Attack/TakeDamageTimer");

        // Player hitbox
        hitbox = GetNode<Hitbox>("Hitbox");
        hitbox_collision_obj = hitbox.GetChild<CollisionShape2D>(0);

        
>>>>>>> Stashed changes:Final Project/Player.cs
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

        health_tick--;
        if (health_tick == 0) {
            health_tick = 60;
            health--;
            health_label.Text = health.ToString();
        }

        

    }

    public void GetInput(float delta) {
        //relative speed refers to the character speed in this frame.
        //speed is a constant for default walking speed, but relative can
        //change depending on if the character is dashing/rolling
        int relative_speed = speed; 

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
            if (Input.IsActionPressed("up")) {velocity.y -= 10;} //apply upward momentum if moving up
        }
        
        /*
        ---RECALL LOGIC---------------------------
        1) check if recall key is pressed
        2) ensure there's at least 1 recall point in the queue
        3) ensure recall cooldown is not active  
        */
        if (Input.IsActionJustPressed("recall") && recall_statuses.Count >= 1
            && recall_cooldown.IsStopped()) {
            recall_cooldown.Start();
            PlayerStatus status = recall_statuses.Peek(); //get oldest status
            this.Position = status.position; //set player position to the position in that status
            health = status.health; //set player health to what it was in that status
            //update health visual
        }

<<<<<<< Updated upstream:Player.cs
=======
        /*
        ---Quick Attack Logic----------------------
        */
        if (quick_attack_cooldown.IsStopped()) {
            //while the player is not attacking, disable hitbox
            hitbox_collision_obj.Disabled = true; 
            if (Input.IsActionJustPressed("quick_attack")) { 
                hitbox_collision_obj.Disabled = false;
                hitbox.setDamage(quick_attack_damage);
                hitbox.SetAttackFromVector(this.GlobalPosition);
                quick_attack_cooldown.Start();
            }
        }

        /*
        ---Heavy Attack Logic----------------------
        */
        if (heavy_attack_cooldown.IsStopped()) {
            //while the player is not attacking, disable hitbox
            hitbox_collision_obj.Disabled = true; 
            if (Input.IsActionJustPressed("heavy_attack")) { 
                hitbox_collision_obj.Disabled = false;
                hitbox.setDamage(heavy_attack_damage);
                hitbox.SetAttackFromVector(this.GlobalPosition);
                heavy_attack_cooldown.Start();
            }
        }
        
>>>>>>> Stashed changes:Final Project/Player.cs
        //adjust horizontal speed
        velocity.x *= relative_speed;
        // Move character
        velocity = MoveAndSlide(velocity);
        velocity.x = 0; //stop movement when finished  
    }

    public void CheckAnimations(float delta) {
        //player is moving to the right
        if (Input.IsActionPressed("right")) { _animatedSprite.FlipH = false;}
        //player is moving to the left
        else if (Input.IsActionPressed("left")) {_animatedSprite.FlipH = true;}

        //if attack animation is running, refrain from movement animations
        if (quick_attack_duration.IsStopped() && heavy_attack_duration.IsStopped()) {
            //if player is not on the ground, play jumping animation
            if (!groundray.IsColliding()) {
                //check if player is on a wall
                if (leftray.IsColliding() || rightray.IsColliding()) {
                    _animatedSprite.Stop(); //stop all animations
                    _animatedSprite.Animation = "jump"; //set animation type
                    _animatedSprite.Frame = 1; //set frame
                    // GD.Print("on wall " + Convert.ToString(Time.GetTicksMsec()));
                } else {
                    _animatedSprite.Play("jump");
                    // GD.Print("jumping " + Convert.ToString(Time.GetTicksMsec()));
                }
                if (!dash_timer.IsStopped()) {
                    _animatedSprite.Animation = "run";
                    _animatedSprite.Frame = 2;
<<<<<<< Updated upstream:Player.cs
                    switch (Input.IsActionPressed("left")) {
                        case true: _animatedSprite.Rotate((float)(Math.PI/6.2*-1)); break;
                        case false: _animatedSprite.Rotate((float)(Math.PI/6.2)); break;
                    }
=======
                    // Fliping mechanics
                    // switch (Input.IsActionPressed("left")) {
                    //     case true: _animatedSprite.Rotate((float)(Math.PI/6.2*-1)); break;
                    //     case false: _animatedSprite.Rotate((float)(Math.PI/6.2)); break;
                    // }
>>>>>>> Stashed changes:Final Project/Player.cs
                }
            } else { //player is on the ground
                
                //check direction again to see if running animation needs to be played
                //player is moving to the right
                if (Input.IsActionPressed("right")) {
                    // GD.Print("moving right " + Convert.ToString(Time.GetTicksMsec()));
                    _animatedSprite.FlipH = false;
                    // _animatedSprite.RotationDegrees = 10; //add slight tilt to the right
                    _animatedSprite.Play("run");
                }
                //player is moving to the left
                else if (Input.IsActionPressed("left")) {
                    // GD.Print("moving left " + Convert.ToString(Time.GetTicksMsec()));
                    _animatedSprite.FlipH = true;
                    // _animatedSprite.RotationDegrees = -10; //add slight tilt to the left
                    _animatedSprite.Play("run");
                }
                //no movement is happening, play idle animation
                else {
                    // GD.Print("idling " + Convert.ToString(Time.GetTicksMsec()));
                    _animatedSprite.Play("idle");
                }
            }
<<<<<<< Updated upstream:Player.cs
        } 
=======

            // Attack animation checks
            if (Input.IsActionJustPressed("quick_attack") && quick_attack_cooldown.IsStopped()) {
                hitbox_collision_obj.Disabled = false; //enable the hitbox
                quick_attack_duration.Start(); //timer represents the duration of the attack
                _animatedSprite.Play("quick_attack");
            } else if (Input.IsActionJustPressed("heavy_attack") && heavy_attack_cooldown.IsStopped()) {
                hitbox_collision_obj.Disabled = false; //enable the hitbox
                heavy_attack_duration.Start(); //timer represents the duration of the attack
                _animatedSprite.Play("heavy_attack");
            }

        }//end if quick/heavy_attack_timer.IsStopped()
        
>>>>>>> Stashed changes:Final Project/Player.cs
        
        
        if (Input.IsActionJustPressed("recall")) {

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

<<<<<<< Updated upstream:Player.cs
=======
    /**
    Helper function to update the visual health bar based on health value
    */
    public void UpdateHealth() {
        if (health < 68) {heart3.Frame = 2;}
        else if (health < 84) {heart3.Frame = 1;} 
        if (health < 37) {heart2.Frame = 2;}
        else if (health < 52) {heart2.Frame = 1;} 
        if (health <= 0) {heart1.Frame = 2;}
        else if (health < 20) {heart1.Frame = 1;}  
         
    }


    public void TakeDamage(int damage, Vector2? attackFromVector) {
        //if the cooldown timer for getting attacked has stopped 
        if (take_damage_timer.IsStopped()) {
            health -= damage;
            UpdateHealth();
        }
        
        //check if player died
        if (health <= 0 && !is_dead) {
            is_dead = true;
            GD.Print("player dead");
        } else {
            if (attackFromVector != null) {
                //start cooldown timer for taking damage (player is immune for this duration)
                take_damage_timer.Start();
            }
        }
    }

    public void Die()
    {
        GameManager.RespawnPlayer();
    }
>>>>>>> Stashed changes:Final Project/Player.cs
}
