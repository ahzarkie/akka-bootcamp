using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Akka.Actor;

namespace WinTail
{
    public class TailCoordinatorActor : UntypedActor
    {
        #region Message types

        ///<sumaary>
        ///Start tailing the file at user-specified path.
        ///</sumaary>
        public class StartTail
        {
            public StartTail(string filepath, IActorRef reporterActor)
            {
                FilePath = filepath;
                ReporterActor = reporterActor;
            }

            public string FilePath { get; private set; }

            public IActorRef ReporterActor { get; private set; }
        }

        ///<summary>
        ///stop tailing the file at user-specific path.
        ///</summary>
        public class StopTail
        {
            public StopTail(string filepath)
            {
                FilePath = filepath;
            }

            public string FilePath { get; private set; }
        }

        #endregion

        protected override void OnReceive(object message)
        {
            if (message is StartTail)
            {
                var msg = message as StartTail;
                //creating TailActor as a child of TailActor Coordinator
                Context.ActorOf(Props.Create(
                    () => new TailActor(msg.ReporterActor, msg.FilePath)));
            }
        }

        protected override SupervisorStrategy SupervisorStrategy()
        {
            return new OneForOneStrategy(
                10, // max retries
                TimeSpan.FromSeconds(30), // withinTimeRange
                x => //locationOnlyDecider 
                {
                    // Maybe we consider ArithmeticException to not be application critical
                    // so we just ignore the error and keep going.
                    if (x is ArithmeticException) return Directive.Resume;

                    //Error that we cannot recover from, stop the failing actor
                    else if (x is NotSupportedException) return Directive.Stop;

                    // in all other cases, just restart the failing actor
                    else return Directive.Resume;
                });
        }
    }
}
