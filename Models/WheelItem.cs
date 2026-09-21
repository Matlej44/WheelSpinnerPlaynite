namespace WheelSpinner.Models
{
    public class WheelItem
    {
        public string GameName { get; set; }
        public int Weight { get; set; }

        public WheelItem(string gameName, int weight)
        {
            GameName = gameName;
            Weight = weight;
        }

        public WheelItem()
        {
        }
    }
}