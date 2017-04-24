using System;
using System.Collections.Generic;

namespace AmmySidekick
{
    public class ListenerParserEventArgs : EventArgs
    {
        public List<Message> Messages { get; set; }
    }
}