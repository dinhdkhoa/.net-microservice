using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Automatonymous;

namespace Play.Trading.Service.StateMachines
{
    public class PurchaseState : SagaStateMachineInstance
    {
        public Guid CorrelationId { get ; set ; }
    }
}