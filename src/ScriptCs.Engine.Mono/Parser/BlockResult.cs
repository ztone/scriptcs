namespace ScriptCs.Engine.Mono.Parser
{
    using System;

    public class BlockResult
    {
        public BlockResult()
        {
            IsValid = true;
        }

        public static BlockResult Empty = new BlockResult { Offset = 0, Length = 0 };

        public int LineNr { get;set; }
        public int Offset { get;set; }
        public int Length { get;set; }
        public bool IsCompleteBlock { get;set; }
        public bool IsValid { get; set; }

        public static BlockResult Incomplete()
        {
            return new BlockResult
            {
                Offset = 0,
                Length = 0,
                IsCompleteBlock = true,
                IsValid = true,
            };
        }

        /*
        public static BlockResult Invalid()
        {
            return new BlockResult
            {
                Offset = 0,
                Length = 0,
                IsCompleteBlock = false,
                IsValid = false,
            };
        }*/
    }
}