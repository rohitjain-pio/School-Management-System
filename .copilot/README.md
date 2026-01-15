# .copilot Folder - README

## What is This Folder?

The `.copilot/` folder is an **AI context persistence system** that allows GitHub Copilot (and other AI assistants) to maintain consistent understanding of your project across chat sessions.

Think of it as a **"knowledge base"** that the AI reads before helping you with code.

---

## Why Do We Need This?

### Problem Without `.copilot/`
- ❌ AI forgets project context between sessions
- ❌ Must re-explain architecture, security patterns, tech stack every time
- ❌ AI might make decisions that violate project rules
- ❌ No consistency in code generation patterns
- ❌ Repeat the same questions every session

### Solution With `.copilot/`
- ✅ AI instantly knows project identity and goals
- ✅ AI follows critical security rules (SchoolId isolation)
- ✅ AI uses correct tech stack and patterns
- ✅ AI knows what it can decide autonomously vs. ask first
- ✅ AI has troubleshooting guides for common errors
- ✅ Consistent code quality across sessions

---

## Folder Structure

```
.copilot/
├── project-identity.md          # Who we are, what we're building
├── critical-rules.md            # Non-negotiable security & architecture rules
├── tech-stack.md                # Complete technology inventory
├── current-status.md            # Real-time project status (62% complete)
├── daily-focus.md               # Today's priorities (Jan 15: SchoolId isolation)
│
├── context/                     # Core architectural patterns
│   ├── multi-tenancy-pattern.md # SchoolId isolation everywhere
│   ├── security-patterns.md     # JWT, auth, encryption
│   ├── database-schema.md       # (To be created)
│   ├── api-conventions.md       # (To be created)
│   └── frontend-architecture.md # (To be created)
│
├── workflows/                   # Step-by-step guides
│   ├── add-new-controller.md    # Create API controller (60 min)
│   ├── add-new-table.md         # (To be created)
│   ├── add-new-api-endpoint.md  # (To be created)
│   └── security-review-checklist.md # (To be created)
│
├── troubleshooting/             # Error resolution guides
│   ├── common-errors.md         # Quick lookup table
│   ├── schoolid-issues.md       # (To be created)
│   ├── jwt-auth-issues.md       # (To be created)
│   └── migration-failures.md    # (To be created)
│
├── decisions/                   # Decision frameworks
│   ├── ai-authority-matrix.md   # What AI can decide autonomously
│   ├── architecture-decisions.md # (To be created)
│   └── rejected-approaches.md   # (To be created)
│
├── memory/                      # Project learnings
│   ├── learned-patterns.md      # (To be created)
│   ├── gotchas.md              # (To be created)
│   └── optimization-wins.md    # (To be created)
│
├── goals/                       # Implementation roadmap
│   ├── mvp-roadmap.md          # (To be created)
│   ├── week-1-security.md      # (To be created)
│   ├── week-2-features.md      # (To be created)
│   └── week-4-deployment.md    # (To be created)
│
├── agents/                      # Specialized AI agents
│   ├── backend-agent.md        # (To be created)
│   ├── frontend-agent.md       # (To be created)
│   ├── database-agent.md       # (To be created)
│   └── security-agent.md       # (To be created)
│
└── knowledge/                   # Best practices
    ├── dotnet-best-practices.md # (To be created)
    ├── react-patterns.md        # (To be created)
    └── indian-compliance.md     # (To be created)
```

---

## How AI Uses This Folder

### Session Start
1. AI reads `project-identity.md` → Understands project goals
2. AI reads `critical-rules.md` → Knows what NEVER to do
3. AI reads `current-status.md` → Knows progress (62% complete)
4. AI reads `daily-focus.md` → Knows today's priorities

### During Development
5. AI references `context/multi-tenancy-pattern.md` → Writes SchoolId-safe code
6. AI references `workflows/add-new-controller.md` → Follows step-by-step guide
7. AI references `decisions/ai-authority-matrix.md` → Knows when to ask vs. proceed

