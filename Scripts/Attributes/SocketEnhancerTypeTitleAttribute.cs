using UnityEngine;

namespace MultiplayerARPG
{
    public class SocketEnhancerTypeTitleAttribute : PropertyAttribute
	{
		public int index;

		public SocketEnhancerTypeTitleAttribute(int index)
		{
			this.index = index;
		}
	}
}