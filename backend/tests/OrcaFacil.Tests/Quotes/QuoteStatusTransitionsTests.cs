using OrcaFacil.Domain.Enums;
using Xunit;

namespace OrcaFacil.Tests.Quotes;

public class QuoteStatusTransitionsTests
{
    [Theory]
    [InlineData(QuoteStatus.Draft, QuoteStatus.Sent)]
    [InlineData(QuoteStatus.Draft, QuoteStatus.Cancelled)]
    [InlineData(QuoteStatus.Sent, QuoteStatus.Approved)]
    [InlineData(QuoteStatus.Sent, QuoteStatus.Rejected)]
    [InlineData(QuoteStatus.Sent, QuoteStatus.Expired)]
    [InlineData(QuoteStatus.Approved, QuoteStatus.Cancelled)]
    public void CanTransition_ComTransicaoValida_DeveRetornarTrue(QuoteStatus from, QuoteStatus to)
    {
        Assert.True(QuoteStatusTransitions.CanTransition(from, to));
    }

    [Theory]
    [InlineData(QuoteStatus.Draft, QuoteStatus.Approved)] // não pode aprovar sem antes enviar
    [InlineData(QuoteStatus.Rejected, QuoteStatus.Sent)] // status final não volta atrás
    [InlineData(QuoteStatus.Approved, QuoteStatus.Sent)] // já aprovado não regride
    [InlineData(QuoteStatus.Cancelled, QuoteStatus.Draft)] // status final é final
    public void CanTransition_ComTransicaoInvalida_DeveRetornarFalse(QuoteStatus from, QuoteStatus to)
    {
        Assert.False(QuoteStatusTransitions.CanTransition(from, to));
    }
}
