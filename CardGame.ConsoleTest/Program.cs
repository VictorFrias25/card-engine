using System;
using System.Collections.Generic;
using CardGame.Engine.Core;
using CardGame.Engine.Board;
using CardGame.Engine.Players;
using CardGame.Engine.GameFlow;
using CardGame.Engine.Actions;
using CardGame.Engine.Models;

namespace CardGame.ConsoleTest
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Initializing Card Game Engine...");
            
            // --- Custom Mode Prompt ---
            Console.WriteLine("Load Default Card Stats? (y/n)");
            string? mode = Console.ReadLine();
            if (mode?.ToLower() == "n")
            {
                Console.WriteLine("ENTERING CUSTOM MODE");
                CustomizeCard("Grunt", 100);
                CustomizeCard("Tank", 150);
                // Can add more loops here
            }

            var p1 = new Player(1, 1000, GenerateDeck(1));
            var p2 = new Player(2, 1000, GenerateDeck(2));
            var game = new Game(p1, p2);

            game.StartGame(false); // Disable shuffle for deterministic testing

            while (true)
            {
                // Console.Clear(); // Commented out for better scrolling history in test
                PrintGameState(game);
                
                Console.WriteLine("\nActions:");
                Console.WriteLine(" [num]: Play Card from Hand (e.g. 0)");
                Console.WriteLine(" 'e'+[card]+[target]: Attach Energy (e.g. e00)");
                Console.WriteLine(" 'a'+[source]+[target]: Attack (e.g. a0p or a01)");
                Console.WriteLine(" 'u'+[source]+[target]: Use Ability (e.g. u01)");
                Console.WriteLine(" 'n': End Turn");
                Console.WriteLine(" 'q': Quit");
                
                Console.Write("> ");
                var input = Console.ReadLine();

                if (string.IsNullOrEmpty(input)) continue;
                if (input == "q") break;
                if (input == "n")
                {
                    game.EndTurn();
                    continue;
                }
                
                if (input.StartsWith("u"))
                {
                     // Format: u[source][target]
                     if (input.Length >= 3 && 
                        int.TryParse(input.Substring(1, 1), out int sourceIdx) &&
                        int.TryParse(input.Substring(2, 1), out int targetIdx))
                    {
                        var player = game.CurrentPlayer;
                        if (sourceIdx >= 0 && sourceIdx < player.ActiveAttackers.Count &&
                            targetIdx >= 0 && targetIdx < player.ActiveAttackers.Count)
                        {
                            var source = player.ActiveAttackers[sourceIdx];
                            var target = player.ActiveAttackers[targetIdx];
                            var action = new ActivateAbilityAction(source, target);
                            
                            if (action.IsValid(game))
                            {
                                action.Execute(game);
                                // Turn check handled by action (it doesn't end it)
                            }
                            else Console.WriteLine("\n!!! Invalid Ability (Check requirements/target) !!!");
                        }
                    }
                }
                
                if (input.StartsWith("e"))
                {
                    // Format: e[handIndex][attackerIndex] e.g. e00
                    if (input.Length >= 3 && 
                        int.TryParse(input.Substring(1, 1), out int handIdx) &&
                        int.TryParse(input.Substring(2, 1), out int targetIdx))
                    {
                        var player = game.CurrentPlayer;
                        if (handIdx >= 0 && handIdx < player.Hand.Count &&
                            targetIdx >= 0 && targetIdx < player.ActiveAttackers.Count)
                        {
                            var energyCard = player.Hand[handIdx] as Energy;
                            var target = player.ActiveAttackers[targetIdx];
                            
                            if (energyCard != null)
                            {
                                var action = new AttachEnergyAction(energyCard, target);
                                if (action.IsValid(game))
                                {
                                    action.Execute(game);
                                    Console.WriteLine($"\n*** Attached {energyCard.Name} to {target.Name} ***");
                                }
                                else Console.WriteLine("\n!!! Invalid Energy Attachment (Check rules) !!!");
                            }
                            else Console.WriteLine("\n!!! Selected card is not Energy !!!");
                        }
                    }
                }
                else if (input.StartsWith("a"))
                {
                    // Format: a[attackerIndex][targetIndex][secondaryTargetIndex?]
                    // e.g. a01 (Attacker 0 -> Enemy 1)
                    // e.g. a0p (Attacker 0 -> Enemy Player)
                    // e.g. a012 (Attacker 0 -> Enemy 1, Bonus -> Enemy 2)
                    
                    if (input.Length >= 3 &&
                         int.TryParse(input.Substring(1, 1), out int sourceIdx))
                    {
                         var player = game.CurrentPlayer;
                         var opponent = game.OpponentPlayer;
                         char targetType = input[2];
                         
                         // Optional Secondary Target
                         Attacker? secTarget = null;
                         if (input.Length >= 4 && int.TryParse(input.Substring(3, 1), out int secIdx))
                         {
                             if (secIdx >= 0 && secIdx < opponent.ActiveAttackers.Count)
                             {
                                 secTarget = opponent.ActiveAttackers[secIdx];
                             }
                         }

                         if (sourceIdx >= 0 && sourceIdx < player.ActiveAttackers.Count)
                         {
                             var source = player.ActiveAttackers[sourceIdx];
                             IGameAction? action = null;

                             if (targetType == 'p')
                             {
                                 action = AttackAction.CreateAttackPlayer(source, opponent, secTarget);
                             }
                             else if (int.TryParse(input.Substring(2, 1), out int targetIdx))
                             {
                                 if (targetIdx >= 0 && targetIdx < opponent.ActiveAttackers.Count)
                                 {
                                     var target = opponent.ActiveAttackers[targetIdx];
                                     action = AttackAction.CreateAttackCreature(source, target, secTarget);
                                 }
                             }

                             if (action != null && action.IsValid(game))
                             {
                                 action.Execute(game);
                                 Console.WriteLine($"\n*** Attack Executed! ***");
                             }
                             else Console.WriteLine("\n!!! Invalid Attack (Check rules / Target) !!!");
                         }
                    }
                }
                else if (int.TryParse(input, out int cardIndex))
                {
                    // Try to play card
                    if (cardIndex >= 0 && cardIndex < game.CurrentPlayer.Hand.Count)
                    {
                        var card = game.CurrentPlayer.Hand[cardIndex];
                        var action = new PlayCardAction(card);
                        if (action.IsValid(game))
                        {
                            action.Execute(game);
                            Console.WriteLine($"\n*** Played {card.Name} ***");
                        }
                        else
                        {
                            Console.WriteLine("\n!!! Invalid move! (Check rules) !!!");
                        }
                    }
                    else
                    {
                             Console.WriteLine("\n!!! Invalid card index !!!");
                    }
                }
            }
        }

        static void CustomizeCard(string cardName, int defaultHP)
        {
             var def = CardLibrary.GetCard(cardName);
             if (def == null) return;

             Console.Write($"Enter HP for {cardName} (Default {defaultHP}): ");
             string? input = Console.ReadLine();
             if (int.TryParse(input, out int newHP))
             {
                 def.MaxHealth = newHP;
                 Console.WriteLine($"-> {cardName} Health set to {newHP}");
             }
        }

        static List<Card> GenerateDeck(int playerId)
        {
            var cards = new List<Card>();
             
             // Neutrals
             cards.Add(CardFactory.CreateCard("Grunt"));
             cards.Add(CardFactory.CreateCard("Tank"));
             cards.Add(CardFactory.CreateCard("Minion"));
             
             // Archers (Fire/Water)
             cards.Add(CardFactory.CreateCard("Archer of the Flame"));
             cards.Add(CardFactory.CreateCard("Archer of the Ocean"));
             
             // Warriors (Air/Earth)
             cards.Add(CardFactory.CreateCard("Air Warrior"));
             cards.Add(CardFactory.CreateCard("Earth Warrior"));
             
             // Energies (Need all types)
             for(int i=0; i<3; i++) cards.Add(CardFactory.CreateCard("Air Energy"));
             for(int i=0; i<3; i++) cards.Add(CardFactory.CreateCard("Water Energy"));
             for(int i=0; i<3; i++) cards.Add(CardFactory.CreateCard("Fire Energy"));
             for(int i=0; i<3; i++) cards.Add(CardFactory.CreateCard("Earth Energy"));
             
            return cards;
        }

        static void PrintGameState(Game game)
        {
            Console.WriteLine($"\n--- Turn {game.TurnCount} - Player {game.CurrentPlayer.Id}'s Turn ---");
            Console.WriteLine($"Attackers Played This Turn: {game.AttackersPlayedThisTurn}");
            
            PrintPlayer(game.Player1, game.CurrentPlayer.Id == 1);
            PrintPlayer(game.Player2, game.CurrentPlayer.Id == 2);
        }

        static void PrintPlayer(Player p, bool isActive)
        {
            // Debug Logging for Verification
            try {
                using (var sw = System.IO.File.AppendText("verify.txt")) {
                    sw.WriteLine($"--- VERIFY STEP ---");
                    string status = isActive ? "*" : " ";
                    sw.WriteLine($"{status} Player {p.Id}: HP={p.Shield} Deck={p.Deck.Count}");
                    sw.WriteLine($"  Board: ");
                    for (int i=0; i<p.ActiveAttackers.Count; i++)
                    {
                        var a = p.ActiveAttackers[i];
                        string affinity = a.EnergyAffinity.HasValue ? a.EnergyAffinity.ToString() : "None";
                        sw.WriteLine($"    [{i}] {a.Name} (HP:{a.CurrentHealth}/{a.MaxHealth} Def:{a.Defense} Atk:{a.GetCurrentAttack()} Aff:{affinity} E:{a.AttachedEnergies.Count})");
                    }
                    sw.WriteLine($"  Hand: {p.Hand.Count}");
                    for (int i=0; i<p.Hand.Count; i++)
                    {
                         sw.WriteLine($"    [{i}] {p.Hand[i].Name}");
                    }
                    sw.WriteLine("");
                }
            } catch {}

            string statusConsole = isActive ? "*" : " ";
            Console.WriteLine($"{statusConsole} Player {p.Id}: HP={p.Shield} Deck={p.Deck.Count}");
            Console.WriteLine($"  Board: ");
            for (int i=0; i<p.ActiveAttackers.Count; i++)
            {
                var a = p.ActiveAttackers[i];
                string affinity = a.EnergyAffinity.HasValue ? a.EnergyAffinity.ToString() : "None";
                Console.WriteLine($"    [{i}] {a.Name} (HP:{a.CurrentHealth} Atk:{a.GetCurrentAttack()} Aff:{affinity} E:{a.AttachedEnergies.Count})");
            }
            // ...

            Console.WriteLine($"  Hand: ");
            for(int i=0; i<p.Hand.Count; i++)
            {
                string extra = "";
                if (p.Hand[i] is Attacker a) extra = $" [HP:{a.MaxHealth}]";
                Console.WriteLine($"    [{i}] {p.Hand[i]}{extra}");
            }
        }
    }
}
