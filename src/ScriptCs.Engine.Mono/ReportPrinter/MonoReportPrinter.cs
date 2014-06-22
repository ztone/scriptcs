extern alias MonoCSharp;

namespace ScriptCs.Engine.Mono.ReportPrinter
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using MonoCSharp::Mono.CSharp;

    using ScriptCs.Engine.Mono.Parser;

    public class MonoReportPrinter : ReportPrinter
    {
        private List<Tuple<CompileErrorMessage, int>> _compileErrors;
        private int _currentLine;

        public void SetCurrentLine(int lineNumber)
        {
            _currentLine = lineNumber;
        }

        public void Clear()
        {
            _currentLine = 0;
            _compileErrors = new List<Tuple<CompileErrorMessage, int>>();
            Reset();
        }

        public override void Print (AbstractMessage msg, bool showFullPath)
        {
            if(!msg.IsWarning)
            {
                _compileErrors.Add(new Tuple<CompileErrorMessage, int>(
                    new CompileErrorMessage(msg), _currentLine));
            }

            base.Print(msg, showFullPath);
        }

        public void AddErrors(List<CompileErrorMessage> errorMessages)
        {
            foreach(var message in errorMessages)
            {
                _compileErrors.Add(new Tuple<CompileErrorMessage, int>(message, _currentLine));
            }
        }

        public string GetCompileExceptionMessages()
        {
            if(_compileErrors == null || !_compileErrors.Any())
            {
                return string.Empty;
            }

            return _compileErrors.Aggregate(string.Empty, 
                (x, y) => x + Environment.NewLine + FormatMessage(y)).Trim();
        }

        private string FormatMessage(Tuple<CompileErrorMessage, int> msg)
        {

            return string.Format(
                "({0},{1}) CS{2:0000} {3}", 
                msg.Item1.Location.Row + msg.Item2, 
                msg.Item1.Location.Column, 
                msg.Item1.Code, 
                msg.Item1.Text);
        }
    }
}