using Epocha.Application.Search;
using Epocha.Domain.Entities;
using Epocha.Domain.Enums;

namespace Epocha.Application.Tests;

public class ArtworkSearchDocumentTests
{
    [Fact]
    public void From_flattens_artist_and_movements_and_uses_enum_names()
    {
        var artwork = new Artwork
        {
            Id = 42,
            SourceSystem = SourceSystem.ArtInstituteOfChicago,
            SourceExternalId = "abc",
            Title = "Water Lilies",
            Era = Era.Modern,
            MediumCategory = MediumCategory.Painting,
            DateStartYear = 1906,
            Artist = new Artist { SourceExternalId = "1", Name = "Claude Monet" },
            Movements =
            [
                new Movement { Name = "Impressionism", Slug = "impressionism" },
                new Movement { Name = "Alpha", Slug = "alpha" }
            ]
        };

        var doc = ArtworkSearchDocument.From(artwork);

        Assert.Equal(42, doc.Id);
        Assert.Equal("Claude Monet", doc.ArtistName);
        Assert.Equal("Modern", doc.Era);
        Assert.Equal("Painting", doc.MediumCategory);
        Assert.Equal("ArtInstituteOfChicago", doc.SourceSystem);
        Assert.Equal(["Alpha", "Impressionism"], doc.Movements);
    }

    [Fact]
    public void From_handles_missing_artist()
    {
        var artwork = new Artwork { SourceExternalId = "x", Title = "Untitled" };

        var doc = ArtworkSearchDocument.From(artwork);

        Assert.Null(doc.ArtistName);
        Assert.Empty(doc.Movements);
    }
}
