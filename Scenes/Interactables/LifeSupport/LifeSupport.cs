using Godot;
using SpaceshipStartup.Scenes.Levels;
using System;

namespace SpaceshipStartup.Scenes.Interactables
{
	public partial class LifeSupport : Node3D
	{
		#region Enums

		private enum Status
		{
			Off,
			PreScan,
			ScanAlien,
			ScanTentacle,
			ScanBlob,
			ScanHuman,
			ScanComplete,
			PreCheckGas,
			CheckGasXe,
			CheckGasAr,
			CheckGasO2,
			CheckGasRn,
			CheckGasCl,
			CheckGasU,
			CheckGasFire,
			CheckGasComplete,
			PreCheckFilter,
			CheckFilter,
			FullyOperational
		}

		#endregion

		#region Components & Nodes

		private PipePuzzle pipePuzzle;

		private Inspectable inspectable;
		private Label3D numberDisplayAlien;
		private Label3D numberDisplayTentacle;
		private Label3D numberDisplayBlob;
		private Label3D numberDisplayHuman;
		private Sprite3D buttonPressed;
		private Sprite3D buttonIdle;
		private Interactable3D buttonInteractable;
		private AnimationPlayer scannerAnimationPlayer;
		private LampAlarm lampFilter;
		private LampAlarm lampGases;
		private LampAlarm lampAlien;
		private LampAlarm lampTentacle;
		private LampAlarm lampBlob;
		private LampAlarm lampHuman;
		private LampAlarm gasLampXe;
		private LampAlarm gasLampAr;
		private LampAlarm gasLampO2;
		private LampAlarm gasLampRn;
		private LampAlarm gasLampCl;
		private LampAlarm gasLampU;
		private LampAlarm gasLampFire;
		private AnimationPlayer podAnimationPlayer;
		private Timer phaseDelayTimer;
		private AudioStreamPlayer3D generatorHum;

		// Display
		private AnimationPlayer displayAnimationPlayer;
		private Sprite3D displayLoadingSprite;
		private Sprite3D displayScanSprite;
		private Sprite3D displayGasesSprite;
		private Sprite3D displayFilterSprite;
		private Sprite3D displayCheckmarkSprite;
		private Sprite3D displayErrorSprite;

		// Filter
		private Interactable3D emptyFilterInteractable;
		private Interactable3D fullFilterInteractable;
		private AnimationPlayer filterAnimationPlayer;

		#endregion

		#region Private Properties

		private float ScanCompleteDelay { get; set; } = 2;
		private float CheckGasDelay { get; set; } = 0.5f;
		private float CheckGasCompletedDelay { get; set; } = 1f;
		private float CheckFilterDelay { get; set; } = 2f;
		private bool FullFilterInserted { get; set; } = false;

		#endregion

		#region Private Variables

		private Status currentStatus = Status.Off;

		#endregion

		#region Method Overrides

		public override void _Ready()
		{
			pipePuzzle = GetNode<PipePuzzle>("../PipePuzzle");

			inspectable = GetNode<Inspectable>("Inspectable");
			numberDisplayAlien = GetNode<Label3D>("NumberDisplayAlien/Label3D");
			numberDisplayTentacle = GetNode<Label3D>("NumberDisplayTentacle/Label3D");
			numberDisplayBlob = GetNode<Label3D>("NumberDisplayBlob/Label3D");
			numberDisplayHuman = GetNode<Label3D>("NumberDisplayHuman/Label3D");
			buttonPressed = GetNode<Sprite3D>("ButtonPressed");
			buttonIdle = GetNode<Sprite3D>("ButtonIdle");
			buttonInteractable = GetNode<Interactable3D>("ButtonInteractable");
			scannerAnimationPlayer = GetNode<AnimationPlayer>("Scanner/AnimationPlayer");
			lampFilter = GetNode<LampAlarm>("Lamps/LampFilter");
			lampGases = GetNode<LampAlarm>("Lamps/LampGases");
			lampAlien = GetNode<LampAlarm>("Lamps/LampAlien");
			lampTentacle = GetNode<LampAlarm>("Lamps/LampTentacle");
			lampBlob = GetNode<LampAlarm>("Lamps/LampBlob");
			lampHuman = GetNode<LampAlarm>("Lamps/LampHuman");
			gasLampXe = GetNode<LampAlarm>("Lamps/GasLamps/LampXe");
			gasLampAr = GetNode<LampAlarm>("Lamps/GasLamps/LampAr");
			gasLampO2 = GetNode<LampAlarm>("Lamps/GasLamps/LampO2");
			gasLampRn = GetNode<LampAlarm>("Lamps/GasLamps/LampRn");
			gasLampCl = GetNode<LampAlarm>("Lamps/GasLamps/LampCl");
			gasLampU = GetNode<LampAlarm>("Lamps/GasLamps/LampU");
			gasLampFire = GetNode<LampAlarm>("Lamps/GasLamps/LampFire");
			podAnimationPlayer = GetNode<AnimationPlayer>("PodAnimationPlayer");
			phaseDelayTimer = GetNode<Timer>("PhaseDelayTimer");
			generatorHum = GetNode<AudioStreamPlayer3D>("GeneratorHum");

			// Display
			displayAnimationPlayer = GetNode<AnimationPlayer>("Display/DisplayAnimationPlayer");
			displayLoadingSprite = GetNode<Sprite3D>("Display/LoadingIcon");
			displayScanSprite = GetNode<Sprite3D>("Display/Scan");
			displayGasesSprite = GetNode<Sprite3D>("Display/Gases");
			displayFilterSprite = GetNode<Sprite3D>("Display/Filter");
			displayCheckmarkSprite = GetNode<Sprite3D>("Display/Checkmark");
			displayErrorSprite = GetNode<Sprite3D>("Display/Error");

			// Filter
			emptyFilterInteractable = GetNode<Interactable3D>("Filter/EmptyFilterInteractable");
			fullFilterInteractable = GetNode<Interactable3D>("Filter/FullFilterInteractable");
			filterAnimationPlayer = GetNode<AnimationPlayer>("Filter/FilterAnimationPlayer");
		}

