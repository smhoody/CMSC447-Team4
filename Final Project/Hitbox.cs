using Godot;
using System;

public class Hitbox : Area2D
{
    [Export]
    public int damage = 10; //damage the hitbox will deliver
    public Vector2? AttackFromVector = null; //vector the attack came from


    // Called when the node enters the scene tree for the first time.
    public override void _Ready() {

    }

//  // Called every frame. 'delta' is the elapsed time since the previous frame.
//  public override void _Process(float delta)
//  {
//      
//  }

    public void setDamage(int new_damage) {
        this.damage = new_damage;
    }
    public int getDamage() {
        return this.damage;
    }
    public void SetAttackFromVector(Vector2 attackVector) {
        this.AttackFromVector = attackVector;
    }

}
