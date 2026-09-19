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
        private List<Game> _games;
        public readonly List<Guid> ExcludedGames = new List<Guid>();
        public readonly List<MultipliedGame> MultipliedGames = new List<MultipliedGame>();

        public SpinWheelWindow(IPlayniteAPI api)
        {
            InitializeComponent();
            if (Application.Current.TryFindResource("TextBlockBaseStyle") is Style textBlockStyle)
            {
                var newStyle = new Style(typeof(TextBlock), textBlockStyle);
                Resources.Add(typeof(TextBlock), newStyle);
            }

            Api = api;
            _games = Api.MainView.FilteredGames;
            InsertGamesIntoExtenders();
            SizeChanged += MainWindow_SizeChanged;
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
            e.Handled = new Regex("[^0-9]+").IsMatch(e.Text);
            if (e.Handled)
                return;
            var textBox = sender as TextBox;
            if (textBox == null)
                return;
            var mult = int.Parse(textBox.Text);
            if (mult > 1)
            {
                MultipliedGames.Add(new MultipliedGame(Guid.Parse(textBox.Tag.ToString()), mult));
            }
        }

        private void ExcludeButton_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            if(button==null)
                return;
            ExcludedGames.Add(Guid.Parse(button.Tag.ToString()));
            InsertGamesIntoExtenders();
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            if (button == null)
                return;
            ExcludedGames.Remove(Guid.Parse(button.Tag.ToString()));
            InsertGamesIntoExtenders();
        }

        private void CheckboxChanged(object sender, RoutedEventArgs e)
        {
            if (!(sender is CheckBox checkbox))
                return;
            if (checkbox.IsChecked == true)
            {
                _games = Api.Database.Games.ToList();
            }
            else
            {
                _games = Api.MainView.FilteredGames;
            }
        }
        
        private void InsertGamesIntoExtenders()
        {
            Active.Children.Clear();
            Excluded.Children.Clear();
            var style = TryFindResource("ItemRow") as Style ?? CreateItemRowStyle();
            var styleTextBox = TryFindResource("WeightBox") as Style ?? CreateWeightBoxStyle();
            var excludedStyle = TryFindResource("ExcludeButton") as Style;
            var addButton = TryFindResource("AddButton") as Style;
            foreach (var game in _games)
            {
                if (!ExcludedGames.Contains(game.Id))
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
                    var textBox = new TextBox
                    {
                        Text = "1",
                        Style = styleTextBox,
                        Tag = game.Id.ToString()
                    };
                    textBox.PreviewTextInput += UIElement_OnPreviewTextInput;
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
                    Active.Children.Add(grid);
                }
                else
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
                    Excluded.Children.Add(grid);
                }
            }
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
        
    }
}