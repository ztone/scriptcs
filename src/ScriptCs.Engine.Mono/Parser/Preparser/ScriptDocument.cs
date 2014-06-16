namespace ScriptCs.Engine.Mono.Preparser
{
    using System;
    using System.Collections.Generic;

    public class ScriptDocument
    {
        public string ExtractRegions(string code, IEnumerable<RegionResult> regions)
        {
            var document = new string(' ', code.Length);
            foreach(var region in regions)
            {
                var snippet = code.Substring(region.Offset, region.Length);
                document = document.Remove(region.Offset, region.Length);
                document = document.Insert(region.Offset, snippet);
            }

            return document;
        }

        public string PurgeRegions(string code, IEnumerable<RegionResult> regions)
        {
            var document = code;
            foreach(var region in regions)
            {
                document = document.Remove(region.Offset, region.Length);
                document = document.Insert(region.Offset, new string(' ', region.Length));
            }

            return document;
        }
    }
}