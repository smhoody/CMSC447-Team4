using Godot;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

public struct PlayerStatus {
    public Vector2 position;
    public int health;
}

public class Player : KinematicBody2D
{
    [Export] public bool right = true;
    [Export] public int speed = 300;
    [Export] public int dash_speed = 700;
    [Export] public float gravity = 9.81f;
    [Export] public float jump_power = 600f;
    [Export] public float mass = 2.5f;
    [Export] public float acceleration = 20f;

    // member variables here
    public Vector2 velocity = new Vector2();
    public Vector2 position = new Vector2();
    private AnimatedSprite _animatedSprite;
    private RayCast2D groundray;
    private Timer dash_timer;
    public int frame_counter; //used to count frames to measure seconds  
    public Queue<PlayerStatus> recall_statuses = new Queue<PlayerStatus>();
    public Label health_label; //visual label for health  
    public int health = 100; //actual health value
    public int health_tick = 60; //delete this

    // Called when the node enters the scene tree for the first time.
    public override void _Ready() {
        //Get player sprite
        _animatedSprite = GetNode<AnimatedSprite>("AnimatedSprite");

        // Get the player GroundRay (used to detect if the player is on the ground) 
        groundray = (RayCast2D) FindNode("GroundRay"); 
        groundray.CollideWithAreas = true; //enable collision
        groundray.Enabled = true; //turn ray on

        // Dash timer
        dash_timer = GetNode<Timer>("Timer");
        dash_timer.Connect("timeout",this,"OnTimerTimeout");

        // Health bar
        Camera2D cam = GetNode<Camera2D>("Camera2D");
        health_label = cam.GetChild<Label>(0);
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(float delta) {
        
    }

    public override void _PhysicsProcess(float delta) {
        // Apply gravity
        velocity.y += gravity*mass;
        
        // Get user input
        GetInput(delta);
        // adjust character animations
        CheckAnimations(delta);
        
        health_tick--;
        if (health_tick == 0) {
            health_tick = 60;
            health--;
            health_label.Text = health.ToString();
        }

        // check Recall ability 
        ProcessRecall();

    }

    public void GetInput(float delta) {
        //relative speed refers to this the character speed in this frame.
        //speed is a constant for default walking speed, but relative can
        //change depending on if the character is dashing/rolling
        int relative_speed = speed; 

        //WALKING LOGIC--------------------------   
        if (Input.IsActionPressed("right")) {
            _animatedSprite.FlipH = false;
            velocity.x += 1;
        }
        else if (Input.IsActionPressed("left")) { 
            _animatedSprite.FlipH = true;
            velocity.x -= 1;
        }
        
        //JUMP LOGIC------------------------------
        if (groundray.IsColliding()) { //if player is on the ground
            if (Input.IsActionJustPressed("jump")) {
                velocity.y -= jump_power; // negative y-values move player up
            }
        }

        // DASH LOGIC-----------------------------
        // if dash is activated, set flag and start timer
        if (Input.IsActionJustPressed("dash")) 
            dash_timer.Start();
        if (!dash_timer.IsStopped()) { //while dashing is activated
            relative_speed = dash_speed;
            if (Input.IsActionPressed("up")) {velocity.y -= 10;}
        }
        
        // RECALL LOGIC---------------------------
        if (Input.IsActionJustPressed("recall") && recall_statuses.Count >= 1) {
            PlayerStatus status = recall_statuses.Peek();
            this.Position = status.position; 
            health = status.health;
            health_label.Text = health.ToString();
        }

        //adjust horizontal speed
        velocity.x *= relative_speed;
        // Move character
        velocity = MoveAndSlide(velocity);
        velocity.x = 0; //stop movement when finished  
    }

    public void CheckAnimations(float delta) {
        //if player is not on the ground, play jumping animation
        if (!groundray.IsColliding()) {
            _animatedSprite.Play("jump");
        } else { //player is on the ground
            //player is moving to the right
            if (Input.IsActionPressed("right")) {
                _animatedSprite.Play("run");
            }
            //player is moving to the left
            else if (Input.IsActionPressed("left")) {
                _animatedSprite.Play("run");
            }
            //no movement is happening, stop animations and reset frame
            else {
                _animatedSprite.Stop();
                _animatedSprite.Frame = 0;
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
            GD.Print(new_status.health);
            if (recall_statuses.Count > 7) {recall_statuses.Dequeue();} //remove oldest status
            recall_statuses.Enqueue(new_status); //add latest status
        }
    }


    public void OnTimerTimeout() {

    }
}
