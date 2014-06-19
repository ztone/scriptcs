namespace ScriptCs.Engine.Mono.Parser
{
    using System;

    public class RegionResult
    {
        public RegionResult()
        {
            IsValid = true;
        }

        public int LineNr { get;set; }
        public int Offset { get;set; }
        public int Length { get;set; }
        public bool IsCompleteBlock { get;set; }
        public bool IsValid { get; set; }

        public static RegionResult Incomplete()
        {
            return new RegionResult
            {
                Offset = 0,
                Length = 0,
                IsCompleteBlock = true,
                IsValid = true,
            };
        }

        public static RegionResult Invalid()
        {
            return new RegionResult
            {
                Offset = 0,
                Length = 0,
                IsCompleteBlock = false,
                IsValid = false,
            };
        }
    }
}

