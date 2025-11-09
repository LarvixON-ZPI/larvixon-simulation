using System;
using Events.Signal;

namespace Context
{
    [Serializable]
    public struct SignalBinding
    {
        public GameSignalId id;
        public SignalEventChannel channel;
    }
}