### When Errors Occur
8. AI checks `troubleshooting/common-errors.md` → Finds quick fix
9. AI applies fix autonomously (if low risk)
10. AI asks user if unsure (medium/high risk)

### Learning & Improvement
11. AI logs lessons in `memory/learned-patterns.md`
12. AI updates decision matrix based on user feedback

---

## Key Files Explained

### 📄 project-identity.md
**Purpose:** Foundation of who we are  
**Contents:**
- Project name, type, target market
- Launch date (Feb 13, 2026)
- Technology stack (.NET 9, React 18, SQL Server)
- User roles (SuperAdmin, SchoolAdmin, Teacher, etc.)

**When AI Uses It:** Every session start, to understand context

---

### 📄 critical-rules.md
**Purpose:** Non-negotiable rules  
**Contents:**
- ✅ Always filter by SchoolId
- ✅ Inherit from BaseSchoolController
- ✅ Never accept SchoolId from request body
- ❌ Never remove SchoolId filters
- ❌ Never hardcode secrets

**When AI Uses It:** Before generating any code

---

### 📄 current-status.md
**Purpose:** Real-time project state  
**Contents:**
- MVP progress: 62% complete
- Week 1, Day 3 (Jan 15, 2026)
- Today's focus: SchoolId isolation implementation
- Blockers: SchoolId isolation not yet implemented
- Next milestone: End of Week 1

**When AI Uses It:** To know what's done vs. pending

---

### 📄 daily-focus.md
**Purpose:** Today's work plan  
**Contents:**
- 9-hour task breakdown (morning, afternoon, evening)
- Database migration steps
- Code implementation steps
- Testing checklist
- Definition of done

**When AI Uses It:** To prioritize today's work

---

### 📄 tech-stack.md
**Purpose:** Complete technology inventory  
**Contents:**
- Backend: .NET 9.0, EF Core, SQL Server
- Frontend: React 18, TypeScript, Vite, Tailwind
- Auth: JWT Bearer tokens
- Real-time: SignalR
- Cloud: Azure (App Service, SQL DB, Blob Storage)

**When AI Uses It:** To generate code with correct packages/syntax

---

### 📁 context/multi-tenancy-pattern.md
**Purpose:** Core architectural pattern  
**Contents:**
- SchoolId on every table (except system tables)
- JWT token with SchoolId claim
- SchoolIsolationMiddleware pattern
- BaseSchoolController pattern
- Repository pattern with SchoolId filtering
- Code examples (what to do vs. what not to do)

**When AI Uses It:** Before writing any database query or controller

---

### 📁 context/security-patterns.md
**Purpose:** Security best practices  
**Contents:**
- JWT token generation
- Role-based authorization
- Input validation (FluentValidation)
- SQL injection prevention
- XSS/CSRF protection
- Password security
- Audit logging

**When AI Uses It:** When implementing authentication, authorization, or data access

---

### 📁 workflows/add-new-controller.md
**Purpose:** Step-by-step controller creation  
**Contents:**
- Complete controller template
- Swagger documentation template
- Service registration
- Testing steps (Postman + unit tests)
- Security testing (cross-school access)
- Time estimate: 60 minutes

**When AI Uses It:** When user asks "create a new controller for X"

---

### 📁 decisions/ai-authority-matrix.md
**Purpose:** Decision-making framework  
**Contents:**
- ✅ What AI can decide autonomously (bug fixes, code generation)
- ⚠️ What AI should ask first (architecture changes, business logic)
- ❌ What AI should NEVER do (delete production data, deploy)

**When AI Uses It:** Before making any significant decision

---

### 📁 troubleshooting/common-errors.md
**Purpose:** Quick error resolution  
**Contents:**
- Quick lookup table (error → fix → time)
- SchoolId missing → Check JWT claims → 5 min
- CORS error → Update Program.cs → 3 min
- Migration failed → Check pending migrations → 10 min
- Detailed fix steps with code examples

**When AI Uses It:** When compilation errors, runtime errors, or test failures occur

---

