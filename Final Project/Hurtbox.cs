/**
Hurtbox class for nodes that deal damage 

Godot steps:
1. Go to the scene of the object that deals damage (e.g. an enemy)
2. Right click the root node
3. Add the Hurtbox.tscn as an instance child scene
4. Right click the Hurtbox node -> Make Local
5. Select the CollisionShape2D node under the Hurtbox
6. Under the Inspector, adjust the size of the hurtbox in the transform setting


@Author Steven Hoodikoff
@Date 11/15/2023
*/
using Godot;
using System;

public class Hurtbox : Area2D {

    // Called when the node enters the scene tree for the first time.
    public override void _Ready() {
        Connect("area_entered", this, nameof(OnAreaEntered)); //when collision, use OnAreaEntered()
    }


    private void OnAreaEntered(Hitbox hitbox) {

    }
}
