# Reviewer Agent - User Preferences

## User Identity
- Name: [User name or role]
- Experience level: [Beginner / Intermediate / Expert]
- Review role: [Lead reviewer / Team member / Occasional reviewer]

## Review Style Preferences

### Depth of Review
- Level of detail: [Line by line / High-level only / Balanced]
- Time spent per file: [5 min / 15 min / 30 min / Until perfect]

### Severity Tolerance
- Critical issues: [Must fix / Can discuss / Optional]
- Major issues: [Must fix / Should fix / Suggest only]
- Minor issues: [Fix if time / Note only / Ignore]

### What to Prioritize
Rank from 1 (most important) to 6 (least):
- Correctness: [Rank]
- Performance: [Rank]
- Security: [Rank]
- Maintainability: [Rank]
- Style: [Rank]
- Documentation: [Rank]

## Communication Preferences

### Review Tone
- Preferred tone: [Direct / Diplomatic / Encouraging / Strict]
- Use of emojis: [Yes / No / Only for emphasis]
- Praise for good code: [Always / Sometimes / Never]

### Feedback Format
- Issue location: [File and line number / File only / Description only]
- Code examples: [Always provide / Only for complex issues / Never]
- Explanation depth: [Detailed why / Brief summary / Just the fix]

### Comment Style
- For critical issues: [Bold / Prefix with CRITICAL / Normal text]
- For questions: [Phrase as question / Assume ignorance / Challenge decision]

## Review Focus Areas

### Always Check
- [ ] Null reference possibilities
- [ ] Event subscription cleanup
- [ ] Performance in Update methods
- [ ] Edge case handling

### Sometimes Check
- [ ] Naming conventions
- [ ] XML documentation
- [ ] Code duplication
- [ ] Test coverage

### Rarely Check
- [ ] Comment quality
- [ ] Import organization
- [ ] Whitespace
- [ ] Variable naming style

## Design Doc Alignment

### Check Design Docs
- When: [Always / Only for new features / Only when requested]
- Strictness: [Must match exactly / Should be close / Rough guideline only]

### Deviation Handling
- When code differs from design: [Request code change / Request doc update / Ask for clarification]

## Testing Review Preferences

### Test Review Priority
- Test existence: [Required / Nice to have / Optional]
- Test quality: [Review thoroughly / Quick check / Ignore]
- Test coverage: [Must meet target / Suggest improvements / Not reviewed]

### What to Check in Tests
- [ ] Edge cases covered
- [ ] Assertions meaningful
- [ ] No test dependencies
- [ ] Tests are deterministic

## Performance Review

### Performance Standards
- Acceptable violations: [None / Documented exceptions only / Any with justification]

### Performance Review Depth
- Flag ALL Update allocations: [Yes / Only major ones / No]
- Benchmark required: [For gameplay code / For hot paths / Not required]

## Approval Criteria

### Merge Conditions
- Approve when: [No criticals / No criticals or majors / All issues fixed]
- Who can override: [Only user / Team lead / Anyone]
- Re-review after fixes: [Always required / Only for criticals / Optional]

### Partial Approval
- Approve with comments: [Allowed / Not allowed]
- Request changes: [For any issue / Only critical / Only critical or major]

## Process Preferences

### Review Speed
- Expected turnaround: [Within 1 hour / Same day / 24 hours]
- Reminders: [Send if overdue / Don't send]

### Follow-up
- Check fixes: [Automatically / Only when requested / Never]
- Verify in branch: [Yes / No]
- Re-review whole file: [Yes / Only changed lines / No]

## Learning Preferences

### New Issue Patterns
- When learning new pattern: [Add to memory.md / Just handle case / Ask for clarification]

### Feedback on Reviews
- Accept review feedback on my reviews: [Yes / No / Only from lead]

## Notification Preferences

### Review Assignment
- Notify me: [Immediately / Daily digest / Don't notify]
- Via: [In chat / Email / Both]

### Review Completion
- Notify author: [Yes / No]
- Notify team: [Yes / No]
- Summary in chat: [Yes / No]