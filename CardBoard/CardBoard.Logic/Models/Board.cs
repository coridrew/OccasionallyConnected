﻿using Assisticant.Collections;
using Assisticant.Fields;
using CardBoard.Messaging;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CardBoard.Models
{
    public class Board : IMessageHandler
    {

        private static readonly MessageDispatcher<Board> _dispatcher = new MessageDispatcher<Board>()
            .On("CardCreated", (o, m) => o.HandleCardCreatedMessage(m))
            .On("CardDeleted", (o, m) => o.HandleCardDeletedMessage(m));

        private Observable<string> _name = new Observable<string>("Pluralsight");
        private ObservableList<Card> _cards = new ObservableList<Card>();

        public void HandleMessage(Message message)
        {
            _dispatcher.Dispatch(this, message);
        }

        public void HandleAllMessages(IEnumerable<Message> messages)
        {
            var cardsDeleted = messages
                .Where(m => m.Type == "CardDeleted")
                .Select(m => Guid.Parse(m.Body.CardId))
                .ToLookup(g => g);
            var cardsCreated = messages
                .Where(m => m.Type == "CardCreated")
                .Select(m => Guid.Parse(m.Body.CardId))
                .Where(g => !cardsDeleted.Contains(g))
                .Select(g => new Card(g));

            _cards.AddRange(cardsCreated);
        }

        public string Name
        {
            get { return _name; }
        }

        public IEnumerable<Card> Cards
        {
            get { return _cards; }
        }

        public Message CreateCard(Guid cardId)
        {
            return Message.CreateMessage(
                "CardCreated",
                Guid.Empty,
                new
                {
                    CardId = cardId
                });
        }

        public Message DeleteCard(Guid cardId)
        {
            return Message.CreateMessage(
                "CardDeleted",
                Guid.Empty,
                new
                {
                    CardId = cardId
                });
        }

        private void HandleCardCreatedMessage(Message message)
        {
            Guid cardId = Guid.Parse(message.Body.CardId);
            if (!_cards.Any(c => c.CardId == cardId))
                _cards.Add(new Card(cardId));
        }

        private void HandleCardDeletedMessage(Message message)
        {
            Guid cardId = Guid.Parse(message.Body.CardId);
            _cards.RemoveAll(c => c.CardId == cardId);
        }

        public Guid GetObjectId()
        {
            return Guid.Empty;
        }
    }
}
