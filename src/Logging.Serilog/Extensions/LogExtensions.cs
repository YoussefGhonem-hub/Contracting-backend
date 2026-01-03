using Serilog;
using Serilog.Context;

namespace Logging.Serilog.Extensions
{
    /// <summary>
    /// App-wide logging facade built on Serilog's static <c>Log</c>.
    /// Use for structured logging without injecting <c>ILogger</c>.
    /// </summary>
    /// <remarks>
    /// Examples:
    /// - AppLog.Info("WebForms page hit {Path}", Request.Url?.AbsolutePath);
    /// - using (AppLog.Correlation(correlationId)) { AppLog.Info("Processing..."); }
    /// - AppLog.InfoKey("1", "Started job {JobId}", jobId);
    /// - using (AppLog.Key("1")) { AppLog.Warn("This will also have Key"); }
    /// </remarks>
    public static class AppLog
    {
        private const string KeyPropertyName = "Key"; // change if you prefer "LogKey"

        /// <summary>
        /// Creates a typed logger bound to <typeparamref name="T"/> (sets <c>SourceContext</c>).
        /// </summary>
        /// <typeparam name="T">Type used as the log source.</typeparam>
        /// <returns>A Serilog logger enriched with <c>SourceContext = typeof(T).FullName</c>.</returns>
        /// <remarks>
        /// Use when you want logs to carry a specific source (class) automatically.
        /// Example: <c>AppLog.For&lt;MyService&gt;().Information("Started {At:O}", DateTime.UtcNow);</c>
        /// </remarks>
        public static ILogger For<T>() => Log.ForContext<T>();

        /// <summary>
        /// Creates a logger for an arbitrary <see cref="Type"/> (sets <c>SourceContext</c>).
        /// </summary>
        /// <param name="t">Type to use as the log source.</param>
        /// <returns>Logger enriched with <c>SourceContext</c>.</returns>
        /// <remarks>
        /// Use when the type is only known at runtime.
        /// Example: <c>AppLog.ForType(GetType()).Information("Dynamic type source");</c>
        /// </remarks>
        public static ILogger ForType(Type t) => Log.ForContext("SourceContext", t?.FullName ?? "Unknown");

        // ---------- Key Scoping ----------

        /// <summary>
        /// Pushes a Key property (e.g., grouping id) for all logs in the scope.
        /// Seq search: Key = '123'
        /// </summary>
        public static IDisposable Key(string key) =>
            LogContext.PushProperty(KeyPropertyName, key);

        // Correlation scope (unchanged)
        /// <summary>
        /// Pushes a <c>CorrelationId</c> property for the lifetime of the returned scope.
        /// </summary>
        /// <param name="correlationId">Correlation id to attach to subsequent logs.</param>
        /// <returns>An <see cref="IDisposable"/> scope that must be disposed (use <c>using</c>).</returns>
        /// <remarks>
        /// Use early in an operation (e.g., HTTP handler, background job) to tag all logs with the same CorrelationId.
        /// Example: <c>using (AppLog.Correlation(cid)) { AppLog.Info("Start"); }</c>
        /// </remarks>
        public static IDisposable Correlation(string correlationId) =>
            LogContext.PushProperty("CorrelationId", correlationId);

        /// <summary>
        /// Pushes a single property into the log context for the lifetime of the returned scope.
        /// </summary>
        /// <param name="name">Property name.</param>
        /// <param name="value">Property value.</param>
        /// <param name="destructure">When true, serialize complex objects.</param>
        /// <returns>Disposable scope to remove the property when disposed.</returns>
        /// <remarks>
        /// Use to attach per-operation metadata (TenantId, RequestId, etc.).
        /// Example: <c>using (AppLog.Push("TenantId", tenantId)) { ... }</c>
        /// </remarks>
        public static IDisposable Push(string name, object value, bool destructure = false) =>
            LogContext.PushProperty(name, value, destructure);

