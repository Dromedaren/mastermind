using System;
using System.Windows;
using System.Collections.Generic;
using System.Diagnostics;
using MastaMind.DataRepresentations;
using MastaMind.Engine;

namespace MastaMind.RuleEngine {
    public class RulesEngine {

        public static RulesEngine instance;

        public RulesEngine() {
            instance = this;
        }

        #region RuleEngine Functions
        public bool IsGameboardUnlocked() {
            // The gameboard cannot be manipulated if the codemaker is the codebreaker
            if (GameEngine.instance.WhosCodeMaker == GameEngine.instance.WhosCodeBreaker) {
                return false;
            }
            return true;
        }

        public bool IsLegalMove(Pin pin) {
            // test if the pin clicked is at the currentlevel
            if ( GameEngine.instance.IsGameOver ) {
                MessageBox.Show("The game is over, restart to play more!");
                return false;
            }

            if ( pin.YPosition != GameEngine.instance.CurrentLevel ) {
                MessageBox.Show("Cannot set a pin that is not on the current level!");
                return false;
            }
            return true;
        }

        public bool IsGuessLimitReached() {
            if ( GameEngine.instance.GuessesLeft > 0 ) {
                return false;
            } else {
                return true;
            }
        }

        public bool IsGuessPinsSet(List<DataPin> guess) {
            // make sure that the four guess-pins are set
            if ( guess.Count != 4 ) {
                return false;
            } else {
                return true;
            }
        }

        public bool IsCodePinsSet(List<DataPin> code) {
            // make sure that the four code-pins are set
            if (code.Count != 4) {
                return false;
            }
            return true;
        }
    }
        #endregion
}
