using System.Collections.Generic;

namespace CardGame.Engine.Core
{
    public class Attacker : Card
    {
        public int MaxHealth { get; set; }
        public int CurrentHealth { get; set; }
        public int Defense { get; set; }
        public int BaseAttack { get; set; }
        public int[] AttackScaling { get; set; }
        public int MaxEnergy { get; set; }
        
        public int BonusDamageChance { get; set; }
        public int BonusDamageAmount { get; set; }
        public int BonusDamageThreshold { get; set; }
        
        public double AbilityDamageMultiplier { get; set; }
        public int AbilityEnergyCost { get; set; }
        
        public Element? EnergyAffinity { get; set; } // Null = unset (for Neutral), specific element = locked
        public List<Card> AttachedEnergies { get; set; } = new List<Card>();

        // Constructor
        public Attacker(string name, Element element, int maxHealth, int defense, int baseAttack, 
                        int[] attackScaling = null, int maxEnergy = 4, 
                        int bonusChance = 0, int bonusAmount = 0, int bonusThreshold = 0,
                        double abilityMult = 1.0, int abilityCost = 0) 
            : base(name, element, CardType.Attacker)
        {
            MaxHealth = maxHealth;
            CurrentHealth = maxHealth;
            Defense = defense;
            BaseAttack = baseAttack;
            AttackScaling = attackScaling ?? Array.Empty<int>();
            MaxEnergy = maxEnergy;
            
            BonusDamageChance = bonusChance;
            BonusDamageAmount = bonusAmount;
            BonusDamageThreshold = bonusThreshold;
            
            AbilityDamageMultiplier = abilityMult;
            AbilityEnergyCost = abilityCost;

            // If the card itself is elemental (e.g. Fire Attacker), affinity is locked from start
            // If Neutral, it starts as null
            if (element != Element.Neutral)
            {
                EnergyAffinity = element;
            }
        }

        public int GetCurrentAttack()
        {
            int scalingBonus = 0;
            for (int i = 0; i < AttachedEnergies.Count; i++)
            {
                if (i < AttackScaling.Length)
                {
                    scalingBonus += AttackScaling[i];
                }
            }
            return BaseAttack + scalingBonus;
        }

        public bool CanAttachEnergy(Energy energy)
        {
            if (AttachedEnergies.Count >= MaxEnergy) return false;
            
            if (EnergyAffinity.HasValue)
            {
                return energy.Element == EnergyAffinity.Value;
            }
            // If valid (Neutral and no affinity yet), calling code should set affinity
            return true;
        }

        public void TakeDamage(int damage)
        {
            CurrentHealth -= damage;
            if (CurrentHealth < 0) CurrentHealth = 0;
        }

        public bool IsKO()
        {
            return CurrentHealth <= 0;
        }
    }
}
