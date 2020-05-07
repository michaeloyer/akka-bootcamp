using Akka.Actor;
using System;
using System.IO;
using System.Text;

namespace WinTail
{
    /// <summary>
    /// Monitors the file at <see cref="_filePath"/> for changes and sends
    /// file updates to console.
    /// </summary>
    public class TailActor : UntypedActor
    {
        #region Message types

        public class FileWrite
        {
            public FileWrite(string fileName)
            {
                FileName = fileName;
            }

            public string FileName { get; }
        }

        public class FileError
        {
            public FileError(string fileName, string reason)
            {
                FileName = fileName ?? throw new ArgumentNullException(nameof(fileName));
                Reason = reason ?? throw new ArgumentNullException(nameof(reason));
            }

            public string FileName { get; }
            public string Reason { get; }
        }

        public class InitialRead
        {
            public InitialRead(string fileName, string text)
            {
                FileName = fileName ?? throw new ArgumentNullException(nameof(fileName));
                Text = text ?? throw new ArgumentNullException(nameof(text));
            }

            public string FileName { get; }
            public string Text { get; }
        }

        #endregion

        private readonly string _filePath;
        private readonly IActorRef _reporterActor;
        private readonly FileObserver _observer;
        private readonly Stream _fileStream;
        private readonly StreamReader _fileStreamReader;

        public TailActor(IActorRef reporterActor, string filePath)
        {
            _reporterActor = reporterActor ?? throw new ArgumentNullException(nameof(reporterActor));
            _filePath = filePath ?? throw new ArgumentNullException(nameof(filePath));

            // start watching file for changes
            _observer = new FileObserver(Self, Path.GetFullPath(_filePath));
            _observer.Start();

            // open the file stream with shared read/write permissions
            // (so file can be written to while open)
            _fileStream = new FileStream(Path.GetFullPath(_filePath),
                FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            _fileStreamReader = new StreamReader(_fileStream, Encoding.UTF8);

            // read the initial contents of the file and send it to console as first msg
            var text = _fileStreamReader.ReadToEnd();
            Self.Tell(new InitialRead(_filePath, text));
        }

        protected override void OnReceive(object message)
        {
            if (message is FileWrite)
            {
                var text = _fileStreamReader.ReadToEnd();
                if (!string.IsNullOrEmpty(text))
                    _reporterActor.Tell(text);
            }
            else if (message is FileError fileErrorMessage)
            {
                _reporterActor.Tell($"Tail error: {fileErrorMessage.Reason}");
            }
            else if (message is InitialRead initialReadMessage)
            {
                _reporterActor.Tell(initialReadMessage.Text);
            }
        }
    }
}
