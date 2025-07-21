using Godot;
using SpaceshipStartup.Scenes.Levels;
using System;

namespace SpaceshipStartup.Scenes.Interactables
{
	public partial class BackupGenerator : Inspectable
	{
		private enum Phase
		{
			Off,
			Init,
			ChargerOpen,
			ChargingPhase1,
			ChargingPhase2,
			ChargingPhase3,
			ChargingFinalCheck,
			FullyCharged
		}

		#region Private Properties

		private float OffEnergy { get; set; } = 0;
		private float PowerDownDuration { get; set; } = 1;
		private float OffToStartupDuration { get; set; } = 2.3f;
		private float StartupReadyEnergy { get; set; } = 10;
		private float ChargePhase1Energy { get; set; } = 25;
		private float ChargePhase1Duration { get; set; } = 4f;
		private float ChargePhase2Energy { get; set; } = 50;
		private float ChargePhase2Duration { get; set; } = 2.5f;
		private float ChargePhase3Energy { get; set; } = 75;
		private float ChargePhase3Duration { get; set; } = 2.5f;
		private float FullyChargedEnergy { get; set; } = 100;
		private float FullyChargedDuration { get; set; } = 3f;
		private float TemperatureSafeDuration { get; set; } = 14f;
		private float TemperatureSafeGaugeValue { get; set; } = 0.7f;
		private float TemperatureDangerousDuration { get; set; } = 6f;
		private float TemperatureDangerousGaugeValue { get; set; } = 1f;

		private float ChargePhase3PowerOutput { get; set; } = 0.2f;
		private float FullyChargedPowerOutput { get; set; } = 0.5f;
		private float EnergySwitchPowerConsumption { get; set; } = 0.2f;
		private float LifeSupportSwitchPowerConsumption { get; set; } = 0.1f;
		private float CoolingSwitchPowerConsumption { get; set; } = 0.05f;
		private float DecorativeSwitch1PowerConsumption { get; set; } = 0.08f;
		private float DecorativeSwitch2PowerConsumption { get; set; } = 0.05f;
		private float DecorativeSwitch3PowerConsumption { get; set; } = 0.02f;

		private float FanRotationSpeedRad { get; set; } = -0.2f;

		#endregion

		#region Components & Nodes

		// Visuals
		private Sprite3D mainPowerSwitchOff;
		private Sprite3D mainPowerSwitchOn;
		private TextureProgressBar powerProgressBar;
		private Gauge inputGauge;
		private Gauge distributionGauge;
		private Gauge temperatureGauge;
		private Node3D fanPropeller;

		// Audio
		private AudioStreamPlayer3D mainPowerSwitchAudio;
		private AudioStreamPlayer3D generatorHumAudio;
		private AudioStreamPlayer3D powerOnAudio;

		// Controls
		private Interactable3D chargerInteractable;
		private AnimationPlayer animationPlayer;
		private Inspectable inspectable;

		// Switches
		private Switch energySwitch;
		private Switch lifeSupportSwitch;
		private Switch coolingSwitch;
		private Switch decorative1Switch;
		private Switch decorative2Switch;
		private Switch decorative3Switch;

		#endregion

		#region Private Properties

		private Phase CurrentStatus = Phase.Off;
		private Tween powerTween;
		private Tween temperatureTween;

		#endregion

		#region Method Overrides

		public override void _Ready()
		{
			base._Ready();

			// Visuals
			mainPowerSwitchOff = GetNode<Sprite3D>("Main Power Switch OFF");
			mainPowerSwitchOn = GetNode<Sprite3D>("Main Power Switch ON");
			powerProgressBar = GetNode<TextureProgressBar>("Center Energy/SubViewport/TextureProgressBar");
			inputGauge = GetNode<Gauge>("InputGauge");
			distributionGauge = GetNode<Gauge>("DistributionGauge");
			temperatureGauge = GetNode<Gauge>("TemperatureGauge");
			fanPropeller = GetNode<Node3D>("FanPropellerPivot");

			// Audio
			mainPowerSwitchAudio = GetNode<AudioStreamPlayer3D>("Audio/MainPowerSwitch");
			generatorHumAudio = GetNode<AudioStreamPlayer3D>("Audio/GeneratorHum");
			powerOnAudio = GetNode<AudioStreamPlayer3D>("Audio/PowerOn");

			// Controls
			chargerInteractable = GetNode<Interactable3D>("Charger Background/Interactable3D");
			animationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");

			// Switches
			energySwitch = GetNode<Switch>("Switches Holder/EnergySwitch");
			lifeSupportSwitch = GetNode<Switch>("Switches Holder/LifeSupportSwitch");
			coolingSwitch = GetNode<Switch>("Switches Holder/CoolingSwitch");
			decorative1Switch = GetNode<Switch>("Switches Holder/DecorativeSwitch1");
			decorative2Switch = GetNode<Switch>("Switches Holder/DecorativeSwitch2");
			decorative3Switch = GetNode<Switch>("Switches Holder/DecorativeSwitch3");

			animationPlayer.Play("ShieldingClosed");
		}

