using Playnite.SDK.Models;

namespace WheelSpinner.Models
{
    public class SpinPlan
    {
        public Game Winner { get; set; }
        public int WinningIndex { get; set; }
        public double Roll { get; set; }
        public double WinStart { get; set; }
        public double WinAngle { get; set; }
        public double TargetSliceCenter { get; set; }
        public double Jitter { get; set; }
        public int FullSpins { get; set; }
        public double FinalAngle { get; set; }
        
        public double DurationSeconds => FullSpins;

        public string Describe()
        {
            return $"Spin result winner={Winner.Name} roll={Roll} winningIndex={WinningIndex} " +
                   $"winStart={WinStart} winAngle={WinAngle} finalAngle%360={FinalAngle % 360} " +
                   $"fullspin={FullSpins} jitter={Jitter} targetCenter={TargetSliceCenter}";
        }
    }
}
