using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Drawing;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.ComponentModel;

using MastaMind.Engine;
using MastaMind.RuleEngine;

namespace MastaMind {
    /// <summary>
    /// GUI representation of a Pin
    /// </summary>
    public partial class Pin : Button, INotifyPropertyChanged {
        /// <summary>
        /// This is the GUI bindings for the Pin on the board 
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;
        public BrushConverter brushConverter;
        public ColorConverter colorConverter;

        private int xPosition;
        private int yPosition;
        public int XPosition { get { return xPosition; } set { xPosition = value; Grid.SetColumn(this, value); OnPropertyChanged("XPosition"); } }
        public int YPosition { get { return yPosition; } set { yPosition = value; Grid.SetRow(this, value); OnPropertyChanged("YPosition"); } }

        private Brush brushPinColor;
        private string pinColor;
        public bool isPinSet { get; set; }

        public Pin() {
            InitializeComponent();
            brushConverter = new BrushConverter();
        }

        public void OnPropertyChanged(string property) {
            var handler = PropertyChanged;
            if ( handler != null ) {
                handler(this, new PropertyChangedEventArgs(property));
            }
        }

        public String PinColor {
            get { return pinColor; }
            set {
                //if ( value != null || value != "" ) {
                    pinColor = value;
                    this.isPinSet = true;
                    BrushPinColor = (Brush)brushConverter.ConvertFromString(pinColor);
                //}
            }
        }
        public Brush BrushPinColor {
            get {
                return brushPinColor;
            }
            set {
                //if ( value != null ) {
                    brushPinColor = value;
                    OnPropertyChanged("BrushPinColor");
                //}
            }
        }

        private void BoardPin_Click(object sender, RoutedEventArgs e)
        {
            
            if (GameEngine.instance.lastChosenPinColor != null && RulesEngine.instance.IsLegalMove(this) && RulesEngine.instance.IsGameboardUnlocked()) {
                PinColor = GameEngine.instance.lastChosenPinColor;
            }
        }
    }
}
