using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
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

namespace MastaMind {
    /// <summary>
    /// A GUI Representation of a CluePin
    /// </summary>
    public partial class CluePin : Button, INotifyPropertyChanged  {
        public event PropertyChangedEventHandler PropertyChanged;
        public BrushConverter brushConverter;

        private int xPosition;
        private int yPosition;
        private int internalXPosition;

        public int InternalXPosition { get { return internalXPosition; } set { internalXPosition = value; } }
        public int XPosition { get { return xPosition; } set { xPosition = value; Grid.SetColumn(this, value); OnPropertyChanged("XPosition"); } }
        public int YPosition { get { return yPosition; } set { yPosition = value; Grid.SetRow(this, value); OnPropertyChanged("YPosition"); } }

        private Brush brushPinColor;
        private string pinColor;
        public bool isPinSet { get; set; }

        public CluePin() {
            InitializeComponent();
            brushConverter = new BrushConverter();
        }
        
        public String PinColor {
            get { return pinColor; }
            set {
                if ( value != null || value != "" ) {
                    pinColor = value;
                    this.isPinSet = true;
                    CluePinColor = (Brush)brushConverter.ConvertFromString(pinColor);
                }
            }
        }
        public Brush CluePinColor {
            get {
                return brushPinColor;
            }
            set {
                //if ( value != null ) {
                    brushPinColor = value;
                    OnPropertyChanged("CluePinColor");
                //}
            }
        }

        public void OnPropertyChanged(string property) {
            var handler = PropertyChanged;
            if ( handler != null ) {
                handler(this, new PropertyChangedEventArgs(property));
            }
        }
    }
}
