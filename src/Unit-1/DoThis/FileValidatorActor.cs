using Akka.Actor;
using System.IO;

namespace WinTail
{
    public class FileValidatorActor : UntypedActor
    {
        private readonly IActorRef _consoleWriterActor;

        public FileValidatorActor(IActorRef consoleWriterActor)
        {
            _consoleWriterActor = consoleWriterActor;
        }

        protected override void OnReceive(object message)
        {
            var msg = message as string;
            if (string.IsNullOrEmpty(msg))
            {
                //signal that the user needs to supply an input
                _consoleWriterActor.Tell(new Messages.NullInputError("Input was blank." +
                    "Please try again.\n"));

                Sender.Tell(new Messages.ContinueProcessing());
            }
            else
            {
                var valid = IsFileUri(msg);
                if (valid)
                {
                    _consoleWriterActor.Tell(new Messages.InputSuccess(
                        string.Format("Starting processing for {0}", msg)));

                    Context.ActorSelection("akka://MyActorSystem/user/tailCoordinatorActor").Tell(
                        new TailCoordinatorActor.StartTail(msg, _consoleWriterActor));
                }
                else
                {
                    _consoleWriterActor.Tell(new Messages.ValidationError(
                        string.Format("{0} is not an existing URI on disk.", msg)));

                    Sender.Tell(new Messages.ContinueProcessing());
                }
            }

            //tell sender to continue doing its thing
            //(whatever that may be, this actor doesn't care)
            Sender.Tell(new Messages.ContinueProcessing());
        }

        /// <summary>
        /// Determines if the message received is valid.
        /// Checks if number of chars in message received is even.
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        private bool IsFileUri(string path)
        {
            return File.Exists(path);
        }
    }
}
