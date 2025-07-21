using Godot;
using SpaceshipStartup.Scenes.Levels;
using System;
using System.Collections.Generic;

namespace SpaceshipStartup.Scenes.Interactables
{
	public partial class PipeTile : Sprite3D
	{
		#region Constants

		private float TILE_SIZE = 2;

		#endregion

		#region Enums

		[Flags]
		public enum Gas
		{
			None = 0,
			Xenon = 1 << 1,
			Argon = 1 << 2,
			Oxygen = 1 << 3,
			Radon = 1 << 4,
			Chlorine = 1 << 5,
			Uranium = 1 << 6,
			Fire = 1 << 7,
		}

		#endregion

		#region Signals

		[Signal] public delegate void RotatedEventHandler();
		[Signal] public delegate void GasOutputAddedEventHandler(Gas gas);

		#endregion

		#region Exports

		[Export] private Cardinal BaseConnections { get; set; }
		[Export] private int InitOrientation { get; set; }
		[Export] public Cardinal InputPipeDirection { get; set; }
		[Export] public Gas InputPipeGas { get; set; }
		[Export] public Cardinal OutputPipeDirection { get; set; }

		#endregion

		#region Components & Nodes

		private Node3D pipePivot;
		private AudioStreamPlayer interactionAudio;

		#endregion

		#region Public Properties

		public int Orientation { get; set; } = 0; // How often the tile got rotated clockwise from its original rotation
		public Cardinal Connections { get; private set; }
		public Gas ContainedGases { get; private set; }
		public Dictionary<Cardinal, PipeTile> Neighbors { get; private set; }

		#endregion

		#region Method Overrides

		public override void _Ready()
		{
			pipePivot = GetNode<Node3D>("PipePivot");
			interactionAudio = GetNode<AudioStreamPlayer>("InteractionAudio");

			Neighbors = [];
			Connections = BaseConnections;

			if (InitOrientation < 0 || InitOrientation > 3)
			{
				Log.Warning(this, $"Invalid initOrientation {InitOrientation}!");
			}
			else
			{
				while (Orientation < InitOrientation) {
					RotateClockwise();
				}
			}
		}

		#endregion

		#region Private Methods

		private void RotateClockwise()
		{
			Orientation = (Orientation + 1) % 4;
			Connections = CardinalUtils.Clockwise(Connections);
			pipePivot.RotateZ(DegreeUtils.DegToRad(-90));
			EmitSignal(SignalName.Rotated);
		}

		#endregion

		#region Public Methods

		public void AddIfNeighbor(PipeTile tile)
		{
			if (tile == this)
				return;

			Vector3 offset = tile.Position - Position;
			float offsetX = MathF.Round(offset.X);
			float offsetY = MathF.Round(offset.Y);

			if (offsetX == 0 && offsetY == 0)
			{
				Log.Warning(this, $"This node {Name} and {tile.Name} share the same position {Position} & {tile.Position}!) with offset {offset}!");
			}

			if (offsetX != 0 && offsetY != 0) // Not along a cardinal
				return;

			if (offsetY != 0 && Mathf.Abs(offsetY) <= TILE_SIZE) // North or south neighbor
			{
				Neighbors[offsetY > 0 ? Cardinal.North : Cardinal.South] = tile;
			}
			else if (offsetX != 0 && Mathf.Abs(offsetX) <= TILE_SIZE) // West or east neighbor
			{
				Neighbors[offsetX < 0 ? Cardinal.West : Cardinal.East] = tile;
			}
		}

		public void LogNeighbors()
		{
			Neighbors.TryGetValue(Cardinal.North, out PipeTile north);
			Neighbors.TryGetValue(Cardinal.East, out PipeTile east);
			Neighbors.TryGetValue(Cardinal.South, out PipeTile south);
			Neighbors.TryGetValue(Cardinal.West, out PipeTile west);
			Log.Info(this, $"{Name}'s neighbors: North = {north?.Name}, East = {east?.Name}, South = {south?.Name}, West = {west?.Name}");
		}

		public void Clear()
		{
			ContainedGases = Gas.None;
		}

		public void CheckInput()
		{
			if (InputPipeDirection == Cardinal.None || InputPipeGas == Gas.None)
				return;

			if (!Connections.HasFlag(InputPipeDirection))
				return;

			ContainedGases |= InputPipeGas;

			Propagate();
		}

		#endregion

		#region Private Methods

		private void AddGas(Gas containedGases)
		{
			Gas previous = ContainedGases;
			ContainedGases |= containedGases;

			if (previous != ContainedGases)
			{
				Propagate();
			}
		}

		private void Propagate()
		{
			foreach (KeyValuePair<Cardinal, PipeTile> neighbor in Neighbors)
			{
				// Connected from both sides?
				if (Connections.HasFlag(neighbor.Key) && neighbor.Value.Connections.HasFlag(CardinalUtils.Opposite(neighbor.Key)))
				{
					neighbor.Value.AddGas(ContainedGases);
				}
			}

			if (OutputPipeDirection != Cardinal.None && Connections.HasFlag(OutputPipeDirection))
			{
				EmitSignal(SignalName.GasOutputAdded, (int)ContainedGases);
			}
		}

		#endregion

		#region Events

		public void OnClicked()
		{
			RotateClockwise();
			interactionAudio.Play();
		}

		#endregion
	}
}