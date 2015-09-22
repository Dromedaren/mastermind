using System;
using System.IO;
using System.Threading;
using System.Runtime.InteropServices;
using Newtonsoft.Json;
using Newtonsoft.Json.Utilities;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using Newtonsoft.Json.Schema;
using System.Windows;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

using MastaMind;
using MastaMind.DataRepresentations;
using MastaMind.RuleEngine;


namespace MastaMind.Engine {
    public class GameEngine : INotifyPropertyChanged {
      
        #region Game-engine fields
        public string filesavelocation = @"C:\Users\elili552\Documents\gamedata.json";
        public event PropertyChangedEventHandler PropertyChanged;

        public static GameEngine instance;

        public Player player;
        public Ai ai;

        public List<DataPin> createdCode;
        public List<DataPin> CurrentGuessRowCode;
        public List<DataCluePin> CurrentClueRowCode;
        private List<List<DataPin>> gameBoard;
        private List<List<DataCluePin>> gameBoardClues;
        private bool guessButtonEnableStatus;
        private bool isGameOver;
        private bool isCodeSet;
        private int currentLevel = 0;
        private int guessesLeft;
        public  String lastChosenPinColor;
        private String whosCodeBreaker;
        private String whosCodeMaker;
        public BrushConverter brushConverter;
        public Dictionary<String, int> code_numberOfWhatColor;
        #endregion

        #region Game-engine constants
        public const int MAX_LEVELS = 10; // 0-9 is 10 levels
        #endregion

        #region properties

        public bool GuessButtonEnableStatus {
            get {
                return guessButtonEnableStatus;
            }
            set {
                guessButtonEnableStatus = value;
                OnPropertyChanged("GuessButtonEnableStatus");
            }
        }


        public int CurrentLevel {
            get {
                return currentLevel;
            }
            set { 
                currentLevel = value;
            }
        }


        public bool IsGameOver {
            get {
                return isGameOver;
            }
            set {
                isGameOver = value;
            }
        }


        public String WhosCodeBreaker {
            get { return whosCodeBreaker; }
            set {
                whosCodeBreaker = value;
                OnPropertyChanged("WhosCodeBreaker");
            }
        }


        public String WhosCodeMaker {
            get {
                return whosCodeMaker;
            }
            set {
                whosCodeMaker = value;
                if ( whosCodeMaker == "Player" ) { 
                    GuessButtonEnableStatus = false;
                } else {
                    GuessButtonEnableStatus = true;
                }
                OnPropertyChanged("WhosCodeMaker");
            }
        }


        public bool IsCodeSet {
            get {
                return isCodeSet;
            }
            set {
                isCodeSet = value;
                OnPropertyChanged("IsCodeSet");
                if ( isCodeSet && whosCodeMaker == "Player" ) {
                    // AI.BeginGuessing();
                    ai.MakeGuess();
                }
            }
        }


        public int GuessesLeft {
            get { return guessesLeft; }
            set {
                guessesLeft = value;
                OnPropertyChanged("GuessesLeft");
            }
        }
        #endregion

        public GameEngine(String whoStarts) {
            /// <summary>
            /// Game-engine constructor 
            /// </summary>
            instance = this;
            player = new Player();
            
            ai = new Ai();

            WhosCodeMaker = whoStarts;
            InitializeEngine();
            DetermineStartTurn();
        }

        #region Game-Engine Functions
        private void InitializeEngine() {
            GuessesLeft = MAX_LEVELS;
            CurrentLevel = 0;
            brushConverter = new BrushConverter();
            gameBoard = new List<List<DataPin>>();
            gameBoardClues = new List<List<DataCluePin>>();
            createdCode = new List<DataPin>();
            CurrentClueRowCode = new List<DataCluePin>();
            CurrentGuessRowCode = new List<DataPin>();
            isCodeSet = false;
            isGameOver = false;
        }

        public void DetermineStartTurn() {
            if ( WhosCodeMaker == "Player" ) {
                WhosCodeBreaker = "Ai";
            } else {
                ai.CreateCode();
                WhosCodeBreaker = "Player";
            }
        }

        public void SetCode(List<DataPin> code) {
            createdCode = code;
            IsCodeSet = true;
        }

        public void DecreaseNGuesses() {
            CurrentLevel += 1;
            GuessesLeft = ( guessesLeft - 1 );  
        }

