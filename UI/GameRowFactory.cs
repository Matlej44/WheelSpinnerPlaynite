using System;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Playnite.SDK.Models;

namespace WheelSpinner.UI
{
    /// <summary>Builds the rows shown in the "Active" and "Excluded" lists.</summary>
    public class GameRowFactory
    {
        private static readonly Regex WeightPattern = new Regex(@"^[1-9][0-9]{0,2}$");

        private readonly Style _rowStyle;
        private readonly Style _weightBoxStyle;
        private readonly Style _excludeButtonStyle;
        private readonly Style _addButtonStyle;

        private readonly Func<Guid, int> _getWeight;
        private readonly Action<Guid, int> _onWeightChanged;
        private readonly Action<Guid> _onExclude;
        private readonly Action<Guid> _onInclude;

        public GameRowFactory(
            FrameworkElement resourceOwner,
            Func<Guid, int> getWeight,
            Action<Guid, int> onWeightChanged,
            Action<Guid> onExclude,
            Action<Guid> onInclude)
        {
            _getWeight = getWeight;
            _onWeightChanged = onWeightChanged;
            _onExclude = onExclude;
            _onInclude = onInclude;

            _rowStyle = resourceOwner.TryFindResource("ItemRow") as Style ?? CreateItemRowStyle();
            _weightBoxStyle = resourceOwner.TryFindResource("WeightBox") as Style ?? CreateWeightBoxStyle();
            _excludeButtonStyle = resourceOwner.TryFindResource("ExcludeButton") as Style;
            _addButtonStyle = resourceOwner.TryFindResource("AddButton") as Style;
        }

        public Grid CreateActiveRow(Game game)
        {
            var grid = CreateRow(game, 3);

            var weightBox = new TextBox
            {
                Text = _getWeight(game.Id).ToString(),
                Style = _weightBoxStyle
            };
            weightBox.PreviewTextInput += OnWeightPreviewTextInput;
            weightBox.TextChanged += (s, e) =>
            {
                if (int.TryParse(weightBox.Text, out var weight))
                    _onWeightChanged(game.Id, weight);
            };
            Grid.SetColumn(weightBox, 1);

            var excludeButton = new Button { Content = "-", Style = _excludeButtonStyle };
            excludeButton.Click += (s, e) => _onExclude(game.Id);
            Grid.SetColumn(excludeButton, 2);

            grid.Children.Add(weightBox);
            grid.Children.Add(excludeButton);
            return grid;
        }

        public Grid CreateExcludedRow(Game game)
        {
            var grid = CreateRow(game, 2);

            var addButton = new Button { Content = "+", Style = _addButtonStyle };
            addButton.Click += (s, e) => _onInclude(game.Id);
            Grid.SetColumn(addButton, 1);

            grid.Children.Add(addButton);
            return grid;
        }
        
        private Grid CreateRow(Game game, int columnCount)
        {
            var grid = new Grid { Style = _rowStyle };
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            for (var i = 1; i < columnCount; i++)
            {
                grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) });
            }

            var nameText = new TextBlock { Text = game.Name, VerticalAlignment = VerticalAlignment.Center };
            Grid.SetColumn(nameText, 0);
            grid.Children.Add(nameText);
            return grid;
        }


        private static void OnWeightPreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            if (!(sender is TextBox textBox))
                return;

            var fullText = textBox.Text
                .Remove(textBox.SelectionStart, textBox.SelectionLength)
                .Insert(textBox.SelectionStart, e.Text);

            e.Handled = !WeightPattern.IsMatch(fullText);
        }

        private static Style CreateItemRowStyle()
        {
            var style = new Style(typeof(Grid));
            style.Setters.Add(new Setter(FrameworkElement.MarginProperty, new Thickness(0, 3, 0, 3)));
            return style;
        }

        private static Style CreateWeightBoxStyle()
        {
            var style = new Style(typeof(TextBox));
            style.Setters.Add(new Setter(FrameworkElement.WidthProperty, 42d));
            style.Setters.Add(new Setter(FrameworkElement.HeightProperty, 24d));
            style.Setters.Add(new Setter(Control.HorizontalContentAlignmentProperty, HorizontalAlignment.Center));
            style.Setters.Add(new Setter(Control.VerticalContentAlignmentProperty, VerticalAlignment.Center));
            style.Setters.Add(new Setter(FrameworkElement.MarginProperty, new Thickness(8, 0, 8, 0)));
            return style;
        }
    }
}
