using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

[Table("players")]
public class Player
{
	[Key]
	public int Id { get; set; }

	public string Username { get; set; }
	public int ClassId { get; set; }

	public int Str { get; set; }
	public int Dex { get; set; }
	public int Con { get; set; }
	public int IntStat { get; set; }
	public int Wis { get; set; }
	public int Cha { get; set; }

	public int Health { get; set; }
	public int Mana { get; set; }
	public int Stamina { get; set; }
	public int MovementSpeed { get; set; }

	public int Experience { get; set; }
	public int Level { get; set; }
	public int Gold { get; set; }

	[ForeignKey("ClassId")]
	public Class Class { get; set; }
}