        /// <summary>
        /// Pushes multiple properties into the log context for the lifetime of the returned scope.
        /// </summary>
        /// <param name="props">Dictionary of properties to push.</param>
        /// <returns>Disposable scope that removes all properties on dispose.</returns>
        /// <remarks>
        /// Use when you have multiple related properties to attach at once.
        /// Example:
        /// <code>
        /// using (AppLog.Push(new Dictionary&lt;string,object&gt; {
        ///     ["TenantId"] = tenantId, ["UserId"] = userId
        /// })) { ... }
        /// </code>
        /// </remarks>
        public static IDisposable Push(IDictionary<string, object> props)
        {
            if (props == null || props.Count == 0) return DummyScope.Instance;
            var disposables = new List<IDisposable>(props.Count);
            foreach (var kv in props)
                disposables.Add(LogContext.PushProperty(kv.Key, kv.Value));
            return new CompositeScope(disposables);
        }

        // ---------- Info ----------

        /// <summary>
        /// Writes an Information log with a static message.
        /// </summary>
        /// <param name="template">Message template (no parameters).</param>
        /// <remarks>
        /// Use for simple status lines without parameters.
        /// Example: <c>AppLog.Info("Service is ready");</c>
        /// </remarks>
        public static void Info(string template) => Log.Information(template);

        /// <summary>
        /// Writes an Information log using a message template and parameters.
        /// </summary>
        /// <param name="template">Message template using Serilog placeholders, e.g. "Processed {Id}".</param>
        /// <param name="args">Template parameters.</param>
        /// <remarks>
        /// Prefer named placeholders for structured data over string interpolation.
        /// Example: <c>AppLog.Info("Processed {OrderId} in {ElapsedMs} ms", id, elapsed);</c>
        /// </remarks>
        public static void Info(string template, params object[] args) => Log.Information(template, args);

        /// <summary>
        /// Writes an Information log with a strongly-typed payload as a structured property.
        /// </summary>
        /// <typeparam name="TPayload">Type of the payload.</typeparam>
        /// <param name="template">Message template.</param>
        /// <param name="payload">Object to attach as <c>Payload</c>.</param>
        /// <param name="destructure">When true, serialize the object (recommended).</param>
        /// <remarks>
        /// Use when you want to include a whole object (e.g., request/command DTO) as structured data.
        /// Example: <c>AppLog.Info("Payment created", new { Id = id, Amount = amount });</c>
        /// </remarks>
        public static void Info<TPayload>(string template, TPayload payload, bool destructure = true) =>
            Log.ForContext("Payload", payload, destructureObjects: destructure).Information(template);

        /// <summary>
        /// Information log tagged with a Key property (single call, no scope).
        /// </summary>
        public static void InfoKey(string key, string template, params object[] args) =>
            Log.ForContext(KeyPropertyName, key).Information(template, args);

        public static void InfoKey<TPayload>(string key, string template, TPayload payload, bool destructure = true) =>
            Log.ForContext(KeyPropertyName, key)
               .ForContext("Payload", payload, destructureObjects: destructure)
               .Information(template);

        // ---------- Debug ----------

        /// <summary>
        /// Writes a Debug log with template and parameters.
        /// </summary>
        /// <param name="template">Message template.</param>
        /// <param name="args">Template parameters.</param>
        /// <remarks>
        /// Use for verbose/internal diagnostics; typically disabled in production.
        /// Example: <c>AppLog.Debug("Fetched {Count} items", count);</c>
        /// </remarks>
        public static void Debug(string template, params object[] args) => Log.Debug(template, args);

        public static void DebugKey(string key, string template, params object[] args) =>
            Log.ForContext(KeyPropertyName, key).Debug(template, args);

        // ---------- Warning ----------

        /// <summary>
        /// Writes a Warning log with a static message.
        /// </summary>
        /// <param name="template">Message template.</param>
        /// <remarks>
        /// Use for unexpected but handled conditions.
        /// Example: <c>AppLog.Warn("Cache miss");</c>
        /// </remarks>
        public static void Warn(string template) => Log.Warning(template);

        /// <summary>
        /// Writes a Warning log with template and parameters.
        /// </summary>
        /// <param name="template">Message template.</param>
        /// <param name="args">Template parameters.</param>
        /// <remarks>
        /// Example: <c>AppLog.Warn("Retrying {Attempt} of {Max}", attempt, max);</c>
        /// </remarks>
        public static void Warn(string template, params object[] args) => Log.Warning(template, args);

