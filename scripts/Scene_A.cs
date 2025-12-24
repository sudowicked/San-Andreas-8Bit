using Godot;
using System;

public class Scene_A : Node2D
{
    private readonly PackedScene enemyScene = GD.Load<PackedScene>("res://scenes/Enemy.tscn");
    readonly private RandomNumberGenerator rng = new RandomNumberGenerator();
    private Enemy enemy;

    public override void _Ready()
    {
        rng.Randomize();
        for (int i = 0; i < 3; i++)
        {
            enemy = enemyScene.Instance() as Enemy;
            GetNode<Node2D>("World").AddChild(enemy);
            enemy.GlobalPosition = new Vector2(rng.RandfRange(-480, 480), 0);           
        }
    }
}
