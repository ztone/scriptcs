using System;

namespace ScriptCs.Engine.Mono.Parser
{
    public class SegmentResult
    {
        public SegmentType Segment { get; set; }
        public RegionResult Region { get; set; }
        public string CodeSegment { get ; set; }
    }
}

