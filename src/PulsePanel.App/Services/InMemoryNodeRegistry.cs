using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using PulsePanel.App.Models;

namespace PulsePanel.App.Services
{
    public sealed class InMemoryNodeRegistry : INodeRegistry
    {
        private readonly ConcurrentDictionary<string, NodeInfo> _nodes = new();

        public IEnumerable<NodeInfo> GetAll() => _nodes.Values.OrderBy(n => n.Name);
        public NodeInfo? GetById(string id) => _nodes.TryGetValue(id, out var n) ? n : null;
        public void Add(NodeInfo node) => _nodes[node.Id] = node;
        public void Remove(string id) => _nodes.TryRemove(id, out _);
        public void Update(NodeInfo node) => _nodes[node.Id] = node;
    }
}