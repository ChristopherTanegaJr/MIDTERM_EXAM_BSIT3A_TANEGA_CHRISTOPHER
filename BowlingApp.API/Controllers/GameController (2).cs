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
                player.Frames.Add(new Frame { FrameNumber = i });

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

        if (game == null) return NotFound();

        return Ok(game);
    }

    [HttpPost("{id}/roll")]
    public async Task<IActionResult> Roll(int id, [FromBody] RollRequest request)
    {
        var player = await _context.Players
            .Include(p => p.Frames)
            .FirstOrDefaultAsync(p => p.Id == request.PlayerId);

        if (player == null) return NotFound();

        var frame = player.Frames
            .OrderBy(f => f.FrameNumber)
            .FirstOrDefault(f => f.Roll1 == null ||
                                 (f.FrameNumber < 10 && f.Roll2 == null) ||
                                 (f.FrameNumber == 10 && 
                                   (f.Roll2 == null || 
                                    (IsSpareOrStrike(f) && f.Roll3 == null))));

        if (frame == null) return BadRequest("Game finished.");

        if (frame.Roll1 == null)
            frame.Roll1 = request.Pins;
        else if (frame.Roll2 == null)
            frame.Roll2 = request.Pins;
        else if (frame.FrameNumber == 10)
            frame.Roll3 = request.Pins;

        CalculateScores(player);

        await _context.SaveChangesAsync();
        return Ok();
    }

    private bool IsSpareOrStrike(Frame f)
    {
        return (f.Roll1 == 10) || 
               (f.Roll1.HasValue && f.Roll2.HasValue && f.Roll1 + f.Roll2 == 10);
    }

    private void CalculateScores(Player player)
    {
        int total = 0;

        for (int i = 0; i < 10; i++)
        {
            var frame = player.Frames[i];

            if (frame.Roll1 == null) break;

            if (frame.FrameNumber < 10)
            {
                if (frame.Roll1 == 10) // Strike
                {
                    var bonus = GetNextTwoRolls(player.Frames, i);
                    if (bonus == null) break;
                    total += 10 + bonus.Value;
                }
                else if (frame.Roll2 != null && frame.Roll1 + frame.Roll2 == 10) // Spare
                {
                    var bonus = GetNextRoll(player.Frames, i);
                    if (bonus == null) break;
                    total += 10 + bonus.Value;
                }
                else if (frame.Roll2 != null)
                {
                    total += frame.Roll1.Value + frame.Roll2.Value;
                }
                else break;
            }
            else
            {
                if (frame.Roll2 == null) break;
                total += frame.Roll1.Value + frame.Roll2.Value + (frame.Roll3 ?? 0);
            }

            frame.Score = total;
        }
    }

    private int? GetNextRoll(List<Frame> frames, int index)
    {
        if (index + 1 >= frames.Count) return null;
        return frames[index + 1].Roll1;
    }

    private int? GetNextTwoRolls(List<Frame> frames, int index)
    {
        if (index + 1 >= frames.Count) return null;

        var next = frames[index + 1];

        if (next.Roll1 == null) return null;

        if (next.Roll1 == 10)
        {
            if (index + 2 < frames.Count)
                return 10 + (frames[index + 2].Roll1 ?? 0);
            else
                return 10 + (next.Roll2 ?? 0);
        }

        if (next.Roll2 != null)
            return next.Roll1 + next.Roll2;

        return null;
    }
}

public class RollRequest
{
    public int PlayerId { get; set; }
    public int Pins { get; set; }
}
