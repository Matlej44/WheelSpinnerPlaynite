using System;
using System.Collections.Generic;

namespace WheelSpinner.Models
{
    public class SaveState
    {
        public Dictionary<Guid, int> MultipliedGames { get; set; } = new Dictionary<Guid, int>();
        public HashSet<Guid> ExcludedGames { get; set; } = new HashSet<Guid>();
        public bool IsCheckboxChecked { get; set; } = true;
        public bool IsMuted { get; set; } = false;

        public SaveState(Dictionary<Guid, int> multipliedGames, HashSet<Guid> excludedGames, bool isCheckboxChecked, bool isMuted = false)
        {
            MultipliedGames = multipliedGames;
            ExcludedGames = excludedGames;
            IsCheckboxChecked = isCheckboxChecked;
            IsMuted = isMuted;
        }

        public SaveState()
        {
            
        }
    }
}