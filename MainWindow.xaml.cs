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
        /*
        The game is coded using a state manager with 3 game states. 
        The Manager class simply switches the UI for the current state, the states are the ones that call the switch method though
        The states all have their own UI's, logic, and input handling that they take care of themselves. 
        They also take care of leaving to a different state by calling a manager class
        The manager has a board which is sort of overtaken by the respective current state when the state is changed, this is how the UI is changed and controlled by the states
        */
        //Game myGame = new Game();
        Manager manager = new Manager();

        public MainWindow()
        {
            InitializeComponent();
            this.DataContext = manager;
            MainGrid.Children.Add(manager.board);
        }

        public void SubscribeToKeyDown(KeyEventHandler myEventHandler)
        {
            KeyDown += myEventHandler;
        }
        
        public void SubscribeToKeyUp(KeyEventHandler myEventHandler)
        {
            KeyUp += myEventHandler;
        }
    }

    public class Manager
    {
        public enum State
        {
            Menu,
            Play,
            GameOver
        }

        public enum Opponent
        {
            AI,
            Human
        }

        State currentState;// May not be needed
        public Opponent opponentSetting;

        public Canvas board = new Canvas();

        public Manager()
        {
            ChangeState(State.Menu);//Initializes UI in menu mode
        }
        //Add method to delete previous state
        public void ChangeState(State nextState)
        {
            currentState = nextState;

            if (nextState == State.Menu)
            {
                MenuState menu = new MenuState(this);
            }
            else if (nextState == State.Play)
            {
                if (opponentSetting == Opponent.AI)
                {
                    SinglePlayerPlayState playState = new SinglePlayerPlayState(this);
                }
                else if (opponentSetting == Opponent.Human)
                {
                    TwoPlayerState playState = new TwoPlayerState(this);
                }
            }
            else if (nextState == State.GameOver)
            {
                GameOverState gameOver = new GameOverState(this);
            }
        }

    }

    public abstract class GameShape
    {
        public Point coordinates;// May want to use my onw xand y coordinates. Also may want to turn my fields into properties. Using a struct may inadvertentl cause boxing and unboxing issues
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
        public PlayState.KeyInputs input;

        public HumanStick(int x, int y) : base(x, y) { }

        protected override void MoveCoordinates()
        {
            if (input == PlayState.KeyInputs.Up)
            {
                coordinates.Y -= 5;
            }
            else if (input == PlayState.KeyInputs.Down)
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
            if (targetY != null)
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

        public Ball() : base(25, 25)
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
            if (hasHitAStick == true)
            {
                Random rand = new Random(); //I may want to implement a different algorithm that used doubles for speed to allow more flexibility

                xSpeed = xSpeed * -1;
                xSpeed = (xSpeed >= 0) ? xSpeed + rand.Next(0, 2) : xSpeed - rand.Next(0, 2);
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
