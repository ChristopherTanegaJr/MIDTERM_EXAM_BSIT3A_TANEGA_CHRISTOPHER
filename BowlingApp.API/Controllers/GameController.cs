using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

[ApiController]
[Route("api/game")]
public class GameController : ControllerBase
{
    private readonly AppDbContext _context;

    public GameController(AppDbContext context)
    {
        _context = context;
    }

    [HttpPost]
    public async Task<IActionResult> CreateGame([FromBody] List<string> playerNames)
    {
        var game = new Game();

        foreach (var name in playerNames)
        {
            var player = new Player { Name = name };

            for (int i = 1; i <= 10; i++)
            {
                player.Frames.Add(new Frame { FrameNumber = i });
            }

            game.Players.Add(player);
        }

        _context.Games.Add(game);
        await _context.SaveChangesAsync();

        return Ok(game);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetGame(int id)
    {
        var game = await _context.Games
            .Include(g => g.Players)
            .ThenInclude(p => p.Frames)
            .FirstOrDefaultAsync(g => g.Id == id);

        if (game == null)
            return NotFound();

        return Ok(game);
    }
}
