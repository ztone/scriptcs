﻿extern alias MonoCSharp;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Common.Logging;
using MonoCSharp::Mono.CSharp;
using ScriptCs.Contracts;

namespace ScriptCs.Engine.Mono
{
    using ScriptCs.Engine.Mono.Segmenter;

    public class MonoScriptEngine : IScriptEngine
    {
        private readonly IScriptHostFactory _scriptHostFactory;
        public string BaseDirectory { get; set; }
        public string CacheDirectory { get; set; }
        public string FileName { get; set; }

        public const string SessionKey = "MonoSession";

        public MonoScriptEngine(IScriptHostFactory scriptHostFactory, ILog logger)
        {
            _scriptHostFactory = scriptHostFactory;
            Logger = logger;
        }

        public ILog Logger { get; set; }

        public ScriptResult Execute(string code, string[] scriptArgs, AssemblyReferences references, IEnumerable<string> namespaces,
            ScriptPackSession scriptPackSession)
        {
            Guard.AgainstNullArgument("references", references);
            Guard.AgainstNullArgument("scriptPackSession", scriptPackSession);
            
            references.PathReferences.UnionWith(scriptPackSession.References);

            SessionState<Evaluator> sessionState;
            var isFirstExecution = !scriptPackSession.State.ContainsKey(SessionKey);

            if (isFirstExecution)
            {
                code = code.DefineTrace();
                Logger.Debug("Creating session");
                var context = new CompilerContext(new CompilerSettings
                {
                    AssemblyReferences = references.PathReferences.ToList()
                }, new ConsoleReportPrinter());

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
                object scriptResult = null;
                var segmenter = new ScriptSegmenter();
                foreach (var segment in segmenter.Segment(code))
                {
                    if (segment.Type != SegmentType.MethodError)
                    {
                        bool resultSet;
                        session.Evaluate(segment.Code, out scriptResult, out resultSet);
                    }
                }

                return new ScriptResult(returnValue: scriptResult);
            }
            catch (Exception ex)
            {
                return new ScriptResult(executionException: ex);
            }
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