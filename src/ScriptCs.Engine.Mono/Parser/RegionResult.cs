namespace ScriptCs.Engine.Mono.Parser
{
    using System;

    public class RegionResult
    {
        public int LineNr { get;set; }
        public int Offset { get;set; }
        public int Length { get;set; }
        public bool IsCompleteBlock { get;set; }
    }
}