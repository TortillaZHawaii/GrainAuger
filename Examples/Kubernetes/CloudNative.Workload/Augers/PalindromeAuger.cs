using Orleans.Streams;

namespace CloudNative.Workload.Augers;

public class PalindromeAuger(IAsyncObserver<List<List<string>>> output) : IAsyncObserver<CardTransaction>
{
    public async Task OnNextAsync(CardTransaction item, StreamSequenceToken? token = null)
    {
        var solver = new Solver();
        var res = solver.Partition(item.CardNumber);
        await output.OnNextAsync(res);
    }

    public async Task OnCompletedAsync()
    {
        await output.OnCompletedAsync();
    }

    public async Task OnErrorAsync(Exception ex)
    {
        await output.OnCompletedAsync();
    }
}

public class Solver
{
    private List<List<string>> res = new();
    private List<string> part = new();

    public List<List<string>> Partition(string s)
    {
        Dfs(0, 0, s);
        return res;
    }

    private void Dfs(int j, int i, string s)
    {
        while (true)
        {
            if (i >= s.Length)
            {
                if (i == j)
                {
                    res.Add(new List<string>(part));
                }

                return;
            }

            if (isPali(s, j, i))
            {
                part.Add(s.Substring(j, i - j + 1));
                Dfs(i + 1, i + 1, s);
                part.RemoveAt(part.Count - 1);
            }

            i = i + 1;
        }
    }

    private bool isPali(string s, int l, int r)
    {
        while (l < r)
        {
            if (s[l] != s[r])
            {
                return false;
            }

            l++;
            r--;
        }

        return true;
    }
}