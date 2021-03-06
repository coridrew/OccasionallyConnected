﻿using CardBoard.Tasks;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Windows.Storage;

namespace CardBoard.Messaging
{
    public class FileMessageQueue : Process, IMessageQueue
    {
        private readonly string _folderName;

        private StorageFile _messageQueueFile;
        private JsonSerializer _serializer = new JsonSerializer();

        public FileMessageQueue(string folderName)
        {
            _folderName = folderName;
        }

        public Task<ImmutableList<Message>> LoadAsync()
        {
            var completion = new TaskCompletionSource<ImmutableList<Message>>();
            Perform(() => LoadInternalAsync(completion));
            return completion.Task;
        }

        public void Enqueue(Message message)
        {
            Perform(() => EnqueueInternalAsync(message));
        }

        public void Confirm(Message message)
        {
            Perform(() => ConfirmInternalAsync(message));
        }

        private async Task LoadInternalAsync(TaskCompletionSource<ImmutableList<Message>> completion)
        {
            try
            {
                await CreateFileAsync();
                var messages = await ReadMessagesAsync();
                var result = messages
                    .Select(m => Message.FromMemento(m))
                    .ToImmutableList();
                completion.SetResult(result);
            }
            catch (Exception ex)
            {
                completion.SetException(ex);
            }
        }

        private async Task EnqueueInternalAsync(Message message)
        {
            await CreateFileAsync();
            var messageList = await ReadMessagesAsync();

            var memento = message.GetMemento();
            messageList.Add(memento);

            await WriteMessagesAsync(messageList);
        }

        private async Task ConfirmInternalAsync(Message message)
        {
            await CreateFileAsync();
            var messageList = await ReadMessagesAsync();

            string hash = message.Hash.ToString();
            messageList.RemoveAll(o => o.Hash == hash);

            await WriteMessagesAsync(messageList);
        }

        private async Task CreateFileAsync()
        {
            if (_messageQueueFile == null)
            {
                var cardBoardFolder = await ApplicationData.Current.LocalFolder
                    .CreateFolderAsync(_folderName, CreationCollisionOption.OpenIfExists);
                _messageQueueFile = await cardBoardFolder
                    .CreateFileAsync("MessageQueue.json", CreationCollisionOption.OpenIfExists);
            }
        }

        private async Task<List<MessageMemento>> ReadMessagesAsync()
        {
            List<MessageMemento> messageList;
            var inputStream = await _messageQueueFile.OpenStreamForReadAsync();
            using (JsonReader reader = new JsonTextReader(new StreamReader(inputStream)))
            {
                messageList = _serializer.Deserialize<List<MessageMemento>>(reader);
            }

            if (messageList == null)
                messageList = new List<MessageMemento>();
            return messageList;
        }

        private async Task WriteMessagesAsync(List<MessageMemento> messageList)
        {
            var outputStream = await _messageQueueFile.OpenStreamForWriteAsync();
            using (JsonWriter writer = new JsonTextWriter(new StreamWriter(outputStream)))
            {
                _serializer.Serialize(writer, messageList);
            }
        }
    }
}
