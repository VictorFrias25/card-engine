namespace CardGame.Engine.Core
{
    public enum Element
    {
        Neutral,
        Fire,
        Water,
        Air,
        Earth
    }

    public enum CardType
    {
        Attacker,
        Support,
        Landscape,
        Energy
    }

    public enum Zone
    {
        Deck,
        Hand,
        Board,
        Discard,
        Lost
    }

    public enum ActiveAbilityType
    {
        None,
        SearchEnergy // Keeping for now or removing?
    }

    public enum OnAttackEffectType
    {
        None,
        SearchEnergyChance
    }
}
