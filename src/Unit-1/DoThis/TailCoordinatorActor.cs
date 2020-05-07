using Akka.Actor;
using System;

namespace WinTail
{
    public class TailCoordinatorActor : UntypedActor
    {
        #region Message types

        public class StartTail
        {
            public StartTail(string filePath, IActorRef reporterActor)
            {
                FilePath = filePath ?? throw new ArgumentNullException(nameof(reporterActor));
                ReporterActor = reporterActor ?? throw new ArgumentNullException(nameof(reporterActor));
            }

            public string FilePath { get; }
            public IActorRef ReporterActor { get; }
        }

        public class StopTail
        {
            public StopTail(string filePath)
            {
                FilePath = filePath ?? throw new ArgumentNullException(nameof(filePath));
            }

            public string FilePath { get; }
        }

        #endregion

        protected override void OnReceive(object message)
        {
            if (message is StartTail startMessage)
            {
                Context.ActorOf(Props.Create(() =>
                    new TailActor(startMessage.ReporterActor, startMessage.FilePath)));
            }
        }

        protected override SupervisorStrategy SupervisorStrategy()
        {
            return new OneForOneStrategy(
                maxNrOfRetries: 10,
                withinTimeRange: TimeSpan.FromSeconds(30),
                localOnlyDecider: x =>
                {
                    //Maybe we consider ArithmeticException to not be application critical
                    //so we just ignore the error and keep going.
                    if (x is ArithmeticException) return Directive.Resume;

                    //Error that we cannot recover from, stop the failing actor
                    else if (x is NotSupportedException) return Directive.Stop;

                    //In all other cases, just restart the failing actor
                    else return Directive.Restart;
                });
        }
    }
}
