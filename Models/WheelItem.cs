using Playnite.SDK.Models;

namespace WheelSpinner.Models
{
    public class WheelItem
    {
        public WheelItem(Game game, int weight)
        {
            Game = game;
            Weight = weight;
        }

        public Game Game { get; }
        public int Weight { get; }
    }
}
