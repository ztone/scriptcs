namespace ScriptCs.Engine.Mono.Tests.Parser
{
    using System.Linq;

    using ScriptCs.Engine.Mono.Parser;
    using Should;
    using Xunit;

    public class ScriptSegmenterTests
    {
        public class SegmentCode
        {
            [Fact]
            public void ShouldSegmentCodeAndReturnInCorrectOrder()
            {
                const string Code = "void Bar() {} Bar(); class A {}";

                var segmenter = new ScriptSegmenter();

                var result = segmenter.Segment(Code);

                result.Count().ShouldEqual(4);

                result[0].SegmentType.ShouldEqual(SegmentType.Class);
                result[0].SegmentCode.ShouldEqual("class A {}");
                result[1].SegmentType.ShouldEqual(SegmentType.Prototype);
                result[1].SegmentCode.ShouldEqual("Action Bar;");
                result[2].SegmentType.ShouldEqual(SegmentType.Method);
                result[2].SegmentCode.ShouldEqual("Bar = delegate () {};");
                result[3].SegmentType.ShouldEqual(SegmentType.Evaluation);
                result[3].SegmentCode.ShouldEqual("Bar();");
            }
        }
    }
}