using System;
using System.Data;
using Godot;
using MySqlConnector;

public partial class DatabaseManager : Node
{
	private const string CONNECTION_STRING = "Server=localhost;Database=Faydark_db;User=root;Password=;";

	public override void _Ready()
	{
		TestDatabaseConnection();
	}

	private void TestDatabaseConnection()
	{
		using (var connection = new MySqlConnection(CONNECTION_STRING))
		{
			try
			{
				connection.Open();
				GD.Print("MySQL Connection Successful!");
			}
			catch (Exception ex)
			{
				GD.PrintErr("MySQL Connection Failed: ", ex.Message);
			}
		}
	}
}
