using System;
using System.Collections.Generic;
namespace MastaMind.Engine {
    public static class PinColors {
        public static List<String> pinColors_t = new List<String>() { "Red", "Yellow", "Blue", "Purple", "Green", "Orange" };

        public static Dictionary<int, string> AVAILABLE_COLORS = new Dictionary<int, string>() {
                {1,"Red"},
                {2,"Yellow"},
                {4,"Blue"},
                {8,"Purple"},
                {16,"Green"},
                {32,"Orange"},
            };

        public static Dictionary<string, int> AVAILABLE_COLORS_REVERSE = new Dictionary<string, int>() {
                {"Red", 1},
                {"Yellow", 2},
                {"Blue", 4},
                {"Purple", 8},
                {"Green", 16},
                {"Orange", 32},
            };

        public static Dictionary<string, string> hexColorToName = new Dictionary<string, string>{
            {"#FFFF0000","Red"},
            {"#FFFFFF00","Yellow"},
            {"#FF0000FF","Blue"},
            {"#FF800080","Purple"},
            {"#FF008000","Green"},
            {"#FFFFA500","Orange"}
        };

        public static Dictionary<string, string> colorToHexName = new Dictionary<string, string>{
            {"Red","#FFFF0000"},
            {"Yellow","#FFFFFF00"},
            {"Blue","#FF0000FF"},
            {"Purple","#FF800080"},
            {"Green","#FF008000"},
            {"Orange","#FFFFA500"}
        };
    }
}
