namespace Funny.WebScrape;

public class NodeScoreInfo
{
    public double Score { get; set; }
    public int TextLength { get; set; }
    public Dictionary<string, int> TagCount { get; set; } = new Dictionary<string, int>();
    public int ChildCount { get; set; }
}