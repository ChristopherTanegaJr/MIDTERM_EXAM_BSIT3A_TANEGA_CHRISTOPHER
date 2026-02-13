public class Frame
{
    public int Id { get; set; }
    public int FrameNumber { get; set; }
    public int? Roll1 { get; set; }
    public int? Roll2 { get; set; }
    public int? Roll3 { get; set; }
    public int? Score { get; set; }

    public int PlayerId { get; set; }
    public Player Player { get; set; }
}
