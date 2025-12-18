// This file is used by Code Analysis to maintain SuppressMessage
// attributes that are applied to this project.
// Project-level suppressions either have no target or are given
// a specific target and scoped to a namespace, type, member, etc.

using System.Diagnostics.CodeAnalysis;

[assembly: SuppressMessage("Performance", "CA1859:Use concrete types when possible for improved performance", Justification = "Using abstractions in public APIs promotes encapsulation, testability, and long-term maintainability, as prescribed by the .NET Framework Design Guidelines.", Scope = "member", Target = "~M:Codebelt.Extensions.BenchmarkDotNet.BenchmarkWorkspace.LoadAssemblies(System.String,System.String,System.String,System.String,System.Boolean)~System.Collections.Generic.IEnumerable{System.Reflection.Assembly}")]
