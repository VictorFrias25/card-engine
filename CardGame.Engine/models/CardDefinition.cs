using CardGame.Engine.Core;

namespace CardGame.Engine.Models
{
    public class CardDefinition
    {
        public string Name { get; set; }
        public Element Element { get; set; }
        public CardType Type { get; set; }
        
        // Attacker Stats
        public int MaxHealth { get; set; }
        public int Defense { get; set; }
        public int BaseAttack { get; set; }
        public int[] AttackScaling { get; set; } = Array.Empty<int>(); // Bonus damage per energy step
        public int MaxEnergy { get; set; } = 4;
        
        // RNG / Bonus Damage Logic
        public int BonusDamageChance { get; set; } // 0-100
        public int BonusDamageAmount { get; set; }
        public int BonusDamageThreshold { get; set; } // Energy count required
        
        // Active Ability Logic (Costly ultimate)
        public double AbilityDamageMultiplier { get; set; } = 1.0;
        public int AbilityEnergyCost { get; set; }
        // public int EnergyReq { get; set; } // If we implement energy requirement

        // Support/Landscape Properties could go here or be in a dictionary
        public int SupportHealth { get; set; }
        
        public CardDefinition(string name, Element element, CardType type)
        {
            Name = name;
            Element = element;
            Type = type;
        }
    }
}
