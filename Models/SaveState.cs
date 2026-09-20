using System;
using System.Collections.Generic;

namespace WheelSpinner.Models
{
    public class SaveState
    {
        public List<MultipliedGame> MultipliedGames { get; set; }
        public List<Guid> ExcludedGames { get; set; }

        public SaveState(List<MultipliedGame> multipliedGames, List<Guid> excludedGames)
        {
            MultipliedGames = multipliedGames;
            ExcludedGames = excludedGames;
        }
    }
}