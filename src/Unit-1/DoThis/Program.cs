using Akka.Actor;
using System.Threading.Tasks;

namespace WinTail
{
    #region Program
    class Program
    {
        public static ActorSystem MyActorSystem;

        static async Task Main(string[] args)
        {
            // initialize MyActorSystem
            MyActorSystem = ActorSystem.Create(nameof(MyActorSystem));

            Props consoleWriterProps = Props.Create<ConsoleWriterActor>();
            IActorRef consoleWriterActor = MyActorSystem.ActorOf(consoleWriterProps, "consoleWriterActor");
            // make tailCoordinatorActor
            Props tailCoordinatorProps = Props.Create<TailCoordinatorActor>();
            IActorRef tailCoordinatorActor = MyActorSystem.ActorOf(tailCoordinatorProps, "tailCoordinatorActor");

            Props validationActorProps = Props.Create(() =>
                new FileValidatorActor(consoleWriterActor, tailCoordinatorActor));
            IActorRef validationActor = MyActorSystem.ActorOf(validationActorProps, "fileValidationActor");

            Props consoleReaderProps = Props.Create<ConsoleReaderActor>(validationActor);
            IActorRef consoleReaderActor = MyActorSystem.ActorOf(consoleReaderProps, "consoleReaderActor");

            // pass tailCoordinatorActor to fileValidatorActorProps (just adding one extra arg)
            // tell console reader to begin
            consoleReaderActor.Tell(ConsoleReaderActor.StartCommand);

            // blocks the main thread from exiting until the actor system is shut down
            await MyActorSystem.WhenTerminated;
        }
    }
    #endregion
}
