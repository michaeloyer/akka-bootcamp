using Akka.Actor;
using System;
using System.IO;

namespace WinTail
{
    public class FileValidatorActor : UntypedActor
    {
        private readonly IActorRef _consoleWriterActor;
        private readonly IActorRef _tailCoordinatorActor;

        public FileValidatorActor(IActorRef consoleWriterActor,
            IActorRef tailCoordinatorActor)
        {
            _consoleWriterActor = consoleWriterActor ?? throw new ArgumentNullException(nameof(consoleWriterActor));
            _tailCoordinatorActor = tailCoordinatorActor ?? throw new ArgumentNullException(nameof(tailCoordinatorActor));
        }

        protected override void OnReceive(object message)
        {
            var msg = message as string;
            if (string.IsNullOrEmpty(msg))
            {
                // signal that the user needs to supply an input
                _consoleWriterActor.Tell(new Messages.NullInputError("Input was blank. Please try again.\n"));

                // tell sender to continue doing its thing (whatever that may be,
                // this actor doesn't care)
                Sender.Tell(new Messages.ContinueProcessing());
            }
            else
            {
                if (IsFileUri(msg))
                {
                    // signal successful input
                    _consoleWriterActor.Tell(new Messages.InputSuccess($"Starting processing for {msg}"));

                    //start coordinator
                    _tailCoordinatorActor.Tell(new TailCoordinatorActor.StartTail(msg, _consoleWriterActor));
                }
                else
                {
                    // signal that input was bad
                    _consoleWriterActor.Tell(new Messages.ValidationError(
                        $"{msg} is not an existing URI on disk."));

                    // tell sender to continue doing its thing (whatever that
                    // may be, this actor doesn't care)
                    Sender.Tell(new Messages.ContinueProcessing());
                }
            }
        }

        private bool IsFileUri(string path)
        {
            return File.Exists(path);
        }
    }
}
