using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

[Table("character")]
public class Character
{
	[Key]
	public int Id { get; set; }

	public string Name { get; set; }
	public int Level { get; set; }
	public int Experience { get; set; }
	public int ExpToNextLevel { get; set; }

	// Vitals
	public int Health { get; set; }
	public int Mana { get; set; }
	public int Stamina { get; set; }
	public int MovementSpeed { get; set; }
	public int HpRegen { get; set; }
	public int MpRegen { get; set; }
	public int StRegen { get; set; }
	public int Weight { get; set; }

	// Offense
	public int AttackDamage { get; set; }
	public int RangedDamage { get; set; }
	public int SpellDamage { get; set; }
	public int AttackRange { get; set; }
	public float AttackSpeed { get; set; }
	public float CastSpeed { get; set; }
	public string WeaponType { get; set; }
	public int HitChanceBonus { get; set; }

	// Defense
	public int ArmorClass { get; set; }
	public string ArmorType { get; set; }
	public int Defense { get; set; }
	public int Dodge { get; set; }
	public int Block { get; set; }
	public int Parry { get; set; }

	// Stats
	public int Agility { get; set; }
	public int Constitution { get; set; }
	public int Dexterity { get; set; }
	public int Intelligence { get; set; }
	public int Strength { get; set; }
	public int Wisdom { get; set; }
	public int Charisma { get; set; }

	// Mental Stats
	public float Aggression { get; set; }
	public string WeaponPreference { get; set; }
	public string AttackStyle { get; set; }
	public float Distance { get; set; }
	public float ExplorationFocus { get; set; }
	public float Revealing { get; set; }
	public float LootVsMonsters { get; set; }
	public float ItemPriority { get; set; }
	public float UpgradeFocus { get; set; }
	public float HealingPriority { get; set; }
	public float HealingUse { get; set; }
	public float HealingItemVsSpell { get; set; }
	public float ResourceEfficiency { get; set; }
	public float HazardAvoidance { get; set; }
	public float AmbushReaction { get; set; }
	public float TreasurePriority { get; set; }
	public float Courage { get; set; }
	public float Subservience { get; set; }
}
