using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using MastaMind.DataRepresentations;

namespace MastaMind.Engine {
    public class GameData {
        public IList<List<DataPin>> gameBoard { get; set; }
        public IList<List<DataCluePin>> gameBoardClues { get; set; }
        public int CurrentLevel { get; set; }
        public int GuessesLeft { get; set; }
        public List<DataPin> createdCode { get; set; }
        public bool IsGameOver { get; set; }
        public String WhosCodeMaker { get; set; }
        public String WhosCodeBreaker { get; set; }
    }
}
