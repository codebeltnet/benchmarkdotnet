using System;
using BenchmarkDotNet.Configs;
using Cuemon;

namespace Codebelt.Extensions.BenchmarkDotNet
{
    /// <summary>
    /// Extension methods for the <see cref="BenchmarkWorkspaceOptions"/> class.
    /// </summary>
    public static class BenchmarkWorkspaceOptionsExtensions
    {
        /// <summary>
        /// Configures the BenchmarkDotNet configuration for the specified <paramref name="options"/>.
        /// </summary>
        /// <param name="options">The <see cref="BenchmarkWorkspaceOptions"/> to extend.</param>
        /// <param name="configure">The function delegate that configures the <see cref="IConfig"/>.</param>
        /// <returns>The <paramref name="options"/> instance for chaining.</returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="options"/> cannot be null -or-
        /// <paramref name="configure"/> cannot be null.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// <paramref name="configure"/> must not return null.
        /// </exception>
        /// <remarks>
        /// <para>
        /// BenchmarkDotNet's configuration model is intentionally immutable-ish: methods such as
        /// <c>AddJob</c>, <c>AddColumn</c>, and <c>AddDiagnoser</c> do <strong>not</strong> mutate the
        /// incoming <see cref="IConfig"/> instance. Instead, they produce and return a <em>new</em>
        /// configuration object. This behavior is powerful but can be unintuitive when used inside
        /// option delegates where callers naturally expect fluent configuration to modify the
        /// underlying options instance.
        /// </para>
        /// <para>
        /// Without this helper, callers would need to explicitly reassign:
        /// </para>
        /// <code>
        /// options.Configuration = options.Configuration.AddJob(job);
        /// </code>
        /// <para>
        /// This extension method abstracts away that requirement by:
        /// </para>
        /// <list type="bullet">
        ///     <item>
        ///         <description>Forcing initialization of the default configuration if needed,</description>
        ///     </item>
        ///     <item>
        ///         <description>Passing the current configuration to the delegate for fluent BenchmarkDotNet operations,</description>
        ///     </item>
        ///     <item>
        ///         <description>Assigning the delegate’s returned configuration back to <see cref="BenchmarkWorkspaceOptions.Configuration"/>.</description>
        ///     </item>
        /// </list>
        /// <para>
        /// This preserves BenchmarkDotNet's design while providing an intuitive experience for users configuring <see cref="BenchmarkWorkspaceOptions"/> via delegates.
        /// </para>
        /// </remarks>
        public static BenchmarkWorkspaceOptions ConfigureBenchmarkDotNet(this BenchmarkWorkspaceOptions options, Func<IConfig, IConfig> configure)
        {
            Validator.ThrowIfNull(options);
            Validator.ThrowIfNull(configure);

            var current = options.Configuration; // forces default config if needed
            var updated = configure(current) ?? throw new InvalidOperationException("Configuration delegate must not return null.");

            options.Configuration = updated;

            return options;
        }
    }
}
