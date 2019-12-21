using System;
using System.IO;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using System.ComponentModel;
using MastaMind;
using MastaMind.DataRepresentations;
using MastaMind.Engine;
using MastaMind.RuleEngine;
using Newtonsoft.Json;

namespace MastaMind
{
    /// <summary>
    /// This class contains data nessecary for the GUI and instances of the engine and rule engine
    /// for GameWindow.xaml
    /// </summary>

    public partial class GameWindow : INotifyPropertyChanged {
        #region GameWindow Fields
        public event PropertyChangedEventHandler PropertyChanged;
        private FileSystemWatcher watcher;
		string filesavelocation = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + @"\gamedata.json";

        List<List<Pin>> GUIBoardPins;
        List<List<CluePin>> GUICluePins;

        public static GameWindow instance;
        private RulesEngine rules;
        private GameEngine engine;
        //public Player player;
        //public Ai ai;
        #endregion

        public GameWindow() {
            instance = this;
            InitializeFileWatcher();
            InitializeComponent(); // initialize Board and custom UserControls
            InitializeGUIPins();
            
            // If we have a saved game state then load from that instead of starting from scratch
            if ( DoesFileExist(filesavelocation) ) {
                GameData gameData = JsonConvert.DeserializeObject<GameData>(File.ReadAllText(filesavelocation));
                engine = new GameEngine(gameData.WhosCodeMaker);
                engine.CurrentLevel = gameData.CurrentLevel;
                engine.createdCode = gameData.createdCode;
                engine.GuessesLeft = gameData.GuessesLeft;
                engine.IsGameOver = gameData.IsGameOver;
                LoadGameAtStartup(gameData);
            } else {
                engine = new GameEngine("Ai");
            }

            rules = new RulesEngine();
            #region context setting for properties
                GuessesLeft.DataContext = engine;
                WhosCodeMaker.DataContext = engine;
                WhosCodeBreaker.DataContext = engine;
                GuessButton.DataContext = engine;
            #endregion
            DetermineStartGUILayout();
        }

        #region FileSystemWatcher
        private void InitializeFileWatcher() {
            watcher = new FileSystemWatcher();
            watcher.Path = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + "\";
            watcher.Filter = @"gamedata.json";
            watcher.Changed += new FileSystemEventHandler(LoadGameData);
            watcher.EnableRaisingEvents = true;
        }

        private bool DoesFileExist(string file) {
            //check that problem is not in destination file
            if ( File.Exists(file) == true ) {
                return true;
            } else {
                return false;
            }
        }
        #endregion

        #region GameWindow Functions

        public void InitializeGUIPins() {
            /// <summary>
            /// We instantiate the GUI containers
            /// </summary>
            GUIBoardPins = new List<List<Pin>> {
                new List<Pin>(),
                new List<Pin>(),
                new List<Pin>(),
                new List<Pin>(),
                new List<Pin>(),
                new List<Pin>(),
                new List<Pin>(),
                new List<Pin>(),
                new List<Pin>(),
                new List<Pin>(),
            };
			
            GUICluePins = new List<List<CluePin>> {
                new List<CluePin>(),
                new List<CluePin>(),
                new List<CluePin>(),
                new List<CluePin>(),
                new List<CluePin>(),
                new List<CluePin>(),
                new List<CluePin>(),
                new List<CluePin>(),
                new List<CluePin>(),
                new List<CluePin>()
            };

            ResetGameBoard();
            SetGameBoard();
        }

        void SetGameBoard() {
            /// <summary>
            /// We want to hold the GUI Representation as a list of pin rows so that we can match them up with the
            /// game data we have. This way we can reference the data and expect savedgamedata[row] == GUIrepresentation[row]
            /// </summary>
            List<Pin> pins = ( from pin in GameBoardGrid.Children.OfType<Pin>()
                               select pin ).ToList();
            foreach ( var pin in pins ) {
                GUIBoardPins[pin.YPosition].Add(pin);
            }

            List<CluePin> cluePins = ( from cluepin in GameBoardGrid.Children.OfType<CluePin>()
                                       select cluepin ).ToList();
            foreach ( var cluepin in cluePins ) {
                GUICluePins[cluepin.YPosition].Add(cluepin);
            }
        }

