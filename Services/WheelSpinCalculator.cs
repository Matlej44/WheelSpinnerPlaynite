using System;
using System.Collections.Generic;
using System.Linq;
using WheelSpinner.Models;
using WheelSpinner.Rendering;

namespace WheelSpinner.Services
{
     
    public class WheelSpinCalculator
    {
        private readonly Random _random;

        public WheelSpinCalculator(Random random)
        {
            _random = random;
        }

        public SpinPlan CreatePlan(IReadOnlyList<WheelItem> items, double currentAngle)
        {
            var total = items.Sum(i => i.Weight);
            var roll = _random.NextDouble() * total;

            var winningIndex = 0;
            double winStart = 0;
            double winAngle = 0;
            double cumulative = 0;

            for (var i = 0; i < items.Count; i++)
            {
                if (roll <= cumulative + items[i].Weight)
                {
                    winningIndex = i;
                    winStart = (cumulative / total) * 360;
                    winAngle = (items[i].Weight / (double)total) * 360;
                    break;
                }

                cumulative += items[i].Weight;
            }

            var targetSliceCenter = winStart + winAngle / 2;
            var jitter = (_random.NextDouble() - 0.5) * winAngle * 0.6;
            var fullSpins = 5 + _random.Next(3);

            var baseTarget = AngleMath.Normalize(360 - targetSliceCenter);
            var desiredAngle = AngleMath.Normalize(baseTarget + jitter);
            var forwardDelta = AngleMath.Normalize(desiredAngle - currentAngle);

            return new SpinPlan
            {
                Winner = items[winningIndex].Game,
                WinningIndex = winningIndex,
                Roll = roll,
                WinStart = winStart,
                WinAngle = winAngle,
                TargetSliceCenter = targetSliceCenter,
                Jitter = jitter,
                FullSpins = fullSpins,
                FinalAngle = currentAngle + forwardDelta + fullSpins * 360
            };
        }
    }
}
