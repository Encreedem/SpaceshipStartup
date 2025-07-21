using Godot;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace SpaceshipStartup.Scenes.Interactables
{
	public partial class PipePuzzle : Node3D
	{
		#region Components & Nodes

		private List<PipeTile> Map { get; set; }

		#endregion

		#region Public Properties

		public PipeTile.Gas Output { get; private set; }

		#endregion

		#region Private Variables

		private List<PipeTile> inputTiles;

		#endregion

		#region Method Overrides

		public override void _Ready()
		{
			Map = GetNode("PipeTiles").GetChildren().ToList().ConvertAll(x => (PipeTile)x);
			inputTiles = [];

			foreach (PipeTile tile in Map)
			{
				tile.Rotated += TileRotated;

				if (tile.InputPipeGas != PipeTile.Gas.None)
				{
					inputTiles.Add(tile);
				}

				foreach (PipeTile potentialNeighbor in Map)
				{
					tile.AddIfNeighbor(potentialNeighbor);
				}
			}

			Refresh();
		}

		#endregion

		#region Private Methods

		private void Refresh()
		{
			// Clear all pipes
			Output = PipeTile.Gas.None;

			foreach (PipeTile tile in Map)
			{
				tile.Clear();
			}

			// Iterate through all inputs
			foreach (PipeTile tile in inputTiles)
			{
				// Tell the input pipe to refresh
				tile.CheckInput();
			}

			// After everything is done, check the output pipe's content
			Log.Debug(this, $"Pipe output: {Output}");
		}

		#endregion

		#region Events

		public void TileRotated()
		{
			Refresh();
		}

		public void OnOutputAdded(PipeTile.Gas gas)
		{
			Output |= gas;
		}

		#endregion
	}
}