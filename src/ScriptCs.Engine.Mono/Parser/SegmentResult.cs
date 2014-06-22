namespace ScriptCs.Engine.Mono.Parser
{
    using System.Collections.Generic;

    public class SegmentResult
    {
        public SegmentType Type { get; set; }
        public int BeginLine { get; set; }
        public string Code { get ; set; }
        public List<CompileErrorMessage> ErrorMessages { get; set; }
    }
}