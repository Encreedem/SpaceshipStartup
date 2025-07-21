using Godot;
using SpaceshipStartup.Scenes.Levels;
using System;

namespace SpaceshipStartup.Scenes.UI
{
	public partial class MouseControls : Control
	{
		#region Components & Nodes

		private Control moveForward;
		private Control turnLeft;
		private Control turnRight;
		private Control goBack;
		private AnimationPlayer moveForwardHighlighter;
		private AnimationPlayer turnHighlighter;
		private AnimationPlayer goBackHighlighter;
		private Control tutorialWindow;
		private Label tutorialText;
		private Control winDialog;

		#endregion

		#region Public Properties

		public Control HoveredControl { get; private set; }

		#endregion

		#region Private Variables

		private int tutorialStep = 0;

		#endregion

		#region Method Overrides

		public override void _Ready()
		{
			moveForward = GetNode<Control>("MoveForward");
			turnLeft = GetNode<Control>("TurnLeft");
			turnRight = GetNode<Control>("TurnRight");
			goBack = GetNode<Control>("GoBack");
			moveForwardHighlighter = GetNode<AnimationPlayer>("MoveForward/MoveForwardHighlighter");
			turnHighlighter = GetNode<AnimationPlayer>("TurnHighlighter");
			goBackHighlighter = GetNode<AnimationPlayer>("GoBack/GoBackHighlighter");

			tutorialWindow = GetNode<Control>("Tutorial");
			tutorialText = GetNode<Label>("Tutorial/TutorialBox/Label");

			winDialog = GetNode<Control>("WinDialog");

			SetControlsMovement();
		}
		
		#endregion

		#region Public Methods

		public Interaction GetInteraction()
		{
			if (HoveredControl == moveForward)
				return Interaction.MoveForward;
			else if (HoveredControl == turnLeft)
				return Interaction.TurnLeft;
			else if (HoveredControl == turnRight)
				return Interaction.TurnRight;
			else if (HoveredControl == goBack)
				return Interaction.GoBack;
			else
				return Interaction.None;
		}

		/// <summary>
		/// Enables and disables screen mouse controls appropriate with the player currently inspecting something.
		/// </summary>
		public void SetControlsInspecting()
		{
			turnLeft.Hide();
			turnRight.Hide();
			goBack.Show();
		}

		/// <summary>
		/// Enables and disables screen mouse controls appropriate with the player being able to move around.
		/// </summary>
		public void SetControlsMovement()
		{
			turnLeft.Show();
			turnRight.Show();
			goBack.Hide();
		}

		public void SetMoveForward(bool enabled)
		{
			moveForward.Visible = enabled;
		}

		public void HighlightGoBack()
		{
			goBackHighlighter.Play("HighlightGoBack");
		}

		public void StartTutorial()
		{
			tutorialWindow.Show();
		}

		public void ShowWinDialog()
		{
			winDialog.Show();
		}

		#endregion

		#region Private Methods

		private void RemoveHoveredControl(Control control)
		{
			if (HoveredControl == control)
				HoveredControl = null;
		}

		#endregion

		#region Events

		public void OnMoveForwardGuiInput(InputEvent @event)
		{
			if (@event is InputEventMouseButton mouseButton && mouseButton.ButtonIndex == MouseButton.Left && mouseButton.Pressed == true)
			{
				Player.Instance.MoveForward();
			}
		}

		public void OnMoveForwardMouseEntered()
		{
			Level.Instance.SetCursorIcon(Interaction.MoveForward);
			HoveredControl = moveForward;
		}

		public void OnMoveForwardMouseExited()
		{
			Level.Instance.SetCursorIcon(Interaction.None);
			RemoveHoveredControl(moveForward);
		}

		public void OnTurnLeftGuiInput(InputEvent @event)
		{
			if (@event is InputEventMouseButton mouseButton && mouseButton.ButtonIndex == MouseButton.Left && mouseButton.Pressed == true)
			{
				Player.Instance.TurnLeft();
			}
		}

		public void OnTurnLeftMouseEntered()
		{
			Level.Instance.SetCursorIcon(Interaction.TurnLeft);
			HoveredControl = turnLeft;
		}

		public void OnTurnLeftMouseExited()
		{
			Level.Instance.SetCursorIcon(Interaction.None);
			RemoveHoveredControl(turnLeft);
		}

		public void OnTurnRightGuiInput(InputEvent @event)
		{
			if (@event is InputEventMouseButton mouseButton && mouseButton.ButtonIndex == MouseButton.Left && mouseButton.Pressed == true)
			{
				Player.Instance.TurnRight();
			}
		}

		public void OnTurnRightMouseEntered()
		{
			Level.Instance.SetCursorIcon(Interaction.TurnRight);
			HoveredControl = turnRight;
		}

		public void OnTurnRightMouseExited()
		{
			Level.Instance.SetCursorIcon(Interaction.None);
			RemoveHoveredControl(turnRight);
		}

		public void OnGoBackGuiInput(InputEvent @event)
		{
			if (@event is InputEventMouseButton mouseButton && mouseButton.ButtonIndex == MouseButton.Left && mouseButton.Pressed == true)
			{
				if (goBackHighlighter.IsPlaying())
				{
					goBackHighlighter.Stop();
				}

				Level.Instance.ExitCurrentInspectable();
			}
		}

		public void OnGoBackMouseEntered()
		{
			Level.Instance.SetCursorIcon(Interaction.GoBack);
			HoveredControl = goBack;
		}

		public void OnGoBackMouseExited()
		{
			Level.Instance.SetCursorIcon(Interaction.None);
			RemoveHoveredControl(goBack);
		}

		public void OnConfirmPressed()
		{
			switch (tutorialStep++)
			{
				case 0:
					tutorialText.Text = "Click the sides of the screen or press A or D or arrow keys to turn.";
					turnHighlighter.Play("HighlightTurn");
					break;
				case 1:
					tutorialText.Text = "Click the center or press W or the forward arrow key to move forward.";
					turnHighlighter.Play("RESET");
					moveForward.Show();
					moveForwardHighlighter.Play("HighlightMoveForward");
					break;
				case 2:
					tutorialText.Text = "Click the bottom of the screen to return from inspecting an object.";
					moveForwardHighlighter.Play("RESET");
					moveForward.Hide();
					goBack.Show();
					goBackHighlighter.Play("HighlightGoBack");
					break;
				case 3:
					tutorialText.Text = "Uh oh...";
					Level.Instance.OnTutorialAlmostCompleted();
					goBack.Hide();
					goBackHighlighter.Play("RESET");
					break;
				case 4:
					tutorialWindow.Hide();
					Level.Instance.OnTutorialCompleted();
					break;
			}
		}

		#endregion
	}
}