extern alias MonoCSharp;

namespace ScriptCs.Engine.Mono
{
    using System.Collections.Generic;

    using MonoCSharp::Mono.CSharp;

    public sealed class CompileErrorMessage : AbstractMessage
    {
        public CompileErrorMessage (int code, int row, int column, string message)
            : base (code, new Location(null, row, column), message, new List<string>())
        {
        }

        public CompileErrorMessage (AbstractMessage aMsg)
            : base (aMsg)
        {
        }

        public override bool IsWarning 
        {
            get { return false; }
        }

        public override string MessageType 
        {
            get {
                return "error";
            }
        }
    }
}

