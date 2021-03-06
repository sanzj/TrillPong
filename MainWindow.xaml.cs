﻿using System;
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
        The manager has a UIboard which is sort of overtaken by the respective current state when the state is changed, this is how the UI is changed and controlled by the states

        One thing I should've done and could do in refactoring is implement a factory class that returns the correct state class.
        I can also improve AI and tracking to be more dynamic and not start and stop as well as tweak how the ball bounces and hits work.
        */
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

        public void UnSubscribeToKeyDown(KeyEventHandler myEventHandler)
        {
            KeyDown -= myEventHandler;
        }

        public void UnSubscribeToKeyUp(KeyEventHandler myEventHandler)
        {
            KeyUp -= myEventHandler;
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

        public enum Players
        {
            PlayerOne,
            PlayerTwo
        }

        public Opponent opponentSetting;
        public string currentWinner;
        public Players LastScorer;

        public Canvas board = new Canvas();

        public Manager()
        {
            board.Background = (Brush)(new BrushConverter().ConvertFrom("#A1004F"));
            ChangeState(State.Menu);//Initializes UI in menu mode
        }
        //Add method to delete previous state
        public void ChangeState(State nextState)
        {
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
        // Used x and Y as built in coordinate struct gave me problems when boxing and unboxing. Also may want to turn my fields into properties.
        public double x;
        public double y;
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
            Canvas.SetLeft(myShape, x);
            Canvas.SetTop(myShape, y);
        }
    }

    public abstract class Stick : GameShape
    {//Everything from GameShape is automatically passed in although it is not mentioned. All subclasses still implement them
        protected const int movementSpeed = 5;
        public PlayState.KeyInputs LastDirection;

        protected Stick(int x, int y) : base(75, 10)
        {
            myShape = new Rectangle();
            myShape.Height = height;
            myShape.Width = width;
            myShape.Fill = (Brush)(new BrushConverter().ConvertFrom("#03D4FB"));
            Initialize(x, y);
        }

        void Initialize(int xCoord, int yCoord)
        {
            x = xCoord;
            y = yCoord;
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
                y -= movementSpeed;
                LastDirection = PlayState.KeyInputs.Up;
            }
            else if (input == PlayState.KeyInputs.Down)
            {
                y += movementSpeed;
                LastDirection = PlayState.KeyInputs.Down;
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
                if(targetY < y + (height / 3) || targetY > y + ((height / 3) * 2))// If target y is not in the middle of the stick
                {
                    if (targetY > y + (height / 3))
                    {
                        y += movementSpeed;
                        LastDirection = PlayState.KeyInputs.Down;
                    }
                    else if (y + ((height / 3) * 2) > targetY)
                    {
                        y -= movementSpeed;
                        LastDirection = PlayState.KeyInputs.Up;
                    }
                }
            }
        }

        public void ChangeTargetY(double? newTarget)
        {
            targetY = newTarget;
        }

        public override void Update()
        {
            MoveCoordinates();
        }
    }

    public class Ball : GameShape
    {
        public int xSpeed = 5;
        public int ySpeed = 2;
        private int xSpeedMax = 15;
        private int ySpeedMax = 25;
        public int randomSeed = new Random().Next();//I need to know the random number in another class, so I could just save the seed here and generate the number elsewhere

        public Ball() : base(25, 25)
        {
            myShape = new Ellipse();
            myShape.Height = height;
            myShape.Width = width;
            myShape.Fill = (Brush)(new BrushConverter().ConvertFrom("#FFD900"));

            Random rand = new Random();
            do// make sure that ySpeed is always moving at a speed of at least 5 in either direction when spawned
            {
                ySpeed = rand.Next(-7, 7);
            } while (ySpeed < 3 && ySpeed > -3);
            

            x = 200;
            y = 200;
        }

        public void BounceOffWall()
        {
                ySpeed = ySpeed * -1;
        }

        public void BounceOffHit(PlayState.KeyInputs LastInput)
        {
            Random rand = new Random(randomSeed); //I may want to implement a different algorithm that used doubles for speed to allow more flexibility

            xSpeed = xSpeed * -1;
            xSpeed = (xSpeed >= 0) ? xSpeed + rand.Next(0, 2) : xSpeed - rand.Next(0, 2);
            if(xSpeed > xSpeedMax)
            {
                xSpeed = xSpeedMax;
            }
            else if(xSpeed < -xSpeedMax)
            {
                xSpeed = -xSpeedMax;
            }

            if (LastInput == PlayState.KeyInputs.Up)
            {
                ySpeed = (ySpeed >= 0) ? (ySpeed * -1) - rand.Next(0, 2) : ySpeed + rand.Next(0, 3);
            }
            else //Last input == down
            {
                ySpeed = (ySpeed >= 0) ? ySpeed + rand.Next(0, 3) : (ySpeed * -1) + rand.Next(0, 2);
            }

            if(ySpeed > ySpeedMax)
            {
                ySpeed = ySpeedMax;
            }
            else if(ySpeed < -ySpeedMax)
            {
                ySpeed = -ySpeedMax;
            }

            randomSeed = new Random().Next();
        }

        protected override void MoveCoordinates()
        {
            x += xSpeed;
            y += ySpeed;
        }
    }
}
