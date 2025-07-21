using Godot;
using SpaceshipStartup;
using SpaceshipStartup.Scenes.Levels;
using System;

public partial class Player : Node3D
{
	public static Player Instance { get; private set; }

	#region Signals
	
	[Signal] public delegate void MovedEventHandler();
	[Signal] public delegate void TurnedEventHandler();
	
	#endregion

	#region Public Properties

	public bool CanMove { get; set; } = true;
	public MapNode CurrentMapNode { get; private set; }

	#endregion

	#region Private Properties

	private Cardinal Facing { get; set; }

	#endregion

	#region Method Overrides

	public override void _Ready()
	{
		Instance = this;

		Facing = CardinalUtils.RotationCardinal(GlobalRotationDegrees.Y);
	}

	public override void _PhysicsProcess(double delta)
	{
		CheckMoveInput();
	}

	#endregion

	#region Public Methods

	public bool CanMoveForward()
	{
		return CurrentMapNode.CanMove(Facing);
	}

	public void MoveTowardCardinal(Cardinal cardinal)
	{
		if (CurrentMapNode.CanMove(cardinal))
			MoveToNode(CurrentMapNode.Neighbors[cardinal]);
	}

	public void MoveToNode(MapNode node)
	{
		CurrentMapNode = node;
		GlobalPosition = node.GlobalPosition;
		EmitSignal(SignalName.Moved);
	}

	public void MoveForward()
	{
		MoveTowardCardinal(Facing);
	}

	public void MoveBackward()
	{
		MoveTowardCardinal(CardinalUtils.Opposite(Facing));
	}

	public void StrafeLeft()
	{
		MoveTowardCardinal(CardinalUtils.Counterclockwise(Facing));
	}

	public void StrafeRight()
	{
		MoveTowardCardinal(CardinalUtils.Clockwise(Facing));
	}

	public void FaceCardinal(Cardinal cardinal)
	{
		Facing = cardinal;
		LookAt(GlobalPosition + CardinalUtils.CardinalVector(Facing));
		EmitSignal(SignalName.Turned);
	}

	public void TurnLeft()
	{
		FaceCardinal(CardinalUtils.Counterclockwise(Facing));
	}

	public void TurnRight()
	{
		FaceCardinal(CardinalUtils.Clockwise(Facing));
	}

	#endregion

	#region Movement

	private void CheckMoveInput()
	{
		if (!CanMove)
			return;

		if (Input.IsActionJustPressed("move_forward"))
		{
			MoveForward();
		}
		else if (Input.IsActionJustPressed("move_backward"))
		{
			MoveBackward();
		}
		else if (Input.IsActionJustPressed("turn_left"))
		{
			TurnLeft();
		}
		else if (Input.IsActionJustPressed("turn_right"))
		{
			TurnRight();
		}
		else if (Input.IsActionJustPressed("strafe_left"))
		{
			StrafeLeft();
		}
		else if (Input.IsActionJustPressed("strafe_right"))
		{
			StrafeRight();
		}
	}

	#endregion
}
