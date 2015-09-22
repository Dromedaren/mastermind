using System;
using System.Collections.Generic;
using System.Timers;
using System.Linq;
using System.Threading;
using MastaMind;
using MastaMind.DataRepresentations;
using MastaMind.RuleEngine;
using MastaMind.Engine;

namespace MastaMind {
    public class Ai {
        private int guessAttempt;
        private List<DataPin> newGuess;
        private List<List<string>> combinations;
        public List<string> colors = new List<string>() { "Red", "Yellow", "Blue", "Purple", "Green", "Orange" };
        public static Ai instance;
        private Random rnd;

        public Ai() {
            instance = this;
            //Combination.instance.createCombinations();
            rnd = new Random();
        }

        public void CreateCode() {
            List<DataPin> code = new List<DataPin>();
            for ( int i = 0; i < 4; i++ ) {
                code.Add(new DataPin { XPosition = i, YPosition = GameEngine.instance.CurrentLevel, IsPinSet = true, PinColor = PinColors.colorToHexName[chooseRandomColor()] });
            }
            Console.WriteLine("AI has set code!");
            GameEngine.instance.SetCode(code);
        }

        public String chooseRandomColor() {
            int number = rnd.Next(1, 6);
            switch ( number ) {
                case 1:
                    return "Red";
                case 2:
                    return "Yellow";
                case 3:
                    return "Blue";
                case 4:
                    return "Purple";
                case 5:
                    return "Green";
                case 6:
                    return "Orange";
                default:
                    return "";
            }
        }

        private void createGuess() {
            Console.WriteLine("Ai creating a guess");
            if ( guessAttempt == 0 ) {
                combinations = new List<List<string>>();
                combinations = Combination.instance.PossibleCombinations;
            }

            while ( combinations.Count() > 0 ) {
                if ( combinations.Count() == 1296 ) {
                    DataPin x1 = new DataPin { XPosition = 0, YPosition = GameEngine.instance.CurrentLevel, PinColor = PinColors.colorToHexName["Red"] };
                    DataPin x2 = new DataPin { XPosition = 1, YPosition = GameEngine.instance.CurrentLevel, PinColor = PinColors.colorToHexName["Red"] };
                    DataPin x3 = new DataPin { XPosition = 2, YPosition = GameEngine.instance.CurrentLevel, PinColor = PinColors.colorToHexName["Blue"] };
                    DataPin x4 = new DataPin { XPosition = 3, YPosition = GameEngine.instance.CurrentLevel, PinColor = PinColors.colorToHexName["Blue"] };
                    newGuess = new List<DataPin>() { x1, x2, x3, x4 };
                } else {
                    DataPin x1 = new DataPin { XPosition = 0, YPosition = GameEngine.instance.CurrentLevel, PinColor = PinColors.colorToHexName[combinations[0][0]] };
                    DataPin x2 = new DataPin { XPosition = 1, YPosition = GameEngine.instance.CurrentLevel, PinColor = PinColors.colorToHexName[combinations[0][1]] };
                    DataPin x3 = new DataPin { XPosition = 2, YPosition = GameEngine.instance.CurrentLevel, PinColor = PinColors.colorToHexName[combinations[0][2]] };
                    DataPin x4 = new DataPin { XPosition = 3, YPosition = GameEngine.instance.CurrentLevel, PinColor = PinColors.colorToHexName[combinations[0][3]] };
                    newGuess = new List<DataPin>() { x1, x2, x3, x4 };
                }

                // calculate outcome from a set guess
                GameEngine.instance.CheckGuess(newGuess);
                if ( RulesEngine.instance.IsGuessLimitReached() ) {
                    GameEngine.instance.GameOver();
                }
                if ( GameEngine.instance.IsGameOver != true ) {
                    var clueOutcome = GameEngine.instance.CurrentGuessRowCode;
                    List<string> outcome = new List<string>();
                    List<List<string>> dump = new List<List<string>>();
                    foreach ( var clue in clueOutcome ) {
                        outcome.Add(PinColors.hexColorToName[clue.PinColor]);
                    }
                    combinations.RemoveAll(combination => !CheckGuess(newGuess, combination).SequenceEqual(outcome));
                } else {
                    break;
                }
            }

        }

        public void MakeGuess() {
            /// <summary>
            /// This function probably needs a remake with the algorithm for guessing, looks like shit
            /// </summary>
            while ( GameEngine.instance.IsGameOver == false ) {
                createGuess();
                guessAttempt += 1;
            }
        }

