using System.Collections.Generic;
using PulsePanel.App.Models;

namespace PulsePanel.App.Services
{
    public interface INodeRegistry
    {
        IEnumerable<NodeInfo> GetAll();
        NodeInfo? GetById(string id);
        void Add(NodeInfo node);
        void Remove(string id);
        void Update(NodeInfo node);
    }
}