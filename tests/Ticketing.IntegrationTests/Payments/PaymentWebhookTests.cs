using System.Net.Http.Json;
using Microsoft.EntityFrameworkCore;
using Ticketing.Domain.Payments;

namespace Ticketing.IntegrationTests.Payments;

[Collection("Integration")]
public class PaymentWebhookTests : IntegrationTestBase
{
    public PaymentWebhookTests(TestDatabaseFixture fixture) : base(fixture) { }

    [Fact]
    public async Task Webhook_WithFailedStatus_ShouldFailPayment()
    {
        using var db = CreateDbContext();
        var screening = await db.Screenings.Include(s => s.Seats).FirstAsync();
        var seatId = screening.Seats.First().SeatId;

        var reservationResponse = await Client.PostAsJsonAsync("/api/Reservations", new
        {
            screeningId = screening.Id,
            seatIds = new[] { seatId }
        });
        reservationResponse.EnsureSuccessStatusCode();

        await RunOutboxProcessorOnce();

        using var dbAfterPayment = CreateDbContext();
        var payment = await dbAfterPayment.Payments.FirstAsync();

        var webhookResponse = await Client.PostAsJsonAsync("/api/payments/webhook", new
        {
            paymentId = payment.Id,
            status = "failed"
        });
        webhookResponse.EnsureSuccessStatusCode();

        using var verifyDb = CreateDbContext();
        var updatedPayment = await verifyDb.Payments.FirstAsync();
        Assert.Equal(PaymentStatus.Failed, updatedPayment.Status);
    }

    [Fact]
    public async Task Webhook_WithUnknownStatus_ShouldReturnBadRequest()
    {
        var response = await Client.PostAsJsonAsync("/api/payments/webhook", new
        {
            paymentId = Guid.NewGuid(),
            status = "unknown"
        });

        Assert.False(response.IsSuccessStatusCode);
        Assert.Equal(System.Net.HttpStatusCode.BadRequest, response.StatusCode);
    }
}
