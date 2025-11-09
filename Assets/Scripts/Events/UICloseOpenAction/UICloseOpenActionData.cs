using System;

namespace Events.UICloseOpenAction
{
    public enum WindowType
    {
        All
    }

    public enum ActionType
    {
        Open,
        Close,
        Reverse
    }

    [Serializable]
    public struct UICloseOpenActionData
    {
        public ActionType action;
        public WindowType windowType;
    }
}