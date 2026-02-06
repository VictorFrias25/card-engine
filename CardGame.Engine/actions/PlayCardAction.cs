using System;
using CardGame.Engine.Core;
using CardGame.Engine.GameFlow;
using CardGame.Engine.Players;

namespace CardGame.Engine.Actions
{
    public class PlayCardAction : IGameAction
    {
        public Card CardToPlay { get; }
        
        public PlayCardAction(Card card)
        {
            CardToPlay = card;
        }

        public bool IsValid(Game game)
        {
            var player = game.CurrentPlayer;
            
            // 1. Card must be in hand
            if (!player.Hand.Contains(CardToPlay)) return false;

            // 2. Check rules based on card type
            if (CardToPlay.Type == CardType.Attacker)
            {
                // "1-2 attackers per turn played"
                if (game.AttackersPlayedThisTurn >= 2) return false;
            }
            if (CardToPlay.Type == CardType.Energy)
            {
                // Energy cannot be played directly to the board via PlayCardAction
                // It must be attached using AttachEnergyAction
                return false;
            }
            // Landscape/Support/Energy rules can be added here
            // Note: Energy usually needs a target, so it might be a separate action or handled differently.
            // For now, PlayCardAction handles "Playing to the Board" (Attacker, Support, Landscape).

            return true;
        }

        public void Execute(Game game)
        {
            var player = game.CurrentPlayer;
            
            // Remove from hand
            player.Hand.Remove(CardToPlay);

            // Add to board/active slot
            switch (CardToPlay.Type)
            {
                case CardType.Attacker:
                    player.ActiveAttackers.Add((Attacker)CardToPlay);
                    game.AttackersPlayedThisTurn++;
                    break;
                case CardType.Support:
                    player.ActiveSupports.Add((Support)CardToPlay);
                    break;
                case CardType.Landscape:
                    game.ActiveLandscape = (Landscape)CardToPlay;
                    break;
                // Energy is typically attached, not "played" to board on its own in this context, 
                // but if we support playing energy to pool, we'd add it here. 
                // For this game, Energy is attached to attackers.
                // If user tries to "Play" energy without target, it might be invalid or a discard? 
                // Leaving as no-op or throw for now if not handled by AttachEnergyAction.
            }
        }
    }
}
