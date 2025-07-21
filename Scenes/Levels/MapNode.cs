using Godot;
using System;
using System.Collections.Generic;

namespace SpaceshipStartup.Scenes.Levels
{
	public partial class MapNode : Marker3D
	{
		// Blocked areas
		// Neighboring nodes
		#region Constants

		private const float NEIGHBOR_MAX_DISTANCE = 5f;

		#endregion

		#region Exports

		[Export] public Cardinal BlockedCardinals { get; set; }

		#endregion

		#region Public Properties

		public Dictionary<Cardinal, MapNode> Neighbors { get; set; }

		#endregion

		#region Method Overrides

		public override void _Ready()
		{
			Neighbors = [];
		}

		#endregion

		#region Public Methods

		/// <summary>
		/// Adds the <paramref name="node"/> to <see cref="Neighbors"/> if it's within a single tile along a cardinal.
		/// </summary>
		public void AddIfNeighbor(MapNode node)
		{
			if (node == this)
				return;

			Vector3 offset = node.GlobalPosition - GlobalPosition;
			float offsetX = MathF.Round(offset.X);
			float offsetZ = MathF.Round(offset.Z);

			if (offsetX == 0 && offsetZ == 0)
			{
				Log.Warning(this, $"This node {Name} and {node.Name} share the same position {GlobalPosition} & {node.GlobalPosition}!) with offset {offset}!");
			}

			if (offsetX != 0 && offsetZ != 0) // Not along a cardinal
				return;

			if (offsetZ != 0 && Mathf.Abs(offsetZ) <= NEIGHBOR_MAX_DISTANCE) // North or south neighbor
			{
				Neighbors[offsetZ < 0 ? Cardinal.North : Cardinal.South] = node;
			}
			else if (offsetX != 0 && Mathf.Abs(offsetX) <= NEIGHBOR_MAX_DISTANCE) // West or east neighbor
			{
				Neighbors[offsetX < 0 ? Cardinal.West : Cardinal.East] = node;
			}
		}

		public void LogNeighbors()
		{
			Neighbors.TryGetValue(Cardinal.North, out MapNode north);
			Neighbors.TryGetValue(Cardinal.East, out MapNode east);
			Neighbors.TryGetValue(Cardinal.South, out MapNode south);
			Neighbors.TryGetValue(Cardinal.West, out MapNode west);
			Log.Info(this, $"{Name}'s neighbors: North = {north?.Name}, East = {east?.Name}, South = {south?.Name}, West = {west?.Name}");
		}

		public bool CanMove(Cardinal cardinal)
		{
			return Neighbors.ContainsKey(cardinal) && (BlockedCardinals & cardinal) != cardinal;
		}

		#endregion
	}
}