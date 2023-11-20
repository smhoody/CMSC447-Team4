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

public class Player : KinematicBody2D, TakeDamage
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
    private Hitbox hitbox;
    private Hurtbox hurtbox;

    private Timer dash_timer; //for adjusting dash duration
    public Timer dash_cooldown; //for adjusting dash duration
    public float dash_cooldown_value = 1f; //literal dash cooldown
    private float dash_upward_force = 14f; //vertical boost for an upward dash
    private bool can_wall_jump = true;
    public int frame_counter; //used to count frames to measure seconds  
    public Queue<PlayerStatus> recall_statuses = new Queue<PlayerStatus>();
    private int recall_length = 2; //determines the number of seconds to store statuses (n - 1)
    public Timer recall_cooldown; //cooldown for recall ability
    public float recall_cooldown_value = 5f; //recall ability cooldown for player
    private float recall_animation = 3f; 
    public int health = 100; //actual health value
    public Timer quick_attack_timer; //cooldown timer for quick attack (prevents spam)
    public Timer heavy_attack_timer; //cooldown timer for heavy attack
    //heart sprites for the hud
    public AnimatedSprite heart1; 
    public AnimatedSprite heart2;
    public AnimatedSprite heart3;
    public bool is_dead;
    public Timer take_damage_timer;

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
        dash_timer = GetNode<Timer>("DashDuration");

        // Dash cooldown timer
        dash_cooldown = GetNode<Timer>("DashCooldown");
        dash_cooldown.WaitTime = dash_cooldown_value; //set cooldown for dash
        dash_cooldown.OneShot = true; 


        // Health bar hearts
        HUD hud = (HUD)GetNode("/root/HUD");
        Panel health_bar = hud.GetChild<Panel>(1);
        heart1 = health_bar.GetChild<AnimatedSprite>(0);
        heart2 = health_bar.GetChild<AnimatedSprite>(1);
        heart3 = health_bar.GetChild<AnimatedSprite>(2);

        // Attack timers
        quick_attack_timer = GetNode<Timer>("QuickAttackTimer");
        heavy_attack_timer = GetNode<Timer>("HeavyAttackTimer");

        // Recall cooldown
        recall_cooldown = GetNode<Timer>("RecallCooldown");
        recall_cooldown.WaitTime = recall_cooldown_value; //cooldown for recall
        recall_cooldown.OneShot = true;

        //Received Damage Timer
        take_damage_timer = GetNode<Timer>("TakeDamageTimer");

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
        

    }

    public void GetInput(float delta) {
        //relative speed refers to this the character speed in this frame.
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
            recall_cooldown.Start();
            PlayerStatus status = recall_statuses.Peek(); //get oldest status
            this.Position = status.position; //set player position to the position in that status
            health = status.health; //set player health to what it was in that status
            //update health visual
        }

        /*
        ---Quick Attack Logic----------------------
        */
        if (Input.IsActionJustPressed("quick_attack") && quick_attack_timer.IsStopped()) {
            hitbox.SetAttackFromVector(this.GlobalPosition);
        }

        //adjust horizontal speed
        velocity.x *= relative_speed;
        // Move character
        velocity = MoveAndSlide(velocity);
        velocity.x = 0; //stop movement when finished  
    }

    public void CheckAnimations(float delta) {

        //if attack animation is running, refrain from movement animations
        if (quick_attack_timer.IsStopped() && heavy_attack_timer.IsStopped()) {
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
                    // Flipping mechanics
                    // switch (Input.IsActionPressed("left")) {
                    //     case true: _animatedSprite.Rotate((float)(Math.PI/6.2*-1)); break;
                    //     case false: _animatedSprite.Rotate((float)(Math.PI/6.2)); break;
                    // }
                }
            } else { //player is on the ground
                //reset possible rotations from jumping 
                _animatedSprite.RotationDegrees = 0;
                //player is moving to the right
                if (Input.IsActionPressed("right")) {
                    // GD.Print("moving right " + Convert.ToString(Time.GetTicksMsec()));
                    _animatedSprite.FlipH = false;
                    _animatedSprite.Play("run");
                }
                //player is moving to the left
                else if (Input.IsActionPressed("left")) {
                    // GD.Print("moving left " + Convert.ToString(Time.GetTicksMsec()));
                    _animatedSprite.FlipH = true;
                    _animatedSprite.Play("run");
                }
                //no movement is happening, play idle animation
                else {
                    // GD.Print("idling " + Convert.ToString(Time.GetTicksMsec()));
                    _animatedSprite.Play("idle");
                }
            }
        }//end if quick_attack_timer.IsStopped()
        
        // Attack animation checks
        if (Input.IsActionJustPressed("quick_attack")) {
            quick_attack_timer.Start(); //timer represents the duration of the attack
            _animatedSprite.Play("quick_attack");
        } else if (Input.IsActionJustPressed("heavy_attack")) {
            heavy_attack_timer.Start(); //timer represents the duration of the attack
            _animatedSprite.Play("heavy_attack");
        }
        
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

<<<<<<< Updated upstream
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
        health -= damage;
        UpdateHealth();
        
        //check if player died
        if (health <= 0 && !is_dead) {
            is_dead = true;
            GD.Print("dead");
        } else {
            if (attackFromVector != null) {
                //start cooldown timer for taking damage (player is immune for this duration)
                take_damage_timer.Start();
            }
        }
    }
}
=======
    public void Die()
    {
        GameManager.RespawnPlayer();
    }
}
>>>>>>> Stashed changes
