using System.Linq;
using System.Collections.Generic;
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
            
            // "Limit of 1 energy applied to attacker on board per turn"
            if (game.EnergyAttachmentsThisTurn.ContainsKey(TargetAttacker.Id))
            {
                if (game.EnergyAttachmentsThisTurn[TargetAttacker.Id] >= 1) return false;
            }
            
            // Check if target is on board
            if (!player.ActiveAttackers.Contains(TargetAttacker)) 
                return false;

             if (!TargetAttacker.CanAttachEnergy(EnergyCard))
                return false;

            return true;
        }

        public void Execute(Game game)
        {
             var player = game.CurrentPlayer;
             player.Hand.Remove(EnergyCard);
             TargetAttacker.AttachedEnergies.Add(EnergyCard);
             
             // If this is the first energy, set affinity
             if (TargetAttacker.EnergyAffinity == null && !TargetAttacker.AllowMixedEnergy)
             {
                 TargetAttacker.EnergyAffinity = EnergyCard.Element;
             }
             
             // Track usage
            if (!game.EnergyAttachmentsThisTurn.ContainsKey(TargetAttacker.Id))
            {
                game.EnergyAttachmentsThisTurn[TargetAttacker.Id] = 0;
            }
            game.EnergyAttachmentsThisTurn[TargetAttacker.Id]++;
        }
    }
}
