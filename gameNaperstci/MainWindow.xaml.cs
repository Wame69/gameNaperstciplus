using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace NapierstkiGame
{
    public partial class MainWindow : Window
    {
        private int hiddenCupIndex; private int initialCupIndex; private Random random; private List<Image> cups; private bool isShuffling; private const double Cup1StartX = 100; private const double Cup2StartX = 200; private const double Cup3StartX = 300; private const double StartY = 150; private int shuffleCount; private bool hasBallMoved;
        public MainWindow()
        {
            InitializeComponent();
            random = new Random();
            cups = new List<Image> { Cup1, Cup2, Cup3 };
            ResetGame();
        }

        private void StartButton_Click(object sender, RoutedEventArgs e)
        {
            hiddenCupIndex = random.Next(0, cups.Count); Ball.Visibility = Visibility.Hidden; ResultText.Text = "Запоминайте!";

            AnimateAllCupsUp(() =>
            {
                PositionBallUnderCup(hiddenCupIndex); Ball.Visibility = Visibility.Visible;
                AnimateAllCupsDown(() =>
                {
                    Ball.Visibility = Visibility.Hidden; StartShuffleAnimation();
                });
            });
        }

        private void PositionBallUnderCup(int cupIndex)
        {
            var cup = cups[cupIndex];
            double ballX = Canvas.GetLeft(cup) + (cup.Width - Ball.Width) / 2;
            double ballY = Canvas.GetTop(cup) + (cup.Height - Ball.Height) / 2 + 25;
            Canvas.SetLeft(Ball, ballX);
            Canvas.SetTop(Ball, ballY);
        }

        private void AnimateAllCupsUp(Action completedAction = null)
        {
            int completedCount = 0;
            foreach (var cup in cups)
            {
                var animation = new DoubleAnimation(-30, TimeSpan.FromSeconds(0.3));
                if (completedAction != null)
                {
                    animation.Completed += (s, e) =>
                    {
                        completedCount++;
                        if (completedCount == cups.Count) completedAction();
                    };
                }
                cup.RenderTransform = new TranslateTransform();
                cup.RenderTransform.BeginAnimation(TranslateTransform.YProperty, animation);
            }
        }

        private void AnimateAllCupsDown(Action completedAction = null)
        {
            int completedCount = 0;
            foreach (var cup in cups)
            {
                var animation = new DoubleAnimation(0, TimeSpan.FromSeconds(0.3));
                if (completedAction != null)
                {
                    animation.Completed += (s, e) =>
                    {
                        completedCount++;
                        if (completedCount == cups.Count) completedAction();
                    };
                }
                cup.RenderTransform.BeginAnimation(TranslateTransform.YProperty, animation);
            }
        }

        private void StartShuffleAnimation()
        {
            ResultText.Text = "Перемешиваем!";
            isShuffling = true;
            EnableCups(false);
            shuffleCount = 0; hasBallMoved = false;
            ShuffleStep();
        }

        private void ShuffleStep()
        {
            if (shuffleCount >= 10)
            {
                isShuffling = false;
                ResultText.Text = "Угадайте, где мяч!";
                EnableCups(true);
                return;
            }

            int firstIndex = random.Next(cups.Count);
            int secondIndex;
            do
            {
                secondIndex = random.Next(cups.Count);
            } while (secondIndex == firstIndex);

            if (shuffleCount < 5)
            {
                SwapCupsPosition(firstIndex, secondIndex, () =>
                {
                    shuffleCount++;
                    ShuffleStep();
                });
            }
            else if (!hasBallMoved)
            {
                initialCupIndex = hiddenCupIndex; AnimateCupPairWithBallMove(firstIndex, secondIndex, () =>
                 {
                     hasBallMoved = true; shuffleCount++;
                     ShuffleStep();
                 });
            }
            else
            {
                SwapCupsPosition(firstIndex, secondIndex, () =>
                {
                    shuffleCount++;
                    ShuffleStep();
                });
            }
        }

        private void SwapCupsPosition(int firstCupIndex, int secondCupIndex, Action completedAction = null)
        {
            var firstCup = cups[firstCupIndex];
            var secondCup = cups[secondCupIndex];

            double firstCupX = Canvas.GetLeft(firstCup);
            double secondCupX = Canvas.GetLeft(secondCup);

            var firstCupAnimation = new DoubleAnimation(firstCupX, secondCupX, TimeSpan.FromSeconds(0.3));
            var secondCupAnimation = new DoubleAnimation(secondCupX, firstCupX, TimeSpan.FromSeconds(0.3));

            if (completedAction != null)
            {
                secondCupAnimation.Completed += (s, e) => completedAction();
            }

            firstCup.BeginAnimation(Canvas.LeftProperty, firstCupAnimation);
            secondCup.BeginAnimation(Canvas.LeftProperty, secondCupAnimation);
        }

        private void AnimateCupPairWithBallMove(int firstCupIndex, int secondCupIndex, Action completedAction = null)
        {
            var firstCup = cups[firstCupIndex];
            var secondCup = cups[secondCupIndex];

            AnimateCupUp(firstCupIndex, () =>
            {
                PositionBallUnderCup(firstCupIndex);
                Ball.Visibility = Visibility.Visible;

                MoveBallBetweenCups(firstCup, secondCup, () =>
                {
                    AnimateCupUp(secondCupIndex, () =>
                    {
                        hiddenCupIndex = secondCupIndex; Ball.Visibility = Visibility.Hidden; AnimateCupDown(firstCupIndex);
                        AnimateCupDown(secondCupIndex, completedAction);
                    });
                });
            });
        }

        private void MoveBallBetweenCups(Image firstCup, Image secondCup, Action completedAction = null)
        {
            double startX = Canvas.GetLeft(firstCup) + (firstCup.Width - Ball.Width) / 2;
            double startY = Canvas.GetTop(firstCup) + (firstCup.Height - Ball.Height) / 2 + 25;
            double endX = Canvas.GetLeft(secondCup) + (secondCup.Width - Ball.Width) / 2;
            double endY = Canvas.GetTop(secondCup) + (secondCup.Height - Ball.Height) / 2 + 25;

            var ballXAnimation = new DoubleAnimation(startX, endX, TimeSpan.FromSeconds(0.3));
            var ballYAnimation = new DoubleAnimation(startY, endY, TimeSpan.FromSeconds(0.3));

            Ball.Visibility = Visibility.Visible;

            if (completedAction != null)
            {
                ballYAnimation.Completed += (s, e) => completedAction();
            }

            Canvas.SetLeft(Ball, startX);
            Canvas.SetTop(Ball, startY);

            Ball.BeginAnimation(Canvas.LeftProperty, ballXAnimation);
            Ball.BeginAnimation(Canvas.TopProperty, ballYAnimation);
        }
        private void Cup_Click(object sender, MouseButtonEventArgs e)
        {
            if (isShuffling) return;

            if (sender is Image clickedCup)
            {
                int selectedCupIndex = cups.IndexOf(clickedCup);
                EnableCups(false);

                AnimateCupUp(selectedCupIndex, () =>
                {
                    if (selectedCupIndex == hiddenCupIndex)
                    {
                        PositionBallUnderCup(hiddenCupIndex);
                        Ball.Visibility = Visibility.Visible;
                        ResultText.Text = "Поздравляем! Вы угадали!";
                    }
                    else
                    {
                        AnimateCupUp(hiddenCupIndex, () =>
                        {
                            PositionBallUnderCup(hiddenCupIndex);
                            Ball.Visibility = Visibility.Visible;
                        });
                        ResultText.Text = "Не угадали! Попробуйте снова.";
                    }

                    AnimateCupDown(selectedCupIndex);
                });
            }
        }

        private void AnimateCupUp(int cupIndex, Action completedAction = null)
        {
            var animation = new DoubleAnimation(-30, TimeSpan.FromSeconds(0.3));
            if (completedAction != null)
                animation.Completed += (s, e) => completedAction();

            cups[cupIndex].RenderTransform = new TranslateTransform();
            cups[cupIndex].RenderTransform.BeginAnimation(TranslateTransform.YProperty, animation);
        }

        private void AnimateCupDown(int cupIndex, Action completedAction = null)
        {
            var animation = new DoubleAnimation(0, TimeSpan.FromSeconds(0.3));
            if (completedAction != null)
                animation.Completed += (s, e) => completedAction();

            cups[cupIndex].RenderTransform.BeginAnimation(TranslateTransform.YProperty, animation);
        }

        private void EnableCups(bool enable)
        {
            foreach (var cup in cups)
            {
                cup.IsHitTestVisible = enable;
            }
        }

        private void ResetGame()
        {
            hiddenCupIndex = -1;
            initialCupIndex = -1; isShuffling = false;
            hasBallMoved = false; EnableCups(true);
        }
    }
}