		public override void _Process(double delta)
		{
			if (CurrentStatus == Phase.FullyCharged && coolingSwitch.SwitchedOn)
			{
				fanPropeller.RotateZ(FanRotationSpeedRad);
			}
		}

		public override void _PhysicsProcess(double delta)
		{
			base._PhysicsProcess(delta);
		}

		#endregion

		#region Private Methods

		private void SetMainPower(bool turnedOn)
		{
			if (turnedOn && CurrentStatus != Phase.Off)
			{
				Log.Warning(this, $"Main power turned on from status {CurrentStatus}!");
			}

			CloseCharger();
			CurrentStatus = turnedOn ? Phase.Init : Phase.Off;
			mainPowerSwitchOff.Visible = !turnedOn;
			mainPowerSwitchOn.Visible = turnedOn;
			mainPowerSwitchAudio.Play();
			generatorHumAudio.Stop();

			StartPowerTween(turnedOn ? StartupReadyEnergy : OffEnergy, turnedOn ? OffToStartupDuration : PowerDownDuration);
			CalculatePowerDistribution();

			if (turnedOn)
			{
				powerTween.Finished += OnChargeReady;
				inputGauge.SetValue(0.3f, true, 0.4f);
				powerOnAudio.Play();
			}
			else
			{
				TurnOffSwitches();
				ResetGauges();
			}
		}

		/// <summary>
		/// Checks whether the secondary switches (life support, cooling, decorative 1, 2 and 3) would impact the generator in its current status.
		/// If necessary, shuts off the charge and sets the switches' alarms or lights accordingly.
		/// </summary>
		/// <returns>Returns true if the switches are fine and false if the switches have caused a shutoff.</returns>
		private bool CheckSecondarySwitches(Switch flickedSwitch)
		{
			// Off, init or phase 1: ignore
			// phase 2 & 3: Anything secondary will cause a shutoff
			// Fully charged: doesn't matter, unless I implement a breakable crystal

			CalculatePowerDistribution();

			switch (CurrentStatus)
			{
				case Phase.Off:
				case Phase.Init:
				case Phase.ChargerOpen:
				case Phase.ChargingPhase1:
					if (flickedSwitch != null && !flickedSwitch.SwitchedOn)
					{
						flickedSwitch.TurnOff();
					}
					return true;
				case Phase.ChargingPhase2:
				case Phase.ChargingPhase3:
				case Phase.ChargingFinalCheck:
					if (lifeSupportSwitch.SwitchedOn)
					{
						ChargeShutoff();
						lifeSupportSwitch.PlayShortRedAlarm();
					}
					else if (coolingSwitch.SwitchedOn)
					{
						ChargeShutoff();
						coolingSwitch.PlayShortRedAlarm();
					}
					else if (decorative1Switch.SwitchedOn)
					{
						ChargeShutoff();
						decorative1Switch.PlayShortRedAlarm();
					}
					else if (decorative2Switch.SwitchedOn)
					{
						ChargeShutoff();
						decorative2Switch.PlayShortRedAlarm();
					}
					else if (decorative3Switch.SwitchedOn)
					{
						ChargeShutoff();
						decorative3Switch.PlayShortRedAlarm();
					}
					else
					{
						return true;
					}

					return false;
				case Phase.FullyCharged:
					if (flickedSwitch == null)
						return true;

					if (flickedSwitch.SwitchedOn)
					{
						flickedSwitch.SetLampGreen();
					}
					else
					{
						flickedSwitch.TurnOff();
					}

					CalculatePowerDistribution();

					return true;
				default:
					throw new Exception($"Unhandled phase {CurrentStatus}!");
			}
		}

		private void CheckTemperature()
		{
			if (CurrentStatus != Phase.FullyCharged)
				return;

			if (coolingSwitch.SwitchedOn)
			{
				temperatureGauge.SetValue(TemperatureSafeGaugeValue);
				coolingSwitch.SetLampGreen();
			}
			else
			{
				temperatureGauge.SetValue(TemperatureDangerousGaugeValue, true, TemperatureDangerousDuration);
				coolingSwitch.PlayGreenAlarm();
			}
		}

		private void CheckPuzzleFinished()
		{
			if (CurrentStatus == Phase.FullyCharged &&
				energySwitch.SwitchedOn &&
				lifeSupportSwitch.SwitchedOn &&
				coolingSwitch.SwitchedOn &&
				decorative1Switch.SwitchedOn &&
				decorative2Switch.SwitchedOn &&
				decorative3Switch.SwitchedOn)
			{
				DisableInspection();
				Level.Instance.SetGeneratorPuzzleComplete();
			}
		}

