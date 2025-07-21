using Godot;
using System;

namespace SpaceshipStartup.Scenes.Interactables
{
	public partial class Gauge : Sprite3D
	{
		#region Signals

		[Signal] public delegate void SetValueFinishedEventHandler(float target);

		#endregion

		#region Exports

		[Export] private float Value { get; set; } = 0.5f;
		[Export] private float TweenDuration { get; set; } = 1f;
		[Export] private float MinAngleDegrees { get; set; } = 115;
		[Export] private float MaxAngleDegrees { get; set; } = -115;
		[Export] private Tween.EaseType TweenEaseType { get; set; } = Tween.EaseType.Out;
		[Export] private Tween.TransitionType TweenTransitionType { get; set; } = Tween.TransitionType.Quad;

		#endregion

		#region Components & Nodes

		private Node3D needlePivot;

		#endregion

		#region Private Variables

		Tween rotationTween;

		#endregion

		#region Method Overrides

		public override void _Ready()
		{
			needlePivot = GetNode<Node3D>("NeedlePivot");
			SetValue(Value, false);
		}

		#endregion

		#region Public Methods

		public void SetValue(float value, bool tween = true, float customTweenTime = 0)
		{
			Value = value;

			float targetAngle = (float)Mathf.Lerp(MinAngleDegrees, MaxAngleDegrees, (double)value);
			float targetRadians = DegreeUtils.DegToRad(targetAngle);
			Vector3 targetRotation = new(0, 0,  targetRadians);

			if (tween)
			{
				if (rotationTween != null && rotationTween.IsRunning())
					rotationTween.Kill();

				rotationTween = GetTree().CreateTween();
				rotationTween.SetEase(TweenEaseType);
				rotationTween.SetTrans(TweenTransitionType);
				rotationTween.TweenProperty(needlePivot, "rotation", targetRotation, customTweenTime > 0 ? customTweenTime : TweenDuration);
				rotationTween.Finished += () => EmitSignal(SignalName.SetValueFinished, value);
			}
			else
			{
				needlePivot.Rotation = targetRotation;
			}
		}

		#endregion
	}
}