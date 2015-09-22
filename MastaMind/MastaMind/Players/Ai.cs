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
        private ColorSequenceComparer colorseqcomparer;
        private const int COLUMNS = 4; // There are 4 pins per sequence
        private const int COLORS = 6; // six colors to choose from
        bool[] blackVisited; // store if we recorded a black hit here
        bool[] whiteVisited; // store if we recorded a white hit here
        private HashSet<ColorSequence> current_set; // We have a set of colorsequences that will decrease as the AI makes guesses.

        public static Ai instance;
        private Random rnd;

        public struct Match {
            public int blackHits, whiteHits;
            public Match(int blackHits, int whiteHits) {
                this.blackHits = blackHits;
                this.whiteHits = whiteHits;
            }
        }

        public Ai() {
            instance = this;
            colorseqcomparer = new ColorSequenceComparer();
            rnd = new Random();
        }

        public void CreateCode() {
            List<DataPin> code = new List<DataPin>();
            for ( int i = 0; i < 4; i++ ) {
                code.Add(new DataPin { XPosition = i, YPosition = GameEngine.instance.CurrentLevel, IsPinSet = true, PinColor = PinColors.colorToHexName[chooseRandomColor()] });
                Console.WriteLine("Code {0}", PinColors.hexColorToName[code[i].PinColor]);
            }
            Console.WriteLine("AI has set code!");
            GameEngine.instance.SetCode(code);
        }

        public void MakeGuess() {
            /// <summary>
            /// This function probably needs a remake with the algorithm for guessing, looks like shit
            /// </summary>
            current_set = new HashSet<ColorSequence>();
            blackVisited = new bool[COLUMNS];
            whiteVisited = new bool[COLUMNS];
            generatePossibilities(new ColorSequence(), 0);
            bool isCodeFound = false;
            while ( GameEngine.instance.IsGameOver == false || isCodeFound == false) {
                isCodeFound = IsCodeFound();
                if ( RulesEngine.instance.IsGuessLimitReached() ) {
                    GameEngine.instance.GameOver();
                }
                Thread.Sleep(40);
            }
        }

        private bool IsCodeFound() {
            // Convert the code in DataPin format to the ColorSequence

            byte[] seq = new byte[4];
            for ( int i = 0; i < COLUMNS; i++ ) {
                seq[i] = (byte)PinColors.AVAILABLE_COLORS_REVERSE[PinColors.hexColorToName[GameEngine.instance.createdCode[i].PinColor]];
            }

            ColorSequence code = new ColorSequence(seq);
           
            // The first guess is assumed to be the best if it is Red Red Yellow Purple Orange 
            byte[] firstGuess = { 1, 1, 2, 4, 8 };
            ColorSequence colorSeqfirstGuess = new ColorSequence(firstGuess);

            // Send the guess to the engine to check the guess against the code
            GameEngine.instance.CheckGuess(ColorSequenceToGuess(colorSeqfirstGuess));
            Thread.Sleep(20);

            Match match = calculateMatch(code, colorSeqfirstGuess);
            if ( match.blackHits == 4 ) {
                return true;
            }
            // Reduce the set by the restrictSet predicate
            current_set = restrictSet(colorSeqfirstGuess, match.whiteHits, match.blackHits);

            for ( int i = 1; i <= 8; i++ ) {
                ColorSequence next_guess = current_set.First();
                // Send the guess to the engine to check the guess against the code
                GameEngine.instance.CheckGuess(ColorSequenceToGuess(next_guess));
                Thread.Sleep(40);
                match = calculateMatch(code, next_guess);
                if ( match.blackHits == 4 ) {
                    Console.WriteLine("The AI DID IT");
                    return true;
                }
                current_set = restrictSet(next_guess, match.whiteHits, match.blackHits);
            }
            return false;
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

        #region ColorSequence related functions 

        
        public List<DataPin> ColorSequenceToGuess(ColorSequence sequenceGuess) {
            List<DataPin> guess = new List<DataPin>();
            for ( int i = 0; i < COLUMNS; i++ ) {
                guess.Add(new DataPin {
                    XPosition = i,
                    YPosition = GameEngine.instance.CurrentLevel,
                    IsPinSet = true,
                    PinColor = PinColors.colorToHexName[PinColors.AVAILABLE_COLORS[sequenceGuess[i]]],
                });
            }
            return guess;
        }

        private void generatePossibilities(ColorSequence c, int n) {
            if ( n == COLUMNS ) {
                current_set.Add(c);
                return;
            }
            for ( int i = 0; i < COLORS; i++ ) {
                ColorSequence c2 = (ColorSequence)c.Clone();
                c2[n] = (byte)( 1 << i );
                generatePossibilities(c2, n + 1);
            }
        }
        private HashSet<ColorSequence> restrictSet(ColorSequence c, int w, int b) {
            HashSet<ColorSequence> new_set = new HashSet<ColorSequence>(colorseqcomparer);
            foreach ( ColorSequence possibility in current_set ) {
                Match m = calculateMatch(possibility, c);
                if ( m.blackHits == b && m.whiteHits == w )
                    new_set.Add(possibility);
            }
            return new_set;
        }

        private Match calculateMatch(ColorSequence guess, ColorSequence c) {
            int blackHits = 0;
            int whiteHits = 0;
            // compare for black hits
            for ( int i = 0; i < COLUMNS; i++ ) {
                blackVisited[i] = false;
                whiteVisited[i] = false;
                // compare the selected color with the code in same location to see if black hit
                if ( guess[i] == c[i] ) {
                    blackHits++;
                    blackVisited[i] = true;
                    continue;
                }
            }
            // compare for white hits
            for ( int i = 0; i < COLUMNS; i++ ) {
                if ( blackVisited[i] ) continue; // if guess generated black hit, no need to see if it will be a white hit
                // compare with all other colors in code to see if white hit
                for ( int j = 0; j < COLUMNS; j++ ) {
                    if ( j == i ) continue; // already checked these when looking for black hits above
                    if ( guess[j] != c[i] || blackVisited[j] || whiteVisited[j] ) continue;
                    whiteHits++;
                    whiteVisited[j] = true;
                    break;
                }
            }
            return new Match(blackHits, whiteHits);
        }

        public class ColorSequence : ICloneable {

            private byte[] colors;
            public ColorSequence() {
                colors = new byte[4];
            }
            public ColorSequence(byte[] colors) {
                this.colors = colors;
            }
            public byte this[int index] {
                get {
                    return colors[index];
                }
                set {
                    colors[index] = value;
                }
            }
            public object Clone() {
                return new ColorSequence((byte[])colors.Clone());
            }
            public byte[] getColorsArray() {
                return colors;
            }

            public List<string> ToSequence() {
                List<string> sequence = new List<string>();
                for ( int i = 0; i < colors.Length; i++ ) {           
                    sequence.Add(PinColors.AVAILABLE_COLORS[colors[i]]);
                }
                return sequence;
            }

            public override string ToString() {
                String s = "[";
                for ( int i = 0; i < colors.Length; i++ ) {
                    s += PinColors.AVAILABLE_COLORS[colors[i]];
                    if ( i != colors.Length - 1 ) {
                        s += ",";
                    }
                }
                s += "]";
                return s;
            }
        }

        public class ColorSequenceComparer : EqualityComparer<ColorSequence> {
            public override bool Equals(ColorSequence x, ColorSequence y) {
                for ( int i = 0; i < 4; i++ )
                    if ( x[i] != y[i] )
                        return false;
                return true;
            }

            public override int GetHashCode(ColorSequence obj) {
                return ( obj.getColorsArray() ).GetHashCode();  
            }
        }
    }
    #endregion
}