		private void TurnOffSwitches()
		{
			energySwitch.TurnOff();
			lifeSupportSwitch.TurnOff();
			coolingSwitch.TurnOff();
			decorative1Switch.TurnOff();
			decorative2Switch.TurnOff();
			decorative3Switch.TurnOff();
		}

		private void ResetGauges()
		{
			inputGauge.SetValue(0);
			distributionGauge.SetValue(0.5f);
			temperatureGauge.SetValue(0);
		}

		private void CalculatePowerDistribution()
		{
			float powerGeneration = CurrentStatus switch
			{
				Phase.Off or Phase.Init or Phase.ChargerOpen or Phase.ChargingPhase1 or Phase.ChargingPhase2 => 0f,
				Phase.ChargingPhase3 or Phase.ChargingFinalCheck => ChargePhase3PowerOutput,
				Phase.FullyCharged => FullyChargedPowerOutput,
				_ => throw new Exception($"Unhandled phase {CurrentStatus}!"),
			};

			float powerConsumption = CurrentStatus == Phase.Off ? 0 : // Off -> no consumption
				(energySwitch.SwitchedOn ? EnergySwitchPowerConsumption : 0) // other -> cumulative consumption of all on switches
				+ (lifeSupportSwitch.SwitchedOn ? LifeSupportSwitchPowerConsumption : 0)
				+ (coolingSwitch.SwitchedOn ? CoolingSwitchPowerConsumption: 0)
				+ (decorative1Switch.SwitchedOn ? DecorativeSwitch1PowerConsumption : 0)
				+ (decorative2Switch.SwitchedOn ? DecorativeSwitch2PowerConsumption : 0)
				+ (decorative3Switch.SwitchedOn ? DecorativeSwitch3PowerConsumption : 0);

			distributionGauge.SetValue(Mathf.Clamp(0.5f + powerGeneration - powerConsumption, 0, 1));

			Log.Debug(this, $"0.5 + generation - consumption: 0.5 + {powerGeneration} - {powerConsumption} = {0.5f + powerGeneration - powerConsumption}");
		}

		/// <summary>
		/// Sets a tween for the center power circle progress bar. Kills any already existing power tween.
		/// </summary>
		/// <param name="target">Target energy</param>
		/// <param name="duration">Duration of the tween</param>
		private void StartPowerTween(float target, float duration)
		{
			if (powerTween != null && powerTween.IsRunning())
				powerTween.Kill();

			powerTween = GetTree().CreateTween();
			powerTween.TweenProperty(
				powerProgressBar,
				"value",
				target,
				duration);
		}

		#endregion

		#region Charge Phase

		/// <summary>
		/// Stops anything related to the Charging phases and transitions to the Off phase.
		/// </summary>
		private void ChargeShutoff()
		{
			CurrentStatus = Phase.Off;
			animationPlayer.Stop(); // Otherwise charge audio would keep playing.
			animationPlayer.Play("ChargeShutoff");
			CloseCharger();
			SetMainPower(false);
		}

		private void ChargeShutoffEnergyProblem()
		{
			ChargeShutoff();
			energySwitch.PlayShortRedAlarm();
		}

		/// <summary>
		/// Closes the charger and prevents interaction. Doesn't change the <see cref="CurrentStatus"/>.
		/// Needs to be called before the <see cref="CurrentStatus"/> changes to something non-charger related because I can't be bothered to program this properly in a one week timeframe.
		/// </summary>
		private void CloseCharger()
		{
			if (CurrentStatus != Phase.ChargerOpen && CurrentStatus != Phase.ChargingPhase1 && CurrentStatus != Phase.ChargingPhase2 && CurrentStatus != Phase.ChargingFinalCheck)
				return;

			chargerInteractable.Hide();
			animationPlayer.Play("CloseShielding");
		}

		/// <summary>
		/// Opens the charger and allows interaction. Doesn't change the <see cref="CurrentStatus"/>.
		/// Needs to be called before the <see cref="CurrentStatus"/> changes to something charger related.
		/// </summary>
		private void OpenCharger()
		{
			if (CurrentStatus == Phase.ChargerOpen || CurrentStatus == Phase.ChargingPhase1 || CurrentStatus == Phase.ChargingPhase2 || CurrentStatus == Phase.ChargingFinalCheck)
				return;

			chargerInteractable.Show();
			animationPlayer.Play("OpenShielding");
		}

		/// <summary>
		/// Transitions from the charging phase to the fully charged phase.
		/// </summary>
		private void FullyCharged()
		{
			if (CurrentStatus == Phase.FullyCharged)
				return;

			CloseCharger();
			CurrentStatus = Phase.FullyCharged;
			generatorHumAudio.Play();
			StartPowerTween(FullyChargedEnergy, FullyChargedDuration);
			CalculatePowerDistribution();
		}

