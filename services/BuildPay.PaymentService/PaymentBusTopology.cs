using BuildPay.Contracts;
using MassTransit;

namespace BuildPay.PaymentService;

public static class PaymentBusTopology
{
    public static void Configure(IRabbitMqBusFactoryConfigurator cfg)
    {
        cfg.Message<ProjectMilestoneCompleted>(m => m.SetEntityName("project.events"));
        cfg.Message<PaymentMilestoneReleased>(m => m.SetEntityName("payment.events"));
    }
}

