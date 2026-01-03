
# Pre-Implementation Validation

## Mandatory Confidence Check

**Before implementing ANY new feature or non-trivial change**, you MUST run the confidence-check skill:

## When to Apply

**ALWAYS run confidence-check for**:
- New feature implementation (any size)
- Architecture changes
- Adding new dependencies or libraries
- Multi-file modifications (>3 files)
- Complex bug fixes requiring investigation

**MAY skip for**:
- Trivial fixes (<5 lines, single file, no new dependencies)
- Documentation-only changes
- Simple typo corrections

## Validation Criteria

Confidence check validates:
1. **Duplicate Check**: No existing functionality that accomplishes the same goal
2. **Architecture Compliance**: Approach matches existing project patterns
3. **Official Docs Verification**: Dependencies have official documentation
4. **OSS References**: Libraries are well-maintained and trustworthy
5. **Root Cause Identification**: (For bug fixes) True cause is understood

## Workflow

```
Read Requirements
    ↓
Run /confidence-check
    ↓
Confidence ≥ 90%?
    ├─ YES → Create TodoWrite → Implement
    └─ NO  → Clarify requirements or abort
```

## Enforcement

If you skip confidence-check for a non-trivial change:
- Document the reason in commit message
- Create retrospective explaining the decision
- Accept responsibility for any rework needed

## Rationale

Confidence-check exists to:
- Catch duplicate effort before implementation
- Validate architectural alignment early
- Ensure proper research of dependencies
- Reduce rework and technical debt
- Save development time and tokens
