﻿using CardBoard.Tasks;
using Newtonsoft.Json;
using System;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using Windows.Web.Http;

namespace CardBoard.Messaging
{
    public class HttpMessagePump : Process, IMessagePump
    {
        private readonly Uri _uri;
        private readonly IMessageQueue _messageQueue;

        private ImmutableQueue<Message> _queue = ImmutableQueue<Message>.Empty;

        public HttpMessagePump(Uri uri, IMessageQueue messageQueue)
        {
            _uri = uri;
            _messageQueue = messageQueue;
        }

        public void Enqueue(Message message)
        {
            lock (this)
            {
                _queue = _queue.Enqueue(message);
            }
            Perform(() => SendAndReceiveMessagesInternalAsync());
        }

        public void SendAllMessages(ImmutableList<Message> messages)
        {
            lock (this)
            {
                _queue = messages.Aggregate(_queue,
                    (queue, message) => _queue.Enqueue(message));
            }
            Perform(() => SendAndReceiveMessagesInternalAsync());
        }

        public void SendAndReceiveMessages()
        {
            Perform(() => SendAndReceiveMessagesInternalAsync());
        }

        private async Task SendAndReceiveMessagesInternalAsync()
        {
            using (HttpClient client = new HttpClient())
            {
                while (true)
                {
                    var queue = _queue;
                    if (queue.IsEmpty)
                        return;
                    var message = queue.Peek();

                    var memento = message.GetMemento();
                    string messageJson = JsonConvert.SerializeObject(memento);

                    IHttpContent content = new HttpStringContent(messageJson);
                    var result = await client.PostAsync(_uri, content);
                    if (!result.IsSuccessStatusCode)
                        throw new CommunicationException(result.ReasonPhrase);

                    lock (this)
                    {
                        _queue = _queue.Dequeue();
                    }
                    _messageQueue.Confirm(message);
                }
            }
        }
    }
}