        List<List<Pin>> GetGameBoardPins() {
            return GUIBoardPins;
        }

        List<List<CluePin>> GetGameBoardClues() {
            return GUICluePins;
        }

        void LoadGameAtStartup(GameData gameData) {
            ///<summary>
            /// This function sets up the GUI with the saved state information
            ///</summary>
            var boardpins = GetGameBoardPins();
            var cluepins = GetGameBoardClues();

            // Select the GUI Codepins with LINQ
            var codepins = (from codepin in CreateCodeGrid.Children.OfType<Pin>() select codepin).ToList();

            // Query the game data with LINQ
            var queryGameBoardPins = from pinRow in gameData.gameBoard select pinRow;
            var queryGameBoardClues = from clueRow in gameData.gameBoardClues select clueRow;
            var querySavedCode = from codePin in gameData.createdCode select codePin;


            foreach (var gameDataPinList in queryGameBoardPins)
            {
                foreach (var gameDataPin in gameDataPinList)
                {
                    boardpins[gameDataPin.YPosition][gameDataPin.XPosition].PinColor = gameDataPin.PinColor;
                }
            }

            foreach (var gameDataCluePinList in queryGameBoardClues)
            {
                foreach (var gameDataCluePin in gameDataCluePinList)
                {
                    Console.WriteLine("internal x: {0}", gameDataCluePin.InternalXPosition);
                    cluepins[gameDataCluePin.YPosition][gameDataCluePin.InternalXPosition].PinColor = gameDataCluePin.PinColor;
                }
            }
            foreach (var savedDataPin in querySavedCode)
            {
                codepins[savedDataPin.XPosition].PinColor = savedDataPin.PinColor;
            }
        }

        void LoadGameData(object sender, FileSystemEventArgs eventArgs) {
            ///<summary>
            /// FileSystemWatcher fires this function on file change events.
            /// We reload the GUI from the gamedata
            ///</summay>
           Thread.Sleep(40);
            try {

                // so we dont fire multiple events for one action
                watcher.EnableRaisingEvents = false;
                Console.WriteLine("Redrawing board");

                GameData gameData = JsonConvert.DeserializeObject<GameData>(File.ReadAllText(filesavelocation));

                // Invoke the updating on the UI thread.
                GameBoardGrid.Dispatcher.Invoke(
                    (Action)( () => {
                        var boardpins = GetGameBoardPins();
                        var cluepins = GetGameBoardClues();

                        // Query the game data with LINQ
                        var queryGameBoardPins = from pinRow in gameData.gameBoard select pinRow;
                        var queryGameBoardClues = from clueRow in gameData.gameBoardClues select clueRow;

                        foreach ( var gameDataPinList in queryGameBoardPins ) {
                            foreach ( var gameDataPin in gameDataPinList ) {
                                boardpins[gameDataPin.YPosition][gameDataPin.XPosition].PinColor = gameDataPin.PinColor;
                            }
                        }

                        foreach (var gameDataCluePinList in queryGameBoardClues)
                        {
                            foreach (var gameDataCluePin in gameDataCluePinList)
                            {
                                Console.WriteLine("internal x: {0}", gameDataCluePin.InternalXPosition);
                                cluepins[gameDataCluePin.YPosition][gameDataCluePin.InternalXPosition].PinColor = gameDataCluePin.PinColor;
                            }
                        }
      
                    }));
                
            } catch ( System.IO.IOException e ) {
                Console.WriteLine("Error reading from {0}. Message = {1}", eventArgs.Name, e.Message);
            }
            finally {
               watcher.EnableRaisingEvents = true;
            }
        }

