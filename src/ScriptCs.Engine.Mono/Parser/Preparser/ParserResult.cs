namespace ScriptCs.Engine.Mono.Preparser
{
    using System;
    using System.Collections.Generic;

    public class ParserResult
    {
        public ParserResult ()
        {
            Classes = new List<Tuple<bool, string>>();
            Methods = new List<Tuple<bool, string>>();

            ClassRegions = new List<RegionResult>();
            MethodRegions = new List<RegionResult>();
        }

        public List<Tuple<bool, string>> Classes { get; set; }

        public List<Tuple<bool, string>> Methods { get; set; }

        public List<RegionResult> ClassRegions { get; set; }

        public List<RegionResult> MethodRegions { get; set; }
    }
}
