using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Playnite.SDK;
using Playnite.SDK.Models;


namespace WheelSpinner
{
    public partial class SpinWheelWindow : UserControl
    {
        private IPlayniteAPI Api { get; set; }
        private List<Game> _games;
        private List<Guid> _excludedGames =  new List<Guid>();
        public SpinWheelWindow(IPlayniteAPI api)
        {
            InitializeComponent();
            if (Application.Current.TryFindResource("TextBlockBaseStyle") is Style textBlockStyle)
            {
                var newStyle = new Style(typeof(TextBlock),  textBlockStyle);
                this.Resources.Add(typeof(TextBlock), newStyle);
            }
            this.Api = api;
            _games = Api.MainView.FilteredGames;
        }

        private void UIElement_OnPreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = new Regex("[^0-9]+").IsMatch(e.Text);
        }

        private void CheckboxChanged(object sender, RoutedEventArgs e)
        {
            if(!(sender is CheckBox checkbox))
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
            var style = Application.Current.TryFindResource("ItemRow") as Style ?? CreateItemRowStyle();
            var styleTextBox = Application.Current.TryFindResource("WeightBox") as Style ?? CreateWeightBoxStyle();
            foreach (var game in _games)
            {
                if (!_excludedGames.Contains(game.Id))
                {
                    var grid = new Grid
                    {
                        Style = style
                    };
                    var definition = grid.ColumnDefinitions;
                    definition.Add(new ColumnDefinition
                    {
                        Width = new GridLength(1, GridUnitType.Star)
                    });
                    definition.Add(new ColumnDefinition
                    {
                        Width = new GridLength(1, GridUnitType.Auto)
                    });
                    definition.Add(new ColumnDefinition
                    {
                        Width = new GridLength(1, GridUnitType.Auto)
                    });
                    var textBlock = new TextBlock
                    {
                        Text = game.Name,
                        VerticalAlignment = VerticalAlignment.Center
                    };
                    var textBox =  new TextBox
                    {
                        Text = "1",
                        Style = styleTextBox,

                    };
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