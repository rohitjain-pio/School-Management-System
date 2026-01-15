# Generic Prompt: Create Comprehensive AI Context System

**Purpose:** Create a `.copilot/` folder structure that gives GitHub Copilot deep context about your project, patterns, and standards.

**Use this prompt in any new workspace to replicate this system.**

---

## 📋 Prompt for AI Assistant

```
Create a comprehensive `.copilot/` folder system for this workspace to provide GitHub Copilot with deep project context. The system should include:

## 1. PROJECT OVERVIEW (4-5 files in `.copilot/`)

Create these foundational files:

1. **project-overview.md** - High-level summary
   - Technology stack (languages, frameworks, databases)
   - Architecture type (monolith, microservices, multi-tenant, etc.)
   - Key features and business domain
   - Team size and structure
   - Deployment environment (cloud provider, CI/CD)

2. **tech-stack.md** - Complete technology inventory
   - Frontend: Frameworks, UI libraries, state management
   - Backend: Language, framework, ORM, authentication
   - Database: Type, version, key features used
   - Infrastructure: Cloud services, Docker, Kubernetes
   - Dev Tools: Testing frameworks, linters, formatters

3. **architecture-decisions.md** - Why we chose what we chose
   - Major architectural decisions (with rationale)
   - Trade-offs made
   - Alternatives considered and rejected
   - Future migration plans

4. **daily-focus.md** - Current sprint/work
   - Today's priority tasks
   - Current sprint goals
   - Known blockers
   - Team member assignments

5. **critical-rules.md** - Non-negotiable standards
   - Security requirements (authentication, data isolation, etc.)
   - Code patterns that MUST be followed
   - Common mistakes that cause production issues
   - Compliance requirements (GDPR, HIPAA, etc.)

## 2. CONTEXT FILES (4-5 files in `.copilot/context/`)

Document your specific patterns:

1. **{domain}-pattern.md** - Key business logic patterns
   - Example: `multi-tenancy-pattern.md`, `payment-pattern.md`
   - How your core business logic works
   - Data flow diagrams
   - Edge cases and gotchas

2. **naming-conventions.md** - Consistent naming
   - File naming (kebab-case, PascalCase, etc.)
   - Variable naming patterns
   - Database naming (tables, columns, indexes)
   - API endpoint naming

3. **code-structure.md** - Project organization
   - Folder structure explanation
   - Where to put new files
   - Layered architecture (controllers, services, repositories)
   - Import/dependency patterns

4. **error-handling-strategy.md** - How errors are managed
   - Global error handlers
   - Exception hierarchy
   - Logging strategy
   - User-facing error messages

## 3. SPECIALIZED AGENTS (4-6 files in `.copilot/agents/`)

Create expert personas for different domains:

1. **backend-agent.md** - Backend specialist
   - Responsibilities: API endpoints, business logic, database queries
   - Code generation patterns (controllers, services, repositories)
   - Security rules specific to backend
   - Testing standards (unit tests, integration tests)
   - Performance standards (query optimization, caching)
   - Usage: "Backend-agent: Create UserController with CRUD operations"

2. **frontend-agent.md** - Frontend specialist
   - Responsibilities: UI components, forms, API integration
   - Component patterns (functional, class, hooks)
   - State management approach
   - Form validation strategy
   - Accessibility standards
   - Usage: "Frontend-agent: Create user registration form with validation"

3. **database-agent.md** - Database specialist
   - Responsibilities: Schema design, migrations, query optimization
   - Indexing strategy
   - Migration patterns (safe column additions, data migrations)
   - Query performance standards
   - Usage: "Database-agent: Create Users table with proper indexes"

4. **security-agent.md** - Security specialist
   - Responsibilities: Vulnerability audits, penetration testing, compliance
   - Critical security rules (authentication, authorization, data isolation)
   - Common vulnerabilities to check (SQL injection, XSS, CSRF)
   - Security testing patterns
   - Usage: "Security-agent: Audit PaymentController for vulnerabilities"

5. **devops-agent.md** (optional) - Infrastructure specialist
   - Deployment workflows
   - Docker/Kubernetes configurations
   - CI/CD pipeline patterns
   - Monitoring and alerting

6. **testing-agent.md** (optional) - QA specialist
   - Test strategy (unit, integration, e2e)
   - Test naming conventions
   - Mock/stub patterns
   - Coverage requirements

## 4. MEMORY SYSTEM (4-5 files in `.copilot/memory/`)

Document learnings over time:

1. **learned-patterns.md** - Successful solutions
   - Pattern name and description
   - Code examples (before/after if applicable)
   - When to use this pattern
   - Performance impact
   - Entry format:
     ```markdown
     ## Pattern Name
     **Problem:** What this solves
     **Solution:** How it works
     **Code Example:** [show code]
     **Impact:** Performance/maintainability improvement
     ```

2. **gotchas.md** - Common mistakes
   - Mistake with severity rating (Critical, High, Medium, Low)
   - Why it happens
   - How to fix it
   - How to prevent it
   - Automated detection method (grep/search pattern)
   - Entry format:
     ```markdown
     ## 🔴 CRITICAL: Mistake Title
     **Problem:** What goes wrong
     **Example (Wrong):** [bad code]
     **Example (Correct):** [good code]
     **Detection:** [search pattern]
     **Fix Time:** X minutes
     ```

3. **optimization-wins.md** - Performance improvements
   - Optimization description
   - Before/after metrics (response time, memory, cost)
   - Implementation details
   - Tools used (profiler, monitoring)
   - Entry format:
     ```markdown
     ## Optimization Name
     **Problem:** Slow query taking 450ms
     **Solution:** Added composite index
     **Before:** 450ms, 10,000 rows scanned
     **After:** 38ms, 50 rows scanned (92% faster)
     **Cost Savings:** $X/month
     ```

4. **rejected-solutions.md** - What NOT to do
   - Approach that was tried and failed
   - Why it failed (technical, complexity, team fit)
   - Time wasted
   - Alternative used instead
   - Decision criteria for future
   - Entry format:
     ```markdown
     ## Rejected: Solution Name
     **Tried:** What we attempted
     **Why Failed:** Technical/organizational reasons
     **Time Wasted:** X days
     **Alternative:** What we use instead
     **Lesson:** Key takeaway
     ```

5. **technical-debt.md** (optional) - Known issues
   - Debt item with priority
   - Why it exists (deadline, temporary fix)
   - Impact on development
   - Remediation plan
   - Estimated effort

## 5. WORKFLOWS (4-6 files in `.copilot/workflows/`)

Step-by-step guides for common tasks:

1. **add-new-{resource}.md** - Example: `add-new-table.md`, `add-new-component.md`
   - Time estimate (e.g., 45-60 minutes)
   - Prerequisites
   - Step-by-step instructions with code templates
   - Checklist at each step
   - Common mistakes to avoid
   - Example file paths
   - Final checklist

2. **security-review-checklist.md** - Pre-merge security audit
   - Critical vulnerabilities to check (P0)
   - High-priority issues (P1)
   - Automated detection scripts
   - Manual testing procedures
   - Approval criteria

3. **performance-audit.md** - Performance review process
   - Tools needed (profiler, load testing)
   - Metrics to measure
   - Target response times
   - Optimization recommendations
   - Performance baseline documentation

4. **debugging-guide.md** - Common debugging workflows
   - How to diagnose common issues
   - Logging patterns
   - Debugging tools and techniques
   - Production troubleshooting

5. **deployment-checklist.md** - Pre-deployment verification
   - Tests to run
   - Configuration checks
   - Database migration verification
   - Rollback procedure

## 6. CODE REVIEW CHECKLISTS (3-4 files in `.copilot/code-review-checklists/`)

PR review guidelines:

1. **backend-checklist.md** - Backend code review
   - P0 (Critical): Security, data integrity
   - P1 (High): Performance, error handling
   - P2 (Medium): Code quality, testing
   - P3 (Low): Documentation, logging
   - Common issues frequency table
   - Automated check scripts

2. **frontend-checklist.md** - Frontend code review
   - Type safety (no `any` types)
   - XSS prevention
   - Accessibility (ARIA labels, keyboard nav)
   - Performance (bundle size, lazy loading)
   - Responsive design

3. **security-checklist.md** - Security-focused review
   - Authentication/authorization
   - Input validation
   - Data exposure
   - CORS configuration
   - Security headers
   - Penetration testing procedures

4. **performance-checklist.md** (optional)
   - Query optimization
   - Caching strategy
   - Bundle size
   - API response times
   - Database indexes

## 7. API CONTRACTS (3-5 files in `.copilot/api-contracts/`)

Complete API specifications:

1. **{resource}-api-contract.md** - Example: `user-api-contract.md`
   - Base URL and authentication
   - Data models (TypeScript interfaces)
   - Request/response examples (with actual JSON)
   - All endpoints (GET, POST, PUT, DELETE)
   - Query parameters and validation rules
   - Error responses (with status codes)
   - Performance targets (P50, P95, P99)
   - Security considerations
   - Rate limiting
   - Code examples (frontend + backend)

2. **authentication-contract.md** - Auth endpoints
   - Login, register, refresh token
   - JWT token structure
   - Token storage best practices
   - Password requirements
   - Account lockout policy

3. **error-responses-contract.md** - Standard error format
   - Error response structure
   - Common error codes
   - User-facing messages
   - Logging requirements

## 8. TESTING (2-3 files in `.copilot/testing/`)

Testing standards and scenarios:

1. **test-scenarios-library.md** - Common test cases
   - Multi-tenancy tests (if applicable)
   - Security tests (SQL injection, XSS, auth bypass)
   - Edge cases (empty data, large datasets, special characters)
   - Integration test patterns

2. **test-data-strategy.md** - Test data management
   - Seed data for development
   - Test fixtures
   - Mock data generators
   - Database reset procedures

## 9. ADDITIONAL FILES (Optional)

Based on your project needs:

- **dependency-management.md** - Approved/banned packages, upgrade policy
- **onboarding-guide.md** - New developer setup (Day 1, Week 1)
- **prompt-library.md** - Reusable AI prompts for common tasks
- **decision-log.md** - Architecture Decision Records (ADRs)
- **performance-baselines.md** - Target metrics for response times
- **incident-response.md** - What to do when production breaks
- **sprint-planning/*.md** - Weekly/sprint goal breakdowns

---

## IMPLEMENTATION INSTRUCTIONS

1. **Start with high-priority files** (in order):
   - project-overview.md
   - critical-rules.md
   - {your-main-domain}-pattern.md (e.g., multi-tenancy-pattern.md)
   - backend-agent.md
   - frontend-agent.md

2. **Add files incrementally** - Don't create everything at once
   - Add 3-5 files per day
   - Test that Copilot uses the context
   - Refine based on actual usage

3. **Keep files concise but complete**
   - Target: 300-800 lines per file
   - Include real code examples (not placeholders)
   - Use actual patterns from your codebase
   - Reference specific file paths

4. **Use cross-references**
   - Link related files at the bottom
   - Create a web of interconnected knowledge
   - Example: "See also: `.copilot/agents/backend-agent.md`"

5. **Maintain over time**
   - Update learned-patterns.md when you solve complex problems
   - Add to gotchas.md when bugs occur
   - Update optimization-wins.md after performance improvements
   - Revise workflows based on team feedback

6. **Measure effectiveness**
   - Track how often Copilot generates correct code on first try
   - Measure time saved on common tasks
   - Count security bugs prevented
   - Assess team onboarding speed

---

## ADAPTATION GUIDE

**For Different Project Types:**

### Web Application (Full-Stack)
- Focus on: API contracts, frontend/backend agents, security patterns
- Add: authentication-contract.md, deployment workflows
- Emphasize: Multi-layer architecture, API design, state management

### Microservices
- Focus on: Service boundaries, inter-service communication, distributed tracing
- Add: service-agent.md per service, api-gateway-pattern.md, event-driven-pattern.md
- Emphasize: Service discovery, circuit breakers, eventual consistency

### Mobile App
- Focus on: Offline-first patterns, native APIs, performance on low-end devices
- Add: platform-agent.md (iOS/Android), offline-sync-pattern.md
- Emphasize: Battery usage, data usage, app size

### Data Pipeline / ETL
- Focus on: Data transformation patterns, error handling, idempotency
- Add: pipeline-agent.md, data-quality-checks.md, backfill-strategy.md
- Emphasize: Retry logic, monitoring, data validation

### Library / SDK
- Focus on: Public API design, backward compatibility, versioning
- Add: api-design-principles.md, breaking-changes-policy.md
- Emphasize: Documentation, examples, migration guides

### DevOps / Infrastructure
- Focus on: IaC patterns, deployment automation, monitoring
- Add: terraform-patterns.md, kubernetes-patterns.md, incident-runbooks.md
- Emphasize: Idempotency, rollback procedures, disaster recovery

---

## FILE STRUCTURE SUMMARY

```
.copilot/
├── project-overview.md
├── tech-stack.md
├── architecture-decisions.md
├── daily-focus.md
├── critical-rules.md
├── naming-conventions.md
├── context/
│   ├── {domain}-pattern.md (e.g., multi-tenancy-pattern.md)
│   ├── code-structure.md
│   ├── error-handling-strategy.md
│   └── ...
├── agents/
│   ├── backend-agent.md
│   ├── frontend-agent.md
│   ├── database-agent.md
│   ├── security-agent.md
│   └── ...
├── memory/
│   ├── learned-patterns.md
│   ├── gotchas.md
│   ├── optimization-wins.md
│   ├── rejected-solutions.md
│   └── technical-debt.md
├── workflows/
│   ├── add-new-table.md
│   ├── add-frontend-component.md
│   ├── security-review-checklist.md
│   ├── performance-audit.md
│   └── ...
├── code-review-checklists/
│   ├── backend-checklist.md
│   ├── frontend-checklist.md
│   ├── security-checklist.md
│   └── ...
├── api-contracts/
│   ├── {resource}-api-contract.md
│   ├── authentication-contract.md
│   └── error-responses-contract.md
└── testing/
    ├── test-scenarios-library.md
    └── test-data-strategy.md
