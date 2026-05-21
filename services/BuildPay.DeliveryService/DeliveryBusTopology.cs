using BuildPay.Contracts;
using MassTransit;

namespace BuildPay.DeliveryService;

public static class DeliveryBusTopology
{
    public static void Configure(IRabbitMqBusFactoryConfigurator cfg)
    {
        cfg.Message<OrderCreated>(m => m.SetEntityName("order.events"));
        cfg.Message<DeliveryUpdated>(m => m.SetEntityName("delivery.events"));
        cfg.Message<DeliveryCompleted>(m => m.SetEntityName("delivery.events"));
    }
}

