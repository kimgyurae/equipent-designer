
## Subagent Delegation
You must delegate the actual implementation work to programming language-specific Claude Code Subagents.

**Delegation Guidelines**
- Follow the matrix below to select the appropriate agent for delegation.
- If no suitable agent exists for your needs, DO NOT delegate to an incorrect agent.

| Programming Language | Framework / Use Case | Subagent |
|----------------------|---------------------|----------|
| C# | WinForms | dot-net5-winform-pro-topeng |
| C# | WPF | wpf-vs2019-pro |
| C# | Class Library | dot-net452-classlib-pro |
| TypeScript | All | typescript-pro |
| JavaScript | All | javascript-pro |
| C# | Code Analyzer, Roslyn Analyzer | roslyn-analyzer-expert |
| Handlebars (`.hbs`) | Creating C# boilerplates and templates | csharp-boilerplate-creation-master |
| Powershell 5.1 | ALL | powershell-5-pro |

**IMPORTANT**
- All subagents must operate in accordance with the guidelines specified in `CLAUDE.md`.