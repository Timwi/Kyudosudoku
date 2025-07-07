﻿// This file is used by Code Analysis to maintain SuppressMessage
// attributes that are applied to this project.
// Project-level suppressions either have no target or are given
// a specific target and scoped to a namespace, type, member, etc.

using System.Diagnostics.CodeAnalysis;

[assembly: SuppressMessage("Style", "IDE0220:Add explicit cast", Justification = "This is fine")]
[assembly: SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "Let me name things how I want")]
[assembly: SuppressMessage("Style", "IDE0053:Use expression body for lambda expression", Justification = "Not if it isn’t a pure expression please")]
[assembly: SuppressMessage("Style", "IDE0305:Simplify collection initialization", Justification = "Let me use .ToArray()/.ToList()")]
