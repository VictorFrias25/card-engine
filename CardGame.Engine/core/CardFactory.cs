using System;
using CardGame.Engine.Models;
using CardGame.Engine.Core;

namespace CardGame.Engine.Core
{
    public static class CardFactory
    {
        public static Card CreateCard(CardDefinition def)
        {
            switch (def.Type)
            {
                case CardType.Attacker:
                    return new Attacker(def.Name, def.Element, def.MaxHealth, def.Defense, def.BaseAttack, 
                        def.AttackScaling, def.MaxEnergy, 
                        def.BonusDamageChance, def.BonusDamageAmount, def.BonusDamageThreshold,
                        def.AbilityDamageMultiplier, def.AbilityEnergyCost);
                case CardType.Support:
                    // Assuming all supports from library are "Active" for now, or check def properties
                    bool isActive = def.SupportHealth > 0;
                    return new Support(def.Name, def.Element, isActive, def.SupportHealth);
                case CardType.Landscape:
                    return new Landscape(def.Name); // Stats?
                case CardType.Energy:
                    return new Energy(def.Element);
                default:
                    throw new ArgumentException("Unknown card type");
            }
        }

        public static Card CreateCard(string name)
        {
            var def = CardLibrary.GetCard(name);
            if (def == null) throw new ArgumentException($"Card definition not found: {name}");
            return CreateCard(def);
        }
    }
}
