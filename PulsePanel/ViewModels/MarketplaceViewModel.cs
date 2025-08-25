
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using PulsePanel.Core.Models;
using PulsePanel.Core.Services;

namespace PulsePanel.ViewModels
{
    public sealed class MarketplaceViewModel : ObservableObject
    {
        private readonly IBlueprintService _blueprints;
        public ObservableCollection<Blueprint> Items { get; } = new();
        public Blueprint? Selected { get; set; }
        public AsyncCommand Refresh { get; }
        public MarketplaceViewModel(IBlueprintService blueprints)
        {
            _blueprints = blueprints; Refresh = new AsyncCommand(LoadAsync);
        }
        public async Task LoadAsync()
        {
            var list = await _blueprints.GetBlueprintsAsync();
            Items.Clear(); foreach (var b in list.OrderBy(b => b.Name)) Items.Add(b);
        }
    }
}
