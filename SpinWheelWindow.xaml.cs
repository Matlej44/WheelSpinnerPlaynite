using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using Playnite.SDK;
using Playnite.SDK.Models;
using WheelSpinner.Models;
using WheelSpinner.Rendering;
using WheelSpinner.Services;
using WheelSpinner.UI;

namespace WheelSpinner
{
    public partial class SpinWheelWindow : UserControl
    {
        private readonly IPlayniteAPI _api;
        private readonly ILogger _logger;
        private readonly WheelSpinnerSettingsViewModel _settings;

        private readonly GameSelection _selection;
        private readonly WheelRenderer _renderer;
        private readonly WheelSpinCalculator _spinCalculator;
        private readonly TickSoundPlayer _tickSound;
        private readonly GameRowFactory _rowFactory;

        private IReadOnlyList<WheelItem> _wheelItems = new List<WheelItem>();
        private double _angle;
        private int _lastSliceIndex = -1;
        private bool _isSpinning;

        public event Action<Game> SpinCompleted;

        public SpinWheelWindow(IPlayniteAPI api, ILogger logger, WheelSpinnerSettingsViewModel settings, SaveState saveState = null)
        {
            InitializeComponent();
            ApplyThemeTextStyle();

            _api = api;
            _logger = logger;
            _settings = settings;

            var random = new Random();
            _selection = new GameSelection(api, saveState);
            _renderer = new WheelRenderer(WheelCanvas);
            _spinCalculator = new WheelSpinCalculator(random);
            _tickSound = new TickSoundPlayer(logger, random);
            _tickSound.Load();
            _tickSound.IsMuted = saveState?.IsMuted ?? false;
            ApplySettings();
            MuteButton.Content = _tickSound.IsMuted ? "🔇" : "🔊";
            _rowFactory = new GameRowFactory(this, _selection.GetWeight, OnWeightChanged, ExcludeGame, IncludeGame);

            if (FilteredBox.IsChecked != _selection.UseFilteredView)
            {
                FilteredBox.IsChecked = _selection.UseFilteredView;
            }

            RefreshGameListsAsync();
        }

        private WheelSpinnerSettings CurrentSettings => _settings?.Settings ?? new WheelSpinnerSettings();

        private void ApplySettings()
        {
            _tickSound.Volume = CurrentSettings.Volume;
        }


        public SaveState GetSaveState()
        {
            
            var save = _selection.ToSaveState();
            save.IsMuted = _tickSound.IsMuted;
            return save;
        }

        public void ExcludeGame(Guid gameId)
        {
            _selection.Exclude(gameId);
            RefreshGameListsAsync();
        }


        private void ApplyThemeTextStyle()
        {
            if (Application.Current.TryFindResource("TextBlockBaseStyle") is Style baseStyle)
            {
                Resources.Add(typeof(TextBlock), new Style(typeof(TextBlock), baseStyle));
            }
        }



        private void IncludeGame(Guid gameId)
        {
            _selection.Include(gameId);
            RefreshGameListsAsync();
        }

        private void OnWeightChanged(Guid gameId, int weight)
        {
            _selection.SetWeight(gameId, weight);
            RedrawWheelAsync();
        }

        private void RefreshGameListsAsync() => RunOnUiThread(RefreshGameLists, "Failed to insert games into extenders.");

        private void RedrawWheelAsync() => RunOnUiThread(RedrawWheel, "Failed to draw wheel.");

        private async void RunOnUiThread(Action action, string errorMessage)
        {
            try
            {
                await Dispatcher.InvokeAsync(action);
            }
            catch (Exception e)
            {
                _logger.Error(e, errorMessage);
            }
        }

        private void RefreshGameLists()
        {
            Active.Visibility = Visibility.Collapsed;
            Excluded.Visibility = Visibility.Collapsed;
            Active.Children.Clear();
            Excluded.Children.Clear();

            foreach (var game in _selection.Games)
            {
                if (_selection.IsExcluded(game.Id))
                    Excluded.Children.Add(_rowFactory.CreateExcludedRow(game));
                else
                    Active.Children.Add(_rowFactory.CreateActiveRow(game));
            }

            Active.Visibility = Visibility.Visible;
            Excluded.Visibility = Visibility.Visible;

            RedrawWheelAsync();
        }

        private void RedrawWheel()
        {
            _wheelItems = _selection.BuildWheelItems();
            _renderer.Draw(_wheelItems);
        }
        

        private void CheckboxChanged(object sender, RoutedEventArgs e)
        {
            if (!(sender is CheckBox checkbox) || checkbox.IsChecked == null)
                return;

            _selection.SetFilteredView(checkbox.IsChecked == true);
            RefreshGameListsAsync();
        }

        private void ResetPresetButton(object sender, RoutedEventArgs e)
        {
            _selection.Reset();
            RefreshGameLists();
            _api.Dialogs.ShowMessage("Preset has been reset.", "Reset");
        }

        private void MuteOrUnMuteButton(object sender, RoutedEventArgs e)
        {
            if (!(sender is Button button))
                return;

            _tickSound.IsMuted = !_tickSound.IsMuted;
            button.Content = _tickSound.IsMuted ? "🔇" : "🔊";
        }

        private void SpinButtonClick(object sender, RoutedEventArgs e)
        {
            Spin();
        }

        public void StopSpin()
        {
            if (!_isSpinning)
                return;
            _isSpinning = false;
            CompositionTarget.Rendering -= OnRenderingDuringSpin;
            WheelRotation.BeginAnimation(RotateTransform.AngleProperty, null);
            _angle = WheelRotation.Angle;
            SpinButton.IsEnabled = true;
        }
        

        private void Spin()
        {
            if (_wheelItems.Count == 0)
            {
                _api.Dialogs.ShowMessage("No games to spin.", "Error");
                return;
            }
            _isSpinning = true;

            ApplySettings();
            var speed = CurrentSettings.ActiveItem;
            var plan = _spinCalculator.CreatePlan(_wheelItems, _angle);
            _lastSliceIndex = _renderer.GetSliceIndex(AngleMath.Normalize(-_angle));

            SpinButton.IsEnabled = false;
            CompositionTarget.Rendering += OnRenderingDuringSpin;

            var animation = new DoubleAnimation
            {
                From = _angle,
                To = plan.FinalAngle,
                Duration = new Duration(TimeSpan.FromSeconds(plan.DurationSeconds * speed.DurationMultiplier)),
                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut },
                SpeedRatio = speed.Speed
            };
            animation.Completed += (s, e) => OnSpinAnimationCompleted(plan);

            WheelRotation.BeginAnimation(RotateTransform.AngleProperty, animation);
        }

        private void OnSpinAnimationCompleted(SpinPlan plan)
        {
            if (!_isSpinning)
                return;
            _isSpinning = false;
            CompositionTarget.Rendering -= OnRenderingDuringSpin;
            _angle = plan.FinalAngle % 360;
            SpinButton.IsEnabled = true;

            _logger.Info(plan.Describe());
            SpinCompleted?.Invoke(plan.Winner);
        }
        
        private void OnRenderingDuringSpin(object sender, EventArgs e)
        {
            if (!_renderer.HasSlices)
                return;

            var pointerAngle = AngleMath.Normalize(-WheelRotation.Angle);
            var currentIndex = _renderer.GetSliceIndex(pointerAngle);
            if (currentIndex == _lastSliceIndex)
                return;

            _lastSliceIndex = currentIndex;
            _tickSound.Tick();
        }
    }
}
