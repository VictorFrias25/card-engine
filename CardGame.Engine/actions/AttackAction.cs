using CardGame.Engine.Core;
using CardGame.Engine.GameFlow;
using CardGame.Engine.Players;

namespace CardGame.Engine.Actions
{
    public class AttackAction : IGameAction
    {
        public Attacker Source { get; }
        public Attacker? TargetAttacker { get; }
        public Player? TargetPlayer { get; } // Attacking Shield directly
        
        // Bonus Target logic (for Grunt's 4th energy bonus)
        public Attacker? SecondaryTargetAttacker { get; }
        public Player? SecondaryTargetPlayer { get; }
        
        public bool UseAbility { get; }

        private AttackAction(Attacker source, Attacker? targetAttacker, Player? targetPlayer, 
                             Attacker? secTargetAttacker = null, Player? secTargetPlayer = null,
                             bool useAbility = false)
        {
            Source = source;
            TargetAttacker = targetAttacker;
            TargetPlayer = targetPlayer;
            SecondaryTargetAttacker = secTargetAttacker;
            SecondaryTargetPlayer = secTargetPlayer;
            UseAbility = useAbility;
        }

        public static AttackAction CreateAttackCreature(Attacker source, Attacker target, Attacker? secTarget = null, bool useAbility = false)
        {
            return new AttackAction(source, target, null, secTarget, null, useAbility);
        }

        public static AttackAction CreateAttackPlayer(Attacker source, Player target, Attacker? secTarget = null, bool useAbility = false)
        {
            return new AttackAction(source, null, target, secTarget, null, useAbility);
        }

        public bool IsValid(Game game)
        {
            // Simplified validation
            if (!game.CurrentPlayer.ActiveAttackers.Contains(Source)) return false;
            
            // "Player can directly attack the shield if opponent has no attackers on the board"
            if (TargetPlayer != null)
            {
                if (game.OpponentPlayer.ActiveAttackers.Count > 0) return false;
            }
            
            // Ability validation
            if (UseAbility)
            {
                if (Source.AttachedEnergies.Count < Source.BonusDamageThreshold) return false;
                if (Source.AttachedEnergies.Count < Source.AbilityEnergyCost) return false;
            }
            
            return true;
        }

        public void Execute(Game game)
        {
            int damage = Source.GetCurrentAttack(); 
            
            // Check for Probabilistic Bonus (e.g. Minion 50% chance for +15)
            // ... (Same as before) ...
            
            // Active Ability Logic (Tank x2 Dmg, Discard 2)
            if (UseAbility)
            {
                // Apply Multiplier
                damage = (int)(damage * Source.AbilityDamageMultiplier);
                
                // Discard Cost (Remove from end)
                for (int i = 0; i < Source.AbilityEnergyCost; i++)
                {
                    if (Source.AttachedEnergies.Count > 0)
                    {
                        var e = Source.AttachedEnergies[Source.AttachedEnergies.Count - 1];
                        Source.AttachedEnergies.RemoveAt(Source.AttachedEnergies.Count - 1);
                        game.CurrentPlayer.DiscardPile.Add(e);
                    }
                }
            }
            else 
            {
                 // RNG ONLY if not using Ability? Or Stack?
                 // Minion is passive RNG. Tank is Active Choice.
                 // Assuming passive RNG always checks if conditions met.
                 if (Source.BonusDamageChance > 0 && Source.AttachedEnergies.Count >= Source.BonusDamageThreshold)
                {
                    int roll = game.Random.Next(0, 100); 
                    if (roll < Source.BonusDamageChance)
                    {
                        damage += Source.BonusDamageAmount;
                    }
                }
            }

            // Apply modifiers here later

            if (TargetAttacker != null)
            {
                // "Player takes damage based on their attackers’ health and defense"
                // "If an attacker has a defense of 50 and is attacked, 50% of the damage will go to the player"
                
                // Let's implement a simple version first:
                // Damage to creature
                TargetAttacker.TakeDamage(damage);
                
                // Pass-through damage if rule applies
                if (TargetAttacker.IsKO())
                {
                    game.OpponentPlayer.ActiveAttackers.Remove(TargetAttacker);
                    game.OpponentPlayer.DiscardPile.Add(TargetAttacker);
                    // Discard attached energies?
                }
            }
            else if (TargetPlayer != null)
            {
                TargetPlayer.Shield -= damage;
            }
            
            // Bonus Logic: "at 4 energies... 20 bonus damage to a second target"
            if (Source.AttachedEnergies.Count >= Source.MaxEnergy && Source.MaxEnergy >= 4)
            {
                int bonusDmg = 20; 
                // Hardcoded 20 for Grunt for now, or could make it a property of Attacker if standardized
                
                if (SecondaryTargetAttacker != null)
                {
                     SecondaryTargetAttacker.TakeDamage(bonusDmg);
                     if (SecondaryTargetAttacker.IsKO())
                     {
                        game.OpponentPlayer.ActiveAttackers.Remove(SecondaryTargetAttacker);
                        game.OpponentPlayer.DiscardPile.Add(SecondaryTargetAttacker);
                     }
                }
                // Could handle SecondaryTargetPlayer if needed, but usually can't attack shield if defenders exist?
            }
        }
    }
}
