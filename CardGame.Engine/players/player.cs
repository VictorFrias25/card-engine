using System.Collections.Generic;
using CardGame.Engine.Core;
using CardGame.Engine.Board;

namespace CardGame.Engine.Players
{
    public class Player
    {
        public int Id { get; set; }
        public int Shield { get; set; } // Player Health
        public Deck Deck { get; set; }
        public List<Card> Hand { get; set; } = new List<Card>();
        public List<Card> DiscardPile { get; set; } = new List<Card>();
        public List<Attacker> ActiveAttackers { get; set; } = new List<Attacker>();
        public List<Support> ActiveSupports { get; set; } = new List<Support>();

        public Player(int id, int shield, List<Card> deckCards)
        {
            Id = id;
            Shield = shield;
            Deck = new Deck(deckCards);
        }

        public void DrawCards(int count)
        {
            for (int i = 0; i < count; i++)
            {
                var card = Deck.Draw();
                if (card != null)
                {
                    Hand.Add(card);
                }
            }
        }
    }
}
