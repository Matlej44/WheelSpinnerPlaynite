using System;
using Playnite.SDK.Models;

namespace WheelSpinner.Models
{
    class MultipliedGame
    {
        public Guid  GameId { get; set; }
        public int Multiplier { get; set; }

        public MultipliedGame(Guid gameId, int multiplier)
        {
            GameId = gameId;
            Multiplier = multiplier;
        }

        public override string ToString() => $"{GameId}: {Multiplier}";
    }
}