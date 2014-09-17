namespace ScriptCs.Engine.Mono.Segmenter.Parser
{
    public class RegionResult
    {
        public int Offset { get; set; }

        public int Length { get; set; }

        public bool IsCompleteBlock { get; set; }
    }
}