using CardGame.Engine.GameFlow;

namespace CardGame.Engine.Actions
{
    public interface IGameAction
    {
        void Execute(Game game);
        bool IsValid(Game game);
    }
}
