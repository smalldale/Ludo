using Ludo.Api.Contracts;
using Ludo.Api.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Ludo.Api.Hubs;

namespace Ludo.Api.Controllers;

[ApiController]
[Route("api/games")]
public sealed class GamesController : ControllerBase
{
    private readonly GameApplicationService _games;
    private readonly IHubContext<GameHub> _gameHub;

    public GamesController(
        GameApplicationService games,
        IHubContext<GameHub> gameHub)
    {
        _games = games;
        _gameHub = gameHub;
    }

    [HttpGet("{gameId}")]
    public async Task<IActionResult> GetState(
        string gameId,
        CancellationToken cancellationToken)
    {
        try
        {
            var state = await _games.GetStateAsync(
                gameId, cancellationToken);

            return Ok(state);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }

    [HttpPost("{gameId}/roll")]
    [ProducesResponseType(typeof(RollResultDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Roll(
        string gameId,
        [FromBody] PlayerActionRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var result = await _games.RollDiceAsync(
                gameId,
                request.PlayerId,
                cancellationToken);

            await _gameHub.Clients
                .Group(gameId)
                .SendAsync("GameUpdated", result.State, cancellationToken);

            return Ok(result);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { error = ex.Message });
        }
    }

    [HttpPost("{gameId}/move")]
    public async Task<IActionResult> Move(
        string gameId,
        [FromBody] MovePieceRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var result = await _games.MovePieceAsync(
                gameId,
                request.PlayerId,
                request.PieceId,
                cancellationToken);

            if (!result.Result.Success)
                return BadRequest(result);

            await _gameHub.Clients
                .Group(gameId)
                .SendAsync("GameUpdated", result.State, cancellationToken);

            return Ok(result);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
    }

    
    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] CreateGameRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            CreateGameResponse response = await _games.CreateGameAsync(
                request,
                cancellationToken);

            return CreatedAtAction(
                nameof(GetState),
                new { gameId = response.State.GameId },
                response);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { error = ex.Message });
        }
    }

}