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
using System.Reflection;

namespace Pong
{
    public abstract class GameState
    {//When initialized the constructor takes over the managers board essentially and overrides it to show it's own information
        protected Canvas board;
        protected Manager myManager;

        public GameState(Manager myManager)
        {
            this.myManager = myManager;
            board = myManager.board;
            board.Children.Clear();
            InitializeUI();
        }

        protected abstract void InitializeUI();
        //public abstract Canvas GetUI();
    }

    public class MenuState : GameState
    {
        public MenuState(Manager myManager) : base(myManager) { }

        protected override void InitializeUI()
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
            myManager.opponentSetting = Manager.Opponent.AI;
        }

        private void PlayerButton_Click(object sender, RoutedEventArgs e)
        {
            myManager.opponentSetting = Manager.Opponent.Human;
        }

        private void StartButton_Click(object sender, RoutedEventArgs e)
        {
            myManager.ChangeState(Manager.State.Play);
        }
    }

    public class GameOverState : GameState
    {
        System.Windows.Threading.DispatcherTimer GameOverTimer;

        public GameOverState(Manager myManager) : base(myManager)
        {
            GameOverTimer = new System.Windows.Threading.DispatcherTimer();
            GameOverTimer.Interval = new TimeSpan(0, 0, 2);
            GameOverTimer.Tick += GameOverTimer_Tick;
            GameOverTimer.Start();
        }

        protected override void InitializeUI()
        {
            Label gameOverLabel = new Label();
            gameOverLabel.Content = string.Format("{0} Has Won The Game", "Somebody");
            Canvas.SetLeft(gameOverLabel, 175);
            Canvas.SetTop(gameOverLabel, 250);

            board.Children.Add(gameOverLabel);
        }

        private void GameOverTimer_Tick(object sender, EventArgs e)
        {
            GameOverTimer.Stop();
            myManager.ChangeState(Manager.State.Menu);
        }
    }

    public abstract class PlayState : GameState
    {//Remember that I am currently using a dynamic in playstate and setting type in children
        //This may potentially not workout and if so I should just make mos tof the methods virtual or abstract and keep them mostly the same.
        public enum KeyInputs
        {
            None,
            Up,
            Down
        }

        protected HumanStick playerOne;
        protected dynamic playerTwo;
        protected Ball gameBall;

        Label playerOneLabel;
        Label playerTwoLabel;

        protected int playerOneScore;
        protected int playerTwoScore;

        public KeyInputs playerOneKeyInput;
        public KeyInputs playerTwoKeyInput;

        System.Windows.Threading.DispatcherTimer frameTimer;

        public PlayState(Manager myManager) : base(myManager)
        {
            InitializeTimer();
            InitializeKeyEventHandlers();
        }

        protected override void InitializeUI()
        {

            playerOne = new HumanStick(500, 200);
            InitializePlayerTwo();
            gameBall = new Ball();

            board.Children.Add(playerOne.myShape);
            board.Children.Add(playerTwo.myShape);
            board.Children.Add(gameBall.myShape);

            InitializeScoreTable();
        }

        protected abstract void InitializePlayerTwo();

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

        protected void ServeBall()
        {
            board.Children.Remove(gameBall.myShape);
            gameBall = new Ball();
            board.Children.Add(gameBall.myShape);
        }

        protected void UpdateScore()
        {
            playerOneLabel.Content = playerOneScore;
            playerTwoLabel.Content = playerTwoScore;
        }

        public void InitializeKeyEventHandlers()
        {
            MainWindow wnd = (MainWindow)Application.Current.MainWindow;
            wnd.SubscribeToKeyDown(OnKeyPressed);
            wnd.SubscribeToKeyUp(OnKeyUp);
        }

        public void UnsubscribeKeyEventHandlers()
        {
            MainWindow wnd = (MainWindow)Application.Current.MainWindow;
            wnd.UnSubscribeToKeyDown(OnKeyPressed);
            wnd.UnSubscribeToKeyUp(OnKeyUp);
        }

        public virtual void OnKeyPressed(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Up)
            {
                playerOneKeyInput = KeyInputs.Up;
                PlayerOneInputChanged();
            }
            else if (e.Key == Key.Down)
            {
                playerOneKeyInput = KeyInputs.Down;
                PlayerOneInputChanged();
            }
        }

        public virtual void OnKeyUp(object sender, KeyEventArgs e)
        { //Ensures that input is only gotten when the key is being pressed and doesnt keep inputting after it is let go
            if (e.Key == Key.Up || e.Key == Key.Down)
            {
                playerOneKeyInput = KeyInputs.None;
                PlayerOneInputChanged();
            }
        }

        protected void PlayerOneInputChanged()
        {
            playerOne.input = playerOneKeyInput;
        }

        protected bool PlayerOneHasHitBall()
        {//                                   Gameball right is greater than the left side of stick but less than right side of the stick and gameball
            if (gameBall.x + gameBall.width >= playerOne.x && gameBall.x <= playerOne.x + playerOne.width && gameBall.y + gameBall.height >= playerOne.y && gameBall.y <= playerOne.y + playerOne.height)
            {
                return true;
            }
            return false;
        }

        protected bool PlayerTwoHasHitBall()
        {
            if (gameBall.x + gameBall.width >= playerTwo.x && gameBall.x <= playerTwo.x + playerTwo.width && gameBall.y + gameBall.height >= playerTwo.y && gameBall.y <= playerTwo.y + playerTwo.height)
            {
                return true;
            }
            return false;
        }

        protected bool PlayerOneHasScored()
        {
            if (gameBall.x <= 0)
            {
                return true;
            }
            return false;
        }

        protected bool PlayerTwoHasScored()
        {
            if (gameBall.x >= board.ActualWidth)
            {
                return true;
            }
            return false;
        }

        protected bool BallHasHitTopOrBottom()
        {
            double maxHeight = board.ActualHeight;
            if (gameBall.y < 0 || gameBall.y + gameBall.height > maxHeight)
            {
                return true;
            }
            return false;
        }

        protected bool StickOneHasHitTop()
        {
            if (playerOne.y < 0)
            {
                return true;
            }
            return false;
        }

        protected bool StickOneHasHitBottom()
        {
            double maxHeight = board.ActualHeight;
            if (playerOne.y + playerOne.height > maxHeight)
            {
                return true;
            }
            return false;
        }

        protected bool StickTwoHasHitTop()
        {
            if (playerTwo.y < 0)
            {
                return true; // This part works correctly
            }
            return false;
        }

        protected bool StickTwoHasHitBottom()
        {
            double maxHeight = board.ActualHeight;
            if (playerTwo.y + playerTwo.height > maxHeight)
            {
                return true;
            }
            return false;
        }

        protected bool GameHasBeenWon()
        {
            if (playerOneScore == 3 || playerTwoScore == 3)
                return true;
            else
                return false;
        }

        protected virtual void HandleCollisions()
        {
            if (PlayerOneHasHitBall())
            {
                gameBall.BounceOffHit(true);
                gameBall.x = playerOne.x - gameBall.width;
            }

            if (PlayerTwoHasHitBall())
            {
                gameBall.BounceOffHit(true);
                gameBall.x = playerTwo.x + gameBall.width;
            }

            if (BallHasHitTopOrBottom())
            {
                gameBall.BounceOffHit(false);
            }

            if (StickOneHasHitTop())
            {
                playerOne.y = 0;
            }
            else if (StickOneHasHitBottom())
            {
                playerOne.y = board.ActualHeight - playerOne.height;
            }
            //The Second stick checks are not working it may have something to do with stick two being dynamic IDK tho
            if (StickTwoHasHitTop())
            {
                playerTwo.y = 0; // This line is performed but the value does not change
            }
            else if (StickTwoHasHitBottom())
            {
                playerTwo.y = board.ActualHeight - playerTwo.height;
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

        void Update()
        {
            playerOne.Update();
            playerTwo.Update();
            gameBall.Update();
            HandleCollisions();

            if (GameHasBeenWon())
            {
                UnsubscribeKeyEventHandlers(); //These two first methods are called to ensure that the state class doesnt run while in another state. Hopefully causes disposal 
                frameTimer.Stop();
                myManager.ChangeState(Manager.State.GameOver);
            }
        }

        void Draw()
        {
            playerOne.Draw();
            playerTwo.Draw();
            gameBall.Draw();
        }

    }

    public class SinglePlayerPlayState : PlayState
    {
        public SinglePlayerPlayState(Manager myManager) : base(myManager) { }

        protected override void InitializePlayerTwo()
        {
            playerTwo = new AIStick(25, 350);
        }

        double FindYIntersection()
        {//Currently does not take into account the speed change on a hit in ball. I could maybe use the same random seed as in ball or not IDk. Either its off sometimes rn
            Double targetX = playerTwo.x;
            double x = gameBall.x;
            double y = gameBall.y;
            double xSpeed = gameBall.xSpeed;
            double ySpeed = gameBall.ySpeed;

            while (x > targetX)
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

        protected override void HandleCollisions()
        {
            if (PlayerOneHasHitBall())
            {
                gameBall.BounceOffHit(true);
                gameBall.x = playerOne.x - gameBall.width;

                playerTwo.targetY = FindYIntersection();
            }

            if (PlayerTwoHasHitBall())
            {
                gameBall.BounceOffHit(true);
                gameBall.x = playerTwo.x + gameBall.width;
            }

            if (BallHasHitTopOrBottom())
            {
                gameBall.BounceOffHit(false);
            }

            if (StickOneHasHitTop())
            {
                playerOne.y = 0;
            }
            else if (StickOneHasHitBottom())
            {
                playerOne.y = board.ActualHeight - playerOne.height;
            }
            //The Second stick checks are not working it may have something to do with stick two being dynamic IDK tho
            if (StickTwoHasHitTop())
            {
                playerTwo.y = 0;
            }
            else if (StickTwoHasHitBottom())
            {
                playerTwo.y = board.ActualHeight - playerTwo.height;
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

    }

    public class TwoPlayerState : PlayState
    {
        public TwoPlayerState(Manager myManager) : base(myManager) { }

        protected override void InitializePlayerTwo()
        {
            playerTwo = new HumanStick(25, 350);
            //playerTwo = (HumanStick)Activator.CreateInstance(typeof(HumanStick));
        }

        public override void OnKeyPressed(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Up)
            {
                playerOneKeyInput = KeyInputs.Up;
                PlayerOneInputChanged();
            }
            else if (e.Key == Key.Down)
            {
                playerOneKeyInput = KeyInputs.Down;
                PlayerOneInputChanged();
            }

            if (e.Key == Key.W)
            {
                playerTwoKeyInput = KeyInputs.Up;
                PlayerTwoInputChanged();
            }
            else if (e.Key == Key.S)
            {
                playerTwoKeyInput = KeyInputs.Down;
                PlayerTwoInputChanged();
            }
        }

        public override void OnKeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Up || e.Key == Key.Down)
            {
                playerOneKeyInput = KeyInputs.None;
                PlayerOneInputChanged();
            }

            if (e.Key == Key.W || e.Key == Key.S)
            {
                playerTwoKeyInput = KeyInputs.None;
                PlayerTwoInputChanged();
            }
        }

        protected void PlayerTwoInputChanged()
        {
            playerTwo.input = playerTwoKeyInput;
        }
    }
}
