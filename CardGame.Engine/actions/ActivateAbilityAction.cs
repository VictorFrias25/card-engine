using System;
using System.Linq;
using CardGame.Engine.Core;
using CardGame.Engine.GameFlow;

namespace CardGame.Engine.Actions
{
    public class ActivateAbilityAction : IGameAction
    {
        public Attacker Source { get; }
        public Attacker Target { get; } // Target to receive the energy (must be different)

        public ActivateAbilityAction(Attacker source, Attacker target)
        {
            Source = source;
            Target = target;
        }

        public bool IsValid(Game game)
        {
            var player = game.CurrentPlayer;
            if (!player.ActiveAttackers.Contains(Source)) return false;
            if (!player.ActiveAttackers.Contains(Target)) return false;
            
            // Constraint: Different card?
            // "apply to a card seperate from your warrior"
            if (Source == Target) return false;

            // Check Ability Type
            if (Source.ActiveAbility != ActiveAbilityType.SearchEnergy) return false;

            // Check Thresholds
            if (Source.AttachedEnergies.Count < Source.AbilityUnlockThreshold) return false;
            
            // Check Specific Energy Requirement
            if (Source.BonusRequiredElement.HasValue)
            {
                int specificCount = Source.AttachedEnergies.Count(e => e.Element == Source.BonusRequiredElement.Value);
                if (specificCount < Source.AbilitySpecificEnergyReq) return false;
            }

            return true;
        }

        public void Execute(Game game)
        {
            var player = game.CurrentPlayer;
            
            // Search Deck for Energy
            // "pick an energy" - We'll pick the first available energy card
            var energyCard = player.Deck.FindAndRemoveFirst(c => c.Type == CardType.Energy);
            
            if (energyCard != null)
            {
                // player.Deck.Remove(energyCard); // Handled by FindAndRemoveFirst
                Target.AttachedEnergies.Add(energyCard);
                
                // Affinity check if needed
                if (Target.EnergyAffinity == null && !Target.AllowMixedEnergy)
                {
                    Target.EnergyAffinity = energyCard.Element;
                }
                
                // Shuffle deck after search
                player.Deck.Shuffle();
                
                System.Console.WriteLine($"*** Warrior Ability: Attached {energyCard.Name} to {Target.Name} from Deck ***");
            }
            else
            {
                System.Console.WriteLine($"*** Warrior Ability: No Energy found in Deck! ***");
            }
            
            // DOES NOT END TURN
        }
    }
}
