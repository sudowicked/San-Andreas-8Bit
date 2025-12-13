using Godot;
using System;

public class GameManager : Node
{
    public static GameManager Instance;

    readonly PackedScene playerScene = GD.Load<PackedScene>("res://scenes/Player.tscn");
    readonly PackedScene HUDScene = GD.Load<PackedScene>("res://scenes/HUD.tscn");

    public Player Player;
    private HUD HUD;
    private Node2D Level;

    public override void _Ready()
    {
        Engine.TargetFps = 30;
        Input.MouseMode = Input.MouseModeEnum.Hidden;
        Instance = this;
        LoadPersistentObjects();
        ChangeScene("res://scenes/Level_A.tscn");
    }

    // Load player and HUD as persistent objects throughout the game
    private void LoadPersistentObjects()
    {
        var persistent = GetNode("Persistent");

        Player = playerScene.Instance() as Player;
        persistent.AddChild(Player);           

        HUD = HUDScene.Instance() as HUD;
        persistent.AddChild(HUD);
    }
    
    public void ChangeScene(string path)
    {
        var scenes = GetNode("Scenes");

        // Clear previous scene from display
        foreach(Node2D child in scenes.GetChildren()) 
            child.QueueFree();

        PackedScene worldScene = GD.Load<PackedScene>(path);
        Level = worldScene.Instance() as Node2D;
        scenes.AddChild(Level);
    }

    // Handle player transition when changing scenes
    public void PlayerSceneTransition()
    {
        string scene = Player.enteredSceneA ? "B" : "A";
        Player.enteredSceneA = !Player.enteredSceneA;
        ChangeScene($"res://scenes/Level_{scene}.tscn");

        Player.inputEnabled = false;
        Player.isZooming = false;   
        Player.currentDirection = Player.Direction.Back;  
        Player.camera.Zoom = new Vector2(0.71f, 0.71f);
        Player.Position = new Vector2(-Player.Position.x, Player.Position.y);
        Player.SetIdleAnimation(Vector2.Zero);

        Player.enableInputTimer.Start(); // Enable input after 2 seconds  
    }
}