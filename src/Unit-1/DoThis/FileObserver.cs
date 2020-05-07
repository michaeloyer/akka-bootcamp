using Akka.Actor;
using System;
using System.IO;

namespace WinTail
{
    internal class FileObserver : IDisposable
    {
        private readonly IActorRef _tailActor;
        private readonly string _absoluteFilePath;
        private readonly string _fileDir;
        private readonly string _fileNameOnly;
        private FileSystemWatcher _watcher;

        public FileObserver(IActorRef tailActor, string absoluteFilePath)
        {
            _tailActor = tailActor ?? throw new ArgumentNullException(nameof(tailActor));
            _absoluteFilePath = absoluteFilePath ?? throw new ArgumentNullException(nameof(absoluteFilePath));
            _fileDir = Path.GetDirectoryName(absoluteFilePath);
            _fileNameOnly = Path.GetFileName(absoluteFilePath);
        }

        public void Start()
        {
            _watcher = new FileSystemWatcher(_fileDir, _fileNameOnly);
            _watcher.NotifyFilter = NotifyFilters.FileName | NotifyFilters.LastWrite;

            _watcher.Changed += OnFileChanged;
            _watcher.Error += OnFileError;

            _watcher.EnableRaisingEvents = true;
        }

        public void Dispose()
        {
            _watcher.Dispose();
        }

        private void OnFileChanged(object sender, FileSystemEventArgs e)
        {
            _tailActor.Tell(new TailActor.FileWrite(e.Name), ActorRefs.NoSender);
        }

        private void OnFileError(object sender, ErrorEventArgs e)
        {
            _tailActor.Tell(new TailActor.FileError(_fileNameOnly,
                e.GetException().Message),
                ActorRefs.NoSender);
        }
    }
}