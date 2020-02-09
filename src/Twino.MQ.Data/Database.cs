using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Twino.Protocols.TMQ;

namespace Twino.MQ.Data
{
    /// <summary>
    /// Messaging Queue database.
    /// Keeps only one queue data, insert and delete operations are supported, update is not supported.
    /// </summary>
    public class Database
    {
        #region Properties

        /// <summary>
        /// IO Locker
        /// </summary>
        private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);
        
        /// <summary>
        /// Database file message serializer
        /// </summary>
        private readonly DataMessageSerializer _serializer = new DataMessageSerializer();
        
        /// <summary>
        /// Database shrink manager
        /// </summary>
        private readonly ShrinkManager _shrinkManager;

        /// <summary>
        /// Recently deleted message id list
        /// </summary>
        private readonly List<string> _deletedMessages = new List<string>();
        
        /// <summary>
        /// All messages in queue
        /// </summary>
        private readonly Dictionary<string, TmqMessage> _messages = new Dictionary<string, TmqMessage>(StringComparer.InvariantCultureIgnoreCase);

        /// <summary>
        /// Database file
        /// </summary>
        public DatabaseFile File { get; }
        
        /// <summary>
        /// Database options.
        /// Applied once when database Open method is called.
        /// </summary>
        public DatabaseOptions Options { get; }

        #endregion

        #region Open - Close

        /// <summary>
        /// Creates new queue database
        /// </summary>
        public Database(DatabaseOptions options)
        {
            Options = options;
            File = new DatabaseFile(this);
            _shrinkManager = new ShrinkManager(this);
        }

        /// <summary>
        /// Opens database connection
        /// </summary>
        public async Task Open()
        {
            await File.Open();
            await Load();

            if (_deletedMessages.Count > 0)
                await _shrinkManager.FullShrink(_messages, _deletedMessages);

            if (Options.AutoShrink)
                _shrinkManager.Start(Options.ShrinkInterval);
        }

        /// <summary>
        /// Loads all data into memory from disk
        /// </summary>
        private async Task Load()
        {
            await WaitForLock();
            try
            {
                Stream stream = File.GetStream();
                stream.Seek(0, SeekOrigin.Begin);
                await using MemoryStream ms = new MemoryStream();
                await stream.CopyToAsync(ms);
                ms.Position = 0;

                while (ms.Position < ms.Length)
                {
                    DataMessage message = await _serializer.Read(ms);
                    switch (message.Type)
                    {
                        case DataType.Insert:
                            _messages.Add(message.Id, message.Message);
                            break;

                        case DataType.Delete:
                            _deletedMessages.Add(message.Id);
                            break;
                    }
                }
            }
            finally
            {
                ReleaseLock();
            }
        }

        /// <summary>
        /// Closes the database
        /// </summary>
        public async Task Close()
        {
            _shrinkManager.Stop();
            await File.Close();
        }

        #endregion

        #region Management

        /// <summary>
        /// Removes database and deletes file
        /// </summary>
        /// <returns></returns>
        public async Task<bool> RemoveDatabase()
        {
            await File.Close();
            return await File.Delete();
        }

        /// <summary>
        /// Shrinks database transactions and reduces file size on disk
        /// </summary>
        /// <returns></returns>
        public async Task<bool> Shrink()
        {
            Stream stream = File.GetStream();
            await WaitForLock();
            long position;
            try
            {
                position = stream.Position;
            }
            finally
            {
                ReleaseLock();
            }

            List<string> msgs;
            lock (_deletedMessages)
                msgs = new List<string>(_deletedMessages);

            bool success = await _shrinkManager.Shrink(position, msgs);

            //sync deleted messages array
            if (success)
            {
                if (_shrinkManager.DeletedMessages.Count > 0)
                    lock (_deletedMessages)
                        _deletedMessages.RemoveAll(x => _shrinkManager.DeletedMessages.Contains(x));
            }

            return success;
        }

        #endregion

        #region Lock

        /// <summary>
        /// Waits for IO lock
        /// </summary>
        internal async Task WaitForLock()
        {
            await _semaphore.WaitAsync();
        }

        /// <summary>
        /// Releases IO lock
        /// </summary>
        internal void ReleaseLock()
        {
            _semaphore.Release();
        }

        #endregion

        #region Insert - Delete - List

        /// <summary>
        /// Inserts new message to database
        /// </summary>
        /// <returns></returns>
        public async Task<bool> Insert(TmqMessage message)
        {
            if (string.IsNullOrEmpty(message.MessageId))
                return false;

            await WaitForLock();
            try
            {
                if (_messages.ContainsKey(message.MessageId))
                    return false;

                _messages.Add(message.MessageId, message);
                Stream stream = File.GetStream();
                await _serializer.Write(stream, message);

                if (Options.InstantFlush)
                    await stream.FlushAsync();
                else
                    File.FlushRequired = true;

                return true;
            }
            catch
            {
                return false;
            }
            finally
            {
                ReleaseLock();
            }
        }

        /// <summary>
        /// Deletes the message from database
        /// </summary>
        public async Task<bool> Delete(TmqMessage message)
        {
            if (string.IsNullOrEmpty(message.MessageId))
                return false;

            return await Delete(message.MessageId);
        }

        /// <summary>
        /// Deletes the message from database
        /// </summary>
        public async Task<bool> Delete(string message)
        {
            await WaitForLock();
            try
            {
                Stream stream = File.GetStream();
                await _serializer.WriteDelete(stream, message);
                _messages.Remove(message);
                _deletedMessages.Add(message);

                if (!_shrinkManager.ShrinkRequired)
                    _shrinkManager.ShrinkRequired = true;

                if (Options.InstantFlush)
                    await stream.FlushAsync();
                else
                    File.FlushRequired = true;

                return true;
            }
            catch
            {
                return false;
            }
            finally
            {
                ReleaseLock();
            }
        }

        /// <summary>
        /// Lists all messages in database
        /// </summary>
        /// <returns></returns>
        public async Task<Dictionary<string, TmqMessage>> List()
        {
            Dictionary<string, TmqMessage> messages;
            await WaitForLock();
            try
            {
                messages = new Dictionary<string, TmqMessage>(_messages);
            }
            finally
            {
                ReleaseLock();
            }

            return messages;
        }

        /// <summary>
        /// Gets message count in database
        /// </summary>
        public int MessageCount()
        {
            return _messages.Count;
        }

        #endregion
    }
}