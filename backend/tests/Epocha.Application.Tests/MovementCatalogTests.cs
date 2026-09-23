using Epocha.Domain.Common;

namespace Epocha.Application.Tests;

public class MovementCatalogTests
{
    [Theory]
    [InlineData("Impressionism", "Impressionism")]
    [InlineData("impressionism", "Impressionism")]
    [InlineData("Post-Impressionism", "Post-Impressionism")]
    [InlineData("Analytical Cubism", "Cubism")]
    [InlineData("synthetic cubist", "Cubism")]
    [InlineData("Surrealism In Exile", "Surrealism")]
    [InlineData("Gothic (Medieval)", "Gothic")]
    [InlineData("Ukiyo-E", "Ukiyo-e")]
    public void Normalize_maps_known_movements_and_aliases_to_the_canonical_name(string raw, string expected)
    {
        Assert.Equal(expected, MovementCatalog.Normalize(raw)?.Name);
    }

    [Theory]
    [InlineData("21st century")]
    [InlineData("Japanese (Culture or Style)")]
    [InlineData("Twenty-Ninth Dynasty")]
    [InlineData("Red-Figure")]
    [InlineData("Victorian")]
    [InlineData("")]
    [InlineData("???")]
    public void Normalize_rejects_values_that_are_not_movements(string raw)
    {
        Assert.Null(MovementCatalog.Normalize(raw));
    }

    [Fact]
    public void Normalize_returns_a_stable_slug_for_all_aliases()
    {
        Assert.Equal("cubism", MovementCatalog.Normalize("Confetti Cubism")!.Slug);
        Assert.Equal("cubism", MovementCatalog.Normalize("Cubism")!.Slug);
    }
}
