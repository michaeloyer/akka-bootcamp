using Akka.Actor;
using System;

namespace WinTail
{
    /// <summary>
    /// Actor responsible for serializing message writes to the console.
    /// (write one message at a time, champ :)
    /// </summary>
    class ConsoleWriterActor : UntypedActor
    {
        protected override void OnReceive(object message)
        {
            if (message is Messages.InputError inputErrorMessage)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(inputErrorMessage.Reason);
            }
            else if (message is Messages.InputSuccess inputSuccessMessage)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine(inputSuccessMessage.Reason);
            }
            else
            {
                Console.Write(message);
            }

            Console.ResetColor();
        }
    }
}
