﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CardBoard.Messaging
{
    public class NoOpMessageStore : IMessageStore
    {
        public void Save(Message message)
        {
        }
    }
}
