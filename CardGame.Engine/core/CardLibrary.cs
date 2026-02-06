using System.Collections.Generic;
using CardGame.Engine.Core;
using CardGame.Engine.Models;

namespace CardGame.Engine.Core
{
    public static class CardLibrary
    {
        public static List<CardDefinition> Definitions { get; set; } = new List<CardDefinition>();

        static CardLibrary()
        {
            LoadDefaultCards();
        }

        public static void LoadDefaultCards()
        {
            Definitions.Clear();

            // --- Attackers ---
            
            // Grunt (Neutral)
            Definitions.Add(new CardDefinition("Grunt", Element.Neutral, CardType.Attacker)
            {
                MaxHealth = 100,
                Defense = 50,
                BaseAttack = 10,
                AttackScaling = new[] { 10, 10, 10, 10 },
                MaxEnergy = 4
            });

            // Minion (Neutral)
            Definitions.Add(new CardDefinition("Minion", Element.Neutral, CardType.Attacker)
            {
                MaxHealth = 60,
                Defense = 30,
                BaseAttack = 30,
                AttackScaling = new[] { 5, 5, 10 },
                MaxEnergy = 3,
                BonusDamageChance = 50,
                BonusDamageAmount = 15,
                BonusDamageThreshold = 3
            });

            // Tank (Neutral)
            Definitions.Add(new CardDefinition("Tank", Element.Neutral, CardType.Attacker)
            {
                MaxHealth = 150,
                Defense = 75,
                BaseAttack = 5,
                AttackScaling = new[] { 5, 5, 10, 15 },
                MaxEnergy = 4,

                // Active Ability: x2 Damage, Discard 2 Energy
                // Uses same "Threshold" logic or implies MaxEnergy? Rules say "On 4th energy gains ability"
                // So Threshold = 4 (or MaxEnergy)
                BonusDamageThreshold = 4, 
                AbilityDamageMultiplier = 2.0,
                AbilityEnergyCost = 2
            });

            // Archer (Requires Element, defaulting to Fire for generic template for now, or creating variants)
            // "Archer... Requires 1 elemental specific energy" - Defined as Neutral in doc? 
            // Doc says: "Archer ... Types: Requires 1 elemental specific energy. Fire, Water."
            // So we likely need specific Archers or just a generic one that adapts.
            // Let's create specific ones as per the deck list "Archer of the Flame", "Archer of the Ocean"
            
            Definitions.Add(new CardDefinition("Archer of the Flame", Element.Fire, CardType.Attacker)
            {
                MaxHealth = 80,
                Defense = 40,
                BaseAttack = 20
            });
             Definitions.Add(new CardDefinition("Archer of the Ocean", Element.Water, CardType.Attacker)
            {
                MaxHealth = 80,
                Defense = 40,
                BaseAttack = 20
            });


            // Warrior
            // "Warrior... Requires 2 elemental specific energy. Air, Earth"
             Definitions.Add(new CardDefinition("Air Warrior", Element.Air, CardType.Attacker)
            {
                MaxHealth = 100,
                Defense = 50,
                BaseAttack = 15
            });
             Definitions.Add(new CardDefinition("Earth Warrior", Element.Earth, CardType.Attacker)
            {
                MaxHealth = 100,
                Defense = 50,
                BaseAttack = 15
            });


            // --- Energies ---
            Definitions.Add(new CardDefinition("Fire Energy", Element.Fire, CardType.Energy));
            Definitions.Add(new CardDefinition("Water Energy", Element.Water, CardType.Energy));
            Definitions.Add(new CardDefinition("Air Energy", Element.Air, CardType.Energy));
            Definitions.Add(new CardDefinition("Earth Energy", Element.Earth, CardType.Energy));

            // --- Supports ---
            Definitions.Add(new CardDefinition("Spy", Element.Neutral, CardType.Support) { SupportHealth = 50 });
            Definitions.Add(new CardDefinition("Limiter", Element.Neutral, CardType.Support) { SupportHealth = 80 });
            Definitions.Add(new CardDefinition("Elemental Commander", Element.Neutral, CardType.Support) { SupportHealth = 80 });
        }

        public static CardDefinition? GetCard(string name)
        {
            return Definitions.Find(d => d.Name == name);
        }
    }
}
