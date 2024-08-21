using Godot;
using System;

public partial class BingoButton : TextureButton
{

	bool hidden = false;
	double timeInHide = 0;
	double timeToHide = 0;

	Vector2 startPosition;
	Vector2 hiddenPosition;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		startPosition = new Vector2(-128, 180);
        hiddenPosition = new Vector2(-128, 350);

		Pressed += () => { HideBingoButton(3); };
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		if (hidden) {
			timeInHide += delta;

			if (timeInHide <= 0.5f) Position = (Vector2)Tween.InterpolateValue(startPosition, hiddenPosition - startPosition, timeInHide, 0.5, Tween.TransitionType.Cubic, Tween.EaseType.InOut);
            else if (timeInHide >= timeToHide)
            {
                Position = startPosition;
                hidden = false;
                timeInHide = 0;
            }
            else if (timeInHide >= timeToHide - 0.5f) Position = (Vector2)Tween.InterpolateValue(hiddenPosition, startPosition - hiddenPosition, timeInHide - timeToHide + 0.5, 0.5, Tween.TransitionType.Cubic, Tween.EaseType.InOut);
			
        }
	}

	public void HideBingoButton(float duration = float.MaxValue) {
		if (duration < 1.0f) return;
		if (hidden) return;
        timeToHide = duration;
        timeInHide = 0;
        hidden = true;
	}

	public void ShowBingoButton() {
		if (hidden) return;
		timeToHide = 1.5f;
		timeInHide = 1;
	}
}
