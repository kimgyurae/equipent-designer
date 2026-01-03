
## Workflow You Must Follow

When instructed to update the codebase, strictly follow this workflow. **Do not stop or pause between steps** unless explicitly indicated below.


### Steps:

1. **Confidence Check**
   - **Before implementing ANY new feature or non-trivial change**, you MUST execute this step; otherwise, skip to Step 2.
   - @pre-implementation-validation.md
   - **If the check fails:**
     - Halt and report to the user
   - **If the check passes:** Proceed to Step 2

2. **Skill Lookup**
   - Review all available skills and select the relevant skills for the task
   - Actively utilize the skills at your disposal

3. **Implement TDD Workflow**
   - YOU MUST USE `test-driven-development` skill. If this skill is not invoked automatically, try to invoke again or terminate
   - Delegate work to appropriate programming language specialist subagents

4. **Remove Dead Code (If Applicable)**
   - Delegate work to appropriate subagents
   - After implementation is complete, remove all dead code from the updated files to keep the codebase clean and up-to-date

5. **Update Context Documents (Optional)**
   - Use the `context-update` skill if the code update meets any of these conditions:
     - Adds significant new functionality
     - Conflicts with existing context documentation
     - Makes existing context documentation obsolete or outdated
   - Skip this step if none of the above conditions apply

### Critical Rules:
- Complete all steps in sequence without interruption
- Only pause if Step 1 (Confidence Check) fails and requires user intervention
- Do not ask for permission or confirmation between steps

### Priority
- When updating code (including updates made via custom slash commands), you must follow all Workflow steps, described above.
