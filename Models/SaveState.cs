using System;
using System.Collections.Generic;

namespace WheelSpinner.Models
{
    public class SaveState
    {
        public List<MultipliedGame> MultipliedGames { get; set; }
        public HashSet<Guid> ExcludedGames { get; set; }
        public bool IsCheckboxChecked { get; set; } = true;

        public SaveState(List<MultipliedGame> multipliedGames, HashSet<Guid> excludedGames, bool isCheckboxChecked)
        {
            MultipliedGames = multipliedGames;
            ExcludedGames = excludedGames;
            IsCheckboxChecked = isCheckboxChecked;
        }

        public SaveState()
        {
            
        }
    }
}