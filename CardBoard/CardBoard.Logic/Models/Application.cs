﻿using CardBoard.Messaging;

namespace CardBoard.Models
{
    public class Application
    {
        private Board _board = new Board();

        public Board Board
        {
            get { return _board; }
        }

        public void HandleMessage(Message message)
        {
            throw new System.NotImplementedException();
        }
    }
}
