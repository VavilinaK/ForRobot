using System;

namespace ForRobot.Libr.Behavior
{
    public class FindElementByTagMessage
    {
        public string TagProperty { get; private set; }

        public FindElementByTagMessage(string tagProperty)
        {
            this.TagProperty = tagProperty;
        }
    }
}
