using Epocha.Api.Contracts;
using Epocha.Application.Search;
using Microsoft.AspNetCore.Mvc;

namespace Epocha.Api.Controllers;

[ApiController]
[Route("api/artworks")]
public class ArtworksController(ArtworkSearchService searchService) : ControllerBase
{
    /// <summary>Full-text search and browse with filters, facet counts, sorting and paging.</summary>
    [HttpGet("search")]
    [ProducesResponseType<ArtworkSearchResult>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ArtworkSearchResult>> Search(
        [FromQuery] SearchArtworksRequest request,
        CancellationToken cancellationToken)
    {
        var query = request.ToQuery();

        if (ArtworkSearchService.Validate(query) is { } error)
        {
            return Problem(detail: error, statusCode: StatusCodes.Status400BadRequest);
        }

        return await searchService.SearchAsync(query, cancellationToken);
    }
}