## How to Update This Folder

### Daily Updates
```markdown
# At end of each day, update:
1. current-status.md → Mark completed tasks
2. daily-focus.md → Create tomorrow's plan
3. memory/learned-patterns.md → Log any new learnings
```

### Weekly Updates
```markdown
# At end of each week:
1. current-status.md → Update MVP progress percentage
2. goals/week-X.md → Mark week complete, plan next week
```

### When Things Change
```markdown
# Update immediately when:
1. tech-stack.md → New package added
2. critical-rules.md → New security rule discovered
3. decisions/rejected-approaches.md → Tried something that didn't work
```

---

## Benefits for Your Project

### 1. Faster Development
- AI generates code that follows your patterns
- No need to re-explain architecture every time
- Step-by-step workflows reduce errors

### 2. Better Code Quality
- AI enforces critical rules (SchoolId isolation)
- Consistent patterns across all controllers
- Security-first approach baked in

### 3. Easier Onboarding
- New developers read `.copilot/` folder
- AI helps new developers write correct code
- Documentation always up-to-date

### 4. Reduced Errors
- AI checks security before generating code
- Quick troubleshooting guides save time
- Learned patterns prevent repeat mistakes

### 5. AI Autonomy
- AI works independently on low-risk tasks
- AI asks when unsure (medium risk)
- AI never does dangerous things (high risk)

---

## Files Created (10/50+)

✅ **Created:**
1. `project-identity.md` - Project foundation
2. `critical-rules.md` - Security rules
3. `tech-stack.md` - Technology inventory
4. `current-status.md` - Real-time status
5. `daily-focus.md` - Today's work plan
6. `context/multi-tenancy-pattern.md` - Core pattern
7. `context/security-patterns.md` - Security guide
8. `workflows/add-new-controller.md` - Controller workflow
9. `decisions/ai-authority-matrix.md` - Decision framework
10. `troubleshooting/common-errors.md` - Error guide

🔄 **To Be Created:**
- Database schema reference
- API conventions
- Frontend architecture
- More workflows (add table, add endpoint)
- More troubleshooting guides
- Specialized agent instructions
- Best practices guides
- Memory/learning logs
- Weekly goal breakdowns

---

## Next Steps

### Immediate (Today)
1. Start using existing files in AI prompts
2. Test AI generates code following patterns
3. Update `daily-focus.md` as tasks complete

### This Week
1. Create remaining context files
2. Add more workflows
3. Populate troubleshooting guides

### Ongoing
1. Update `current-status.md` daily
2. Log learnings in `memory/` folder
3. Refine AI decision matrix based on experience

---

## Example AI Conversation

**Before `.copilot/` folder:**
```
You: Create a new Student controller
AI: Sure! Here's a basic controller...
[Generates code WITHOUT BaseSchoolController]
[Forgets SchoolId isolation]
[Uses wrong patterns]
```

**After `.copilot/` folder:**
```
You: Create a new Student controller
AI: [Reads project-identity.md]
    [Reads critical-rules.md]
    [Reads context/multi-tenancy-pattern.md]
    [Reads workflows/add-new-controller.md]
    
    I'll create a StudentController following the BaseSchoolController pattern
    with SchoolId isolation. This will take ~60 minutes.
    
    [Generates correct code]
    [Includes security validation]
    [Adds unit tests]
    [Follows your exact patterns]
```

---

## Maintenance

**Keep it updated:** This folder is ALIVE, not static.
- Update after major changes
- Log lessons learned
- Refine AI authority matrix
- Add new troubleshooting entries

**Keep it concise:** AI reads this every session.
- Use bullet points
- Code examples over prose
- Quick reference tables
- Link to detailed docs when needed

---

## Questions?

If AI behavior doesn't match your expectations:
1. Check if relevant `.copilot/` file exists
2. Update the file with correct guidance
3. Test AI again
4. Log the improvement in `memory/learned-patterns.md`

---

**Remember:** This folder makes AI smarter about YOUR project. The more you invest in it, the more productive you become.
