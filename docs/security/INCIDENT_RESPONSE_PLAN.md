# Security Incident Response Plan - School Management System

**Last Updated:** January 9, 2026  
**Version:** 1.0  
**Scope:** SMS API, Database, Infrastructure

---

## Table of Contents

- [Overview](#overview)
- [Incident Classification](#incident-classification)
- [Response Team](#response-team)
- [Incident Response Phases](#incident-response-phases)
- [Communication Plan](#communication-plan)
- [Specific Incident Scenarios](#specific-incident-scenarios)
- [Post-Incident Activities](#post-incident-activities)
- [Tools and Resources](#tools-and-resources)
- [Contact Information](#contact-information)

---

## Overview

### Purpose

This document outlines the procedures for responding to security incidents affecting the School Management System (SMS). The goal is to minimize damage, restore normal operations quickly, and prevent future incidents.

### Scope

**Covered Incidents:**
- Data breaches (unauthorized access to student records)
- Account compromises (stolen credentials)
- Malware infections
- Denial of Service (DoS) attacks
- SQL injection attacks
- Cross-Site Scripting (XSS) attacks
- API abuse
- Insider threats
- Third-party vendor breaches

**Out of Scope:**
- Physical security incidents
- Non-security IT incidents (unless they have security implications)

### Objectives

1. **Contain** the incident to prevent further damage
2. **Eradicate** the threat from the environment
3. **Recover** normal operations
4. **Learn** from the incident to improve defenses

---

## Incident Classification

### Severity Levels

| Level | Description | Response Time | Escalation |
|-------|-------------|---------------|------------|
| **P1 - Critical** | Active data breach, widespread system compromise, sensitive student data exposed | 15 minutes | Immediate: CTO, CISO, Legal |
| **P2 - High** | Suspected breach, isolated compromise, potential data exposure | 1 hour | CTO, CISO |
| **P3 - Medium** | Security control failure, unsuccessful attack attempts, suspicious activity | 4 hours | Security Team Lead |
| **P4 - Low** | Policy violations, minor security issues, informational alerts | 24 hours | Security Team |

### Severity Criteria

**P1 - Critical Examples:**
- Confirmed unauthorized access to student records (FERPA violation)
- Database breach with data exfiltration
- Ransomware encryption of production systems
- Active exploitation of zero-day vulnerability
- Complete system outage due to attack

**P2 - High Examples:**
- Multiple failed authentication attempts from single source
- Privilege escalation attempt detected
- Malware detected on production server
- Suspicious admin account activity
- Unauthorized API access patterns

**P3 - Medium Examples:**
- Security scan detecting vulnerabilities
- Failed phishing attempt against staff
- Anomalous network traffic
- Weak password usage detected
- Outdated software detected

**P4 - Low Examples:**
- Policy violation (e.g., sharing credentials)
- Security awareness training failure
- Minor configuration drift
- Informational security alerts

---

## Response Team

### Incident Response Team (IRT)

**Core Team:**

1. **Incident Commander**
   - **Role:** Lead response efforts, make critical decisions
   - **Primary:** CTO
   - **Backup:** CISO

2. **Security Lead**
   - **Role:** Technical investigation, threat analysis
   - **Primary:** Security Team Lead
   - **Backup:** Senior Security Engineer

3. **Operations Lead**
   - **Role:** System isolation, containment, recovery
   - **Primary:** DevOps Lead
   - **Backup:** Senior DevOps Engineer

4. **Communications Lead**
   - **Role:** Internal/external communications, notifications
   - **Primary:** Marketing/PR Director
   - **Backup:** Chief of Staff

5. **Legal Counsel**
   - **Role:** Legal compliance, regulatory notifications
   - **Primary:** General Counsel
   - **Backup:** Outside Counsel

**Extended Team:**

6. **Database Administrator**
   - **Role:** Database forensics, recovery
   
7. **Application Developer**
   - **Role:** Code analysis, patching

8. **HR Representative**
   - **Role:** Insider threat cases, employee communications

9. **Compliance Officer**
   - **Role:** FERPA, COPPA, GDPR compliance

### Roles and Responsibilities

**Incident Commander:**
- Declare incident severity level
- Activate response team
- Coordinate response activities
- Communicate with executives
- Authorize system changes (e.g., taking systems offline)

**Security Lead:**
- Perform initial triage
- Conduct forensic investigation
- Identify attack vectors
- Recommend containment strategies
- Document technical findings

**Operations Lead:**
- Execute containment actions
- Restore affected systems
- Implement security patches
- Monitor for continued threats

**Communications Lead:**
- Draft internal communications
- Prepare external notifications
- Coordinate with PR team
- Manage media inquiries
- Notify affected parties

**Legal Counsel:**
- Assess legal obligations
- Coordinate regulatory notifications
- Manage law enforcement interaction
- Review communications for legal risk

---

## Incident Response Phases

### Phase 1: Detection and Analysis

**Objectives:**
- Detect security incidents promptly
- Determine scope and severity
- Initiate response procedures

**Activities:**

1. **Detection Sources:**
   - Security monitoring alerts (SIEM)
   - Intrusion Detection System (IDS)
   - Audit log anomalies
   - User reports
   - Third-party notifications
   - Vulnerability scan results

2. **Initial Triage (15 minutes for P1, 1 hour for P2):**
   - Verify incident is real (not false positive)
   - Classify severity (P1-P4)
   - Identify affected systems/data
   - Determine business impact
   - Activate response team

3. **Investigation:**
   - Review audit logs
   - Analyze network traffic
   - Check database access logs
   - Review application logs
   - Interview users/witnesses

**Triage Questions:**
- What happened? (Description of incident)
- When did it happen? (Timeline)
- Where did it happen? (Affected systems)
- Who is affected? (Users, data subjects)
- How did it happen? (Attack vector)
- Why did defenses fail? (Root cause)

**Documentation:**
- Create incident ticket (Jira/ServiceNow)
- Start incident log (timestamp all actions)
- Preserve evidence (logs, screenshots)

### Phase 2: Containment

**Objectives:**
- Stop the attack from spreading
- Preserve evidence for investigation
- Minimize impact on operations

**Short-Term Containment (Immediate):**

1. **Account Compromise:**
   ```bash
   # Disable compromised account
   az ad user update --id <user-id> --account-enabled false
   
   # Revoke all sessions/tokens
   dotnet run --project TokenRevocation -- --user <user-id>
   
   # Force password reset
   ```

2. **Malware Detection:**
   ```bash
   # Isolate infected server
   # Disable network interface
   sudo ifconfig eth0 down
   
   # Take memory dump for forensics
   sudo dd if=/dev/mem of=/forensics/memory.dump bs=1M
   ```

3. **SQL Injection Attack:**
   ```csharp
   // Block malicious IP addresses
   app.UseMiddleware<IpBlockingMiddleware>();
   
   // Disable affected endpoint temporarily
   [ApiController]
   [Route("api/[controller]")]
   [ApiExplorerSettings(IgnoreApi = true)] // Hide from Swagger
   public class AffectedController : ControllerBase { }
   ```

4. **Data Breach:**
   - Revoke API keys
   - Rotate database credentials
   - Block suspicious IP ranges
   - Enable MFA for all admin accounts

**Long-Term Containment:**

1. **System Hardening:**
   - Apply security patches
   - Update security configurations
   - Strengthen access controls
   - Implement additional monitoring

2. **Evidence Preservation:**
   ```bash
   # Create forensic image
   sudo dd if=/dev/sda of=/forensics/disk-image.dd bs=4M
   
   # Calculate hash for integrity
   sha256sum /forensics/disk-image.dd > /forensics/disk-image.dd.sha256
   
   # Preserve logs
   cp -r /var/log/* /forensics/logs/
   ```

3. **Backup Verification:**
   - Verify backups are clean (not infected)
   - Test restore procedures
   - Document backup status

### Phase 3: Eradication

**Objectives:**
- Remove the threat completely
- Close the vulnerability
- Prevent recurrence

**Activities:**

1. **Remove Malware:**
   ```bash
   # Scan with multiple tools
   sudo clamscan -r / --infected --remove
   sudo rkhunter --check
   
   # Rebuild compromised systems from clean images
   ```

2. **Close Vulnerabilities:**
   - Apply security patches
   - Fix vulnerable code
   - Update configurations
   - Remove unauthorized accounts

3. **Strengthen Defenses:**
   ```csharp
   // Implement additional validation
   public class StrengthenedValidator : AbstractValidator<Request>
   {
       public StrengthenedValidator()
       {
           RuleFor(x => x.Input)
               .Must(NotContainSqlKeywords)
               .Must(NotContainScriptTags)
               .Must(BeValidLength);
       }
   }
   
   // Add rate limiting
   [RateLimit(Requests = 5, Period = 60)]
   public async Task<IActionResult> VulnerableEndpoint() { }
   ```

4. **Reset Credentials:**
   - Change all admin passwords
   - Rotate API keys
   - Regenerate JWT signing keys
   - Update database credentials
   - Rotate encryption keys (if compromised)

### Phase 4: Recovery

**Objectives:**
- Restore normal operations
- Verify systems are clean
- Resume business activities

**Activities:**

1. **System Restoration:**
   ```bash
   # Restore from clean backup
   dotnet ef database update --project SMSDataContext
   
   # Verify application integrity
   dotnet publish --configuration Release
   
   # Restart services
   sudo systemctl restart sms-api
   ```

2. **Validation:**
   - Run security scans
   - Test authentication flows
   - Verify data integrity
   - Check audit logs for anomalies

3. **Gradual Restoration:**
   - Start with non-production environments
   - Monitor for 24-48 hours
   - Roll out to production in stages
   - Maintain heightened monitoring

4. **User Communication:**
   ```
   Subject: Service Restoration - SMS Platform
   
   Dear SMS Users,
   
   We have successfully resolved the security incident reported on [DATE].
   All systems have been restored and are operating normally.
   
   As a precautionary measure, we recommend:
   - Changing your password immediately
   - Enabling multi-factor authentication
   - Reviewing your recent activity
   
   If you notice any suspicious activity, please contact support@sms.edu.
   
   Thank you for your patience.
   
   SMS Security Team
   ```

### Phase 5: Post-Incident Activity

**Objectives:**
- Learn from the incident
- Improve defenses
- Update procedures

**Activities:**

1. **Post-Incident Review (PIR):**
   - Schedule within 1 week of resolution
   - Invite all response team members
   - Document lessons learned
   - Identify improvement opportunities

2. **PIR Agenda:**
   - Timeline of events
   - What went well?
   - What could be improved?
   - Root cause analysis
   - Action items (with owners and due dates)

3. **Documentation:**
   - Final incident report
   - Technical analysis
   - Financial impact assessment
   - Regulatory notifications (if required)

4. **Process Improvements:**
   - Update runbooks
   - Improve monitoring
   - Enhance training
   - Implement new controls

---

## Communication Plan

### Internal Communications

**During Incident:**

1. **Initial Alert (Within 15 minutes):**
   - **Audience:** Incident Response Team
   - **Channel:** SMS, Email, Slack #security-incidents
   - **Content:**
     ```
     SECURITY INCIDENT - P1
     
     Type: Data Breach
     Status: Investigating
     Affected Systems: Production Database
     Impact: High
     
     War Room: conference-room-1
     Slack: #incident-12345
     
     Incident Commander: [Name]
     ```

2. **Regular Updates (Every 30 minutes for P1, hourly for P2):**
   - **Audience:** Incident Response Team, Executives
   - **Channel:** Slack, Email
   - **Content:** Status update, actions taken, next steps

3. **All-Staff Notification (If significant impact):**
   - **Audience:** All employees
   - **Channel:** Email, Slack #general
   - **Content:**
     ```
     Subject: Security Incident Notification
     
     We are currently responding to a security incident affecting [SYSTEMS].
     
     Status: Under control
     Impact: [DESCRIPTION]
     Actions required: [IF ANY]
     
     We will provide updates every [FREQUENCY].
     
     Do not discuss this externally. Direct media inquiries to PR.
     ```

### External Communications

**Regulatory Notifications:**

1. **FERPA Breach Notification (If student records exposed):**
   - **Timeline:** Without unreasonable delay
   - **Recipients:** Affected students/parents
   - **Content:** Nature of breach, data exposed, steps taken, contact information

2. **GDPR Breach Notification (If EU residents affected):**
   - **Timeline:** Within 72 hours
   - **Recipients:** Supervisory authority, affected individuals
   - **Content:** Nature/categories/approximate number affected, likely consequences, measures taken

3. **State Data Breach Notifications:**
   - **Timeline:** Varies by state (often 30-90 days)
   - **Recipients:** State attorney general, affected residents

**Customer Communications:**

1. **Affected Users:**
   ```
   Subject: Important Security Notice
   
   Dear [Name],
   
   We are writing to inform you of a security incident that may have affected your account.
   
   What happened: On [DATE], we discovered [DESCRIPTION].
   
   What information was involved: [DATA TYPES]
   
   What we are doing: [ACTIONS TAKEN]
   
   What you should do:
   - Change your password immediately
   - Enable multi-factor authentication
   - Monitor your account for suspicious activity
   - Contact support if you notice anything unusual
   
   We take security seriously and sincerely apologize for this incident.
   
   For more information: [FAQ LINK]
   Contact: security@sms.edu
   
   Sincerely,
   SMS Security Team
   ```

2. **Public Statement (If media coverage):**
   - Approved by Legal and PR
   - Factual, transparent, but limited details
   - Emphasize actions taken and commitment to security

---

## Specific Incident Scenarios

### Scenario 1: Data Breach - Unauthorized Access to Student Records

**Detection:**
- Alert: Unusual database query patterns
- User report: Student saw another student's grades

**Response:**

1. **Immediate (0-15 min):**
   - Classify as P1
   - Activate IRT
   - Query audit logs for access patterns
   - Identify affected student records

2. **Containment (15-60 min):**
   - Disable compromised account
   - Block IP address
   - Revoke API tokens
   - Enable additional logging

3. **Investigation (1-4 hours):**
   - Determine attack vector (IDOR, SQL injection, etc.)
   - Identify all accessed records
   - Check for data exfiltration
   - Review code for vulnerability

4. **Eradication (4-24 hours):**
   - Fix vulnerability (e.g., add authorization check)
   - Deploy patch to production
   - Verify fix with security testing

5. **Recovery (24-48 hours):**
   - Monitor for continued attempts
   - Verify no backdoors remain
   - Resume normal operations

6. **Notification (48-72 hours):**
   - Notify affected students/parents
   - File FERPA report if required
   - Document incident

**Post-Incident:**
- Update authorization logic across all endpoints
- Add automated IDOR tests to CI/CD
- Train developers on authorization best practices

### Scenario 2: Account Takeover - Admin Account Compromised

**Detection:**
- Alert: Admin login from unusual location
- Observation: Unauthorized user creation

**Response:**

1. **Immediate (0-15 min):**
   - Classify as P1
   - Disable compromised admin account
   - Revoke all sessions for that account
   - Check recent admin actions

2. **Containment (15-60 min):**
   - Delete unauthorized user accounts
   - Reset passwords for all admin accounts
   - Enable MFA for all admins (if not already)
   - Block source IP

3. **Investigation (1-4 hours):**
   - Determine how account was compromised (phishing, credential stuffing, etc.)
   - Review audit logs for all actions by compromised account
   - Check for privilege escalation
   - Look for planted backdoors

4. **Eradication (4-24 hours):**
   - Remove any backdoors
   - Reset database credentials
   - Rotate API keys
   - Update security configurations

5. **Recovery (24-48 hours):**
   - Re-enable admin accounts with new credentials
   - Verify MFA is working
   - Monitor for retaliation attempts

6. **Notification:**
   - Internal: All admins about security incident
   - External: Only if user data was accessed

**Post-Incident:**
- Mandatory MFA for all admin roles
- Phishing awareness training
- Implement anomaly detection for admin activity

### Scenario 3: SQL Injection Attack

**Detection:**
- Alert: SQL error patterns in logs
- WAF alert: Suspicious query strings

**Response:**

1. **Immediate (0-15 min):**
   - Classify as P2 (or P1 if data exfiltration confirmed)
   - Block attacker IP
   - Disable affected endpoint

2. **Containment (15-60 min):**
   - Enable request logging
   - Review database query logs
   - Check for unauthorized data access
   - Apply WAF rule to block similar requests

3. **Investigation (1-4 hours):**
   - Identify vulnerable endpoint
   - Review code for SQL injection vulnerability
   - Determine if data was exfiltrated
   - Check for lateral movement

4. **Eradication (4-24 hours):**
   - Fix SQL injection vulnerability (use parameterized queries)
   - Code review similar endpoints
   - Deploy security patch

5. **Recovery (24-48 hours):**
   - Re-enable endpoint
   - Monitor for continued attempts
   - Run security scan

**Post-Incident:**
- Code audit for all SQL queries
- Implement prepared statements everywhere
- Add SQLMap tests to CI/CD

### Scenario 4: DDoS Attack

**Detection:**
- Alert: High request volume
- Users report: Website is slow/unavailable

**Response:**

1. **Immediate (0-15 min):**
   - Classify as P2 (P1 if prolonged outage)
   - Enable DDoS mitigation (Cloudflare, Azure DDoS Protection)
   - Identify attack pattern

2. **Containment (15-60 min):**
   - Enable rate limiting
   - Block attack source IPs/ranges
   - Scale up infrastructure if needed

3. **Investigation (1-4 hours):**
   - Analyze traffic patterns
   - Distinguish legitimate vs. attack traffic
   - Identify attack type (volumetric, application-layer)

4. **Eradication (4-24 hours):**
   - Fine-tune DDoS protection rules
   - Work with ISP/CDN provider
   - Implement CAPTCHA if needed

5. **Recovery (24-48 hours):**
   - Monitor traffic normalization
   - Remove temporary restrictions
   - Verify service availability

**Post-Incident:**
- Implement permanent DDoS protection
- Increase infrastructure capacity
- Create DDoS runbook

### Scenario 5: Ransomware Infection

**Detection:**
- Alert: File encryption activity detected
- User report: Files inaccessible

**Response:**

1. **Immediate (0-15 min):**
   - Classify as P1
   - Isolate infected systems (disconnect network)
   - Do NOT shut down (preserves memory forensics)
   - Alert all staff not to pay ransom

2. **Containment (15-60 min):**
   - Identify ransomware variant
   - Check for spread to other systems
   - Preserve backups (take offline)
   - Document ransom note

3. **Investigation (1-4 hours):**
   - Determine infection vector
   - Identify affected files
   - Check if decryption tool exists (nomoreransom.org)
   - Assess backup integrity

4. **Eradication (4-24 hours):**
   - Wipe infected systems
   - Rebuild from clean images
   - Patch vulnerabilities

5. **Recovery (24-72 hours):**
   - Restore from backups
   - Verify data integrity
   - Test applications

6. **Decision: Pay Ransom?**
   - **Recommendation:** Do NOT pay
   - **Consult:** Legal counsel, law enforcement
   - **Consider:** Backup availability, data criticality

**Post-Incident:**
- Implement endpoint detection and response (EDR)
- Immutable backups
- User training on phishing

---

## Post-Incident Activities

### 1. Root Cause Analysis (RCA)

**5 Whys Technique:**

```
Problem: Unauthorized access to student records

Why? Student A accessed Student B's profile.
Why? API didn't check authorization.
Why? Developer didn't implement resource-based authorization.
Why? No security requirements in user story.
Why? Security requirements not part of development process.

Root Cause: Security not integrated into SDLC.
Fix: Add security requirements to all user stories.
```

### 2. Lessons Learned Report

**Template:**

```markdown
# Incident Report: [INCIDENT-ID]

## Executive Summary
- **Incident:** Unauthorized access to student records
- **Date:** January 5, 2026
- **Severity:** P1 - Critical
- **Impact:** 150 student records accessed
- **Status:** Resolved

## Timeline
| Time | Event |
|------|-------|
| 09:15 | Initial detection |
| 09:30 | IRT activated |
| 10:00 | Containment complete |
| 14:00 | Fix deployed |
| 16:00 | Incident closed |

## What Happened
[Detailed description]

## Root Cause
[Technical and organizational causes]

## Impact
- **Users Affected:** 150 students
- **Data Exposed:** Names, email addresses, grade levels
- **Financial:** $5,000 (staff hours + notifications)
- **Reputational:** Medium (limited media coverage)

## Response Effectiveness
- **What Went Well:**
  - Quick detection (15 minutes)
  - Effective containment
  - Clear communication
  
- **What Needs Improvement:**
  - Delayed notification to users (48 hours)
  - Incident playbook was outdated
  - No automated rollback

## Action Items
| Action | Owner | Due Date | Status |
|--------|-------|----------|--------|
| Fix authorization logic | Dev Team | Jan 12 | Complete |
| Add IDOR tests to CI/CD | QA Team | Jan 15 | In Progress |
| Update incident playbook | Security | Jan 10 | Complete |
| Security training for devs | HR | Feb 1 | Not Started |

## Regulatory Compliance
- FERPA notification sent on January 7, 2026
- No GDPR notification required (no EU residents affected)
```

### 3. Metrics and Tracking

**Key Metrics:**
- Mean Time to Detect (MTTD): Time from incident to detection
- Mean Time to Respond (MTTR): Time from detection to containment
- Mean Time to Resolve (MTTR): Time from detection to resolution

**Track Trends:**
- Number of incidents per month
- Incident severity distribution
- Repeat incidents (same root cause)

---

## Tools and Resources

### Forensic Tools

1. **Volatility** - Memory forensics
   ```bash
   python vol.py -f memory.dump --profile=Win10x64 pslist
   ```

2. **Autopsy** - Disk forensics
3. **Wireshark** - Network traffic analysis
4. **Sysinternals Suite** - Windows system analysis

### Incident Management

1. **Jira/ServiceNow** - Incident ticketing
2. **Slack** - Team communication (#security-incidents channel)
3. **PagerDuty** - On-call alerting
4. **Zoom** - War room video conferencing

### Documentation

1. **Google Docs** - Collaborative incident log
2. **Confluence** - Runbooks and playbooks
3. **GitHub** - Incident response scripts

---

## Contact Information

### Emergency Contacts

**Incident Commander:**
- Name: [CTO Name]
- Mobile: +1-XXX-XXX-XXXX
- Email: cto@sms.edu

**Security Lead:**
- Name: [Security Team Lead]
- Mobile: +1-XXX-XXX-XXXX
- Email: security@sms.edu

**Operations Lead:**
- Name: [DevOps Lead]
- Mobile: +1-XXX-XXX-XXXX
- Email: devops@sms.edu

### External Contacts

**Law Enforcement:**
- FBI Cyber Division: 1-800-CALL-FBI
- Local Police: 911

**Vendors:**
- Azure Support: 1-800-642-7676
- Cloudflare: +1-888-993-5273

**Legal:**
- Outside Counsel: [Law Firm]
- Phone: +1-XXX-XXX-XXXX

**Regulatory:**
- Department of Education (FERPA): privacy@ed.gov
- State Attorney General: [Contact info]

---

## Appendices

### Appendix A: Incident Severity Matrix

| Factor | P1 | P2 | P3 | P4 |
|--------|----|----|----|----|
| Data Exposure | Confirmed | Suspected | Potential | None |
| User Impact | All users | Many users | Few users | Single user |
| System Impact | Complete outage | Partial outage | Degraded | None |
| Compliance Risk | FERPA violation | Possible violation | Low risk | No risk |

### Appendix B: Notification Templates

See [Communication Plan](#communication-plan) section.

### Appendix C: Incident Log Template

```
INCIDENT LOG

Incident ID: INC-2026-001
Started: 2026-01-05 09:15 UTC
Commander: [Name]

TIMELINE:
[09:15] Detection: Alert from SIEM
[09:30] Activation: IRT assembled
[09:45] Triage: Classified as P1
...

ACTIONS TAKEN:
- Disabled account user123
- Blocked IP 203.0.113.45
- Reviewed audit logs
...

EVIDENCE COLLECTED:
- Database query logs (/forensics/db-logs/)
- Application logs (/var/log/sms-api/)
- Network traffic capture (/forensics/pcap/)
...

DECISIONS:
[10:30] Decision to take API offline for 15 minutes - Approved by CTO
...
```

---

**Related Documentation:**
- [Security Implementation Guide](SECURITY_IMPLEMENTATION_GUIDE.md)
- [Security Testing Checklist](SECURITY_TESTING_CHECKLIST.md)
- [Penetration Testing Guide](PENETRATION_TESTING.md)
- [API Security Best Practices](API_SECURITY_BEST_PRACTICES.md)
