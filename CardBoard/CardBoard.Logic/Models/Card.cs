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

        public void SetText(string text)
        {
            var message = _text.CreateMessage("CardTextChanged", _cardId, text);
            HandleCardTextChanged(message);
        }

        public IEnumerable<Candidate<Column>> Column
        {
            get { return _column.Candidates; }
        }

        public void MoveTo(Column column)
        {
            var message = _column.CreateMessage("CardMoved", _cardId, column);
            HandleCardMoved(message);
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
