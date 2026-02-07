using CardGame.Engine.Core;
using CardGame.Engine.GameFlow;
using CardGame.Engine.Players;
using System.Linq;

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
            if (!game.CurrentPlayer.ActiveAttackers.Contains(Source)) return false;
            
            if (TargetPlayer != null)
            {
                if (game.OpponentPlayer.ActiveAttackers.Count > 0) return false;
            }
            
            if (UseAbility)
            {
                if (Source.AttachedEnergies.Count < Source.BonusDamageThreshold) return false;
                if (Source.AttachedEnergies.Count < Source.AbilityEnergyCost) return false;
            }
            
            return true;
        }

        public void Execute(Game game)
        {
            // -- Warrior On-Attack Effect Logic --
            if (Source.OnAttackEffect == OnAttackEffectType.SearchEnergyChance)
            {
                int specificCount = 0;
                if (Source.BonusRequiredElement.HasValue)
                    specificCount = Source.AttachedEnergies.Count(e => e.Element == Source.BonusRequiredElement.Value);
                
                if (Source.AttachedEnergies.Count >= Source.AbilityUnlockThreshold &&
                    specificCount >= Source.AbilitySpecificEnergyReq)
                {
                    var rng = new System.Random();
                    if (rng.Next(0, 100) < Source.OnAttackEffectChance)
                    {
                        var energyCard = game.CurrentPlayer.Deck.FindAndRemoveFirst(c => c.Type == CardType.Energy);
                        if (energyCard != null)
                        {
                            var ally = game.CurrentPlayer.ActiveAttackers.FirstOrDefault(a => a != Source);
                            if (ally != null)
                            {
                                ally.AttachedEnergies.Add(energyCard);
                                if (ally.EnergyAffinity == null && !ally.AllowMixedEnergy)
                                    ally.EnergyAffinity = energyCard.Element;
                                game.CurrentPlayer.Deck.Shuffle();
                                System.Console.WriteLine($"*** ATTACK BONUS: Searched {energyCard.Name} and attached to {ally.Name}! ***");
                            }
                            else game.CurrentPlayer.DiscardPile.Add(energyCard);
                        }
                    }
                }
            }

            int damage = Source.GetCurrentAttack(); 
            
            // -- Global Air Bonus (Dodge) --
            if (TargetAttacker != null && TargetAttacker.Element == Element.Air && TargetAttacker.AttachedEnergies.Count > 0)
            {
                int airCount = game.GetEnergizedAttackerCount(game.OpponentPlayer, Element.Air);
                int dodgeChance = 0;
                if (airCount == 1) dodgeChance = 5;
                else if (airCount == 2) dodgeChance = 10;
                else if (airCount >= 3) dodgeChance = 15;
                
                if (dodgeChance > 0)
                {
                    var rng = new System.Random();
                    if (rng.Next(0, 100) < dodgeChance)
                    {
                        damage = 0;
                        System.Console.WriteLine($"*** Air Bonus: {TargetAttacker.Name} Dodged the attack! (Chance {dodgeChance}%) ***");
                    }
                }
            }
            
            // Active Ability Logic (Tank x2 Dmg, Discard 2)
            if (UseAbility)
            {
                damage = (int)(damage * Source.AbilityDamageMultiplier);
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
                 // Passive Bonus Logic (Minion RNG / Archer Conditional RNG)
                 if (Source.BonusDamageChance > 0 && Source.AttachedEnergies.Count >= Source.BonusDamageThreshold)
                {
                    bool reqMet = true;
                    if (Source.BonusRequiredElement.HasValue)
                    {
                        bool hasType = false;
                        foreach(var e in Source.AttachedEnergies)
                        {
                            if (e.Element == Source.BonusRequiredElement.Value) hasType = true;
                        }
                        if (!hasType) reqMet = false;
                    }

                    if (reqMet)
                    {
                        int roll = game.Random.Next(0, 100); 
                        if (roll < Source.BonusDamageChance)
                        {
                            if (SecondaryTargetAttacker != null)
                            {
                                System.Console.WriteLine($"*** Bonus Triggered against Secondary Target! ***");
                                SecondaryTargetAttacker.TakeDamage(Source.BonusDamageAmount);
                                 if (SecondaryTargetAttacker.IsKO())
                                 {
                                    game.OpponentPlayer.ActiveAttackers.Remove(SecondaryTargetAttacker);
                                    game.OpponentPlayer.DiscardPile.Add(SecondaryTargetAttacker);
                                 }
                            }
                            else
                            {
                                damage += Source.BonusDamageAmount;
                            }
                        }
                    }
                }
            }

            // Damage Mitigation & Passthrough
            double mitigation = TargetAttacker != null ? TargetAttacker.Defense / 100.0 : 0.0;
            if (mitigation > 1.0) mitigation = 1.0;
            if (mitigation < 0.0) mitigation = 0.0;
            
            int damageToPlayer = (int)(damage * (1.0 - mitigation));
            
            // -- Global Earth Bonus (Resist) --
            if (damageToPlayer > 0 && game.OpponentPlayer != null)
            {
                int earthCount = game.GetEnergizedAttackerCount(game.OpponentPlayer, Element.Earth);
                double resistPct = 0.0;
                if (earthCount == 1) resistPct = 0.05;
                else if (earthCount == 2) resistPct = 0.10;
                else if (earthCount >= 3) resistPct = 0.15;
                
                if (resistPct > 0)
                {
                    int reduced = (int)(damageToPlayer * (1.0 - resistPct));
                    if (reduced < damageToPlayer) 
                        System.Console.WriteLine($"*** Earth Bonus: Reduced Player Damage from {damageToPlayer} to {reduced} (-{resistPct*100}%) ***");
                    damageToPlayer = reduced;
                }
                
                game.OpponentPlayer.Shield -= damageToPlayer;
                if (game.OpponentPlayer.Shield < 0) game.OpponentPlayer.Shield = 0;
            }

            if (TargetAttacker != null)
            {
                TargetAttacker.TakeDamage(damage);
                
                // -- Global Fire Bonus (Burn) --
                if (Source.Element == Element.Fire && Source.AttachedEnergies.Count > 0 && !TargetAttacker.IsKO() && damage > 0)
                {
                    int fireCount = game.GetEnergizedAttackerCount(game.CurrentPlayer, Element.Fire);
                    int burnChance = 0;
                    int burnDmg = 0;
                    
                    if (fireCount == 1) { burnChance=5; burnDmg=5; }
                    else if (fireCount == 2) { burnChance=10; burnDmg=10; }
                    else if (fireCount >= 3) { burnChance=15; burnDmg=15; }
                    
                    if (burnChance > 0)
                    {
                        var rng = new System.Random();
                        if (rng.Next(0, 100) < burnChance)
                        {
                            TargetAttacker.ApplyStatus("Burn", burnDmg, 3);
                            System.Console.WriteLine($"*** Fire Bonus: Applied Burn ({burnDmg} dmg/turn) to {TargetAttacker.Name}! (Chance {burnChance}%) ***");
                        }
                    }
                }

                if (TargetAttacker.IsKO())
                {
                    game.OpponentPlayer.ActiveAttackers.Remove(TargetAttacker);
                    game.OpponentPlayer.DiscardPile.Add(TargetAttacker);
                    foreach(var e in TargetAttacker.AttachedEnergies)
                    {
                        game.OpponentPlayer.DiscardPile.Add(e);
                    }
                }
            }
            else if (TargetPlayer != null)
            {
                TargetPlayer.Shield -= damage;
                if (TargetPlayer.Shield < 0) TargetPlayer.Shield = 0;
            }
            
            // Bonus Logic: "at 4 energies... 20 bonus damage to a second target"
            if (Source.AttachedEnergies.Count >= Source.MaxEnergy && Source.MaxEnergy >= 4)
            {
                int bonusDmg = 20; 
                if (SecondaryTargetAttacker != null)
                {
                     SecondaryTargetAttacker.TakeDamage(bonusDmg);
                     if (SecondaryTargetAttacker.IsKO())
                     {
                        game.OpponentPlayer.ActiveAttackers.Remove(SecondaryTargetAttacker);
                        game.OpponentPlayer.DiscardPile.Add(SecondaryTargetAttacker);
                        foreach(var e in SecondaryTargetAttacker.AttachedEnergies)
                        {
                            game.OpponentPlayer.DiscardPile.Add(e);
                        }
                     }
                }
            }
            
            game.EndTurn();
        }
    }
}
