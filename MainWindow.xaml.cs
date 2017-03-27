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

namespace Pong
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        Game myGame = new Game();

        public MainWindow()
        {
            InitializeComponent();
            this.DataContext = myGame;

            MainGrid.Children.Add(myGame.board);
        }
    }

    public class Game
    {
        public Canvas board = new Canvas();
        Stick playerStick;

        public Game()
        {
            playerStick = new Stick(board);
            board.Children.Add(playerStick.stick);
        }

    }

    public class Stick
    {
        enum ControlType
        {
            Human,
            AI
        }

        Point coordinates;
        //int y;
        //public int x;
        public Rectangle stick = new Rectangle();

        public Stick(Canvas board)
        {//Make this take into account the ControlType of the new stick as this only works for the one placed on the right which is the player controlled one
         //Also the actual width and height property on canvas are not initialized until after I initialize my game class and thus cannot be used in setting coord. so i use vals
            coordinates.X = 500;
            coordinates.Y = 200;

            Canvas.SetLeft(stick, coordinates.X);
            Canvas.SetTop(stick, coordinates.Y);

            stick.Height = 75;
            stick.Width = 10;
            stick.Fill = Brushes.Black;
        }

    }
}
