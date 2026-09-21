using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using Playnite.SDK;
using Playnite.SDK.Models;
using WheelSpinner.Models;


namespace WheelSpinner
{
    public partial class SpinWheelWindow : UserControl
    {
        private IPlayniteAPI Api { get; set; }
        private List<Game> _games = new List<Game>();
        private readonly HashSet<Guid> _excludedGames = new HashSet<Guid>();
        private readonly Dictionary<Guid, int> _multipliedGames = new Dictionary<Guid, int>();
        private bool _isCheckboxChecked = true;
        private ILogger logger { get; set; }

        public SpinWheelWindow(IPlayniteAPI api,ILogger logger, SaveState saveState = null)
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
            
            SizeChanged += MainWindow_SizeChanged;
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

        private void MainWindow_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (e.HeightChanged)
            {
                WheelCanvas.Width = e.NewSize.Height - 100;
                WheelCanvas.Height = e.NewSize.Height - 100;
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
            if (textBox == null||string.IsNullOrEmpty(textBox.Text))
                return;
            if(!Guid.TryParse(textBox.Tag.ToString(), out var gameId))
                return;
            if (int.TryParse(textBox.Text, out var multiplier))
            {
                if (_multipliedGames.ContainsKey(gameId))
                {
                    if (multiplier == 1)
                    {
                        _multipliedGames.Remove(gameId);
                        return;
                    }
                    _multipliedGames[gameId] = multiplier;
                }
                else if (multiplier > 1)
                {
                    _multipliedGames.Add(gameId, multiplier);
                }
            }
        }

        private void ExcludeButton_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            if(button==null)
                return;
            _excludedGames.Add(Guid.Parse(button.Tag.ToString()));
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
            ChangeGamesList(checkbox.IsChecked==true);
            InsertGamesAsync();
        }

        private void ChangeGamesList(bool isChecked)
        {
            if (isChecked)
            {
                _games = Api.MainView.FilteredGames;
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
        }

        private Grid CreateActiveRow(Style style, Style styleTextBox, Style excludedStyle, Game game)
        {
            var grid = new Grid
            {
                Style = style
            };
            var definition = grid.ColumnDefinitions;
            definition.Add(new ColumnDefinition {Width = new GridLength(1, GridUnitType.Star)});
            definition.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto)});
            definition.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto)});
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
            definition.Add(new ColumnDefinition {Width = new GridLength(1, GridUnitType.Star)});
            definition.Add(new ColumnDefinition {Width = new GridLength(1, GridUnitType.Auto)});
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
    }
}