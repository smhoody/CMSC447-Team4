using Godot;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;


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

    private Timer dash_timer; //for adjusting dash duration
    private Timer dash_cooldown; //for adjusting dash duration
    private float dash_cooldown_value = 1f; //literal dash cooldown
    private bool can_wall_jump = true;
    public int frame_counter; //used to count frames to measure seconds  
    public Queue<PlayerStatus> recall_statuses = new Queue<PlayerStatus>();
    private int recall_length = 2; //determines the number of seconds to store statuses (n - 1)
    private Timer recall_cooldown; //cooldown for recall ability
    private float recall_cooldown_value = 5f; //recall ability cooldown for player
    private float recall_animation = 3f;
    public Label health_label; //visual label for health  
    public int health = 100; //actual health value
    public int health_tick = 60; //delete this
    private Timer quick_attack_timer;
    private Timer heavy_attack_timer;

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
        dash_cooldown.WaitTime = dash_cooldown_value; //cooldown for dash
        dash_cooldown.OneShot = true; 

        // Health bar
        Camera2D cam = GetNode<Camera2D>("Camera2D");
        health_label = cam.GetChild<Label>(0);

        // Attack timers
        quick_attack_timer = GetNode<Timer>("QuickAttackTimer");
        heavy_attack_timer = GetNode<Timer>("HeavyAttackTimer");

        // Recall cooldown
        recall_cooldown = GetNode<Timer>("RecallCooldown");
        recall_cooldown.WaitTime = recall_cooldown_value; //cooldown for recall
        recall_cooldown.OneShot = true;
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
            if (Input.IsActionPressed("up")) {velocity.y -= 10;} //apply upward momentum if moving up
        }
        
        // RECALL LOGIC---------------------------
        if (Input.IsActionJustPressed("recall") && recall_statuses.Count >= 1
            && recall_cooldown.IsStopped()) {
            recall_cooldown.Start();
            PlayerStatus status = recall_statuses.Peek(); //get oldest status
            this.Position = status.position; //set player position to the position in that status
            health = status.health; //set player health to what it was in that status
            health_label.Text = health.ToString(); //update health label
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
                    GD.Print("on wall " + Convert.ToString(Time.GetTicksMsec()));
                } else {
                    _animatedSprite.Play("jump");
                    GD.Print("jumping " + Convert.ToString(Time.GetTicksMsec()));
                    // if (Input.IsActionPressed("left")) {_animatedSprite.RotationDegrees = -7;}
                    // else if (Input.IsActionPressed("right")) {_animatedSprite.RotationDegrees = 7;}
                }
                if (!dash_timer.IsStopped()) {
                    _animatedSprite.Animation = "run";
                    _animatedSprite.Frame = 2;
                    switch (Input.IsActionPressed("left")) {
                        case true: _animatedSprite.Rotate((float)(Math.PI/6.2*-1)); break;
                        case false: _animatedSprite.Rotate((float)(Math.PI/6.2)); break;
                    }
                }
            } else { //player is on the ground
                //reset possible rotations from jumping 
                _animatedSprite.RotationDegrees = 0;
                //player is moving to the right
                if (Input.IsActionPressed("right")) {
                    GD.Print("moving right " + Convert.ToString(Time.GetTicksMsec()));
                    _animatedSprite.FlipH = false;
                    // _animatedSprite.RotationDegrees = 10; //add slight tilt to the right
                    _animatedSprite.Play("run");
                }
                //player is moving to the left
                else if (Input.IsActionPressed("left")) {
                    GD.Print("moving left " + Convert.ToString(Time.GetTicksMsec()));
                    _animatedSprite.FlipH = true;
                    // _animatedSprite.RotationDegrees = -10; //add slight tilt to the left
                    _animatedSprite.Play("run");
                }
                //no movement is happening, play idle animation
                else {
                    GD.Print("idling " + Convert.ToString(Time.GetTicksMsec()));
                    _animatedSprite.Play("idle");
                }
            }
        } 
        
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
        if (IsOnWall() && (Input.IsActionPressed("right")||Input.IsActionPressed("left"))) {
            if (velocity.y >= 0) { //if moving down, slow the descent
                velocity.y = Math.Min(velocity.y + wall_slide_animation, max_wall_slide_speed);
            } else {
                // Apply gravity
                velocity.y += gravity*mass;
            }
        } else {velocity.y += gravity*mass;}
    }
}
