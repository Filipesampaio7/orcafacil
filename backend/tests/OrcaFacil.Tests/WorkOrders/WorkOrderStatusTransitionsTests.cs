using OrcaFacil.Domain.Enums;
using Xunit;

namespace OrcaFacil.Tests.WorkOrders;

public class WorkOrderStatusTransitionsTests
{
    [Theory]
    [InlineData(WorkOrderStatus.Awaiting, WorkOrderStatus.Scheduled)]
    [InlineData(WorkOrderStatus.Awaiting, WorkOrderStatus.InProgress)]
    [InlineData(WorkOrderStatus.Awaiting, WorkOrderStatus.Cancelled)]
    [InlineData(WorkOrderStatus.Scheduled, WorkOrderStatus.InProgress)]
    [InlineData(WorkOrderStatus.InProgress, WorkOrderStatus.Completed)]
    public void CanTransition_ComTransicaoValida_DeveRetornarTrue(WorkOrderStatus from, WorkOrderStatus to)
    {
        Assert.True(WorkOrderStatusTransitions.CanTransition(from, to));
    }

    [Theory]
    [InlineData(WorkOrderStatus.Awaiting, WorkOrderStatus.Completed)] // não pode concluir sem passar por "em andamento"
    [InlineData(WorkOrderStatus.Completed, WorkOrderStatus.InProgress)] // status final não regride
    [InlineData(WorkOrderStatus.Cancelled, WorkOrderStatus.Awaiting)] // status final é final
    public void CanTransition_ComTransicaoInvalida_DeveRetornarFalse(WorkOrderStatus from, WorkOrderStatus to)
    {
        Assert.False(WorkOrderStatusTransitions.CanTransition(from, to));
    }
}