```

---

## EXAMPLE PROMPT FOR AI

"I have a [PROJECT TYPE] built with [TECH STACK]. Create a comprehensive `.copilot/` folder structure with:

1. Project overview and critical rules
2. Main domain pattern: [YOUR DOMAIN] (e.g., multi-tenancy, e-commerce checkout, data pipeline)
3. Specialized agents: backend, frontend, database, security
4. Memory system: learned patterns, gotchas, optimization wins, rejected solutions
5. Workflows: add new [RESOURCE], security review, performance audit
6. Code review checklists: backend, frontend, security
7. API contracts: [MAIN RESOURCES] (e.g., users, products, orders)

Focus on [YOUR SPECIFIC CONCERNS] (e.g., security, performance, scalability, team consistency).

Our team size is [X] developers, and our main challenges are [CHALLENGES]."

---

## EXPECTED BENEFITS

After implementing this system:

- **75-85% automation** on boilerplate tasks
- **90% reduction** in security bugs (caught by checklists)
- **2.5-3x faster** feature development
- **50% faster** onboarding for new developers
- **Consistent code quality** across team
- **Institutional knowledge preserved** (not lost when developers leave)
- **Reduced context switching** (AI has deep project knowledge)

---

## MAINTENANCE SCHEDULE

- **Daily:** Update daily-focus.md with current tasks
- **Weekly:** Add new entries to learned-patterns.md and gotchas.md
- **Sprint End:** Update optimization-wins.md with performance improvements
- **Monthly:** Review and update workflows based on team feedback
- **Quarterly:** Audit entire `.copilot/` folder for accuracy and relevance

---

## SUCCESS CRITERIA

Your `.copilot/` system is working well if:

1. ✅ Copilot generates 80%+ correct code on first attempt
2. ✅ New developers productive within 3 days (vs. 2 weeks)
3. ✅ Security review finds < 2 issues per PR (vs. 10+)
4. ✅ Code patterns 95%+ consistent across codebase
5. ✅ PR review time < 30 minutes (vs. 2 hours)
6. ✅ Production bugs reduced by 60%+
7. ✅ Team refers to `.copilot/` files multiple times per day

---

## TOKEN BUDGET CONSIDERATIONS

- **Small project** (< 50K lines): 40-50K tokens (~20-25 files)
- **Medium project** (50-200K lines): 80-100K tokens (~35-45 files)
- **Large project** (> 200K lines): 120-150K tokens (~50-60 files)

**Claude 3.5 Sonnet:** 200K context window  
**Optimal usage:** Use 40-60% of context for `.copilot/` files, rest for actual code

**Cost estimate:** $0.003 per 1K input tokens  
- 50K tokens = $0.15 per interaction
- ~100 interactions/month = $15/month
- **ROI:** Saves 20-30 hours/month = $2,000-$3,000 value

---

## TIPS FOR SUCCESS

1. **Use real examples** - Copy actual code from your project, not generic placeholders
2. **Be specific** - "Multi-tenant SaaS with SchoolId discriminator" not "application with users"
3. **Include metrics** - "Response time < 200ms" not "should be fast"
4. **Document failures** - rejected-solutions.md is as valuable as learned-patterns.md
5. **Make it searchable** - Use consistent headings, clear titles, code markers
6. **Test with Copilot** - Ask it to generate code using your patterns, verify it works
7. **Get team buy-in** - Make it a team resource, not individual documentation
8. **Update regularly** - Stale documentation is worse than no documentation

---

## NEXT STEPS

1. Copy this prompt
2. Adapt the [PROJECT TYPE] and [TECH STACK] sections
3. Run it with your AI assistant (Claude, ChatGPT, etc.)
4. Review generated files for accuracy
5. Add real code examples from your codebase
6. Start using with Copilot
7. Iterate based on results

**Time to create:** 4-6 hours for initial setup  
**Time to maintain:** 30 min/week  
**Payback period:** 1-2 weeks (time saved exceeds time invested)

---

**Last Updated:** January 15, 2026  
**Template Version:** 1.0
```

