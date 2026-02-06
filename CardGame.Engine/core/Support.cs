namespace CardGame.Engine.Core
{
    public class Support : Card
    {
        public int Health { get; set; } // For Active supports
        // Some supports are one-use, others are active. 
        // We can track this via a property or subclass, but for now a boolean or checking Health > 0 is fine.
        public bool IsActiveSupport { get; set; }

        public Support(string name, Element element, bool isActiveSupport, int health = 0) 
            : base(name, element, CardType.Support)
        {
            IsActiveSupport = isActiveSupport;
            Health = health;
            
            // "Supports always have 50 defense" - rule note. 
            // We can add a property for that if needed, or derived logic.
        }
    }
}
