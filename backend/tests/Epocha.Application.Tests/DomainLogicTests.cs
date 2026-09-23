using Epocha.Domain.Common;
using Epocha.Domain.Enums;

namespace Epocha.Application.Tests;

public class DomainLogicTests
{
    // [Theory] + [InlineData] runs the same test once per row: xUnit's way of writing
    // table-driven tests.
    [Theory]
    [InlineData("Oil on canvas", null, MediumCategory.Painting)]
    [InlineData("Gelatin silver print", null, MediumCategory.Photograph)]
    [InlineData("Woodcut", "print", MediumCategory.Print)]
    [InlineData("Graphite on paper", null, MediumCategory.Drawing)]
    [InlineData("Bronze", "sculpture", MediumCategory.Sculpture)]
    [InlineData("Printed cotton", null, MediumCategory.Textile)]
    [InlineData("Faience", "ushabti", MediumCategory.Ceramic)]
    [InlineData("Gold and enamel necklace", null, MediumCategory.Metalwork)]
    [InlineData("Portable embedded thing", null, MediumCategory.Unknown)]
    [InlineData(null, null, MediumCategory.Unknown)]
    public void MediumClassifier_buckets_common_mediums(string? medium, string? classification, MediumCategory expected)
    {
        Assert.Equal(expected, MediumClassifier.Classify(medium, classification));
    }

    [Theory]
    [InlineData("Post-Impressionism", "post-impressionism")]
    [InlineData("  late period (Egyptian) ", "late-period-egyptian")]
    [InlineData("???", "")]
    public void Slug_normalises_names(string input, string expected)
    {
        Assert.Equal(expected, Slug.From(input));
    }

    [Theory]
    [InlineData(null, Era.Unknown)]
    [InlineData(-380, Era.Ancient)]
    [InlineData(1503, Era.Renaissance)]
    [InlineData(1889, Era.Modern)]
    [InlineData(1955, Era.Contemporary)]
    public void EraCalculator_maps_years_to_eras(int? year, Era expected)
    {
        Assert.Equal(expected, EraCalculator.FromYear(year));
    }
}