        public void ResetGameBoard() {
            /// <summary>
            /// Resets the gameboard to its initial state by
            /// clearing the gameboard pins, code-pins and clue-pins
            /// </summary>
            InitializeComponent();
            foreach ( var pin in GameBoardGrid.Children.OfType<Pin>() ) {
                pin.BrushPinColor = null;
            }

            foreach ( var cluepin in GameBoardGrid.Children.OfType<CluePin>() ) {
                cluepin.CluePinColor = null;
            }

            foreach (var codepin in CreateCodeGrid.Children.OfType<Pin>())
            {
                codepin.BrushPinColor = null;
            }
        }
        #endregion
  
        #region Gamewindow Game Functions
        public void DetermineStartGUILayout() {
            CodeChoice.Visibility = Visibility.Visible;
            CreateCodeButton.Visibility = Visibility.Visible;

            if ( engine.WhosCodeMaker == "Ai" ) {
                engine.GuessButtonEnableStatus = true;

                // If the Ai is the codemaker then hide the code creation panel and the create code button.
                CodeChoice.Visibility = Visibility.Hidden;
                CreateCodeButton.Visibility = Visibility.Hidden;
            } else {
                // If the Player is the codemaker then hide the guess button.
                engine.GuessButtonEnableStatus = false;
            }
        }


        public List<DataPin> SelectCurrentRow() {
            List<DataPin> guess = new List<DataPin>();
            // LINQ query to select only the current row
            var queryCurrentRow = from pin in GameBoardGrid.Children.OfType<Pin>()
                                  where pin.YPosition == engine.CurrentLevel && pin.isPinSet
                                  select pin;

            foreach ( var pin in queryCurrentRow ) {
                guess.Add(new DataPin {
                    XPosition = pin.XPosition,
                    YPosition = pin.YPosition,
                    IsPinSet = true,
                    PinColor = pin.PinColor
                });
            }
            return guess;
        }
        #endregion

        #region GameWindow Clickhandlers
        private void Button_MakeGuess(object sender, RoutedEventArgs routedEventArgs) {
            /// <summary>
            /// Creates a set of Pins that hold the guess
            /// It is a new list of Pins matching the UI representation.
            /// This handler then sends the guess to the rule-engine and game-engine.
            /// </summary>

            // If the game is won disable the guessing etc.
            if ( engine.IsGameOver ) {
                MessageBox.Show("Restart to play more", "The game is over");
                return;
            }
           
            // Send guess to rules engine and check it
            List<DataPin> guess = SelectCurrentRow();

            if ( RulesEngine.instance.IsGuessPinsSet(guess) ) {        
                GameEngine.instance.CheckGuess(guess);
                if ( RulesEngine.instance.IsGuessLimitReached() ) {
                    GameEngine.instance.GameOver();
                }
            }
        }


        private void Button_CreateCode(object sender, RoutedEventArgs e) {
            /// <summary>
            /// Create a data copy of the pins representing the code
            /// then check with the rule-engine for correctness
            /// and set the code inside the game-engine
            /// </summary>
            
            List<DataPin> SetCodePins = new List<DataPin>();
            // We only add to the code list if the pin has a color,
            // then pass it to the rule && game-engine
            foreach (var pin in CreateCodeGrid.Children.OfType<Pin>())
            {
                if (pin.isPinSet) {
                    SetCodePins.Add(new DataPin { XPosition = pin.XPosition, YPosition = pin.YPosition, IsPinSet = true, PinColor = pin.PinColor });
                }
            }

            bool result = rules.IsCodePinsSet(SetCodePins);
            if ( result ) {
                Console.WriteLine("A Code has been set!");
                engine.SetCode(SetCodePins);
            }
            return;
        }


        private void Button_SelectColorClick(object sender, RoutedEventArgs e)
        {
            /// <summary>
            /// Clicking on the available colors selects the color for use on the gameboard
            /// </summary>
            /// 
            var selectedcolor = e.Source as Button;
            engine.lastChosenPinColor = selectedcolor.Background.ToString();
        }

        private void Button_NewGame_Click(object sender, RoutedEventArgs eventArgs) {
            Console.WriteLine("New game starting");
            ResetGameBoard();
            engine.ResetGame();
            DetermineStartGUILayout();
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