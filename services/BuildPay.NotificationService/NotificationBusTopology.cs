using BuildPay.Contracts;
using MassTransit;

namespace BuildPay.NotificationService;

public static class NotificationBusTopology
{
    public static void Configure(IRabbitMqBusFactoryConfigurator cfg)
    {
        cfg.Message<IdentityContractorVerified>(m => m.SetEntityName("identity.events"));
        cfg.Message<IdentityContractorRejected>(m => m.SetEntityName("identity.events"));
        cfg.Message<ProjectMilestoneCompleted>(m => m.SetEntityName("project.events"));
        cfg.Message<PaymentMilestoneReleased>(m => m.SetEntityName("payment.events"));
        cfg.Message<OrderCreated>(m => m.SetEntityName("order.events"));
        cfg.Message<DeliveryUpdated>(m => m.SetEntityName("delivery.events"));
        cfg.Message<DeliveryCompleted>(m => m.SetEntityName("delivery.events"));
    }
}

