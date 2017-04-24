using System;
using System.Collections.Generic;

namespace AmmySidekick
{
    internal class ListenerParserEventArgs : EventArgs
    {
        public List<Message> Messages { get; set; }
    }
}