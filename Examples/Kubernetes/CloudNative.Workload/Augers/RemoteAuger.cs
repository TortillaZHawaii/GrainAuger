using Orleans.Streams;

namespace CloudNative.Workload.Augers;

[GenerateSerializer]
public class CardCompany
{
    [Id(0)]
    public CardTransaction Transaction { get; set; }
    [Id(1)]
    public string Company { get; set; }
}

public class RemoteAuger(IAsyncObserver<CardCompany> output, HttpClient client, ILogger<RemoteAuger> logger) : IAsyncObserver<CardTransaction>
{
    public async Task OnNextAsync(CardTransaction item, StreamSequenceToken? token = null)
    {
        var result = await client.GetAsync($"http://company-server-svc/company?number={item.CardNumber}");
        if (!result.IsSuccessStatusCode)
        {
            throw new Exception("Failed to get company");
        }
        var company = await result.Content.ReadAsStringAsync();
        await output.OnNextAsync(new CardCompany { Transaction = item, Company = company });
    }

    public async Task OnCompletedAsync()
    {
        await output.OnCompletedAsync();
    }

    public async Task OnErrorAsync(Exception ex)
    {
        logger.LogError("Error processing item: {Error}", ex.Message);
        await output.OnErrorAsync(ex);
    }
}