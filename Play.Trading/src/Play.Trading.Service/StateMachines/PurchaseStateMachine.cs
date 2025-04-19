using System;
using Automatonymous;

namespace Play.Trading.Service.StateMachines
{
    public class PurchaseStateMachine: MassTransitStateMachine<PurchaseState>
    {
        public State Accepted {get; }
        public State ItemsGranted {get; }
        public State Completed {get; }
        public State Faulted {get; }
        public Event<PurchaseRequested> PurchaseRequested { get; }
        public PurchaseStateMachine()
        {
            InstanceState(state => state.CurrentState);
            ConfigureEvents();
            ConfigureInitState();
        }

        private void ConfigureEvents()
        {
            Event(() => PurchaseRequested);
        }

        private void ConfigureInitState()
        {
            Initially(
                When(PurchaseRequested)
                .Then(context => 
                {
                    context.Instance.UserId = context.Data.UserId;
                    context.Instance.ItemId = context.Data.ItemId;
                    context.Instance.Quantity = context.Data.Quantity;
                    context.Instance.ReceivedAt = DateTimeOffset.UtcNow;
                    context.Instance.UpdatedAt = context.Instance.ReceivedAt;
                })
                .TransitionTo(Accepted)
            );
        }
    }
}