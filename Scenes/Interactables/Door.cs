using Godot;
using SpaceshipStartup.Scenes.Levels;
using System;
using System.Collections.Generic;

namespace SpaceshipStartup.Scenes.Interactables
{
	public partial class Door : Node3D
	{
		#region Constants
		private float NEIGHBOR_MAX_DISTANCE = 2.5f;
		#endregion

		#region Exports

		[Export] public bool Locked { get; set; } = false;

		#endregion

		#region Public Properties

		/// <summary>
		/// Contains the nodes relative to this door that are separated by it.
		/// </summary>
		public Dictionary<Cardinal, MapNode> SeparatedNodes { get; set; }

		#endregion

		#region Components & Nodes

		private Interactable3D interactableNorth;
		private Interactable3D interactableSouth;
		private AnimationPlayer animationPlayer;
		private AudioStreamPlayer3D lockedAudio;

		#endregion

		#region Method Overrides

		public override void _Ready()
		{
			interactableNorth = GetNode<Interactable3D>("InteractableNorth");
			interactableSouth = GetNode<Interactable3D>("InteractableSouth");
			animationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
			lockedAudio = GetNode<AudioStreamPlayer3D>("LockedAudio");

			SeparatedNodes = [];
		}

		#endregion

		#region Public Methods

		/// <summary>
		/// Adds the <paramref name="node"/> to <see cref="SeparatedNodes"/> if it's within half a tile along a cardinal.
		/// </summary>
		public void AddIfNeighbor(MapNode node)
		{
			Vector3 offset = node.GlobalPosition - GlobalPosition;
			float offsetX = MathF.Round(offset.X);
			float offsetZ = MathF.Round(offset.Z);

			if (offsetX == 0 && offsetZ == 0)
			{
				Log.Warning(this, $"This node {Name} and {node.Name} share the same position {GlobalPosition} & {node.GlobalPosition}!) with offset {offset}!");
			}

			if (offsetX != 0 && offsetZ != 0) // Not along a cardinal
				return;

			Cardinal direction;

			if (offsetZ != 0 && Mathf.Abs(offsetZ) <= NEIGHBOR_MAX_DISTANCE) // North or south neighbor
			{
				direction = offsetZ < 0 ? Cardinal.North : Cardinal.South;
			}
			else if (offsetX != 0 && Mathf.Abs(offsetX) <= NEIGHBOR_MAX_DISTANCE) // West or east neighbor
			{
				direction = offsetX < 0 ? Cardinal.West : Cardinal.East;
			}
			else
				return;

			SeparatedNodes[direction] = node;
			node.BlockedCardinals |= CardinalUtils.Opposite(direction);
			Log.Debug(this, $"Blocked map node {node.Name}'s cardinal {CardinalUtils.Opposite(direction)}");
		}

		#endregion

		#region Events

		public void OnInteracted()
		{
			if (Locked)
			{
				lockedAudio.Play();
				return;
			}

			foreach(KeyValuePair<Cardinal, MapNode> blocked in SeparatedNodes)
			{
				Log.Debug(this, $"Unlocking {blocked.Key} map node. Its direction {CardinalUtils.Opposite(blocked.Key)}");
				blocked.Value.BlockedCardinals &= ~CardinalUtils.Opposite(blocked.Key);
			}

			interactableNorth.Hide();
			interactableSouth.Hide();

			animationPlayer.Play("Open");

			Level.Instance.RefreshInteraction();
		}

		#endregion
	}
}