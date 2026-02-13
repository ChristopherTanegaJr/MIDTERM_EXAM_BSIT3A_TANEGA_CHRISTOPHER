public class Game
{
    public int Id { get; set; }
    public bool IsFinished { get; set; } = false;
    public List<Player> Players { get; set; } = new();
}
