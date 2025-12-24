using Godot;
using System;

public class GameManager : Node
{
    public static GameManager Instance;

    readonly PackedScene playerScene = GD.Load<PackedScene>("res://scenes/Player.tscn");
    readonly PackedScene HUDScene = GD.Load<PackedScene>("res://scenes/HUD.tscn");
    private ParallaxBackground sceneA, sceneB;
    private Node2D sceneAEnemies;

    public Player player;
  
    private HUD HUD;
    

    public override void _Ready()
    {
        Engine.TargetFps = 30;
        Input.MouseMode = Input.MouseModeEnum.Hidden;
        Instance = this;
        LoadPersistentObjects();
        
        sceneA = GetNode<ParallaxBackground>("Scenes/Scene_A/ParallaxBackground");
        sceneAEnemies = GetNode<Node2D>("Scenes/Scene_A/World");
        sceneB = GetNode<ParallaxBackground>("Scenes/Scene_B/ParallaxBackground");

        // Hiding scene B when starting the game
        sceneB.Visible = false;
    }

    // Load player and HUD as persistent objects throughout the game
    private void LoadPersistentObjects()
    {
        var persistent = GetNode("Persistent");

        player = playerScene.Instance() as Player;
        persistent.AddChild(player);           

        HUD = HUDScene.Instance() as HUD;
        persistent.AddChild(HUD);
    }

    // Handle player transition when changing scenes
    public void PlayerSceneTransition()
    {
        player.enteredSceneA = !player.enteredSceneA;
        sceneA.Visible = player.enteredSceneA;
        sceneAEnemies.Visible = player.enteredSceneA;
        sceneB.Visible = !player.enteredSceneA;
        
        player.sceneTransitioning = true;  
        player.camera.Zoom = new Vector2(0.98f, 0.98f);
        player.Position = new Vector2(-player.Position.x, player.Position.y);
    }
}