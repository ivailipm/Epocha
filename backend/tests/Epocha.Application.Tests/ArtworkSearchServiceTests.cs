using Epocha.Application.Search;

namespace Epocha.Application.Tests;

public class ArtworkSearchServiceTests
{
    private sealed class CapturingSearcher : IArtworkSearcher
    {
        public ArtworkSearchQuery? Received { get; private set; }

        public Task<ArtworkSearchResult> SearchAsync(ArtworkSearchQuery query, CancellationToken cancellationToken)
        {
            Received = query;
            return Task.FromResult(new ArtworkSearchResult(
                [], 0, query.Page, query.PageSize, new Dictionary<string, IReadOnlyList<FacetBucket>>()));
        }
    }

    [Fact]
    public async Task SearchAsync_clamps_paging_and_cleans_input()
    {
        var searcher = new CapturingSearcher();
        var service = new ArtworkSearchService(searcher);

        await service.SearchAsync(new ArtworkSearchQuery
        {
            Text = "   ",
            Page = -3,
            PageSize = 5000,
            Eras = ["Baroque", " Baroque ", "", "  "],
        }, CancellationToken.None);

        var received = searcher.Received!;
        Assert.Null(received.Text);
        Assert.Equal(1, received.Page);
        Assert.Equal(ArtworkSearchService.MaxPageSize, received.PageSize);
        Assert.Equal(["Baroque"], received.Eras);
    }

    [Fact]
    public async Task SearchAsync_trims_text()
    {
        var searcher = new CapturingSearcher();

        await new ArtworkSearchService(searcher).SearchAsync(
            new ArtworkSearchQuery { Text = "  monet " }, CancellationToken.None);

        Assert.Equal("monet", searcher.Received!.Text);
    }

    [Fact]
    public void Validate_accepts_a_normal_query()
    {
        Assert.Null(ArtworkSearchService.Validate(new ArtworkSearchQuery { YearFrom = 1800, YearTo = 1900 }));
    }

    [Fact]
    public void Validate_rejects_inverted_year_range()
    {
        Assert.NotNull(ArtworkSearchService.Validate(new ArtworkSearchQuery { YearFrom = 1900, YearTo = 1800 }));
    }

    [Theory]
    [InlineData(100, 100, false)]   // 10,000 results: the last allowed window
    [InlineData(101, 100, true)]    // 10,100: beyond it
    [InlineData(417, 24, true)]     // 10,008
    [InlineData(416, 24, false)]    // 9,984
    public void Validate_rejects_paging_beyond_the_result_window(int page, int pageSize, bool rejected)
    {
        var error = ArtworkSearchService.Validate(new ArtworkSearchQuery { Page = page, PageSize = pageSize });

        Assert.Equal(rejected, error is not null);
    }
}
