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
using System.Media;

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
        Button startButton;
        Button PlayerButton;
        Button AIButton;

        BitmapImage Title = new BitmapImage(new Uri(@"pack://application:,,,/Images/PONG_Title.png"));// Uri's are used when your appplication may use files not always in the same pah across multiple devices, so almost anytime your app is distributed outside just your own joint
        Image TitleUI = new Image();

        Brush selectedColor = (Brush)(new BrushConverter().ConvertFrom("#FFD900"));

        SoundPlayer sp;

        public MenuState(Manager myManager) : base(myManager) { }

        protected override void InitializeUI()
        {
            startButton = new Button();
            startButton.Click += StartButton_Click;
            startButton.Loaded += StartButton_Loaded;
            startButton.Height = 100;
            startButton.Width = 200;
            startButton.Content = "START GAME";

            PlayerButton = new Button();
            PlayerButton.Click += PlayerButton_Click;
            PlayerButton.Loaded += PlayerButton_Loaded;
            PlayerButton.Height = 50;
            PlayerButton.Width = 125;
            PlayerButton.Content = "PLAYER";
            PlayerButton.Background = Brushes.LightGray;

            AIButton = new Button();
            AIButton.Click += AIButton_Click;
            AIButton.Loaded += AIButton_Loaded;
            AIButton.Height = 50;
            AIButton.Width = 125;
            AIButton.Content = "AI";
            AIButton.Background = Brushes.LightGray;

            TitleUI.Source = Title;
            TitleUI.Height = 125;
            TitleUI.Loaded += TitleUI_Loaded;

            if (myManager.opponentSetting == Manager.Opponent.AI)
            {
                AIButton.Background = selectedColor;
            }
            else
            {
                PlayerButton.Background = selectedColor;
            }

            board.Children.Add(startButton);
            board.Children.Add(PlayerButton);
            board.Children.Add(AIButton);
            board.Children.Add(TitleUI);

            var sri = Application.GetResourceStream(new Uri("pack://application:,,,/Pong;component/Sounds/PongMenuTheme.wav"));
            sp = new SoundPlayer(sri.Stream);
            sp.PlayLooping();
        }

        private void AIButton_Loaded(object sender, RoutedEventArgs e)
        {
            Canvas.SetTop(AIButton, 225);
            Canvas.SetLeft(AIButton, ((board.ActualWidth - startButton.ActualWidth) / 2) + 40);
        }

        private void PlayerButton_Loaded(object sender, RoutedEventArgs e)
        {
            Canvas.SetTop(PlayerButton, 175);
            Canvas.SetLeft(PlayerButton, ((board.ActualWidth - startButton.ActualWidth) / 2) + 40);
        }

        private void StartButton_Loaded(object sender, RoutedEventArgs e) //Actual width and other control properties are only initialized when the control is loaded
        {
            Canvas.SetTop(startButton, 325);
            Canvas.SetLeft(startButton, (board.ActualWidth - startButton.ActualWidth) / 2);
        }

        private void TitleUI_Loaded(object sender, RoutedEventArgs e)
        {
            Canvas.SetTop(TitleUI, 25);
            Canvas.SetLeft(TitleUI, (board.ActualWidth - TitleUI.ActualWidth) / 2);
        }

        private void AIButton_Click(object sender, RoutedEventArgs e)
        {
            myManager.opponentSetting = Manager.Opponent.AI;
            AIButton.Background = selectedColor;
            PlayerButton.Background = Brushes.LightGray;
        }

        private void PlayerButton_Click(object sender, RoutedEventArgs e)
        {
            myManager.opponentSetting = Manager.Opponent.Human;
            PlayerButton.Background = selectedColor;
            AIButton.Background = Brushes.LightGray;
        }

        private void StartButton_Click(object sender, RoutedEventArgs e)
        {
            sp.Stop();
            myManager.ChangeState(Manager.State.Play);
        }
    }

    public class GameOverState : GameState
    {
        System.Windows.Threading.DispatcherTimer GameOverTimer;
        Label gameOverLabel = new Label();

        public GameOverState(Manager myManager) : base(myManager)
        {
            GameOverTimer = new System.Windows.Threading.DispatcherTimer();
            GameOverTimer.Interval = new TimeSpan(0, 0, 3);
            GameOverTimer.Tick += GameOverTimer_Tick;
            GameOverTimer.Start();            
        }

        protected override void InitializeUI()
        {
            gameOverLabel.Content = string.Format("{0} HAS WON THE GAME", myManager.currentWinner);
            gameOverLabel.Foreground = (Brush)(new BrushConverter().ConvertFrom("#FFD900"));
            gameOverLabel.FontSize = 23;
            gameOverLabel.Loaded += GameOverLabel_Loaded;
            
            board.Children.Add(gameOverLabel);
        }

        private void GameOverLabel_Loaded(object sender, RoutedEventArgs e)
        {
            Canvas.SetTop(gameOverLabel, 200);
            Canvas.SetLeft(gameOverLabel, ((board.ActualWidth - gameOverLabel.ActualWidth) / 2) + 10);
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

        public enum HitLocation
        {
            Top,
            Bottom
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
        protected SoundPlayer sp;

        public PlayState(Manager myManager) : base(myManager)
        {
            InitializeTimer();
            InitializeSound();
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
            Brush labelColor = (Brush)(new BrushConverter().ConvertFrom("#00AFD0"));
            playerOneLabel = new Label();
            playerTwoLabel = new Label();
            playerOneLabel.FontSize = 20;
            playerTwoLabel.FontSize = 20;
            playerOneLabel.Foreground = labelColor;
            playerTwoLabel.Foreground = labelColor;

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

            if(myManager.LastScorer == Manager.Players.PlayerOne)
            {
                gameBall.x = playerOne.x - 10;
                gameBall.y = playerOne.y + 10;
            }
            else if(myManager.LastScorer == Manager.Players.PlayerTwo)
            {
                gameBall.xSpeed = gameBall.xSpeed * -1;
                gameBall.x = playerTwo.x + 10;
                gameBall.y = playerTwo.y + 10;
            }

            board.Children.Add(gameBall.myShape);
        }

        protected void UpdateScore()
        {
            playerOneLabel.Content = playerOneScore;
            playerTwoLabel.Content = playerTwoScore;
        }

        protected void InitializeSound()
        {
            var sri = Application.GetResourceStream(new Uri("pack://application:,,,/Pong;component/Sounds/PongHit.wav"));//Using a relative path is kind of extra for media files in wpf
            //sp = new SoundPlayer(@"C:\Users\dj\Desktop\Programs\Pong2\PongHit.wav");
            sp = new SoundPlayer(sri.Stream);
            sp.Load();
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
            if (playerOneScore == 3)
            {
                myManager.currentWinner = "PLAYER ONE";
                return true;
            }
            else if (playerTwoScore == 3)
            {
                myManager.currentWinner = "PLAYER TWO";
                return true;
            }
            else
                return false;
        }

        protected virtual void HandleCollisions()
        {
            if (PlayerOneHasHitBall())
            {
                gameBall.BounceOffHit(playerOne.LastDirection);
                gameBall.x = playerOne.x - gameBall.width;
                sp.Play();
            }

            if (PlayerTwoHasHitBall())
            {
                gameBall.BounceOffHit(playerTwo.LastDirection);
                gameBall.x = playerTwo.x + playerTwo.width;
                sp.Play();
            }

            if (BallHasHitTopOrBottom())
            {
                gameBall.BounceOffWall();
                sp.Play();
            }

            if (StickOneHasHitTop())
            {
                playerOne.y = 0;
            }
            else if (StickOneHasHitBottom())
            {
                playerOne.y = board.ActualHeight - playerOne.height;
            }
            //The Second stick checks are not working it may have something to do with stick two being dynamic IDK tho. Was a result of unboxing in using a struct to hold it. Changed
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
                myManager.LastScorer = Manager.Players.PlayerOne;
                playerOneScore++;
                UpdateScore();
                ServeBall();
            }
            else if (PlayerTwoHasScored())
            {
                myManager.LastScorer = Manager.Players.PlayerTwo;
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
            Ball sampleBall = new Ball();
            sampleBall.randomSeed = gameBall.randomSeed;
            sampleBall.x = gameBall.x;
            sampleBall.y = gameBall.y;
            sampleBall.xSpeed = gameBall.xSpeed;
            sampleBall.ySpeed = gameBall.ySpeed;

            double x = sampleBall.x;
            double y = sampleBall.y;
            double xSpeed = sampleBall.xSpeed;
            double ySpeed = sampleBall.ySpeed;

            //sampleBall.BounceOffHit(playerOne.LastDirection);

            while (x > targetX)
            {
                x += xSpeed;
                y += ySpeed;
                if (y <= 0 || y >= board.ActualHeight)
                {
                    ySpeed = ySpeed * -1;
                }
            }

            //while (x > targetX)
            //{
            //    x += xSpeed;
            //    y += ySpeed;
            //    if (y <= 0 || y >= board.ActualHeight)
            //    {
            //        ySpeed = ySpeed * -1;
            //    }
            //}
            return y;
        }

        protected override void HandleCollisions()
        {
            if (PlayerOneHasHitBall())
            {
                gameBall.BounceOffHit(playerOne.LastDirection);
                gameBall.x = playerOne.x - gameBall.width;
                sp.Play();
                playerTwo.targetY = FindYIntersection();
            }

            if (PlayerTwoHasHitBall())
            {
                gameBall.BounceOffHit(playerOne.LastDirection);
                gameBall.x = playerTwo.x + playerTwo.width;
                sp.Play();
            }

            if (BallHasHitTopOrBottom())
            {
                gameBall.BounceOffWall();
                sp.Play();
            }

            if (StickOneHasHitTop())
            {
                playerOne.y = 0;
            }
            else if (StickOneHasHitBottom())
            {
                playerOne.y = board.ActualHeight - playerOne.height;
            }
            //The Second stick checks were at one point due to being dynamic and using a struct to contain my coordinates. Something about boxing and unboxing
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
                myManager.LastScorer = Manager.Players.PlayerOne;
                playerOneScore++;
                UpdateScore();
                ServeBall();
            }
            else if (PlayerTwoHasScored())
            {
                myManager.LastScorer = Manager.Players.PlayerTwo;
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
