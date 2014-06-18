extern alias MonoCSharp;

using System;
using System.Collections.Generic;
using System.Linq;

//using Mono.CSharp;
using MonoCSharp::Mono.CSharp;
using ScriptCs.Engine.Mono.Parser.Preparser;

namespace ScriptCs.Engine.Mono
{
    public class MonoReporter : ReportPrinter //ConsoleReportPrinter //StreamReportPrinter
    {
        private RegionResult _region;
        private List<Tuple<AbstractMessage, RegionResult>> _compileErrors;
        //private List<string> _executionErrors;

        public MonoReporter ()
        //: base(null)
        {
            _compileErrors = new List<Tuple<AbstractMessage, RegionResult>>();
            //_executionErrors = new List<string>();
        }

        public void SetRegion(RegionResult region)
        {
            _region = region;
        }

        public void Clear()
        {
            _region = null;
            _compileErrors = new List<Tuple<AbstractMessage, RegionResult>>();
            //_executionErrors = new List<string>();
            Reset();
        }

        public override void Print (AbstractMessage msg, bool showFullPath)
        {
            Guard.AgainstNullArgument("Region", _region);

            if(!msg.IsWarning)
            {
                _compileErrors.Add(new Tuple<AbstractMessage, RegionResult>(msg, _region));
            }

            base.Print(msg, showFullPath);
            //Console.WriteLine(string.Format("loc: {0},{1}", msg.Location.Row, msg.Location.Column));
        }

        public string GetCompileExceptionMessages()
        {
            if(!_compileErrors.Any())
            {
                return "No compiler errors found";
            }

            return _compileErrors.Aggregate(string.Empty, 
                (x, y) => x + Environment.NewLine + FormatMessage(y));
        }

        private string FormatMessage(Tuple<AbstractMessage, RegionResult> msg)
        {
            return string.Format(
                "({0},{1}) CS{2:0000} {3}", 
                msg.Item1.Location.Row + msg.Item2.LineNr, 
                msg.Item1.Location.Column, 
                msg.Item1.Code, 
                msg.Item1.Text);
        }
    }
}