        /// <summary>
        /// Writes a Warning log and attaches a structured <c>Payload</c>.
        /// </summary>
        /// <typeparam name="TPayload">Type of the payload.</typeparam>
        /// <param name="template">Message template.</param>
        /// <param name="payload">Object to attach.</param>
        /// <param name="destructure">When true, serialize the object (recommended).</param>
        /// <remarks>
        /// Use to include context data that explains the warning condition.
        /// Example: <c>AppLog.Warn("Validation warnings", validationResult);</c>
        /// </remarks>
        public static void Warn<TPayload>(string template, TPayload payload, bool destructure = true) =>
            Log.ForContext("Payload", payload, destructureObjects: destructure).Warning(template);

        public static void WarnKey(string key, string template, params object[] args) =>
            Log.ForContext(KeyPropertyName, key).Warning(template, args);

        public static void WarnKey<TPayload>(string key, string template, TPayload payload, bool destructure = true) =>
            Log.ForContext(KeyPropertyName, key)
               .ForContext("Payload", payload, destructureObjects: destructure)
               .Warning(template);

        // ---------- Error ----------

        /// <summary>
        /// Writes an Error log with an exception and a static message.
        /// </summary>
        /// <param name="ex">The exception.</param>
        /// <param name="template">Message template.</param>
        /// <remarks>
        /// Use on handled failures where you need the stack trace.
        /// Example: <c>AppLog.Error(ex, "Failed to load config");</c>
        /// </remarks>
        public static void Error(Exception ex, string template) => Log.Error(ex, template);

        /// <summary>
        /// Writes an Error log with an exception, template, and parameters.
        /// </summary>
        /// <param name="ex">The exception.</param>
        /// <param name="template">Message template.</param>
        /// <param name="args">Template parameters.</param>
        /// <remarks>
        /// Use to add identifiers (e.g., {OrderId}) along with the exception.
        /// Example: <c>AppLog.Error(ex, "Payment {Id} failed", id);</c>
        /// </remarks>
        public static void Error(Exception ex, string template, params object[] args) => Log.Error(ex, template, args);

        /// <summary>
        /// Writes an Error log with an exception and a structured <c>Payload</c>.
        /// </summary>
        /// <typeparam name="TPayload">Type of the payload.</typeparam>
        /// <param name="ex">The exception.</param>
        /// <param name="template">Message template.</param>
        /// <param name="payload">Object to attach.</param>
        /// <param name="destructure">When true, serialize the object (recommended).</param>
        /// <remarks>
        /// Use when the payload provides key context for diagnosing the error.
        /// Example: <c>AppLog.Error(ex, "Order creation failed", orderRequest);</c>
        /// </remarks>
        public static void Error<TPayload>(Exception ex, string template, TPayload payload, bool destructure = true) =>
            Log.ForContext("Payload", payload, destructureObjects: destructure).Error(ex, template);

        public static void ErrorKey(string key, Exception ex, string template, params object[] args) =>
            Log.ForContext(KeyPropertyName, key).Error(ex, template, args);

        public static void ErrorKey<TPayload>(string key, Exception ex, string template, TPayload payload, bool destructure = true) =>
            Log.ForContext(KeyPropertyName, key)
               .ForContext("Payload", payload, destructureObjects: destructure)
               .Error(ex, template);

        // ---------- Fatal ----------

        /// <summary>
        /// Writes a Fatal log (service-ending failures).
        /// </summary>
        /// <param name="ex">The critical exception.</param>
        /// <param name="template">Message template.</param>
        /// <param name="args">Template parameters.</param>
        /// <remarks>
        /// Use sparingly for unrecoverable errors (process shutdown, critical initialization failures).
        /// Example: <c>AppLog.Fatal(ex, "Critical startup failure");</c>
        /// </remarks>
        public static void Fatal(Exception ex, string template, params object[] args) => Log.Fatal(ex, template, args);

        public static void FatalKey(string key, Exception ex, string template, params object[] args) =>
            Log.ForContext(KeyPropertyName, key).Fatal(ex, template, args);

        // ---------- Internal helper scopes ----------

        private sealed class DummyScope : IDisposable
        {
            public static readonly DummyScope Instance = new();
            public void Dispose() { }
        }

        private sealed class CompositeScope : IDisposable
        {
            private readonly IReadOnlyList<IDisposable> _scopes;
            public CompositeScope(IReadOnlyList<IDisposable> scopes) => _scopes = scopes;
            public void Dispose()
            {
                for (int i = _scopes.Count - 1; i >= 0; i--) _scopes[i].Dispose();
            }
        }
    }
}