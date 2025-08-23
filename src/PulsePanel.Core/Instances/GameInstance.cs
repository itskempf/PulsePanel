using System;

namespace PulsePanel.Core.Instances
{
    public class GameInstance
    {
        public Guid Id { get; }

        public GameInstance(InstanceConfig config, object blueprint)
        {
            Id = Guid.NewGuid();
        }

        public void Start() { }
        public void Stop() { }
    }
}
