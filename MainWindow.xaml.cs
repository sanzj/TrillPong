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
        public Canvas board = new Canvas();
        HumanStick playerOne; //On Right side
        AIStick playerTwo; //On Left Side
        Ball gameBall;

        Label playerOneLabel;
        Label playerTwoLabel;
        int playerOneScore;
        int playerTwoScore;

        public KeyInputs playerOneKeyInput;
        public KeyInputs playerTwoKeyInput;

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

        void InitializeScoreTable()
        {
            playerOneLabel = new Label();
            playerTwoLabel = new Label();
            Canvas.SetLeft(playerOneLabel, 300);
            Canvas.SetLeft(playerTwoLabel, 250);
            board.Children.Add(playerOneLabel);
            board.Children.Add(playerTwoLabel);
            UpdateScore();
        }

        void ServeBall()
        {
            board.Children.Remove(gameBall.myShape);
            gameBall = new Ball();
            board.Children.Add(gameBall.myShape);
        }

        void UpdateScore()
        {
            playerOneLabel.Content = playerOneScore;
            playerTwoLabel.Content = playerTwoScore;
        }

        public Game()
        {
            playerOne = new HumanStick(500,200);
            playerTwo = new AIStick(25, 350);
            gameBall = new Ball();

            board.Children.Add(playerOne.myShape);
            board.Children.Add(playerTwo.myShape);
            board.Children.Add(gameBall.myShape);

            InitializeScoreTable();

            InitializeTimer();
        }

        bool PlayerOneHasHitBall()
        {//                                   Gameball right is greater than the left side of stick but less than right side of the stick and gameball
            if (gameBall.coordinates.X + gameBall.width >= playerOne.coordinates.X && gameBall.coordinates.X <= playerOne.coordinates.X + playerOne.width && gameBall.coordinates.Y + gameBall.height >= playerOne.coordinates.Y && gameBall.coordinates.Y <= playerOne.coordinates.Y + playerOne.height)
            {
                return true;
            }
            return false;
        }

        bool PlayerTwoHasHitBall()
        {
            if (gameBall.coordinates.X + gameBall.width >= playerTwo.coordinates.X && gameBall.coordinates.X <= playerTwo.coordinates.X + playerTwo.width && gameBall.coordinates.Y + gameBall.height >= playerTwo.coordinates.Y && gameBall.coordinates.Y <= playerTwo.coordinates.Y + playerTwo.height)
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

        bool PlayerOneHasScored()
        {
            if(gameBall.coordinates.X <= 0)
            {
                return true;
            }
            return false;
        }

        bool PlayerTwoHasScored()
        {
            if(gameBall.coordinates.X >= board.ActualWidth)
            {
                return true;
            }
            return false;
        }

        bool BallHasHitTopOrBottom()
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
                    gameBall.coordinates.X = playerOne.coordinates.X - gameBall.width;
                    playerTwo.targetY = FindYIntersection();
                }
            }

            if (BallHasHitTopOrBottom())
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

            if (PlayerOneHasScored())
            {
                playerOneScore++;
                UpdateScore();
                ServeBall();
            }
            else if (PlayerTwoHasScored())
            {
                playerTwoScore++;
                UpdateScore();
                ServeBall();
            }
        }

        double FindYIntersection()
        {
            //Currently does not take into account the speed change on a hit in ball. I could maybe use the same random seed as in ball or not IDk. Either its off sometimes rn
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
        public Point coordinates;// May want to use my onw xand y coordinates. Also may want to turn my fields into properties
        public Shape myShape;

        //protected GameShape(int x, int y)
        //{
        //    //Make a constructor that can be used in children and specialized in base children classes
        //}

        protected abstract void MoveCoordinates();

        public virtual void Update()
        {
            MoveCoordinates();
        }

        public virtual void Draw()
        {
            Canvas.SetLeft(myShape, coordinates.X);
            Canvas.SetTop(myShape, coordinates.Y);
        }
}

    public abstract class Stick : GameShape
    {//Everything from GameShape is automatically passed in although it is not mentioned. All subclasses must still implement them
        protected const int movementSpeed = 5;
        public readonly int height = 75;
        public readonly int width = 10;
    }

    public class HumanStick : Stick
    {
        public Game.KeyInputs input;

        public HumanStick(int x, int y)
        {
            myShape = new Rectangle();
            coordinates.X = x;
            coordinates.Y = y;
            myShape.Height = height;
            myShape.Width = width;
            myShape.Fill = Brushes.Black;
        }

        protected override void MoveCoordinates()
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
    }

    public class AIStick : Stick
    {
        public double? targetY;

        public AIStick(int x, int y)
        {
            myShape = new Rectangle();
            coordinates.X = x;
            coordinates.Y = y;
            myShape.Height = height;
            myShape.Width = width;
            myShape.Fill = Brushes.Black;
        }

        protected override void MoveCoordinates()
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
    }

    public class Ball : GameShape
    {
        public readonly int height = 25;
        public readonly int width = 25;
        public int xSpeed = 5;
        public int ySpeed = 2;

        public Ball()
        {
            myShape = new Ellipse();
            myShape.Height = height;
            myShape.Width = width;
            myShape.Fill = Brushes.Crimson;

            coordinates.X = 200;
            coordinates.Y = 200;
        }

        public void BounceOffHit(bool hasHitAStick)
        {
            if(hasHitAStick == true)
            {
                Random rand = new Random(); //I may want to implement a different algorithm that used doubles for speed to allow more flexibility

                xSpeed = xSpeed * -1;
                xSpeed = (xSpeed >= 0) ? xSpeed + rand.Next(0,2): xSpeed - rand.Next(0,2);
                ySpeed = (ySpeed >= 0) ? ySpeed + rand.Next(-1, 2) : ySpeed - rand.Next(-1, 2);
            }
            else
            {
                ySpeed = ySpeed * -1;
            }
        }

        protected override void MoveCoordinates()
        {
            coordinates.X += xSpeed;
            coordinates.Y += ySpeed;
        }
    }
}
