using System;

namespace CardGame.Engine.Core
{
    public abstract class Card
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Name { get; set; }
        public Element Element { get; set; }
        public CardType Type { get; set; }

        public Card(string name, Element element, CardType type)
        {
            Name = name;
            Element = element;
            Type = type;
        }

        public override string ToString()
        {
            return $"{Name} ({Element} {Type})";
        }
    }
}
