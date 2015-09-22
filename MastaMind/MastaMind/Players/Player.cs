using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.ComponentModel;

namespace MastaMind
{
    public class Player
    {
        public static Player instance;

        public Player() {
            instance = this;
        }
    }
}
