extern alias MonoCSharp;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Common.Logging;
using MonoCSharp::Mono.CSharp;
using ScriptCs.Contracts;
using ScriptCs.Engine.Mono.Parser;
using ScriptCs.Engine.Mono.Parser.NRefactory;

namespace ScriptCs.Engine.Mono
{
    public class MonoScriptEngine : IScriptEngine
    {
        private readonly IScriptHostFactory _scriptHostFactory;
        public string BaseDirectory { get; set; }
        public string CacheDirectory { get; set; }
        public string FileName { get; set; }
        public MonoReporter _reporter;

        public const string SessionKey = "MonoSession";

        public MonoScriptEngine(IScriptHostFactory scriptHostFactory, ILog logger)
        {
            _scriptHostFactory = scriptHostFactory;
            Logger = logger;
            _reporter = new MonoReporter();
        }

        public ILog Logger { get; set; }

        public ScriptResult Execute(string code, string[] scriptArgs, AssemblyReferences references, IEnumerable<string> namespaces,
            ScriptPackSession scriptPackSession)
        {
            Guard.AgainstNullArgument("references", references);
            Guard.AgainstNullArgument("scriptPackSession", scriptPackSession);

            references.PathReferences.UnionWith(scriptPackSession.References);

            _reporter.Clear();

            SessionState<Evaluator> sessionState;
            if (!scriptPackSession.State.ContainsKey(SessionKey))
            {
                Logger.Debug("Creating session");
                var context = new CompilerContext(new CompilerSettings
                {
                    AssemblyReferences = references.PathReferences.ToList()
                    }, _reporter);

                var evaluator = new Evaluator(context);
                var allNamespaces = namespaces.Union(scriptPackSession.Namespaces).Distinct();

                var host = _scriptHostFactory.CreateScriptHost(new ScriptPackManager(scriptPackSession.Contexts), scriptArgs);
                MonoHost.SetHost((ScriptHost)host);

                evaluator.ReferenceAssembly(typeof(MonoHost).Assembly);
                evaluator.InteractiveBaseClass = typeof(MonoHost);

                sessionState = new SessionState<Evaluator>
                {
                    References = new AssemblyReferences(references.PathReferences, references.Assemblies),
                    Namespaces = new HashSet<string>(),
                    Session = evaluator
                };

                ImportNamespaces(allNamespaces, sessionState);

                scriptPackSession.State[SessionKey] = sessionState;
            }
            else
            {
                Logger.Debug("Reusing existing session");
                sessionState = (SessionState<Evaluator>)scriptPackSession.State[SessionKey];

                var newReferences = sessionState.References == null ? references : references.Except(sessionState.References);
                foreach (var reference in newReferences.PathReferences)
                {
                    Logger.DebugFormat("Adding reference to {0}", reference);
                    sessionState.Session.LoadAssembly(reference);
                }

                sessionState.References = new AssemblyReferences(references.PathReferences, references.Assemblies);

                var newNamespaces = sessionState.Namespaces == null ? namespaces : namespaces.Except(sessionState.Namespaces);
                ImportNamespaces(newNamespaces, sessionState);
            }

            Logger.Debug("Starting execution");
            var result = Execute(code, sessionState.Session);
            Logger.Debug("Finished execution");
            return result;
        }

        protected virtual ScriptResult Execute(string code, Evaluator session)
        {
            try
            {
                var segmenter = new ScriptSegmenter();
                object scriptResult = null;
                foreach(var segment in segmenter.Segment(code))
                {
                    bool resultSet;
                    _reporter.SetRegion(segment.Region);
                    _reporter.SetSegment(segment.SegmentType);

                    var expr = session.Evaluate(segment.SegmentCode, out scriptResult, out resultSet);
                    if(expr != null || _reporter.DontErrorOnLooseMethods)
                    {
                        return ScriptResult.Incomplete;
                    }
                }

                if(_reporter.ErrorsCount != 0)
                {
                    return new ScriptResult(compilationException: 
                        new Exception(_reporter.GetCompileExceptionMessages()));
                }

                return new ScriptResult(returnValue: scriptResult);

                /*
                var parser = new SyntaxParser();
                var parseResult = parser.Parse(code);

                if (parseResult.TypeDeclarations != null && parseResult.TypeDeclarations.Any())
                {
                    foreach (var @class in parseResult.TypeDeclarations)
                    {
                        session.Compile(@class);
                    }
                }

                if (parseResult.MethodExpressions != null && parseResult.MethodExpressions.Any())
                {
                    foreach (var prototype in parseResult.MethodPrototypes)
                    {
                        session.Run(prototype);
                    }

                    foreach (var method in parseResult.MethodExpressions)
                    {
                        session.Run(method);
                    }
                }

                if (!string.IsNullOrWhiteSpace(parseResult.Evaluations))
                {
                    object scriptResult;
                    bool resultSet;

                    session.Evaluate(parseResult.Evaluations, out scriptResult, out resultSet);

                    return new ScriptResult(returnValue: scriptResult);
                }
                */
            }
            catch (Exception ex)
            {
                return new ScriptResult(executionException: ex);
            }

            //return ScriptResult.Empty;
        }

        private void ImportNamespaces(IEnumerable<string> namespaces, SessionState<Evaluator> sessionState)
        {
            var builder = new StringBuilder();
            foreach (var ns in namespaces)
            {
                Logger.DebugFormat(ns);
                builder.AppendLine(string.Format("using {0};", ns));
                sessionState.Namespaces.Add(ns);
            }
            sessionState.Session.Compile(builder.ToString());
        }
    }
}
