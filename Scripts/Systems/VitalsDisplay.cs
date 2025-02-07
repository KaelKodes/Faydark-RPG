using Godot;
using System;

public partial class VitalsDisplay : Control
{
	private ProgressBar hpBar;
	private ProgressBar mpBar;
	private ProgressBar stBar;

	private Label hpLabel;
	private Label mpLabel;
	private Label stLabel;

	public override void _Ready()
	{
		GD.Print("🔄 Initializing Vitals Display...");

		// Get references to UI elements
		hpBar = GetNode<ProgressBar>("Bars/HP_Bar");
		mpBar = GetNode<ProgressBar>("Bars/MP_Bar");
		stBar = GetNode<ProgressBar>("Bars/ST_Bar");

		hpLabel = GetNode<Label>("VBoxContainer/HBoxContainer/HP_Label");
		mpLabel = GetNode<Label>("VBoxContainer/HBoxContainer2/MP_Label");
		stLabel = GetNode<Label>("VBoxContainer/HBoxContainer3/ST_Label");

		UpdateVitals();  // ✅ Set initial values
	}

	public override void _Process(double delta)
	{
		UpdateVitals();  // ✅ Keep vitals updated in real-time
	}

	private void UpdateVitals()
	{
		if (CharacterData.Instance == null) return;

		// Update Bars
		hpBar.Value = (CharacterData.Instance.CurrentHP / CharacterData.Instance.MaxHP) * 100;
		mpBar.Value = (CharacterData.Instance.CurrentMP / CharacterData.Instance.MaxMP) * 100;
		stBar.Value = (CharacterData.Instance.CurrentST / CharacterData.Instance.MaxST) * 100;

		// Update Labels (Format: "HP: 75 / 100")
		hpLabel.Text = $"HP: {CharacterData.Instance.CurrentHP} / {CharacterData.Instance.MaxHP}";
		mpLabel.Text = $"MP: {CharacterData.Instance.CurrentMP} / {CharacterData.Instance.MaxMP}";
		stLabel.Text = $"ST: {CharacterData.Instance.CurrentST} / {CharacterData.Instance.MaxST}";
	}
}
