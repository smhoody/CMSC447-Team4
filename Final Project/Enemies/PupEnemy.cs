using Godot;
using System;

public class PupEnemy : KinematicBody2D, TakeDamage
{
    // Exported variables
    [Export] public float gravity = 9.81f;
    [Export] public float mass = 1.5f;
    [Export] public float jump_power = 300f;
    [Export] public float speed = 200f;
    [Export] public float attack_speed = 500f;
    [Export] public float follow_distance = 500f;

    private int moveDirection = -1;
    private Timer attack_cooldown;
    private Timer attack_timer;
    private float attack_cooldown_value = 1f;
    
    public Vector2 velocity = new Vector2();
    public Vector2 player_position = new Vector2();
    public Vector2 target_position = new Vector2();
    private AnimatedSprite _animatedSprite;
    private RayCast2D castDownRight;
    private RayCast2D castDownLeft;
    private RayCast2D castLookAhead;
    private KinematicBody2D player;
    public int health = 100;
    public bool is_dead = false;
    public Timer take_damage_timer;
    public override void _Ready()
    {
        // Both used to check floor collisions
        castDownLeft = GetNode<RayCast2D>("DownLeftRay");
        castDownRight = GetNode<RayCast2D>("DownRightRay");

        // Used to check wall/player collisions
        castLookAhead = GetNode<RayCast2D>("LookRay"); 

        // Player sprite
        _animatedSprite = GetNode<AnimatedSprite>("AnimatedSprite");

        // Used to obtain the position of the Player
        player = GetParent().GetNode<KinematicBody2D>("Player");

        // timer for attack duration
    
        attack_timer = GetNode<Timer>("AttackDuration");

        // timer for attack cooldown
        attack_cooldown = GetNode<Timer>("AttackCooldown");
        attack_cooldown.WaitTime = attack_cooldown_value;
        attack_cooldown.OneShot = true;

        //Received Damage Timer
        take_damage_timer = GetNode<Timer>("TakeDamageTimer");

    }


    public override void _Process(float delta)
    {
         if(ProcessTurn())
        {
            moveDirection *= -1;
        }
        castLookAhead.CastTo = new Vector2(10*moveDirection, 0);
    }
    public override void _PhysicsProcess(float delta)
    {
        ApplyGravity(delta);
        HandleMovement(delta);
        CheckAnimations(delta);
    }

    public void ApplyGravity(float delta)
    {
        velocity.y += mass*gravity;
    }

    public void HandleMovement(float delta)
    {
        float relative_speed = speed;

        player_position = player.Position;
        target_position.x = player_position.x - this.Position.x;
        
        velocity.x = target_position.x > 0 ? speed : speed*-1;
        if(!castDownLeft.IsColliding() && target_position.x < 0)
        {
            velocity.x = 0;
        }
        else if(!castDownRight.IsColliding() && target_position.x > 0)
        {
            velocity.x = 0;
        }
        if(Position.DistanceTo(player_position) < follow_distance)
        {
            /*
            if(attack_cooldown.IsStopped())
            {
                attack_timer.Start();
                attack_cooldown.Start();
            }
            if(!attack_cooldown.IsStopped())
            {
                relative_speed = attack_speed;

            }
            */
            //velocity.x *= relative_speed;
            velocity = MoveAndSlide(velocity);
        }
        else
        {

        }
    }
    
    private bool ProcessTurn()
    {
        if(castLookAhead.IsColliding())
        {
            return true;
        }
        if(moveDirection == -1)
        {
            return (!castDownLeft.IsColliding());
        }
        else if(moveDirection == 1)
        {
            return (!castDownRight.IsColliding());
        }
        return false;
    }

    public void CheckAnimations(float delta)
    {
        if(Position.DistanceTo(player_position) < follow_distance)
        {
            _animatedSprite.Play("run");
        }
        else
        {
            _animatedSprite.Play("idle");
        }

        if(player_position.x > Position.x)
        {
            _animatedSprite.FlipH = true;
        }
        else if(player_position.x < Position.x)
        {
            _animatedSprite.FlipH = false;
        }
    }

    private void _on_Hitbox_area_entered(Area2D area)
    {
        if(area.GetParent() is Player)
        {
            
        }
    }

    //required function for TakeDamage interface
    public void TakeDamage(int damage, Vector2? attackFromVector) {
        GD.Print("pup hit " + damage.ToString());
        //if the cooldown timer for getting attacked has stopped 
        if (take_damage_timer.IsStopped()) {health -= damage;}
        
        //check if character died
        if (health <= 0 && !is_dead) {
            is_dead = true;
            GD.Print("pup dead");
        } else { //character not dead, begin cooldown timer
            if (attackFromVector != null) {
                //start cooldown timer for taking damage (player is immune for this duration)
                take_damage_timer.Start();
            }
        }
    }
}