		public void OnChargePhase1Finished()
		{
			CurrentStatus = Phase.ChargingPhase2;

			// Check phase 2 requirements
			if (energySwitch.SwitchedOn)
			{
				ChargeShutoffEnergyProblem();
				return;
			}
			else if (!CheckSecondarySwitches(null)) // Shutoff if any secondary switch is on
			{
				return;
			}

			// All checks good
			StartPowerTween(ChargePhase2Energy, ChargePhase2Duration);
			powerTween.Finished += OnChargePhase2Finished;
		}

		public void OnChargePhase2Finished()
		{
			CurrentStatus = Phase.ChargingPhase3;
			StartPowerTween(ChargePhase3Energy, ChargePhase3Duration);
			powerTween.Finished += OnChargePhase3Finished;

			CalculatePowerDistribution();
		}

		private void OnChargePhase3Finished()
		{
			CurrentStatus = Phase.ChargingFinalCheck;

			if (!energySwitch.SwitchedOn)
			{
				energySwitch.PlayGreenAlarm();
			}
		}

		#endregion

		#region Events

		public void OnAnimationFinished(StringName animationName)
		{
			switch (animationName)
			{
				case "OpenShielding":
					chargerInteractable.Show();
					break;
				case "Charging":
					if (energySwitch.SwitchedOn)
					{
						FullyCharged();
					}
					else
					{
						ChargeShutoff(); // Animation finished -> other processes didn't finish charge -> shutoff
					}

					break;
			}
		}

		public void OnChargeClicked()
		{
			if (CurrentStatus != Phase.ChargerOpen)
			{
				Log.Warning(this, "Charger clicked with closed shielding!");
				return;
			}

			chargerInteractable.Hide();
			CurrentStatus = Phase.ChargingPhase1;
			animationPlayer.Play("Charging");
			StartPowerTween(ChargePhase1Energy, ChargePhase1Duration);
			temperatureGauge.SetValue(TemperatureSafeGaugeValue, true, TemperatureSafeDuration);
			powerTween.Finished += OnChargePhase1Finished;
		}

		public void OnEnergySwitchToggled(bool switchedOn)
		{
			CalculatePowerDistribution();

			switch (CurrentStatus)
			{
				case Phase.Off:
				case Phase.Init:
				case Phase.ChargerOpen:
				case Phase.ChargingPhase1:
					break;
				case Phase.ChargingPhase2:
					if (switchedOn)
					{
						ChargeShutoffEnergyProblem();
					}
					break;
				case Phase.ChargingPhase3:
					if (switchedOn)
					{
						energySwitch.SetLampGreen();
					}
					else
					{
						energySwitch.TurnOff();
					}
					break;
				case Phase.ChargingFinalCheck:
					if (switchedOn)
					{
						energySwitch.SetLampGreen();
					}
					else
					{
						energySwitch.PlayGreenAlarm();
					}
					break;
				case Phase.FullyCharged:
					if (switchedOn)
					{
						energySwitch.SetLampGreen();
					}
					else
					{
						energySwitch.TurnOff();
						SetMainPower(false);
					}
					break;
				default:
					throw new NotImplementedException($"Energy toggle for current status {CurrentStatus} not implemented");
			}
		}

		public void OnLifeSupportSwitchToggled(bool _)
		{
			CheckSecondarySwitches(lifeSupportSwitch);
			CheckPuzzleFinished();
		}

		public void OnCoolingSwitchToggled(bool _)
		{
			CheckSecondarySwitches(coolingSwitch);
			CheckTemperature();
			CheckPuzzleFinished();
		}

		public void OnDecorativeSwitch1Toggled(bool _)
		{
			CheckSecondarySwitches(decorative1Switch);
			CheckPuzzleFinished();
		}

		public void OnDecorativeSwitch2Toggled(bool _)
		{
			CheckSecondarySwitches(decorative2Switch);
			CheckPuzzleFinished();
		}

		public void OnDecorativeSwitch3Toggled(bool _)
		{
			CheckSecondarySwitches(decorative3Switch);
			CheckPuzzleFinished();
		}

		public void OnMainPowerSwitchOffClicked()
		{
			SetMainPower(true);
		}

		public void OnMainPowerSwitchOnClicked()
		{
			SetMainPower(false);
		}

		public void OnChargeReady()
		{
			OpenCharger();
			CurrentStatus = Phase.ChargerOpen;
		}

		public void OnTemperatureGaugeSetValueFinished(float target)
		{
			if (coolingSwitch.SwitchedOn)
				return;

			if (target == TemperatureSafeGaugeValue)
			{
				CheckTemperature();
			}
			else if (target == TemperatureDangerousGaugeValue)
			{
				SetMainPower(false);
				animationPlayer.Play("FullyChargedShutoff");
			}
		}

		#endregion
	}
}