		#endregion

		#region Public Methods

		public void SetFullFilterInteractable()
		{
			fullFilterInteractable.Interaction = Interaction.Interact;
		}

		#endregion

		#region Private Methods

		/// <summary>
		/// Sets whether the button can be pressed.
		/// </summary>
		/// <param name="interactable"></param>
		private void SetInteractable(bool interactable)
		{
			buttonIdle.Visible = interactable;
			buttonPressed.Visible = !interactable;
			buttonInteractable.Visible = interactable;
		}

		private void Reset()
		{
			// Reset lifeform scans
			numberDisplayAlien.Text = "-";
			numberDisplayTentacle.Text = "-";
			numberDisplayBlob.Text = "-";
			numberDisplayHuman.Text = "-";

			// Lamps
			lampGases.TurnOff();
			lampFilter.TurnOff();
			lampAlien.TurnOff();
			lampTentacle.TurnOff();
			lampBlob.TurnOff();
			lampHuman.TurnOff();
			gasLampXe.TurnOff();
			gasLampAr.TurnOff();
			gasLampO2.TurnOff();
			gasLampRn.TurnOff();
			gasLampCl.TurnOff();
			gasLampU.TurnOff();
			gasLampFire.TurnOff();
		}

		private void StartNextPhase()
		{
			switch (currentStatus) // The status that just finished
			{
				case Status.Off:
					// Scan delay -> OnDisplayAnimationPlayerAnimationFinished
					currentStatus = Status.PreScan;
					displayAnimationPlayer.Play("Load");
					break;
				case Status.PreScan:
					// Scan alien -> OnScannerAnimationPlayerAnimationFinished
					currentStatus = Status.ScanAlien;
					scannerAnimationPlayer.Play("Pulse");
					displayAnimationPlayer.Play("Scan");
					break;
				case Status.ScanAlien:
					numberDisplayAlien.Text = "0";
					currentStatus = Status.ScanTentacle;
					scannerAnimationPlayer.Play("Pulse");
					break;
				case Status.ScanTentacle:
					numberDisplayTentacle.Text = "0";
					currentStatus = Status.ScanBlob;
					scannerAnimationPlayer.Play("Pulse");
					break;
				case Status.ScanBlob:
					numberDisplayBlob.Text = "0";
					currentStatus = Status.ScanHuman;
					scannerAnimationPlayer.Play("Pulse");
					break;
				case Status.ScanHuman:
					numberDisplayHuman.Text = "1";
					currentStatus = Status.ScanComplete;
					displayAnimationPlayer.Play("Checkmark");
					phaseDelayTimer.Start(ScanCompleteDelay);
					break;
				case Status.ScanComplete:
					currentStatus = Status.PreCheckGas;
					displayAnimationPlayer.Play("Load");
					lampGases.BlinkGreen();
					break;
				case Status.PreCheckGas:
					currentStatus = Status.CheckGasXe;
					displayAnimationPlayer.Play("Gases");
					CheckGasLamp(PipeTile.Gas.Xenon, gasLampXe);
					phaseDelayTimer.Start(CheckGasDelay);
					break;
				case Status.CheckGasXe:
					currentStatus = Status.CheckGasAr;
					CheckGasLamp(PipeTile.Gas.Argon, gasLampAr);
					phaseDelayTimer.Start(CheckGasDelay);
					break;
				case Status.CheckGasAr:
					currentStatus = Status.CheckGasO2;
					CheckGasLamp(PipeTile.Gas.Oxygen, gasLampO2);
					phaseDelayTimer.Start(CheckGasDelay);
					break;
				case Status.CheckGasO2:
					currentStatus = Status.CheckGasRn;
					CheckGasLamp(PipeTile.Gas.Radon, gasLampRn);
					phaseDelayTimer.Start(CheckGasDelay);
					break;
				case Status.CheckGasRn:
					currentStatus = Status.CheckGasCl;
					CheckGasLamp(PipeTile.Gas.Chlorine, gasLampCl);
					phaseDelayTimer.Start(CheckGasDelay);
					break;
				case Status.CheckGasCl:
					currentStatus = Status.CheckGasU;
					CheckGasLamp(PipeTile.Gas.Uranium, gasLampU);
					phaseDelayTimer.Start(CheckGasDelay);
					break;
				case Status.CheckGasU:
					currentStatus = Status.CheckGasFire;
					CheckGasLamp(PipeTile.Gas.Fire, gasLampFire);
					phaseDelayTimer.Start(CheckGasDelay);
					break;
				case Status.CheckGasFire:
					currentStatus = Status.CheckGasComplete;
					if (CheckGasBreathable())
					{
						lampHuman.SetLampGreen();
						lampGases.SetLampGreen();
						displayAnimationPlayer.Play("Checkmark");
						phaseDelayTimer.Start(CheckGasCompletedDelay);
					}
					else
					{
						currentStatus = Status.Off;
						lampHuman.PlayShortRedAlarm(true);
						lampGases.SetLampRed();
						displayAnimationPlayer.Play("Error");
						SetInteractable(true);
					}
					break;
				case Status.CheckGasComplete:
					currentStatus = Status.PreCheckFilter;
					displayAnimationPlayer.Play("Load");
					break;
				case Status.PreCheckFilter:
					currentStatus = Status.CheckFilter;
					displayAnimationPlayer.Play("Filter");
					lampFilter.BlinkGreen();
					phaseDelayTimer.Start(CheckFilterDelay);
					break;
				case Status.CheckFilter:
					if (FullFilterInserted)
					{
						currentStatus = Status.FullyOperational;
						displayAnimationPlayer.Play("Checkmark");
						lampFilter.SetLampGreen();
						SetPuzzleFinished();
					}
					else
					{
						currentStatus = Status.Off;
						displayAnimationPlayer.Play("Error");
						lampFilter.PlayShortRedAlarm(true);
						SetInteractable(true);
					}
					break;
				default:
					throw new Exception($"Unexpected currentStatus {currentStatus}");
			}
		}

