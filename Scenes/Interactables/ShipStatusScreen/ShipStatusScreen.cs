using Godot;
using SpaceshipStartup.Scenes.Levels;
using System;

namespace SpaceshipStartup.Scenes.Interactables
{
	public partial class ShipStatusScreen : Node3D
	{
		#region Components & Nodes

		private ShipStatusIcon generatorStatus;
		private ShipStatusIcon lifeSupportStatus;
		private ShipStatusIcon westEngineStatus;
		private ShipStatusIcon eastEngineStatus;
		private ShipStatusIcon cockpitCorridorStatus;

		#endregion

		#region Method Overrides

		public override void _Ready()
		{
			generatorStatus = GetNode<ShipStatusIcon>("GeneratorStatusIcon");
			lifeSupportStatus = GetNode<ShipStatusIcon>("LifeSupportStatusIcon");
			westEngineStatus = GetNode<ShipStatusIcon>("WestEngineStausIcon");
			eastEngineStatus = GetNode<ShipStatusIcon>("EastEngineStausIcon");
			cockpitCorridorStatus = GetNode<ShipStatusIcon>("CockpitCorridorStatusIcon");
		}

		#endregion

		#region Public Methods

		public void SetGeneratorStatus(ShipStatusIcon.Status status) => generatorStatus.SetStatus(status);

		public void SetLifeSupportStatus(ShipStatusIcon.Status status) => lifeSupportStatus.SetStatus(status);

		public void SetWestEngineStatus(ShipStatusIcon.Status status) => westEngineStatus.SetStatus(status);

		public void SetEastEngineStatus(ShipStatusIcon.Status status) => eastEngineStatus.SetStatus(status);

		public void SetCockpitCorridorStatus(ShipStatusIcon.Status status) => cockpitCorridorStatus.SetStatus(status);

		#endregion
	}
}