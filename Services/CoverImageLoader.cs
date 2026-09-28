using System;
using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Playnite.SDK;
using Playnite.SDK.Models;

namespace WheelSpinner.Services
{

    public class CoverImageLoader
    {
        private const int Width = 420;
        private const int Height = 560;

        private readonly IPlayniteAPI _api;
        private readonly ILogger _logger;

        public CoverImageLoader(IPlayniteAPI api, ILogger logger)
        {
            _api = api;
            _logger = logger;
        }

        public ImageSource LoadOrFallback(Game game)
        {
            return LoadGameCover(game) ?? LoadDefaultCover() ?? CreateBlackCover();
        }

        private BitmapImage LoadGameCover(Game game)
        {
            var source = ResolveCoverSource(game.CoverImage);
            if (string.IsNullOrEmpty(source))
                return null;

            try
            {
                var image = new BitmapImage();
                image.BeginInit();
                image.DecodePixelHeight = Height;
                image.DecodePixelWidth = Width;
                image.CacheOption = BitmapCacheOption.OnLoad;
                image.UriSource = new Uri(source, UriKind.Absolute);
                image.EndInit();
                if (image.CanFreeze) image.Freeze();
                return image;
            }
            catch (Exception e)
            {
                _logger.Error(e, "Failed to load cover image.");
                return null;
            }
        }

         
        private string ResolveCoverSource(string cover)
        {
            if (string.IsNullOrEmpty(cover))
                return null;

            if (cover.StartsWith("http", StringComparison.OrdinalIgnoreCase))
                return cover;

            if (Path.IsPathRooted(cover) && File.Exists(cover))
                return cover;

            return _api.Database.GetFullFilePath(cover);
        }

        private static BitmapImage LoadDefaultCover()
        {
            return Application.Current.TryFindResource("DefaultGameCover") as BitmapImage;
        }

        private static BitmapSource CreateBlackCover()
        {
            var bitmap = new WriteableBitmap(Width, Height, 96, 96, PixelFormats.Pbgra32, null);
            var pixels = new byte[Width * Height * 4];
            for (var i = 3; i < pixels.Length; i += 4)
            {
                pixels[i] = 255;  
            }

            bitmap.WritePixels(new Int32Rect(0, 0, Width, Height), pixels, Width * 4, 0);
            return bitmap;
        }
    }
}
