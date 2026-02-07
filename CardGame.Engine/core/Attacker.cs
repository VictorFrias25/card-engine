using System.Collections.Generic;
using CardGame.Engine.Models;

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
        public Element? BonusRequiredElement { get; set; }
        
        public double AbilityDamageMultiplier { get; set; }
        public int AbilityEnergyCost { get; set; }
        public int AbilityUnlockThreshold { get; set; }
        public int AbilitySpecificEnergyReq { get; set; }
        public ActiveAbilityType ActiveAbility { get; set; }
        
        public OnAttackEffectType OnAttackEffect { get; set; }
        public int OnAttackEffectChance { get; set; }
        
        public double PassiveEffectivenessMultiplier { get; set; } = 1.0; // Dynamic property
        
        public bool AllowMixedEnergy { get; set; }
        
        public Element? EnergyAffinity { get; set; } // Null = unset (for Neutral), specific element = locked
        public List<Card> AttachedEnergies { get; set; } = new List<Card>();
        
        // Status Effects
        public List<StatusEffect> StatusEffects { get; set; } = new List<StatusEffect>();

        public void ApplyStatus(string name, int value, int duration)
        {
            // Check if status already exists? Stack? For Burn, we might stack or refresh.
            // Let's allow stacking for now per user requirements (5dmg + 10dmg etc? Or just one instance?)
            // "fire attacker has chance to catch victim on fire for 3 turns"
            // If already burning?
            // Simple approach: Add new instance. If multiple burns, take multiple damage.
            StatusEffects.Add(new StatusEffect(name, value, duration));
        }

        public List<string> ProcessStatusEffects()
        {
            var logs = new List<string>();
            var toRemove = new List<StatusEffect>();

            foreach (var status in StatusEffects)
            {
                if (status.Name == "Burn")
                {
                    CurrentHealth -= status.Value;
                    if (CurrentHealth < 0) CurrentHealth = 0;
                    logs.Add($"{Name} takes {status.Value} Burn damage! (HP: {CurrentHealth})");
                }
                
                status.Duration--;
                if (status.Duration <= 0)
                {
                    toRemove.Add(status);
                    logs.Add($"{Name}'s {status.Name} expired.");
                }
            }
            
            foreach (var r in toRemove) StatusEffects.Remove(r);
            
            return logs;
        }

        // Constructor
        public Attacker(string name, Element element, int maxHealth, int defense, int baseAttack, 
                        int[] attackScaling = null, int maxEnergy = 4, 
                        int bonusChance = 0, int bonusAmount = 0, int bonusThreshold = 0,
                        double abilityMult = 1.0, int abilityCost = 0,
                        bool allowMixed = false, Element? bonusReqElement = null,
                        ActiveAbilityType activeAbility = ActiveAbilityType.None,
                        int abilityUnlockThreshold = 0, int abilitySpecificReq = 0,
                        OnAttackEffectType onAttackEffect = OnAttackEffectType.None,
                        int onAttackEffectChance = 0) 
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
            ActiveAbility = activeAbility;
            AbilityUnlockThreshold = abilityUnlockThreshold;
            AbilitySpecificEnergyReq = abilitySpecificReq;
            
            OnAttackEffect = onAttackEffect;
            OnAttackEffectChance = onAttackEffectChance;
            
            AllowMixedEnergy = allowMixed;
            BonusRequiredElement = bonusReqElement;

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
            
            if (AllowMixedEnergy) return true; // Mixed allows any energy
            
            if (EnergyAffinity.HasValue)
            {
                return energy.Element == EnergyAffinity.Value;
            }
            // If valid (Neutral and no affinity yet), calling code should set affinity
            return true;
        }

        public void Heal(int amount)
        {
            CurrentHealth += amount;
            if (CurrentHealth > MaxHealth) CurrentHealth = MaxHealth;
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
