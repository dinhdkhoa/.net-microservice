using System;
using Automatonymous;
using Play.Trading.Service.Activities;
using Play.Trading.Service.Contracts;
using static Play.Identity.Contracts.Contracts;
using static Play.Inventory.Contracts.Contracts;

namespace Play.Trading.Service.StateMachines
{
    public class PurchaseStateMachine: MassTransitStateMachine<PurchaseState>
    {
        public State Accepted {get; }
        public State ItemsGranted {get; }
        public State Completed {get; }
        public State Faulted {get; }
        public Event<PurchaseRequested> PurchaseRequested { get; }
        public Event<GetPurchaseState> GetPurchaseState { get; }
        public Event<InventoryItemGranted> InventoryItemGranted { get; }
        public Event<GilDebited> GilDebited { get; }
        public PurchaseStateMachine()
        {
            InstanceState(state => state.CurrentState);
            ConfigureEvents();
            ConfigureInitState();
            ConfigureAny();
            ConfigureAccepted();
            ConfigureItemsGranted();
        }

        private void ConfigureEvents()
        {
            Event(() => PurchaseRequested);
            Event(() => GetPurchaseState);
            Event(() => GilDebited);
            Event(() => InventoryItemGranted);
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
                .Activity(x => x.OfType<CalculatePurchaseTotalActivity>())
                .Send(context => new GrantedItem(
                    context.Instance.UserId,
                    context.Instance.ItemId,
                    context.Instance.Quantity,
                    context.Instance.CorrelationId
                ))
                .TransitionTo(Accepted)
                .Catch<Exception>(ex => ex.
                        Then(context =>
                        {
                            context.Instance.ErrorMessage = context.Exception.Message;
                            context.Instance.UpdatedAt = DateTimeOffset.UtcNow;
                        })
                        .TransitionTo(Faulted)
            ));
        }

        private void ConfigureAccepted()
        {
            During(Accepted,
                When(InventoryItemGranted)
                .Then(context => {
                    context.Instance.UpdatedAt = DateTimeOffset.UtcNow;
                })
                .Send(context => new DebitGil(
                    context.Instance.UserId,
                    context.Instance.PurchaseTotal.Value,
                    context.Instance.CorrelationId
                ))
                .TransitionTo(ItemsGranted));
        }
        private void ConfigureItemsGranted()
        {
            During(ItemsGranted,
                When(GilDebited)
                .Then(context => {
                    context.Instance.UpdatedAt = DateTimeOffset.UtcNow;
                })
                .TransitionTo(Completed));
        }

        private void ConfigureAny()
        {
            DuringAny(
                When(GetPurchaseState)
                .Respond(x => x.Instance)
            );
        }
    }
}