using BuildPay.Contracts;
using MassTransit;

namespace BuildPay.MarketplaceService;

public static class MarketplaceBusTopology
{
    public static void Configure(IRabbitMqBusFactoryConfigurator cfg)
    {
        cfg.Message<OrderCreated>(m => m.SetEntityName("order.events"));
    }
}

