using Godot;
using System;
using System.ComponentModel;

public class Player : KinematicBody2D
{
    [Export]
    public int Speed = 80;
    public Node2D background;
    public Vector2 screenSize;
    public AnimatedSprite animatedSprite;
    public Camera2D camera;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        Engine.TargetFps = 30;

        background = GetNode<Node2D>("../ParallaxBackground/ParallaxLayer/BackgroundPivot");
        screenSize = GetViewportRect().Size;
        camera = GetNode<Camera2D>("Camera2D");
        animatedSprite = GetNode<AnimatedSprite>("AnimatedSprite");

        GD.Print(animatedSprite.Scale / camera.Zoom);

    }
    
    public Vector2 getInput()
    {
        var velocity = Vector2.Zero;

        if (Input.IsActionPressed("left"))
        {
            velocity.x -= 1;
            animatedSprite.Animation = "CJ_Walk_Side";
            animatedSprite.FlipH = true;
        }
        if (Input.IsActionJustReleased("left") || Input.IsActionJustReleased("right"))
        {
            animatedSprite.Animation = "CJ_Idle_Side";
        }
        if (Input.IsActionPressed("right"))
        {
            velocity.x += 1;
            animatedSprite.Animation = "CJ_Walk_Side";
            animatedSprite.FlipH = false;
        }
        if (Input.IsActionPressed("up"))
        {
            camera.Zoom -= new Vector2(.006f, .006f);
            animatedSprite.Animation = "CJ_Walk_Back";
        }
         if (Input.IsActionJustReleased("up"))
        {
            animatedSprite.Animation = "CJ_Idle_Back";
        }
        if (Input.IsActionPressed("down"))
        {
            camera.Zoom += new Vector2(.006f, .006f);
            animatedSprite.Animation = "CJ_Walk_Front";
        }
        if (Input.IsActionJustReleased("down"))
        {
            animatedSprite.Animation = "CJ_Idle_Front";
        }

        return velocity;
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(float delta)
    {
        var velocity = getInput();
        
        // Update player position
        if (velocity.Length() > 0)
        {
            velocity = velocity.Normalized() * Speed;
        }
        Position += velocity * delta;

        // Clamp position and camera zoom
        Position = new Vector2(Mathf.Clamp(Position.x, -screenSize.x, screenSize.x), Mathf.Clamp(Position.y, -screenSize.y, screenSize.y));
        camera.Zoom = new Vector2(Mathf.Clamp(camera.Zoom.x, .5f, 1), Mathf.Clamp(camera.Zoom.y, .5f, 1));

        // Keep the player's position the same relative to the camera
        Scale = new Vector2(camera.Zoom.x,  camera.Zoom.y);
    }
}
