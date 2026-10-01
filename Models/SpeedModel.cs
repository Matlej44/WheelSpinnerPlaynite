namespace WheelSpinner.Models
{
    public class SpeedModel
    {
        public string Name {get; set;}
        public double Speed {get; set;}
        public double DurationMultiplier {get; set;}
        public SpeedModel(string name, double speed, double duration)
        {
            Name = name;
            Speed = speed;
            DurationMultiplier = duration;
        }

        public override string ToString()
        {
            return Name;
        }
    }
}