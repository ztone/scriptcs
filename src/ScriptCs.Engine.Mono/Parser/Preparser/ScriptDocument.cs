namespace ScriptCs.Engine.Mono.Parser.Preparser
{
    using System;
    using System.Collections.Generic;

    public class ScriptDocument
    {
        public string ExtractRegions(string code, IEnumerable<RegionResult> regions)
        {
            var document = CloneAsEmptyStringWithNewLine(code);

            if(regions != null)
            {
                foreach(var region in regions)
                {
                    var snippet = code.Substring(region.Offset, region.Length);
                    document = document.Remove(region.Offset, region.Length);
                    document = document.Insert(region.Offset, snippet);
                }
            }

            return document;
        }

        public string PurgeRegions(string code, IEnumerable<RegionResult> regions)
        {
            var document = code;
            if(regions != null)
            {
                foreach(var region in regions)
                {
                    var snippet = CloneAsEmptyStringWithNewLine(code.Substring(region.Offset, region.Length));
                    document = document.Remove(region.Offset, region.Length);
                    document = document.Insert(region.Offset, snippet);
                }
            }

            return document;
        }

        private string CloneAsEmptyStringWithNewLine(string str)
        {
            var result = new string(' ', str.Length);

            var tmp = str;
            int index = 0;
            while((index = tmp.IndexOf(Environment.NewLine)) != -1)
            {
                result = result.Remove(index, Environment.NewLine.Length);
                result = result.Insert(index, Environment.NewLine);

                tmp = tmp.Remove(index, Environment.NewLine.Length);
                tmp = tmp.Insert(index, new string(' ', Environment.NewLine.Length));
            }

            return result;
        }
    }
}