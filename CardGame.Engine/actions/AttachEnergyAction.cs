using System.Linq;
using CardGame.Engine.Core;
using CardGame.Engine.GameFlow;

namespace CardGame.Engine.Actions
{
    public class AttachEnergyAction : IGameAction
    {
        public Energy EnergyCard { get; }
        public Attacker TargetAttacker { get; }

        public AttachEnergyAction(Energy energy, Attacker target)
        {
            EnergyCard = energy;
            TargetAttacker = target;
        }

        public bool IsValid(Game game)
        {
            var player = game.CurrentPlayer;
            if (!player.Hand.Contains(EnergyCard)) return false;
            
            // "Can't play supercharger on attacker that has already had an energy attached to it that turn"
            // The rule implies a limit. For now, enforcing 1 energy attachment per attacker per turn?
            // "1 energy per attacker per turn"
            
            // Check if target is on board
            if (!player.ActiveAttackers.Contains(TargetAttacker)) 
                return false;

             if (!TargetAttacker.CanAttachEnergy(EnergyCard))
                return false;

            // TODO: Track if this attacker received energy this turn in Game state
            // For now, assuming basic validity
            
            return true;
        }

        public void Execute(Game game)
        {
             var player = game.CurrentPlayer;
             player.Hand.Remove(EnergyCard);
             TargetAttacker.AttachedEnergies.Add(EnergyCard);
             
             // If this is the first energy, set affinity
             if (TargetAttacker.EnergyAffinity == null)
             {
                 TargetAttacker.EnergyAffinity = EnergyCard.Element;
             }
             
             // TODO: Mark attacker as having received energy this turn
        }
    }
}
