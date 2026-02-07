namespace CardGame.Engine.Models
{
    public class StatusEffect
    {
        public string Name { get; set; }
        public int Value { get; set; } // e.g. Damage Amount
        public int Duration { get; set; } // Turns remaining

        public StatusEffect(string name, int value, int duration)
        {
            Name = name;
            Value = value;
            Duration = duration;
        }
    }
}
