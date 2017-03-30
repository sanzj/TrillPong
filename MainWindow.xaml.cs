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

            KeyDown += myGame.OnKeyPressed;
            KeyUp += myGame.OnKeyUp;
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
        public KeyInputs playerOneKeyInput;
        public KeyInputs playerTwoKeyInput;
        public Canvas board = new Canvas();
        HumanStick playerOne; //On Right side
        AIStick playerTwo; //On Left Side
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

        public void OnKeyPressed(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Up)
            {
                playerOneKeyInput = Game.KeyInputs.Up;
                playerOneInputChanged();
            }
            else if (e.Key == Key.Down)
            {
                playerOneKeyInput = Game.KeyInputs.Down;
                playerOneInputChanged();
            }
            //Do the other buttons for when the second player is a human player
        }

        public void OnKeyUp(object sender, KeyEventArgs e)
        { //Ensures that input is only gotten when the key is being pressed and doesnt keep inputting after it is let go
            if (e.Key == Key.Up || e.Key == Key.Down)
            {
                playerOneKeyInput = Game.KeyInputs.None;
                playerOneInputChanged();
            }
            //Do other buttons for when second player is a human player
        }

        void playerOneInputChanged()
        {
            playerOne.input = playerOneKeyInput;
        }

        public Game()
        {
            playerOne = new HumanStick(500,200);
            playerTwo = new AIStick(25, 350);
            gameBall = new Ball();

            board.Children.Add(playerOne.stick);
            board.Children.Add(playerTwo.stick);
            board.Children.Add(gameBall.ball);

            InitializeTimer();
        }

        bool PlayerOneHasHitBall()
        {
            if (gameBall.coordinates.X + gameBall.width >= playerOne.coordinates.X && gameBall.coordinates.Y + gameBall.height >= playerOne.coordinates.Y && gameBall.coordinates.Y < playerOne.coordinates.Y + playerOne.height)
            {
                return true;
            }
            return false;
        }

        bool PlayerTwoHasHitBall()
        {
            if (gameBall.coordinates.X <= playerTwo.coordinates.X + playerTwo.width && gameBall.coordinates.Y + gameBall.height >= playerTwo.coordinates.Y && gameBall.coordinates.Y < playerTwo.coordinates.Y + playerTwo.height)
            {
                return true;
            }
            return false;
        }

        bool PlayerHasHitBall()
        {
            if (PlayerOneHasHitBall() || PlayerTwoHasHitBall())
            {
                return true;
            }
            return false;
        }


        bool BallHasHitCanvas()
        {
            double maxHeight = board.ActualHeight;
            if (gameBall.coordinates.Y < 0 || gameBall.coordinates.Y + gameBall.height > maxHeight)
            {
                return true;
            }
            return false;
        }

        bool StickHasHitTop()
        {
            if (playerOne.coordinates.Y < 0)
            {
                return true;
            }
            return false;
        }

        bool StickHasHitBottom()
        {
            double maxHeight = board.ActualHeight;
            if (playerOne.coordinates.Y + playerOne.height > maxHeight)
            {
                return true;
            }
            return false;
        }

        void HandleCollisions()
        {
            if (PlayerHasHitBall())
            {
                gameBall.BounceOffHit(true);
                if (PlayerOneHasHitBall())
                {
                    playerTwo.targetY = FindYIntersection();
                }
            }

            if (BallHasHitCanvas())
            {
                gameBall.BounceOffHit(false);
            }

            if (StickHasHitTop())
            {
                playerOne.coordinates.Y = 0;
            }
            else if (StickHasHitBottom())
            {
                playerOne.coordinates.Y = board.ActualHeight - playerOne.height;
            }
        }

        double FindYIntersection()
        {
            //Find the y at which the ball x will intersect the pong x
            //Do math to add xSpeed to x until x == stick.x , I will have to take into account the ball hitting the canvas and bouncing
            Double targetX = playerTwo.coordinates.X;
            double x = gameBall.coordinates.X;
            double y = gameBall.coordinates.Y;
            double xSpeed = gameBall.xSpeed;
            double ySpeed = gameBall.ySpeed;

            while(x > targetX)
            {
                x += xSpeed;
                y += ySpeed;
                if (y <= 0 || y >= board.ActualHeight)
                {
                    ySpeed = ySpeed * -1;
                }
            }
            return y;          
        }

        void Update()
        {
            playerOne.Update();
            playerTwo.Update();
            gameBall.Update();
            HandleCollisions();
        }

        void Draw()
        {
            playerOne.Draw();
            playerTwo.Draw();
            gameBall.Draw();
        }
    }

    public abstract class GameShape
    {
        //private Point coordinates; //I dont think you can force a subclass to inherit or implement a certain field I would have to pass on a method or constructor that needs that field or maybe just a parameter

        public abstract void Update();
        public abstract void Draw();
        //{
        //    Canvas.SetLeft(stick, coordinates.X);
        //    Canvas.SetTop(stick, coordinates.Y);
        //}
    }

    public abstract class Stick : GameShape
    {//Everything from GameShape is automatically passed in although it is not mentioned. Any subclasses must still implement them
        protected const int movementSpeed = 5;
        public readonly int height = 75;
        public readonly int width = 10;

        protected abstract void MoveStick();

        public override void Update()
        {
            MoveStick();
        }
    }

    public class HumanStick : Stick
    {
        public Point coordinates; //May want to use my own x and Y as I dont want the x value to be changed after it is set
        public Rectangle stick = new Rectangle();
        public Game.KeyInputs input;

        public HumanStick(int x, int y)
        {
            coordinates.X = x;
            coordinates.Y = y;
            stick.Height = height;
            stick.Width = width;
            stick.Fill = Brushes.Black;
        }

        protected override void MoveStick()
        {
                if (input == Game.KeyInputs.Up)
                {
                    coordinates.Y -= 5;
                }
                else if (input == Game.KeyInputs.Down)
                {
                    coordinates.Y += 5;
                }
        }

        //public override void Update()
        //{
        //    MoveStick();
        //}
    
        public override void Draw()
        {
            Canvas.SetLeft(stick, coordinates.X);
            Canvas.SetTop(stick, coordinates.Y);
        }
    }

    public class AIStick : Stick
    {
        public Point coordinates; //May want to use my own x and Y as I dont want the x value to be changed after it is set
        public Rectangle stick = new Rectangle();
        public double? targetY;

        public AIStick(int x, int y)
        {
            coordinates.X = x;
            coordinates.Y = y;
            stick.Height = height;
            stick.Width = width;
            stick.Fill = Brushes.Black;
        }

        protected override void MoveStick()
        {
            if(targetY != null)
            {
                if (coordinates.Y + height < targetY)
                {
                    coordinates.Y += movementSpeed;
                }
                else if (coordinates.Y > targetY)
                {
                    coordinates.Y -= movementSpeed;
                }
            }
        }

        //public override void Update()
        //{
        //    MoveStick();
        //}

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
        public int ySpeed = 2;
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

        public void BounceOffHit(bool hasHitAStick)
        {
            if(hasHitAStick == true)
            {
                xSpeed = xSpeed * -1;
                xSpeed = (xSpeed >= 0) ? xSpeed + 1 : xSpeed - 1;// I need to make this more dynamic and change y speed as well
            }
            else
            {
                ySpeed = ySpeed * -1;
            }
        }

        void MoveBall()
        {
            coordinates.X += xSpeed;
            coordinates.Y += ySpeed;
        }

        public override void Update()
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
