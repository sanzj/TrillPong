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
        Ball gameBall;

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
            playerStick = new Stick();
            gameBall = new Ball();
            board.Children.Add(playerStick.stick);
            board.Children.Add(gameBall.ball);
            InitializeTimer();
        }

        void CheckForPongHit(Ball ball, Stick stick)
        {
            if(ball.coordinates.X + ball.width >= stick.coordinates.X && ball.coordinates.Y + ball.height >= stick.coordinates.Y && ball.coordinates.Y < stick.coordinates.Y + stick.height)
            {//in here I should map where the the ball has hit the stick and accordingly change the deflection angle through the speed
                ball.xSpeed = ball.xSpeed * -1;
            }
        }

        void CheckForCollision()
        {//I should move all collision checks even stick and canvas collision here. each class should just handle itself and not have to checkj if it hit another class

            //Check if the ball has collided with a stick and if so call a function on it or maybe raise an event for it IDK
            CheckForPongHit(gameBall, playerStick);

            //Checks playerStick-CanvasCollision
            {
                if (playerStick.coordinates.Y < 0)
                {
                    playerStick.coordinates.Y = 0;
                }

                double maxHeight = board.ActualHeight;
                if (playerStick.coordinates.Y + playerStick.height > maxHeight)
                {
                    playerStick.coordinates.Y = maxHeight - playerStick.height;
                }
            }
        }

        void Update()
        {
            playerStick.Update(pressedKey);
            gameBall.Update();
            CheckForCollision();
        }

        void Draw()
        {
            playerStick.Draw();
            gameBall.Draw();
        }

    }

    public abstract class GameShape
    {
        public Point coordinates;
        public int height;
        public int width;

        //abstract public void Update(); // I should figure out a way to make the sick update not take in a parameter to help with implementing bette OOP functionality
        abstract public void Draw();
    }

    public class Stick : GameShape
    {
        enum ControlType
        {
            Human,
            AI
        }

        public Point coordinates; //May want to use my own x and Y as I dont want the x value to be changed after it is set
        public readonly int height = 75;
        public readonly int width = 10;
        //int y;
        //public int x;
        public Rectangle stick = new Rectangle();

        public Stick()
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

        public void Update(Game.KeyInputs key)
        {
            MoveStick(key);
        }

        public override void Draw()
        {
            Canvas.SetLeft(stick, coordinates.X);
            Canvas.SetTop(stick, coordinates.Y);
        }
    }

    public class Ball : GameShape
    {
        public Point coordinates;
        public Ellipse ball;
        public int xSpeed = 5;
        public int ySpeed = 0;
        public readonly int height = 25;
        public readonly int width = 25;

        public Ball()
        {
            ball = new Ellipse();
            ball.Height = height;
            ball.Width = width;
            ball.Fill = Brushes.Crimson;

            coordinates.X = 200;
            coordinates.Y = 200;
        }

        void MoveBall()
        {
            coordinates.X += xSpeed;
            coordinates.Y += ySpeed;
        }

        public void Update()
        {
            MoveBall();
        }

        public override void Draw()
        {
            Canvas.SetLeft(ball, coordinates.X);
            Canvas.SetTop(ball, coordinates.Y);
        }
    }
}
