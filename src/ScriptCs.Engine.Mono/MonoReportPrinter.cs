extern alias MonoCSharp;

namespace ScriptCs.Engine.Mono
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using MonoCSharp::Mono.CSharp;

    using ScriptCs.Engine.Mono.Parser;

    public class MonoReportPrinter : ReportPrinter
    {
        private List<Tuple<AbstractMessage, RegionResult>> _compileErrors;
        private RegionResult _region;

        public void SetRegion(RegionResult region)
        {
            _region = region;
        }

        public void Clear()
        {
            _region = null;
            _compileErrors = new List<Tuple<AbstractMessage, RegionResult>>();
            Reset();
        }

        public override void Print (AbstractMessage msg, bool showFullPath)
        {
            if(!msg.IsWarning)
            {
                _compileErrors.Add(new Tuple<AbstractMessage, RegionResult>(msg, _region));
            }

            base.Print(msg, showFullPath);
        }

        public string GetCompileExceptionMessages()
        {
            if(_compileErrors == null || !_compileErrors.Any())
            {
                return string.Empty;
            }

            return _compileErrors.Aggregate(string.Empty, 
                (x, y) => x + Environment.NewLine + FormatMessage(y));
        }

        private string FormatMessage(Tuple<AbstractMessage, RegionResult> msg)
        {
            var lineNr = (msg.Item2 != null) ? msg.Item2.LineNr : 0;

            return string.Format(
                "({0},{1}) CS{2:0000} {3}", 
                msg.Item1.Location.Row + lineNr, 
                msg.Item1.Location.Column, 
                msg.Item1.Code, 
                msg.Item1.Text);
        }
    }
}