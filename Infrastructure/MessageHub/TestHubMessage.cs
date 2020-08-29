namespace maxbl4.Infrastructure.MessageHub
{
    public class TestHubMessage
    {
        public static int StartIndex = 1;
        public string Content { get; set; }
        public int Index { get; set; } = StartIndex++;

        public override string ToString()
        {
            return $"{Index} {Content}";
        }
    }
}