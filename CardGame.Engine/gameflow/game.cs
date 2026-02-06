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
        }

        public void StartGame()
        {
            Player1.Deck.Shuffle();
            Player2.Deck.Shuffle();

            Player1.DrawCards(5);
            Player2.DrawCards(5);
            
            // "Can be redrawn twice" - Logic for Mulligan not implemented yet, just basic start.
        }

        public Player CurrentPlayer => CurrentPlayerIndex == 0 ? Player1 : Player2;
        public Player OpponentPlayer => CurrentPlayerIndex == 0 ? Player2 : Player1;

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
            // "Have to pick up card out of deck each turn (random)"
            CurrentPlayer.DrawCards(1);
        }
    }
}
