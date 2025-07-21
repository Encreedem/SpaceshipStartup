using Godot;
using Microsoft.VisualBasic;
using SpaceshipStartup;
using SpaceshipStartup.Scenes.Interactables;
using SpaceshipStartup.Scenes.UI;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SpaceshipStartup.Scenes.Levels
{
	public partial class Level : Node3D
	{
		public static Level Instance { get; private set; }

		#region Exports

		[Export] private bool StartEmergencyLights { get; set; } = false;
		[Export] public Godot.Collections.Dictionary<Interaction, Resource> InteractionCursors { get; set; }
		[Export] public Godot.Collections.Dictionary<Interaction, Vector2> InteractionHotspots { get; set; }
		[Export] public Color LightsDefaultColor { get; set; } = Color.FromHtml("ffffff");
		[Export] public Color LightsEmergencyColor { get; set; } = Color.FromHtml("ff0000");
		[Export] public float LightsDefaultEnergy { get; set; } = 1;
		[Export] public float LightsEmergencyEnergy { get; set; } = 1;
		[Export] public float LightsDefaultRange { get; set; } = 5;
		[Export] public float LightsEmergencyRange { get; set; } = 3;

		#endregion

		#region Components & Nodes

		private Player player;
		private Camera3D camera;
		private MouseControls mouseControls;
		private Options options;

		// Big doors
		private AnimationPlayer bigDoorsAnimationPlayer;
		private Interactable3D bigDoorsInteractable;
		private AudioStreamPlayer3D bigDoorsLockedAudio;

		#endregion

		#region Public Properties

		public List<MapNode> Map { get; set; }
		public Interactable3D ObservedInteractable { get; set; }
		public Inspectable CurrentInspectable { get; set; }
		public List<ShipStatusScreen> ShipStatusScreens { get; set; }
		public List<Light3D> Lights { get; set; }
		public List<Light3D> SecondaryLights { get; set; }
		public bool LightsEmergecyMode { get; set; } = false;
		public bool IsKeycardObtained { get; set; } = false;
		public bool IsFilterObtained { get; set; } = false;
		public bool StartWithTutorial { get; set; } = true;
		private bool BigDoorsLocked { get; set; } = true;

		#endregion

		#region Method Overrides

		public override void _Ready()
		{
			Instance = this;

			player = Player.Instance;
			camera = GetNode<Camera3D>("Player/Camera3D");
			mouseControls = GetTree().Root.GetNode<MouseControls>("MouseControls");
			options = GetTree().Root.GetNode<Options>("Options");

			bigDoorsAnimationPlayer = GetNode<AnimationPlayer>("Doors/BigDoors/BigDoorsAnimationPlayer");
			bigDoorsInteractable = GetNode<Interactable3D>("Doors/BigDoors/BigDoorInteractable");
			bigDoorsLockedAudio = GetNode<AudioStreamPlayer3D>("Doors/BigDoors/OpenAudio");

			if (player == null)
			{
				Log.Error(this, "Player instance not found!");
			}

			options.Hide();

			InitMap();
			InitDoors();
			InitShipStatusScreens();
			InitInteractables();
			InitLights();

			if (StartEmergencyLights)
				SetLightsEmergencyMode();

			if (StartWithTutorial)
				StartTutorial();
		}

		public override void _UnhandledKeyInput(InputEvent @event)
		{
			if (@event is InputEventKey key)
			{
				if (key.IsActionPressed("test"))
				{
					if (LightsEmergecyMode)
					{
						SetLightsDefaultMode();
					}
					else
					{
						SetLightsEmergencyMode();
					}
				}
			}
		}

		#endregion

		#region Public Methods

		public void SetObservedInteractable(Interactable3D interactable)
		{
			ObservedInteractable = interactable;
			RefreshCursor();
		}

		public void RemoveObservedInteractable(Interactable3D interactable)
		{
			if (interactable != ObservedInteractable)
				return;

			SetCursorIcon(Interaction.None);
		}

		public void SetCurrentInspectable(Inspectable inspectable)
		{
			CurrentInspectable = inspectable;
			SetControlsInspecting();
		}

		public void ExitCurrentInspectable()
		{
			if (CurrentInspectable == null)
			{
				Log.Warning(this, $"Cannot exit current inspectable. None assigned.");
				return;
			}

			camera.Current = true;
			CurrentInspectable.Exit();
			CurrentInspectable = null;
			Player.Instance.CanMove = true;
			mouseControls.SetControlsMovement();
		}

		public void SetCursorIcon(Interaction interaction)
		{
			Input.SetCustomMouseCursor(InteractionCursors[interaction], Input.CursorShape.Arrow, InteractionHotspots[interaction]);
		}

		public void SetMainCameraAsCurrent() => camera.Current = true;

		public void RefreshInteraction()
		{
			RefreshForwardInteraction();
			RefreshCursor();
		}

		public void SetGeneratorPuzzleComplete()
		{
			GetNode<OmniLight3D>("Terrain/BackupGenerator/GeneratorDoorLampRed").Hide();
			GetNode<Door>("Doors/GeneratorDoor").Locked = false;
			GetNode<Door>("Doors/LifeSupportDoor").Locked = false;

			mouseControls.HighlightGoBack();

			SetLightsDefaultMode();

			foreach (ShipStatusScreen screen in ShipStatusScreens)
			{
				screen.SetGeneratorStatus(ShipStatusIcon.Status.Check);
				screen.SetLifeSupportStatus(ShipStatusIcon.Status.Exclamation);
			}
		}

		public void SetLifeSupportPuzzleComplete()
		{
			BigDoorsLocked = false;
			bigDoorsInteractable.Interaction = Interaction.Interact;

			foreach (ShipStatusScreen screen in ShipStatusScreens)
			{
				screen.SetLifeSupportStatus(ShipStatusIcon.Status.Check);
				screen.SetCockpitCorridorStatus(ShipStatusIcon.Status.Exclamation);
			}
		}

		public void SetPickedUpKeycard()
		{
			IsKeycardObtained = true;
			GetNode<WestDoorControl>("Interactables/WestDoorControl").SetDoorControlInteractable();
		}

		public void SetPickedUpFilter()
		{
			IsFilterObtained = true;
			GetNode<LifeSupport>("Interactables/LifeSupport").SetFullFilterInteractable();
		}

		#endregion

		#region Private Methods

		private void InitMap()
		{
			List<Node> mapNodesRaw = [.. GetTree().GetNodesInGroup("MapNode")];
			Map = mapNodesRaw.ConvertAll(x => (MapNode)x);

			MapNode closestToPlayer = Map.FirstOrDefault();
			float closestSquareDistance = player.GlobalPosition.DistanceSquaredTo(closestToPlayer.GlobalPosition);

			// Connect nodes & find closest to player
			foreach (MapNode node in Map)
			{
				foreach (MapNode potentialNeighbor in Map)
				{
					node.AddIfNeighbor(potentialNeighbor);
				}

				float currentSquareDistance = node.GlobalPosition.DistanceSquaredTo(player.GlobalPosition);
				if (currentSquareDistance < closestSquareDistance)
				{
					closestToPlayer = node;
					closestSquareDistance = currentSquareDistance;
				}
			}
			
			/*
			foreach (MapNode node in Map)
			{
				node.LogNeighbors();
			}
			*/
			
			player.MoveToNode(closestToPlayer);
		}

		private void InitDoors()
		{
			List<Door> doors = GetTree().GetNodesInGroup("Door").ToList().ConvertAll(x => (Door)x);

			foreach (Door door in doors)
			{
				foreach (MapNode potentialNeighbor in Map)
				{
					door.AddIfNeighbor(potentialNeighbor);
				}
			}
		}

		private void InitInteractables()
		{
			List<Interactable3D> interactables = GetTree().GetNodesInGroup("Interactable").ToList().ConvertAll(x => (Interactable3D)x);

			foreach (Interactable3D interactable in interactables)
			{
				interactable.InteractableFromMapNode = Map.OrderBy(x => x.GlobalPosition.DistanceSquaredTo(interactable.GlobalPosition)).First();
			}
		}

		private void InitShipStatusScreens()
		{
			ShipStatusScreens = GetTree().GetNodesInGroup("ShipStatusScreen").ToList().ConvertAll(x => (ShipStatusScreen)x);
		}

		private void InitLights()
		{
			Lights = GetTree().GetNodesInGroup("WhiteLight").ToList().ConvertAll(x => (Light3D)x);
			SecondaryLights = GetTree().GetNodesInGroup("SecondaryLight").ToList().ConvertAll(x => (Light3D)x);
		}

		private void RefreshForwardInteraction()
		{
			mouseControls.SetMoveForward(player.CanMoveForward());
		}

		private void RefreshCursor()
		{
			Interaction interaction = Interaction.None;

			if (mouseControls.HoveredControl != null)
			{
				interaction = mouseControls.GetInteraction();
			}
			else if (ObservedInteractable != null && ObservedInteractable.InteractableFromMapNode == player.CurrentMapNode)
			{
				interaction = ObservedInteractable.Interaction;
			}

			SetCursorIcon(interaction);
		}

		/// <summary>
		/// Adjusts the controls so the player can use an <see cref="Inspectable"/> properly and disables their movement.
		/// </summary>
		private void SetControlsInspecting()
		{
			Player.Instance.CanMove = false;
			mouseControls.SetControlsInspecting();
		}

		private void SetLights(Color color, float energy, float range)
		{
			foreach (Light3D light in Lights)
			{
				light.LightColor = color;
				light.LightEnergy = energy;

				if (light is OmniLight3D omniLight)
				{
					omniLight.OmniRange = range;
				}
			}
		}

		private void SetLightsDefaultMode()
		{
			LightsEmergecyMode = false;
			SetLights(LightsDefaultColor, LightsDefaultEnergy, LightsDefaultRange);

			foreach (Light3D light in SecondaryLights)
			{
				light.Show();
			}
		}

		private void SetLightsEmergencyMode()
		{
			LightsEmergecyMode = true;
			SetLights(LightsEmergencyColor, LightsEmergencyEnergy, LightsEmergencyRange);

			foreach(Light3D light in SecondaryLights)
			{
				light.Hide();
			}
		}

		private void StartTutorial()
		{
			mouseControls.StartTutorial();
			player.CanMove = false;
		}

		private void PlayerWins()
		{
			player.CanMove = false;
			mouseControls.ShowWinDialog();
			RefreshCursor();
		}

		#endregion

		#region Events

		public void OnPlayerMoved()
		{
			RefreshInteraction();
		}

		public void OnPlayerTurned()
		{
			RefreshInteraction();
		}

		public void OnTutorialAlmostCompleted()
		{
			GetNode<AnimationPlayer>("Player/AnimationPlayer").Play("Stumble");
			bigDoorsAnimationPlayer.Play("Close");
			SetLightsEmergencyMode();
		}

		public void OnTutorialCompleted()
		{
			player.CanMove = true;
		}

		public void OnBigDoorInteractableClicked()
		{
			if (BigDoorsLocked)
			{
				bigDoorsLockedAudio.Play();
			}
			else
			{
				bigDoorsInteractable.Hide();
				bigDoorsAnimationPlayer.Play("Open");
				PlayerWins();
			}
		}

		#endregion
	}
}