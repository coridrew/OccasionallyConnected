﻿using CardBoard.Messages;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CardBoard.Models
{
    public class Card
    {
        private readonly Guid _cardId;

        private Mutable<string> _text = new Mutable<string>();
        private Mutable<Column> _column = new Mutable<Column>();

        public Card(Guid cardId)
        {
            _cardId = cardId;
        }

        public Guid CardId
        {
            get { return _cardId; }
        }

        public IEnumerable<Candidate<string>> Text
        {
            get { return _text.Candidates; }
        }

        public Message SetText(string text)
        {
            return _text.CreateMessage("CardTextChanged", _cardId, text);
        }

        public IEnumerable<Candidate<Column>> Column
        {
            get { return _column.Candidates; }
        }

        public Message MoveTo(Column column)
        {
            return _column.CreateMessage("CardMoved", _cardId, column);
        }

        public void HandleCardTextChanged(Message message)
        {
            _text.HandleMessage(message);
        }

        public void HandleCardMoved(Message message)
        {
            _column.HandleMessage(message);
        }
    }
}
