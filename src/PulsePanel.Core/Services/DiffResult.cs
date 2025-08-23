using System.Collections.Generic;
using DiffPlex.DiffBuilder.Model;

namespace PulsePanel.Core.Services
{
    public class DiffResult
    {
        public bool HasChanges { get; set; }
        public IList<DiffPiece> DiffLines { get; set; }
    }
}