---

## 🎯 How to Use This Template

1. **Copy the entire prompt above** (between the triple backticks)
2. **Open a new workspace** where you want the `.copilot/` system
3. **Paste the prompt** to your AI assistant (Claude, ChatGPT, GitHub Copilot Chat)
4. **Customize the placeholders:**
   - Replace `[PROJECT TYPE]` with your project type
   - Replace `[TECH STACK]` with your actual technologies
   - Replace `[YOUR DOMAIN]` with your business domain
   - Replace `[MAIN RESOURCES]` with your API resources
5. **Let the AI generate** the folder structure
6. **Review and refine** the generated files
7. **Add real code examples** from your codebase

---

## 📊 Comparison: Different Project Types

| Element | Web App | Microservices | Mobile App | Data Pipeline | Library/SDK |
|---------|---------|---------------|------------|---------------|-------------|
| Focus | API contracts | Service boundaries | Offline-first | Data transforms | API design |
| Agents | Backend, Frontend | Service-specific | Platform (iOS/Android) | Pipeline, Data Quality | API Design |
| Patterns | Multi-tenancy | Event-driven | Sync patterns | Idempotency | Versioning |
| Priority | Security | Resilience | Performance | Reliability | Compatibility |

---

**This template was used to create 34 comprehensive files in the School Management System project.**
