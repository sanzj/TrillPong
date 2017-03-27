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

            KeyDown += MainWindow_KeyDown;
            KeyUp += MainWindow_KeyUp;
        }

        private void MainWindow_KeyUp(object sender, KeyEventArgs e)
        {
            myGame.pressedKey = Game.KeyInputs.None; //Ensures that input is only gotten when the key is being pressed and doesnt keep inputting after it is let go
        }

        private void MainWindow_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Up)
            {
                myGame.pressedKey = Game.KeyInputs.Up;
            }
            else if(e.Key == Key.Down)
            {
                myGame.pressedKey = Game.KeyInputs.Down;
            }
        }
    }

    public class Game
    {
        public enum KeyInputs
        {
            None,
            Up,
            Down
        }
        public KeyInputs pressedKey;
        public Canvas board = new Canvas();
        Stick playerStick;

        System.Windows.Threading.DispatcherTimer frameTimer = new System.Windows.Threading.DispatcherTimer();

        void InitializeTimer()
        {
            frameTimer.Interval = new TimeSpan(0, 0, 0, 0, 025);//Creates framespersecond by calling update and draw after ticks. every 25 millisecond is 40fps
            frameTimer.Tick += FrameTimer_Tick;
            frameTimer.Start();
        }

        private void FrameTimer_Tick(object sender, EventArgs e)
        {
            Update();
            Draw();
        }

        public Game()
        {
            playerStick = new Stick(board);
            board.Children.Add(playerStick.stick);
            InitializeTimer();
        }

        void Update()
        {
            playerStick.UpdateStick(pressedKey);
        }

        void Draw()
        {
            playerStick.DrawStick();
        }

    }

    public class Stick
    {
        enum ControlType
        {
            Human,
            AI
        }
        //enum MovementDirection
        //{
        //    Up,
        //    Down
        //}

        Point coordinates; //May want to use my own x and Y as I dont want the x value to be changed after it is set
        int height = 75;
        int width = 10;
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

            stick.Height = height;
            stick.Width = width;
            stick.Fill = Brushes.Black;
        }

        public void MoveStick(Game.KeyInputs key)
        {
            if (key == Game.KeyInputs.Up)
            {
                coordinates.Y -= 5;
            }
            if (key == Game.KeyInputs.Down)
            {
                coordinates.Y += 5;
            }
        }

        public void CheckBounds()
        {
            if(coordinates.Y < 0)
            {
                coordinates.Y = 0;
            }

            double maxHeight = (double)stick.Parent.GetValue(Canvas.ActualHeightProperty);
            if(coordinates.Y + height > maxHeight)
            {
                coordinates.Y = maxHeight - height;
            }
        }

        public void UpdateStick(Game.KeyInputs key)
        {
            MoveStick(key);
            CheckBounds();
        }

        public void DrawStick()
        {
            Canvas.SetLeft(stick, coordinates.X);
            Canvas.SetTop(stick, coordinates.Y);
        }

    }
}
