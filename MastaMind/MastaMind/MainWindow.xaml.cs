#region Using
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Media.Effects;
using System.ComponentModel;
#endregion

namespace MastaMind
{
    /// <summary>
    /// // <author> Ellie Lilja </author>
    /// // Innovativ Programmering, Linköpings Universitet
    /// // TDDD49
    /// Interaction logic for GUI, MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            this.DataContext = this;
        }

        #region MainMenu
        /// <summary>
        /// Menu Options goes here
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>

        void QuitMasterMind(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }
        #endregion

        private void SpelareVsAI_Click(object sender, RoutedEventArgs e)
        {
            App.Current.MainWindow = new GameWindow();
            this.Close();
            App.Current.MainWindow.Show();
        }
    }
}
