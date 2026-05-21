using BuildPay.Contracts;
using MassTransit;

namespace BuildPay.ProjectService;

public static class ProjectBusTopology
{
    public static void Configure(IRabbitMqBusFactoryConfigurator cfg)
    {
        cfg.Message<IdentityContractorVerified>(m => m.SetEntityName("identity.events"));
        cfg.Message<ProjectMilestoneCompleted>(m => m.SetEntityName("project.events"));
        cfg.Message<ProjectMilestoneApproved>(m => m.SetEntityName("project.events"));
    }
}

