using System;
using System.Collections.Generic;
using System.Linq;
using CardGame.Engine.Core;

namespace CardGame.Engine.Board
{
    public class Deck
    {
        private List<Card> _cards;

        public Deck(List<Card> cards)
        {
            _cards = cards ?? new List<Card>();
        }

        public void Shuffle()
        {
            var rng = new Random();
            int n = _cards.Count;
            while (n > 1)
            {
                n--;
                int k = rng.Next(n + 1);
                Card value = _cards[k];
                _cards[k] = _cards[n];
                _cards[n] = value;
            }
        }

        public Card Draw()
        {
            if (_cards.Count == 0) return null;
            var card = _cards[0];
            _cards.RemoveAt(0);
            return card;
        }

        public Card FindAndRemoveFirst(Func<Card, bool> predicate)
        {
            var card = _cards.FirstOrDefault(predicate);
            if (card != null)
            {
                _cards.Remove(card);
            }
            return card;
        }

        public int Count => _cards.Count;
    }
}