using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;
using PulsePanel.App.Models;
using PulsePanel.App.Services;

namespace PulsePanel.App.ViewModels
{
    public partial class NodesViewModel : ObservableObject
    {
        private readonly INodeRegistry _registry;

        [ObservableProperty] private ObservableCollection<NodeInfo> nodes = new();
        [ObservableProperty] private NodeInfo? selectedNode;

        public NodesViewModel(INodeRegistry registry)
        {
            _registry = registry;
            Nodes = new ObservableCollection<NodeInfo>(_registry.GetAll());
        }
    }
}