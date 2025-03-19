using System;

namespace ForRobot.Libr.Behavior
{
    public class CollapedLayoutAnchorableMessage
    {
        public string ContentId { get; private set; }

        public CollapedLayoutAnchorableMessage(string contentId)
        {
            this.ContentId = contentId;
        }
    }
}
