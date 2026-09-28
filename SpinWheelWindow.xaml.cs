using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using Playnite.SDK;
using Playnite.SDK.Models;
using WheelSpinner.Models;
using System.Windows.Media.Animation;
using Color = System.Windows.Media.Color;
using FontFamily = System.Windows.Media.FontFamily;
using Point = System.Windows.Point;
using Size = System.Windows.Size;

namespace WheelSpinner
{
    public partial class SpinWheelWindow : UserControl
    {
        private IPlayniteAPI Api { get; set; }
        private List<Game> _games = new List<Game>();
        private readonly HashSet<Guid> _excludedGames = new HashSet<Guid>();
        private readonly Dictionary<Guid, int> _multipliedGames = new Dictionary<Guid, int>();
        private bool _isCheckboxChecked = true;
        private bool _isMuted = false;
        private ILogger logger { get; set; }

        private readonly Random _random = new Random();
        private double _angle = 0;

        private readonly List<Color> _palette = new List<Color>
        {
            Colors.Red, Colors.OrangeRed, Colors.Orange, Colors.Yellow, Colors.YellowGreen
        };

        private List<(Game Game, int Weight)> _wheelItems = new List<(Game Game, int Weight)>();

        public event Action<Game> SpinCompleted;


        public SpinWheelWindow(IPlayniteAPI api, ILogger logger, SaveState saveState = null)
        {
            InitializeComponent();
            if (Application.Current.TryFindResource("TextBlockBaseStyle") is Style textBlockStyle)
            {
                var newStyle = new Style(typeof(TextBlock), textBlockStyle);
                Resources.Add(typeof(TextBlock), newStyle);
            }

            this.logger = logger;

            Api = api;
            if (saveState != null)
            {
                _multipliedGames = saveState.MultipliedGames;
                _excludedGames = saveState.ExcludedGames;
                _isCheckboxChecked = saveState.IsCheckboxChecked;
            }

            ChangeGamesList(_isCheckboxChecked);
            if (FilteredBox != null && FilteredBox.IsChecked != _isCheckboxChecked)
            {
                FilteredBox.IsChecked = _isCheckboxChecked;
            }

            InsertGamesAsync();
            
        }

        private async void InsertGamesAsync()
        {
            try
            {
                await Dispatcher.InvokeAsync(InsertGamesIntoExtenders);
            }
            catch (Exception e)
            {
                logger.Error(e, "Failed to insert games into extenders.");
            }
        }

        private async void DrawWheelAsync()
        {
            try
            {
                await Dispatcher.InvokeAsync(DrawWheel);
            }
            catch (Exception e)
            {
                logger.Error(e, "Failed to draw wheel.");
            }
        }
        


        private void UIElement_OnPreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            var textBox = sender as TextBox;
            if (textBox == null)
                return;

            var fullText = textBox.Text.Remove(textBox.SelectionStart, textBox.SelectionLength)
                .Insert(textBox.SelectionStart, e.Text);

            var isValid = Regex.IsMatch(fullText, @"^[1-9][0-9]{0,2}$");

            e.Handled = !isValid;
        }

