namespace ScriptCs.Engine.Mono.Parser.Preparser
{
    using System;

    public class RegionResult
    {
        public RegionResult()
        {
            IsValid = true;
        }

        public int Offset { get;set; }
        public int Length { get;set; }
        public bool IsIncomplete { get;set; }
        public bool IsValid { get; set; }

        public static RegionResult Incomplete()
        {
            return new RegionResult
            {
                Offset = 0,
                Length = 0,
                IsIncomplete = true,
                IsValid = true,
            };
        }

        public static RegionResult Invalid()
        {
            return new RegionResult
            {
                Offset = 0,
                Length = 0,
                IsIncomplete = false,
                IsValid = false,
            };
        }
    }
}

