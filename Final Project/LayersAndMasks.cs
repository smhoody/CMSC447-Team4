using Godot;
using System;

public class LayersAndMasks : Node
{
    public uint GetCollisionLayerByName(string name) {
        //loop through all 2D Physics Layers in Project Settings
        for (uint i = 1; i <= 32; i++) {
            var layer = ProjectSettings.GetSetting("layer_names/2d_physics/layer_" + i).ToString();
            //return the index of the layer that matches the name
            if (layer.Equals(name)) {return i;} 
        }

        GD.Print("Could not find the " + name + " collision layer.");
        return 0;
    }
    
}
