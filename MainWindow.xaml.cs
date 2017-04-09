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
    { // A better way to handle this would be a class that does all the menu and game instantiating and the game instances ,ie most of this, would be their own class, one for Ai and one for human player then I would call the correct class dependent on the options chosen in the main class. This would fix my having to use a dynamic object
        enum GameState
        {
            Play,
            Menu,
            GameOver
        }

        public enum KeyInputs
        {
            None,
            Up,
            Down
        }

        enum OponentSetting
        {
            VersusAI,
            VersusHuman
        }

        public Canvas board = new Canvas();
        GameState currentState = GameState.Menu;

        OponentSetting setting; //Could've used an options class/Struct to hold information but scope rn is small enough for a bool in this class
        HumanStick playerOne; //On Right side
        dynamic playerTwo; //On Left Side, using dynamic kinda hurts the strongly typed characteristic of C#, I can breka this easily if I call something Im not supposed to. This means everything I call with this needs to be checked for or ensured It has it in its class
        Ball gameBall;

        Label playerOneLabel;
        Label playerTwoLabel;
        int playerOneScore;
        int playerTwoScore;

        public KeyInputs playerOneKeyInput;
        public KeyInputs playerTwoKeyInput;

        System.Windows.Threading.DispatcherTimer frameTimer;

        void InitializeTimer()
        {
            frameTimer = new System.Windows.Threading.DispatcherTimer();
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
            if(currentState == GameState.Play)
            {
                if (e.Key == Key.Up)
                {
                    playerOneKeyInput = Game.KeyInputs.Up;
                    PlayerOneInputChanged();
                }
                else if (e.Key == Key.Down)
                {
                    playerOneKeyInput = Game.KeyInputs.Down;
                    PlayerOneInputChanged();
                }

                if(setting == OponentSetting.VersusHuman)
                {
                    if (e.Key == Key.W)
                    {
                        playerTwoKeyInput = Game.KeyInputs.Up;
                        PlayerTwoInputChanged();
                    }
                    else if (e.Key == Key.S)
                    {
                        playerTwoKeyInput = Game.KeyInputs.Down;
                        PlayerTwoInputChanged();
                    }
                }

            }
        }

        public void OnKeyUp(object sender, KeyEventArgs e)
        { //Ensures that input is only gotten when the key is being pressed and doesnt keep inputting after it is let go
            if(currentState == GameState.Play)
            {
                if (e.Key == Key.Up || e.Key == Key.Down)
                {
                    playerOneKeyInput = Game.KeyInputs.None;
                    PlayerOneInputChanged();
                }

                if(setting == OponentSetting.VersusHuman)
                {
                    if (e.Key == Key.W || e.Key == Key.S)
                    {
                        playerTwoKeyInput = Game.KeyInputs.None;
                        PlayerTwoInputChanged();
                    }
                }
            }
        }

        void PlayerOneInputChanged()
        {
            playerOne.input = playerOneKeyInput;
        }

        void PlayerTwoInputChanged()
        {
            playerTwo.input = playerTwoKeyInput;
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

        void InitializeGameState()
        {
            playerOne = new HumanStick(500, 200);
            gameBall = new Ball();

            if(setting == OponentSetting.VersusAI)
                playerTwo = new AIStick(25, 350);
            else
                playerTwo = new HumanStick(25, 350);

            board.Children.Clear();
            board.Children.Add(playerOne.myShape);
            board.Children.Add(playerTwo.myShape);
            board.Children.Add(gameBall.myShape);
            InitializeScoreTable();
        }

        void InitializeMenuState()
        {
            //Grid menuGrid = new Grid();

            Button startButton = new Button();
            startButton.Click += StartButton_Click;
            startButton.Content = "Start Game";
            Canvas.SetTop(startButton, 350);
            Canvas.SetLeft(startButton, 225);

            Button PlayerButton = new Button();
            PlayerButton.Click += PlayerButton_Click;
            PlayerButton.Content = "Player";
            Canvas.SetTop(PlayerButton, 200);
            Canvas.SetLeft(PlayerButton, 200);

            Button AIButton = new Button();
            AIButton.Click += AIButton_Click;
            AIButton.Content = "AI";
            Canvas.SetTop(AIButton, 200);
            Canvas.SetLeft(AIButton, 300);

            board.Children.Add(startButton);
            board.Children.Add(PlayerButton);
            board.Children.Add(AIButton);     
        }

        private void AIButton_Click(object sender, RoutedEventArgs e)
        {
            setting = OponentSetting.VersusAI;
        }

        private void PlayerButton_Click(object sender, RoutedEventArgs e)
        {
            setting = OponentSetting.VersusHuman;
        }

        private void StartButton_Click(object sender, RoutedEventArgs e)
        {
            currentState = GameState.Play;
            ChangeState();
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

        bool StickOneHasHitTop()
        {
            if (playerOne.coordinates.Y < 0)
            {
                return true;
            }
            return false;
        }

        bool StickOneHasHitBottom()
        {
            double maxHeight = board.ActualHeight;
            if (playerOne.coordinates.Y + playerOne.height > maxHeight)
            {
                return true;
            }
            return false;
        }

        bool StickTwoHasHitTop()
        {
            if (playerTwo.coordinates.Y < 0)
            {
                return true;
            }
            return false;
        }

        bool StickTwoHasHitBottom()
        {
            double maxHeight = board.ActualHeight;
            if (playerTwo.coordinates.Y + playerTwo.height > maxHeight)
            {
                return true;
            }
            return false;
        }

        void HandleCollisions()
        {
            if (PlayerOneHasHitBall())
            {
                gameBall.BounceOffHit(true);
                gameBall.coordinates.X = playerOne.coordinates.X - gameBall.width;

                if (setting == OponentSetting.VersusAI)
                    playerTwo.targetY = FindYIntersection();
            }

            if (PlayerTwoHasHitBall())
            {
                gameBall.BounceOffHit(true);
                gameBall.coordinates.X = playerTwo.coordinates.X + gameBall.width;
            }

            if (BallHasHitTopOrBottom())
            {
                gameBall.BounceOffHit(false);
            }

            if (StickOneHasHitTop())
            {
                playerOne.coordinates.Y = 0;
            }
            else if (StickOneHasHitBottom())
            {
                playerOne.coordinates.Y = board.ActualHeight - playerOne.height;
            }
            //The Second stick checks are not working it may have something to do with stick two being dynamic IDK tho
            if (StickTwoHasHitTop())
            {
                playerTwo.coordinates.Y = 0;
            }
            else if (StickTwoHasHitBottom())
            {
                playerTwo.coordinates.Y = board.ActualHeight - playerTwo.height;
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
        {//Currently does not take into account the speed change on a hit in ball. I could maybe use the same random seed as in ball or not IDk. Either its off sometimes rn
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
            if(currentState == GameState.Play)
            {
                playerOne.Update();
                playerTwo.Update();
                gameBall.Update();
                HandleCollisions();
            }
        }

        void ChangeState()
        {
            if (currentState == GameState.Play)
            {
                frameTimer.Start(); //This seems to not be encessary. Odd
                InitializeGameState();
            }               
            else if (currentState == GameState.Menu)
            {
                frameTimer.Stop();
                InitializeMenuState();
            }                
            else if (currentState == GameState.GameOver)
                throw new NotImplementedException();
        }

        void DrawMenu()
        {
                InitializeMenuState();
        }

        void Draw()
        {
            if (currentState == GameState.Play)
            {
                playerOne.Draw();
                playerTwo.Draw();
                gameBall.Draw();
            }
            else if (currentState == GameState.Menu)
                DrawMenu();
        }
    }

    public abstract class GameShape
    {
        public Point coordinates;// May want to use my onw xand y coordinates. Also may want to turn my fields into properties
        public Shape myShape;
        public readonly int height;
        public readonly int width;

        protected GameShape(int shapeHeight, int shapeWidth)
        {
            height = shapeHeight;
            width = shapeWidth;
        }

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

        protected Stick(int x, int y) : base(75, 10)
        {
            myShape = new Rectangle();
            myShape.Height = height;
            myShape.Width = width;
            myShape.Fill = Brushes.Black;
            Initialize(x, y);
        }

        void Initialize(int xCoord, int yCoord)
        {
            coordinates.X = xCoord;
            coordinates.Y = yCoord;
        }
    }

    public class HumanStick : Stick
    {
        public Game.KeyInputs input;

        public HumanStick(int x, int y) : base(x, y) { }

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

        public AIStick(int x, int y) : base(x, y) { }

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
        public int xSpeed = 5;
        public int ySpeed = 2;

        public Ball() : base (25, 25)
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
