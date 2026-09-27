using Epocha.Api.Auth;
using Epocha.Api.Contracts;
using Epocha.Application.Collections;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Epocha.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/collections")]
public class CollectionsController(CollectionService collections) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<CollectionSummary>>> List(CancellationToken cancellationToken) =>
        Ok(await collections.ListAsync(User.GetUserId(), cancellationToken));

    [HttpPost]
    [ProducesResponseType<CollectionSummary>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<CollectionSummary>> Create(SaveCollectionRequest request, CancellationToken cancellationToken)
    {
        var outcome = await collections.CreateAsync(User.GetUserId(), request.Name, cancellationToken);

        return outcome.Error switch
        {
            CollectionError.DuplicateName => Problem(detail: "You already have a collection with this name.", statusCode: StatusCodes.Status409Conflict),
            _ => outcome.Result!
        };
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType<CollectionDetail>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CollectionDetail>> GetById(int id, CancellationToken cancellationToken)
    {
        var detail = await collections.GetDetailAsync(User.GetUserId(), id, cancellationToken);
        return detail is null ? NotFound() : detail;
    }

    [HttpPut("{id:int}")]
    [ProducesResponseType<CollectionSummary>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<CollectionSummary>> Rename(int id, SaveCollectionRequest request, CancellationToken cancellationToken)
    {
        var outcome = await collections.RenameAsync(User.GetUserId(), id, request.Name, cancellationToken);

        return outcome.Error switch
        {
            CollectionError.NotFound => NotFound(),
            CollectionError.DuplicateName => Problem(detail: "You already have a collection with this name.", statusCode: StatusCodes.Status409Conflict),
            _ => outcome.Result!
        };
    }

    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var error = await collections.DeleteAsync(User.GetUserId(), id, cancellationToken);
        return error == CollectionError.NotFound ? NotFound() : NoContent();
    }

    [HttpPost("{id:int}/artworks/{artworkId:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AddArtwork(int id, int artworkId, CancellationToken cancellationToken)
    {
        var error = await collections.AddArtworkAsync(User.GetUserId(), id, artworkId, cancellationToken);

        return error switch
        {
            CollectionError.NotFound => NotFound("Collection not found."),
            CollectionError.ArtworkNotFound => NotFound("Artwork not found."),
            _ => NoContent()
        };
    }

    [HttpDelete("{id:int}/artworks/{artworkId:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RemoveArtwork(int id, int artworkId, CancellationToken cancellationToken)
    {
        var error = await collections.RemoveArtworkAsync(User.GetUserId(), id, artworkId, cancellationToken);
        return error == CollectionError.NotFound ? NotFound() : NoContent();
    }
}