		private void CheckGasLamp(PipeTile.Gas gas, LampAlarm gasLamp)
		{
			if (pipePuzzle.Output.HasFlag(gas))
			{
				gasLamp.SetLampGreen();
			}
			else
			{
				gasLamp.SetLampRed();
			}
		}

		private bool CheckGasBreathable()
		{
			return pipePuzzle.Output == PipeTile.Gas.Oxygen;
		}

		private void SetPuzzleFinished()
		{
			generatorHum.Play();
			inspectable.DisableInspection();
			Level.Instance.SetLifeSupportPuzzleComplete();
		}

		#endregion

		#region Events

		public void OnButtonInteractableClicked()
		{
			SetInteractable(false);

			if (currentStatus != Status.Off)
			{
				Log.Warning(this, "Button pressed while not in status Off!");
				currentStatus = Status.Off;
			}

			Reset();
			StartNextPhase();
		}

		public void OnDisplayAnimationPlayerAnimationFinished(StringName animnName)
		{
			if (animnName == "Load")
			{
				displayAnimationPlayer.Play("RESET");
				StartNextPhase();
			}
		}

		public void OnScannerAnimationPlayerAnimationFinished(StringName animName)
		{
			if (animName == "Pulse")
			{
				StartNextPhase();
			}
		}

		public void OnPhaseDelayTimerTimeout()
		{
			StartNextPhase();
		}

		public void OnEmptyFilterInteractableClicked()
		{
			filterAnimationPlayer.Play("RemoveEmptyFilter");
			emptyFilterInteractable.Hide();
		}

		public void OnFullFilterInteractableClicked()
		{
			if (Level.Instance.IsFilterObtained)
			{
				fullFilterInteractable.Hide();
				filterAnimationPlayer.Play("InsertFullFilter");
			}
		}

		public void OnFilterAnimationPlayerAnimationFinished(StringName animName)
		{
			if (animName == "RemoveEmptyFilter")
			{
				fullFilterInteractable.Show();
			}
			else if (animName == "InsertFullFilter")
			{
				FullFilterInserted = true;
			}
		}

		#endregion
	}
}