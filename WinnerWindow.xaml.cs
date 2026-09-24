using System;
using System.IO;
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
        public WinnerWindow(IPlayniteAPI api, Game winner, ILogger logger)
        {
            InitializeComponent();

            Api = api;
            this.logger = logger;
            GameName.Content = winner.Name;
            CoverArt.Source = GetImage(winner);
            
            
            
        }

        private BitmapImage GetImage(Game game)
        {
            var cover = game.CoverImage;
            if(string.IsNullOrEmpty(cover))
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
                var image = new BitmapImage();
                image.DecodePixelHeight = 560;
                image.DecodePixelWidth = 420;
                image.BeginInit();
                image.CacheOption = BitmapCacheOption.OnLoad;
                image.UriSource = new Uri(source, UriKind.Absolute);
                image.EndInit();
                if(image.CanFreeze) image.Freeze();
                return image;
            }
            catch (Exception e)
            {
                logger.Error(e, "Failed to load cover image.");
                return null;
            }
        }
    }
}