using Godot;
using System;

public class GameManager : Node
{
    public static RespawnPoint current_checkpoint;
    public static Player player;

    public static void RespawnPlayer()
    {
        if(current_checkpoint != null)
        {
            player.position = current_checkpoint.GlobalPosition;
        }
    }

    
}