        private void UIElement_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            var textBox = sender as TextBox;
            if (textBox == null || string.IsNullOrEmpty(textBox.Text))
                return;
            if (!Guid.TryParse(textBox.Tag.ToString(), out var gameId))
                return;
            if (int.TryParse(textBox.Text, out var multiplier))
            {
                if (_multipliedGames.ContainsKey(gameId))
                {
                    if (multiplier == 1)
                    {
                        _multipliedGames.Remove(gameId);
                        DrawWheelAsync();
                        return;
                    }

                    _multipliedGames[gameId] = multiplier;
                }
                else if (multiplier > 1)
                {
                    _multipliedGames.Add(gameId, multiplier);
                }

                DrawWheelAsync();
            }
        }

        private void ExcludeButton_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            if (button == null)
                return;
            ExcludeGame(Guid.Parse(button.Tag.ToString()));
        }

        public void ExcludeGame(Guid gameId)
        {
            _excludedGames.Add(gameId);
            InsertGamesAsync();
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            if (button == null)
                return;
            _excludedGames.Remove(Guid.Parse(button.Tag.ToString()));
            InsertGamesAsync();
        }

        private void CheckboxChanged(object sender, RoutedEventArgs e)
        {
            if (!(sender is CheckBox checkbox))
                return;
            if (checkbox.IsChecked == null)
                return;
            ChangeGamesList(checkbox.IsChecked == true);
            InsertGamesAsync();
        }

        private void ChangeGamesList(bool isChecked)
        {
            if (isChecked)
            {
                _games = Api.MainView.FilteredGames.ToList();
                _isCheckboxChecked = true;
            }
            else
            {
                _games = Api.Database.Games.ToList();
                _isCheckboxChecked = false;
            }
        }

        private void InsertGamesIntoExtenders()
        {
            Active.Visibility = Visibility.Collapsed;
            Excluded.Visibility = Visibility.Collapsed;

            Active.Children.Clear();
            Excluded.Children.Clear();
            var style = TryFindResource("ItemRow") as Style ?? CreateItemRowStyle();
            var styleTextBox = TryFindResource("WeightBox") as Style ?? CreateWeightBoxStyle();
            var excludedStyle = TryFindResource("ExcludeButton") as Style;
            var addButton = TryFindResource("AddButton") as Style;
            foreach (var game in _games)
            {
                if (!_excludedGames.Contains(game.Id))
                {
                    var grid = CreateActiveRow(style, styleTextBox, excludedStyle, game);
                    Active.Children.Add(grid);
                }
                else
                {
                    var grid = CreateExcludedRow(style, addButton, game);
                    Excluded.Children.Add(grid);
                }
            }

            Active.Visibility = Visibility.Visible;
            Excluded.Visibility = Visibility.Visible;

            DrawWheelAsync();
        }

        private Grid CreateActiveRow(Style style, Style styleTextBox, Style excludedStyle, Game game)
        {
            var grid = new Grid
            {
                Style = style
            };
            var definition = grid.ColumnDefinitions;
            definition.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            definition.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) });
            definition.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) });
            var textBlock = new TextBlock
            {
                Text = game.Name,
                VerticalAlignment = VerticalAlignment.Center
            };
            Grid.SetColumn(textBlock, 0);
            if (!_multipliedGames.TryGetValue(game.Id, out var text))
            {
                text = 1;
            }

            var textBox = new TextBox
            {
                Text = $"{text}",
                Style = styleTextBox,
                Tag = game.Id.ToString()
            };
            textBox.PreviewTextInput += UIElement_OnPreviewTextInput;
            textBox.TextChanged += UIElement_OnTextChanged;
            Grid.SetColumn(textBox, 1);
            var button = new Button
            {
                Content = "-",
                Style = excludedStyle,
                Tag = game.Id.ToString()
            };
            button.Click += ExcludeButton_Click;
            Grid.SetColumn(button, 2);

            grid.Children.Add(textBlock);
            grid.Children.Add(textBox);
            grid.Children.Add(button);
            return grid;
        }

        private Grid CreateExcludedRow(Style style, Style addButton, Game game)
        {
            var grid = new Grid
            {
                Style = style
            };
            var definition = grid.ColumnDefinitions;
            definition.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            definition.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) });
            var textBlock = new TextBlock
            {
                Text = game.Name,
                VerticalAlignment = VerticalAlignment.Center
            };
            Grid.SetColumn(textBlock, 0);
            var button = new Button
            {
                Content = "+",
                Style = addButton,
                Tag = game.Id.ToString()
            };
            button.Click += AddButton_Click;
            Grid.SetColumn(button, 1);

            grid.Children.Add(textBlock);
            grid.Children.Add(button);
            return grid;
        }


        private static Style CreateItemRowStyle()
        {
            var style = new Style(typeof(Grid));
            style.Setters.Add(new Setter(MarginProperty, new Thickness(0, 3, 0, 3)));
            return style;
        }

        private static Style CreateWeightBoxStyle()
        {
            var style = new Style(typeof(TextBox));
            style.Setters.Add(new Setter(WidthProperty, 42d));
            style.Setters.Add(new Setter(HeightProperty, 24d));
            style.Setters.Add(new Setter(Control.HorizontalContentAlignmentProperty, HorizontalAlignment.Center));
            style.Setters.Add(new Setter(Control.VerticalContentAlignmentProperty, VerticalAlignment.Center));
            style.Setters.Add(new Setter(MarginProperty, new Thickness(8, 0, 8, 0)));
            return style;
        }

        private void ResetPresetButton(object sender, RoutedEventArgs e)
        {
            _multipliedGames.Clear();
            _excludedGames.Clear();
            InsertGamesIntoExtenders();
            Api.Dialogs.ShowMessage("Preset has been reset.", "Reset");
        }

        public SaveState GetSaveState()
        {
            return new SaveState(_multipliedGames, _excludedGames, _isCheckboxChecked);
        }

        private int GetWeight(Game game)
        {
            return _multipliedGames.TryGetValue(game.Id, out var weight) ? weight : 1;
        }


        private void DrawWheel()
        {
            WheelCanvas.Children.Clear();
            _wheelItems = _games.Where(game => !_excludedGames.Contains(game.Id))
                .Select(game => (game, GetWeight(game)))
                .ToList();
            if (_wheelItems.Count == 0)
                return;

            var radius = Math.Min(WheelCanvas.Width, WheelCanvas.Height) / 2;
            var cx = WheelCanvas.Width / 2;
            var cy = WheelCanvas.Height / 2;

            
            WheelCanvas.Clip = new EllipseGeometry(new Point(cx, cy), radius, radius);

            double total = _wheelItems.Sum(s => s.Weight);
            double cursor = 0;
            const double minAngleForLabel = 4;
            
            for (var i = 0; i < _wheelItems.Count; i++)
            {
                var sliceAngle = (_wheelItems[i].Weight / total) * 360;
                var startAngle = cursor;
                var endAngle = cursor + sliceAngle;
                WheelCanvas.Children.Add(
                    CreateSlice(cx, cy, radius, startAngle, endAngle, _palette[i % _palette.Count]));
                if (sliceAngle >= minAngleForLabel)
                {
                    var label = CreateLabel(_wheelItems[i].Game.Name, cx, cy, radius, startAngle, sliceAngle);
                    if (label != null)
                    {
                        WheelCanvas.Children.Add(label);
                    }
                }
                cursor = endAngle;
            }

            
            var outlineRadius = radius;
            var outline = new System.Windows.Shapes.Ellipse
            {
                Width = outlineRadius * 2,
                Height = outlineRadius * 2,
                Stroke = Brushes.White,
                StrokeThickness = 2,
                Fill = null,
                IsHitTestVisible = false
            };
            Canvas.SetLeft(outline, cx - outlineRadius);
            Canvas.SetTop(outline, cy - outlineRadius);
            WheelCanvas.Children.Add(outline);
        }

        private Path CreateSlice(double cx, double cy, double radius, double startAngle, double endAngle, Color color)
        {
            var startPoint = PointOnCircle(cx, cy, radius, startAngle);
            var endPoint = PointOnCircle(cx, cy, radius, endAngle);
            var isLargeArc = (endAngle - startAngle) > 180;
            var figure = new PathFigure
            {
                StartPoint = new Point(cx, cy),
                IsClosed = true
            };
            figure.Segments.Add(new LineSegment(startPoint, true));
            figure.Segments.Add(new ArcSegment(endPoint, new Size(radius, radius), 0, isLargeArc,
                SweepDirection.Clockwise, true));
            var geometry = new PathGeometry();
            geometry.Figures.Add(figure);
            return new Path
            {
                Data = geometry,
                Fill = new SolidColorBrush(color),
            };
        }

        private TextBlock CreateLabel(string text, double cx, double cy, double radius, double startAngle,
            double sliceAngle)
        {
            var midAngle = startAngle + sliceAngle / 2;

            var innerRadius = radius * 0.22;
            var outerRadius = radius * 0.92;
            var maxWidth = outerRadius - innerRadius;
            var midRadius = (innerRadius + outerRadius) / 2;
            var baseFontSize = radius * 0.06;
            var scaledFontSize = baseFontSize * (sliceAngle / 30);

            var minFontSize = Math.Max(8, radius * 0.03);
            var maxFontSize = radius * 0.09;
            var fontSize = Math.Min(Math.Max(scaledFontSize, minFontSize), maxFontSize);


            var tb = new TextBlock
            {
                Text = text,
                Foreground = new SolidColorBrush(Colors.White),
                FontWeight = FontWeights.Bold,
                FontFamily = new FontFamily("Segoe UI Semibold"),
                FontSize = fontSize,
                MaxWidth = maxWidth,
                TextTrimming = TextTrimming.CharacterEllipsis,
                TextAlignment = TextAlignment.Center,
                Effect = new System.Windows.Media.Effects.DropShadowEffect
                {
                    Color = Colors.Black,
                    BlurRadius = 3,
                    ShadowDepth = 0,
                    Opacity = 0.9
                }
            };

            Point anchor = PointOnCircle(cx, cy, midRadius, midAngle);

            tb.Measure(new Size(maxWidth, double.PositiveInfinity));
            double w = Math.Min(tb.DesiredSize.Width, maxWidth);
            double h = tb.DesiredSize.Height;


            Canvas.SetLeft(tb, anchor.X - w / 2);
            Canvas.SetTop(tb, anchor.Y - h / 2);


            double screenAngle = midAngle - 90;
            double normalized = ((screenAngle + 180) % 360 + 360) % 360 - 180;
            if (normalized > 90) normalized -= 180;
            else if (normalized < -90) normalized += 180;

            tb.RenderTransformOrigin = new Point(0.5, 0.5);
            tb.RenderTransform = new RotateTransform(normalized);

            return tb;
        }


        private Point PointOnCircle(double cx, double cy, double radius, double angleDegrees)
        {
            var rad = (Math.PI / 180) * (angleDegrees - 90);
            return new Point((int)(cx + (radius * Math.Cos(rad))), (int)(cy + (radius * Math.Sin(rad))));
        }

        private void Spin()
        {
            if (!_wheelItems.Any())
            {
                Api.Dialogs.ShowMessage("No games to spin.", "Error");
                return;
            }

            var total = _wheelItems.Sum(s => s.Weight);
            var roll = _random.NextDouble() * total;
            var cumulative = 0.0;
            var winningIndex = 0;
            double winStart = 0;
            double winAngle = 0;
            for (var i = 0; i < _wheelItems.Count; i++)
            {
                var sliceAngle = (_wheelItems[i].Weight / (double)total) * 360;
                if (roll <= cumulative + _wheelItems[i].Weight)
                {
                    winningIndex = i;
                    winStart = (cumulative / total) * 360;
                    winAngle = sliceAngle;
                    break;
                }

                cumulative += _wheelItems[i].Weight;
            }

            var targetSliceCenter = winStart + winAngle / 2;
            var jitter = (_random.NextDouble() - 0.5) * winAngle * 0.6;

            var fullspin = 5 + _random.Next(3);

            
            var baseTarget = ((360 - targetSliceCenter) % 360 + 360) % 360;
            var desiredMod = ((baseTarget + jitter) % 360 + 360) % 360;

           
            var forwardDelta = ((desiredMod - _angle) % 360 + 360) % 360;

            var finalAngle = _angle + forwardDelta + fullspin * 360;

            var animation = new DoubleAnimation
            {
                From = _angle,
                To = finalAngle,
                Duration = new Duration(TimeSpan.FromSeconds(fullspin)),
                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
            };
            var winner = _wheelItems[winningIndex].Game;
            SpinButton.IsEnabled = false;

            animation.Completed += (sender, args) =>
            {
                _angle = finalAngle % 360;
                SpinButton.IsEnabled = true;
                logger.Info($"Spin result winner={winner.Name} roll={roll} winningIndex={winningIndex} " +
                            $"winStart={winStart} winAngle={winAngle} finalAngle%360={_angle} fullspin={fullspin} " +
                            $"jitter={jitter} targetCenter={targetSliceCenter}");
                SpinCompleted?.Invoke(winner);
            };

            WheelRotation.BeginAnimation(RotateTransform.AngleProperty, animation);
        }

        private void SpinButtonClick(object sender, RoutedEventArgs e)
        {
            Spin();
        }

        private void MuteOrUnMuteButton(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var content = button.Content as string;
            //Button is unmuted
            if (content == "🔊")
            {
                _isMuted = !_isMuted;
                button.Content = "🔇";
            }
            //Button is muted
            else
            {
                _isMuted = true;
                button.Content = "🔊";
            }
        }
    }
}