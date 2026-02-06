namespace CardGame.Engine.Core
{
    public class Energy : Card
    {
        public Energy(Element element) 
            : base($"{element} Energy", element, CardType.Energy)
        {
        }
    }
}
