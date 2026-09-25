using System;
using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Playnite.SDK;
using Playnite.SDK.Models;
using Path = System.IO.Path;

namespace WheelSpinner
{
    public partial class WinnerWindow : UserControl
    {
        private IPlayniteAPI Api { get; set; }
        private ILogger logger { get; set; }
        private Game _winner { get; set;}
        public event Action<Window> CloseAll;
        public event Action<Game> ExcludeGame;
        

        public WinnerWindow(IPlayniteAPI api, Game winner, ILogger logger)
        {
            InitializeComponent();

            Api = api;
            this.logger = logger;
            _winner = winner;
            GameName.Content = winner.Name;
            var image = GetImage(winner);
            if (image != null)
            {
                CoverArt.Source = image;
            }
            else
            {
                //We fallBack to the default cover art 
                var defaultImage = Application.Current.TryFindResource("DefaultGameCover") as BitmapImage;
                if (defaultImage != null)
                {
                    CoverArt.Source = defaultImage;
                }
                //If there is no default cover art, we just make a black cover art
                else
                {
                    var bitmap = new WriteableBitmap(420, 560, 96, 96,
                        PixelFormats.Pbgra32,
                        null);
                    byte[] pixels = new byte[420*560*4];
                    for (int i = 0; i < pixels.Length; i+=4)
                    {
                        pixels[i] = 0;
                        pixels[i+1] = 0;
                        pixels[i+2] = 0;
                        pixels[i+3] = 255;
                    }
                    bitmap.WritePixels(new Int32Rect(0, 0, 420, 560), pixels, 420 * 4, 0);
                    CoverArt.Source = bitmap;
                }
            }
        }

        private BitmapImage GetImage(Game game)
        {
            var cover = game.CoverImage;
            if (string.IsNullOrEmpty(cover))
                return null;
            string source;

            //CoverImage can be a URL Path or a database path
            if (cover.StartsWith("http", StringComparison.OrdinalIgnoreCase))
            {
                source = cover;
            }
            else if (Path.IsPathRooted(cover) && File.Exists(cover))
            {
                source = cover;
            }
            else
            {
                source = Api.Database.GetFullFilePath(cover);
            }

            if (string.IsNullOrEmpty(source))
                return null;

            try
            {
                var image = new BitmapImage
                {
                    DecodePixelHeight = 560,
                    DecodePixelWidth = 420
                };
                image.BeginInit();
                image.CacheOption = BitmapCacheOption.OnLoad;
                image.UriSource = new Uri(source, UriKind.Absolute);
                image.EndInit();
                if (image.CanFreeze) image.Freeze();
                return image;
            }
            catch (Exception e)
            {
                logger.Error(e, "Failed to load cover image.");
                return null;
            }
        }

        private void ShowGameAndClose(object sender, RoutedEventArgs e)
        {
            Api.MainView.SelectGame(_winner.Id);
            CloseAll?.Invoke(Window.GetWindow(this));
        }

        private void PlayGameButton(object sender, RoutedEventArgs e)
        {
            CloseAll?.Invoke(Window.GetWindow(this));
            Api.StartGame(_winner.Id);
        }

        private void BackToWheelButton(object sender, RoutedEventArgs e)
        {
            Window window = Window.GetWindow(this);
            window.Close();
        }

        private void ExcludeGameButton(object sender, RoutedEventArgs e)
        {
            Window window = Window.GetWindow(this);
            window.Close();
            ExcludeGame?.Invoke(_winner);
        }
    }
}