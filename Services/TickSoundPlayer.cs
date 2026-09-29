using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Windows.Media;
using Playnite.SDK;

namespace WheelSpinner.Services
{

    public class TickSoundPlayer
    {
        private const int PoolSize = 10;
        private static readonly TimeSpan MinTickInterval = TimeSpan.FromMilliseconds(30);

        private readonly List<MediaPlayer> _players = new List<MediaPlayer>();
        private readonly ILogger _logger;
        private readonly Random _random;
        private int _poolPointer;
        private DateTime _lastTickTime = DateTime.MinValue;
        private double _volume = 0.8;

        public TickSoundPlayer(ILogger logger, Random random)
        {
            _logger = logger;
            _random = random;
        }

        public bool IsMuted { get; set; }

        public void Load()
        {
            try
            {
                var directory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)
                                ?? throw new InvalidOperationException();
                var soundPath = Path.Combine(directory, "Resources", "Click.wav");

                if (!File.Exists(soundPath))
                {
                    _logger.Warn("Sound file not found.");
                    return;
                }

                var uri = new Uri(soundPath);
                for (var i = 0; i < PoolSize; i++)
                {
                    var player = new MediaPlayer();
                    player.Volume = 0;
                    player.Open(uri);
                    _players.Add(player);
                }
            }
            catch (Exception e)
            {
                _logger.Error(e, "Failed to load sound file.");
            }
        }
        
        public void Tick()
        {
            var now = DateTime.UtcNow;
            if (now - _lastTickTime <= MinTickInterval)
                return;

            _lastTickTime = now;
            Play();
        }

        private void Play()
        {
            if (IsMuted || _players.Count == 0)
                return;

            try
            {
                var player = _players[_poolPointer];
                _poolPointer = (_poolPointer + 1) % _players.Count;

                player.Volume = _volume + (_random.NextDouble() - 0.5) * 0.2;
                player.Position = TimeSpan.Zero;
                player.Play();
            }
            catch (Exception e)
            {
                _logger.Error(e, "Failed to play tick sound.");
            }
        }
    }
}
