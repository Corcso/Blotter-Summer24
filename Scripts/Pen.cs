using Godot;
using System;

public partial class Pen : Node2D
{
    [Export] CompressedTexture2D blotTexture;

    [Export] public Node2D myBingoCard;

    Sprite2D newBlot;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
	{
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

    public override void _Input(InputEvent @event)
    {
        base._Input(@event);

        // Mouse in viewport coordinates.
        if (@event is InputEventMouseButton eventMouseButton)
        {
            if (eventMouseButton.Pressed)
            {
                GD.Print("Mouse Click/Unclick at: ", eventMouseButton.Position);
                newBlot = new Sprite2D();
                myBingoCard.AddChild(newBlot);
                newBlot.Texture = blotTexture;
                newBlot.GlobalPosition = TranslateMouseToWorld(eventMouseButton.Position);
            }
        }
        else if (@event is InputEventMouseMotion eventMouseMotion)
        {
            //GD.Print("Mouse Motion at: ", GetViewport().GetMousePosition());
            Position = TranslateMouseToWorld(GetViewport().GetMousePosition());
        }
    }

    private Vector2 TranslateMouseToWorld(Vector2 position)
    {
        return (position - (GetViewport().GetVisibleRect().Size / 2.0f)) / 1.0f;
    }
}
