using System;

namespace MultiplayerARPG
{
    public abstract class BaseGameDatabaseSection : IComparable
    {
        public virtual int Order { get { return 0; } }
        public abstract string MenuTitle { get; }
        public abstract void OnGUI(float width, float height);

        public int CompareTo(object obj)
        {
            if (obj == null)
                return 1;

            BaseGameDatabaseSection castedObj = obj as BaseGameDatabaseSection;
            if (castedObj != null)
                return Order.CompareTo(castedObj.Order);
            else
                throw new ArgumentException("Object is not a BaseGameDatabaseSection");
        }
    }
}
