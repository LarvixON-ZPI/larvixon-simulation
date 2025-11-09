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
        Close
    }

    [Serializable]
    public struct UICloseOpenActionData
    {
        public ActionType action;
        public WindowType windowType;
    }
}