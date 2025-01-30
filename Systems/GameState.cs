using Godot;
using MySqlConnector;
using System;
using System.Collections.Generic;

public partial class GameState : Node  // ✅ Fix: Added "partial"
{
	public CharacterData CurrentCharacter { get; private set; } = new CharacterData();
	private const string CONNECTION_STRING = "Server=localhost;Database=faydark_db;User=root;Password=;";

	public void SaveCharacterToDatabase()
	{
		using (var connection = new MySqlConnection(CONNECTION_STRING))
		{
			try
			{
				connection.Open();
				GD.Print("✅ Connected to MySQL - Saving Character...");

				string query = @"
					INSERT INTO players (CharacterName, Class, Personality, PosX, PosY, PosZ, Zone)
					VALUES (@name, @class, @personality, @posX, @posY, @posZ, @zone);
				";

				using (var command = new MySqlCommand(query, connection))
				{
					command.Parameters.AddWithValue("@name", CurrentCharacter.CharacterName);
					command.Parameters.AddWithValue("@class", CurrentCharacter.SelectedClass);
					command.Parameters.AddWithValue("@personality", CurrentCharacter.SelectedPersonality);
					command.Parameters.AddWithValue("@posX", CurrentCharacter.Position.X);
					command.Parameters.AddWithValue("@posY", CurrentCharacter.Position.Y);
					command.Parameters.AddWithValue("@posZ", CurrentCharacter.Position.Z);
					command.Parameters.AddWithValue("@zone", CurrentCharacter.CurrentZone);

					command.ExecuteNonQuery();
					GD.Print("💾 Character saved to database!");
				}
			}
			catch (Exception ex)
			{
				GD.PrintErr("❌ Failed to save character: ", ex.Message);
			}
		}
	}
}
