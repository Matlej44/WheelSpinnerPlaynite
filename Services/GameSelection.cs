using System;
using System.Collections.Generic;
using System.Linq;
using Playnite.SDK;
using Playnite.SDK.Models;
using WheelSpinner.Models;

namespace WheelSpinner.Services
{
     
     
     
     
    public class GameSelection
    {
        private readonly IPlayniteAPI _api;
        private readonly Dictionary<Guid, int> _weights;
        private readonly HashSet<Guid> _excluded;
        private List<Game> _games = new List<Game>();

        public GameSelection(IPlayniteAPI api, SaveState saveState)
        {
            _api = api;
            _weights = saveState?.MultipliedGames ?? new Dictionary<Guid, int>();
            _excluded = saveState?.ExcludedGames ?? new HashSet<Guid>();
            UseFilteredView = saveState?.IsCheckboxChecked ?? true;

            LoadGames();
        }

         
        public bool UseFilteredView { get; private set; }

        public IReadOnlyList<Game> Games => _games;

        public void SetFilteredView(bool useFilteredView)
        {
            UseFilteredView = useFilteredView;
            LoadGames();
        }

        public bool IsExcluded(Guid gameId) => _excluded.Contains(gameId);

        public void Exclude(Guid gameId) => _excluded.Add(gameId);

        public void Include(Guid gameId) => _excluded.Remove(gameId);

        public int GetWeight(Guid gameId)
        {
            return _weights.TryGetValue(gameId, out var weight) ? weight : 1;
        }

        public void SetWeight(Guid gameId, int weight)
        {
            if (weight <= 1)
                _weights.Remove(gameId);
            else
                _weights[gameId] = weight;
        }

        public void Reset()
        {
            _weights.Clear();
            _excluded.Clear();
        }

        public List<WheelItem> BuildWheelItems()
        {
            return _games
                .Where(game => !_excluded.Contains(game.Id))
                .Select(game => new WheelItem(game, GetWeight(game.Id)))
                .ToList();
        }

        public SaveState ToSaveState()
        {
            return new SaveState(_weights, _excluded, UseFilteredView);
        }

        private void LoadGames()
        {
            _games = UseFilteredView
                ? _api.MainView.FilteredGames.ToList()
                : _api.Database.Games.ToList();
        }
    }
}
