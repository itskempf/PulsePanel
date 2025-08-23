using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using PulsePanel.App.Models;
using PulsePanel.App.Services;

namespace PulsePanel.App.ViewModels
{
    public partial class NodesAdminViewModel : ObservableObject
    {
        private readonly INodeRegistry _registry;

        [ObservableProperty] private string newNodeName = "";
        [ObservableProperty] private string newNodeUrl = "";
        [ObservableProperty] private NodeInfo? selectedNode;
        public ObservableCollection<NodeInfo> Nodes { get; } = new();

        public NodesAdminViewModel(INodeRegistry registry)
        {
            _registry = registry;
            foreach (var n in _registry.GetAll()) Nodes.Add(n);
        }

        [RelayCommand]
        private void Remove(NodeInfo? node)
        {
            if (node is null) return;
            _registry.Remove(node.Id);
            Nodes.Remove(node);
        }

        public void Add(string? apiKey)
        {
            var node = new NodeInfo { Name = NewNodeName, Url = NewNodeUrl, ApiKey = apiKey };
            _registry.Add(node);
            Nodes.Add(node);
            NewNodeName = ""; NewNodeUrl = "";
        }
    }
}