        public List<string> CheckGuess(List<DataPin> guess, List<string> combination) {
            #region setup
            List<string> outcome = new List<string>();
            var guess_numberOfWhatColor = new Dictionary<String, int>();
            var combination_numberOfWhatColor = new Dictionary<String, int>();
            var currentClueRow = new List<DataCluePin>();
            var currentGuessRow = guess;
            var checkIfGuessColorInCode = new List<DataPin>();

            guess_numberOfWhatColor.Add("Red", 0);
            guess_numberOfWhatColor.Add("Blue", 0);
            guess_numberOfWhatColor.Add("Yellow", 0);
            guess_numberOfWhatColor.Add("Green", 0);
            guess_numberOfWhatColor.Add("Purple", 0);
            guess_numberOfWhatColor.Add("Orange", 0);

            combination_numberOfWhatColor.Add("Red", 0);
            combination_numberOfWhatColor.Add("Blue", 0);
            combination_numberOfWhatColor.Add("Yellow", 0);
            combination_numberOfWhatColor.Add("Green", 0);
            combination_numberOfWhatColor.Add("Purple", 0);
            combination_numberOfWhatColor.Add("Orange", 0);

            // setup count for each color in the combination
            foreach ( var entry in combination ) {
                combination_numberOfWhatColor[entry] += 1;
            }

            #endregion

            /* First pass, select pins that are both in the same color and position
             * then add to the cluepins and fire the redraw event
             */
            for ( int i = 0; i < guess.Count; i++ ) {
                // Keep a count for each color in the guess, this is also done with the created code.
                guess_numberOfWhatColor[PinColors.hexColorToName[guess[i].PinColor]] += 1;

                // if they match up, add a black clue peg
                if ( guess[i].PinColor == PinColors.colorToHexName[combination[i]] ) {
                    currentClueRow.Add(new DataCluePin {
                        InternalXPosition = guess[i].XPosition,
                        XPosition = 4,
                        YPosition = guess[i].YPosition,
                        IsPinSet = true,
                        PinColor = "Black"
                    });
                } else {
                    // Add to this list for futher checking if position not equal but color is
                    checkIfGuessColorInCode.Add(guess[i]);
                }

            }

            // Were all pins correct after the first passthrough?
            // If so, then the game is won
            if ( currentClueRow.Count == 4 ) {
                // We have to sort the list so that each pin matches the uis foreach loop by x-position
                currentClueRow.Sort((x, y) => x.InternalXPosition.CompareTo(y.InternalXPosition));
                foreach ( var clue in currentClueRow ) {
                    outcome.Add(PinColors.hexColorToName[clue.PinColor]);
                }
                return outcome;
                // return list of the clue pegs in string list
            }

            /*
             * Second pass, go through the remainding pins,
             * check each pin if color is in the code and that the count (n) of that color is either less or equal to the count of that color in the code
             * If they are the same then add the cluepin to that index as a pin that is the right color but in the wrong place
             * else then add the cluepin and make it gray
            */
            foreach ( var pin in checkIfGuessColorInCode ) {
                if ( combination_numberOfWhatColor.ContainsKey(PinColors.hexColorToName[pin.PinColor]) && ( guess_numberOfWhatColor[PinColors.hexColorToName[pin.PinColor]] <= combination_numberOfWhatColor[PinColors.hexColorToName[pin.PinColor]] ) ) {
                    currentClueRow.Add(new DataCluePin {
                        InternalXPosition = pin.XPosition,
                        XPosition = 4,
                        YPosition = pin.YPosition,
                        IsPinSet = true,
                        PinColor = "White"
                    });
                } else {
                    currentClueRow.Add(new DataCluePin {
                        InternalXPosition = pin.XPosition,
                        XPosition = 4,
                        YPosition = pin.YPosition,
                        IsPinSet = true,
                        PinColor = "Gray"
                    });
                }
            }

            currentClueRow.Sort((x, y) => x.InternalXPosition.CompareTo(y.InternalXPosition));
            foreach ( var clue in currentClueRow ) {
                outcome.Add(PinColors.hexColorToName[clue.PinColor]);
            }
            return outcome;
        }

        private class Combination {
            public Combination() { }
            public static Combination instance = new Combination();

            private List<string> colors = new List<string>() { "Red", "Yellow", "Blue", "Purple", "Green", "Orange" };
            List<string> currentRow = new List<string>();
            private List<List<string>> allCombinations;
            private List<List<string>> AllCombinations {
                get { return allCombinations; }
                set { allCombinations = value; }
            }

            private List<List<string>> possibleCombinations;
            public List<List<string>> PossibleCombinations {
                get { return possibleCombinations; }
                set { possibleCombinations = value; }
            }

            public void createCombinations() {
                List<List<string>> combination = new List<List<string>>();
                rec(combination, 0, 0, 0, 0);
            }

            private void rec(List<List<string>> combination, int a, int b, int c, int d) {
                // Orsakar stackoverflow, Skiter snart i det här.
                // if d = colors.Count
                if ( d == 6 ) {
                    Console.WriteLine("All done");
                    allCombinations = combination;
                    possibleCombinations = combination;
                    return;
                } else {
                    currentRow.Add(colors.ElementAt(a));
                    currentRow.Add(colors.ElementAt(b));
                    currentRow.Add(colors.ElementAt(c));
                    currentRow.Add(colors.ElementAt(d));
                    combination.Add(currentRow);
                    currentRow.Clear();
                    // Modulus creation of all combinations 1111-6666
                    if ( a % colors.Count() == 0 && a != 0) {
                        a = 0;
                        b++;
                        if ( b % colors.Count() == 0 && b != 0 ) {
                            b = 0;
                            c++;
                            if ( c % colors.Count() == 0 && c != 0 ) {
                                c = 0;
                                d++;
                            }
                        }
                    }
                    rec(combination, a, b, c, d);
                }
            }
        }

    }
}