        public void GameWon() {
            IsGameOver = true;
            SaveGameData(CurrentClueRowCode, CurrentGuessRowCode); // If the game is over, save the state
            MessageBox.Show("Code is correct!", "Round Over!", MessageBoxButton.OK);
        }

        public void GameOver() {
            IsGameOver = true;
            SaveGameData(CurrentClueRowCode, CurrentGuessRowCode); // If the game is over, save the state
            MessageBox.Show("You have reached the limit of available guesses!", "Game Over!", MessageBoxButton.OK);
        }

        public void ResetGame() {
            InitializeEngine(); 
            SwitchCodeMakerAndBreaker();
            DetermineStartTurn();
        }

        private void SwitchCodeMakerAndBreaker() {
            var temp = WhosCodeMaker;
            WhosCodeMaker = WhosCodeBreaker;
            WhosCodeBreaker = temp;
        }

        public void SaveGameData(List<DataCluePin> currentClue, List<DataPin> currentGuess) {
            // Save Data using LINQ and a new instantiation of GameData

            List<DataPin> savedGameBoardData = (from pin in currentGuess select pin).ToList();
            List<DataCluePin> savedGameBoardClueData = (from clue in currentClue select clue).ToList();
            List<DataPin> savedCreatedCode = (from codepin in createdCode select codepin).ToList();

            gameBoard.Add(savedGameBoardData);
            gameBoardClues.Add(savedGameBoardClueData);

            GameData gameData = new GameData {
                gameBoard = gameBoard,
                gameBoardClues = gameBoardClues,
                CurrentLevel = CurrentLevel,
                GuessesLeft = GuessesLeft,
                createdCode = savedCreatedCode,
                IsGameOver = IsGameOver,
                WhosCodeBreaker = whosCodeBreaker,
                WhosCodeMaker = WhosCodeMaker
            };

            using ( StreamWriter file = File.CreateText(filesavelocation) ) {
                JsonSerializer seralizer = new JsonSerializer();
                seralizer.Serialize(file, gameData);
            }
        }

        public bool CheckGuess(List<DataPin> guess) {
            var currentClueRow = new List<DataCluePin>();
            var currentGuessRow = guess;
            bool[] blackVisited = new bool[4]; // We have 4 positions for clues
            bool[] whiteVisited = new bool[4]; // same here
            int blackHits = 0;
            int whiteHits = 0;

            // Compare to see what cluepegs become black
            for ( int i = 0; i < 4; i++ ) {
                if ( createdCode[i].PinColor.Equals(guess[i].PinColor) ) {
                    blackHits++;
                    blackVisited[i] = true;

                    // Create and add a Black clue peg
                    currentClueRow.Add(new DataCluePin {
                        InternalXPosition = i,
                        XPosition = 4,
                        YPosition = guess[i].YPosition,
                        IsPinSet = true,
                        PinColor = "Black"
                    });

                } 
            }
          
            for ( int i = 0; i < 4; i++ ) {
                if (blackVisited[i]) continue; // If we have already gone over this index i.e setting it already then skip
                for ( int j = 0; j < 4; j++ ) {
                    if (j == i) continue; // if the indexes match then we have been here already.
                    if (!blackVisited[j] && !whiteVisited[j] && createdCode[j].PinColor == guess[i].PinColor) {
                        whiteHits++;
                        whiteVisited[j] = true;

                        // Create and add a White clue peg
                        currentClueRow.Add(new DataCluePin {
                            InternalXPosition = guess[i].XPosition,
                            XPosition = 4,
                            YPosition = guess[j].YPosition,
                            IsPinSet = true,
                            PinColor = "White"
                        });
                        break;
                    }

                }
            }
            // Sort the cluepins so they match the guess indexes i.e first guess will equal the first clue etc.
            currentClueRow.Sort((x, y) => x.InternalXPosition.CompareTo(y.InternalXPosition));
            CurrentClueRowCode = currentClueRow;

            SaveGameData(currentClueRow, currentGuessRow);
            if ( blackHits == 4 ) {
                // We have to sort the list so that each pin matches the uis foreach loop by x-position
                GameWon();
                return true;
            }
            DecreaseNGuesses();
            return false;
        }
        #endregion

        public void OnPropertyChanged(string property) {
            var handler = PropertyChanged;
            if ( handler != null ) {
                handler(this, new PropertyChangedEventArgs(property));
            }
        }
    }
}