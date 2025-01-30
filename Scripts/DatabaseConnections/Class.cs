using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

[Table("classes")]
public class Class
{
	[Key]
	public int Id { get; set; }

	public string ClassName { get; set; }

	// Stat Modifiers
	public int ModStr { get; set; }
	public int ModDex { get; set; }
	public int ModCon { get; set; }
	public int ModInt { get; set; }
	public int ModWis { get; set; }
	public int ModCha { get; set; }

	public int ModHealth { get; set; }
	public int ModMana { get; set; }
	public int ModStamina { get; set; }
	public int ModMovementSpeed { get; set; }
}
