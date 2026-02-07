using System;
using CardGame.Engine.Players;
using CardGame.Engine.Core;

namespace CardGame.Engine.GameFlow
{
    public class Game
    {
        public Player Player1 { get; private set; }
        public Player Player2 { get; private set; }
        public int TurnCount { get; private set; }
        public Landscape? ActiveLandscape { get; set; }
        public Random Random { get; } = new Random();
        public int CurrentPlayerIndex { get; private set; } // 0 or 1
        
        // Turn State Tracking
        public int AttackersPlayedThisTurn { get; set; }

        public Game(Player p1, Player p2)
        {
            Player1 = p1;
            Player2 = p2;
            TurnCount = 1;
            CurrentPlayerIndex = 0; // P1 starts
            EnergyAttachmentsThisTurn = new Dictionary<string, int>();
        }

        // Track how many energy cards attached to specific attacker ID this turn
        public Dictionary<string, int> EnergyAttachmentsThisTurn { get; private set; }

        public void StartGame(bool shuffleDecks = true)
        {
            if (shuffleDecks)
            {
                Player1.Deck.Shuffle();
                Player2.Deck.Shuffle();
            }

            // Draw Initial Hands
            Player1.DrawCards(5);
            Player2.DrawCards(5);
            
            // "Can be redrawn twice" - Logic for Mulligan not implemented yet, just basic start.
        }

        public Player CurrentPlayer => CurrentPlayerIndex == 0 ? Player1 : Player2;
        public Player OpponentPlayer => CurrentPlayerIndex == 0 ? Player2 : Player1;

        public int GetEnergizedAttackerCount(Player p, Element element)
        {
            // "Energized" means having at least one energy? Or having energy of that element?
            // "energized water attackers"
            // Interpretation: Attacker is Element.Water AND has > 0 Energy attached.
            // Or has Water Energy attached?
            // Usually "Energized [Element] Attacker" means the Attacker Card Itself is [Element] and has energy.
            
            return p.ActiveAttackers.Count(a => a.Element == element && a.AttachedEnergies.Count > 0);
        }

        public void EndTurn()
        {
            CurrentPlayerIndex = (CurrentPlayerIndex + 1) % 2;
            if (CurrentPlayerIndex == 0)
            {
                TurnCount++;
            }
            StartTurn();
        }

        private void StartTurn()
        {
            // The following two lines are syntactically incorrect as CurrentPlayer and OpponentPlayer are read-only properties.
            // They are also logically redundant as CurrentPlayerIndex is updated in EndTurn, which correctly switches these properties.
            // CurrentPlayer = (CurrentPlayer == Player1) ? Player2 : Player1;
            // OpponentPlayer = (CurrentPlayer == Player1) ? Player2 : Player1;
            
            // TurnCount is already incremented in EndTurn when CurrentPlayerIndex becomes 0 (start of a new round).
            // Incrementing it here would double the turn count.
            // TurnCount++; 
            Console.WriteLine($"--- Turn {TurnCount} - Player {CurrentPlayer.Id}'s Turn ---");
            
            // 1. Process Status Effects (e.g. Burn)
            foreach (var attacker in CurrentPlayer.ActiveAttackers.ToList()) // ToList in case of death/removal logic later
            {
                var logs = attacker.ProcessStatusEffects();
                foreach (var log in logs) Console.WriteLine($"*** {log} ***");
                
                if (attacker.IsKO())
                {
                    CurrentPlayer.ActiveAttackers.Remove(attacker);
                    CurrentPlayer.DiscardPile.Add(attacker);
                    foreach(var e in attacker.AttachedEnergies) CurrentPlayer.DiscardPile.Add(e);
                    Console.WriteLine($"*** {attacker.Name} succumbed to status effects! ***");
                }
            }

            // 2. Global Water Bonus (Heal)
            // "Can heal 1 specific attacker... based on number of energized water attackers"
            // Start of Turn is a good place.
            int waterCount = GetEnergizedAttackerCount(CurrentPlayer, Element.Water);
            if (waterCount > 0)
            {
                int healAmount = 0;
                if (waterCount == 1) healAmount = 5;
                else if (waterCount == 2) healAmount = 10;
                else if (waterCount >= 3) healAmount = 15;
                
                // Find Target: Lowest HP Ally (active or support?) User said "attacker or support".
                // We only track ActiveAttackers and ActiveSupports.
                // Priority: Attackers first? Or absolute lowest HP?
                // Let's check Attackers for now.
                var target = CurrentPlayer.ActiveAttackers
                    .Where(a => a.CurrentHealth < a.MaxHealth)
                    .OrderBy(a => a.CurrentHealth)
                    .FirstOrDefault();
                    
                if (target != null)
                {
                    target.Heal(healAmount);
                    Console.WriteLine($"*** Water Bonus: Healed {target.Name} by {healAmount} HP! (Energized Water: {waterCount}) ***");
                }
                else
                {
                    // Full health? Maybe heal shield? NO, req says "attacker or support".
                }
            }

            // Reset per-turn trackers
            AttackersPlayedThisTurn = 0;
            EnergyAttachmentsThisTurn.Clear();
            
            // "Have to pick up card out of deck each turn (random)"
            CurrentPlayer.DrawCards(1);
        }

    }
}
