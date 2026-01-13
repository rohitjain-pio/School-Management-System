# Deployment Architecture
## Production Infrastructure & CI/CD Pipeline

**Document Version:** 1.0  
**Last Updated:** January 13, 2026  
**Reading Time:** 18 minutes  
**Status:** 🟡 In Progress

---

## 🎯 **Deployment Goals**

1. **Zero Downtime:** Deploy without service interruption
2. **Automated:** Push to main → automatic deployment
3. **Rollback:** Quick revert if issues detected
4. **Scalable:** Handle traffic growth automatically
5. **Cost-Effective:** Optimize cloud costs for startup

---

## 🏗️ **Infrastructure Overview**

### **Cloud Provider Options**

**Option A: Microsoft Azure (Recommended)**
- Native .NET support
- Azure App Service for backend
- Azure Static Web Apps for frontend
- Azure SQL Database (managed)
- Tight integration with Visual Studio

**Option B: AWS**
- Elastic Beanstalk for backend
- S3 + CloudFront for frontend
- RDS SQL Server (managed)
- More granular control

**Option C: Hybrid (Budget-Friendly)**
- DigitalOcean/Linode for app (cheaper)
- Managed database service
- Cloudflare CDN (free tier)

---

## 📦 **Containerized Architecture**

### **Docker Compose (Development)**

```yaml
version: '3.8'

services:
  # Frontend (React + Nginx)
  frontend:
    build:
      context: ./Frontend
      dockerfile: Dockerfile
    ports:
      - "80:80"
      - "443:443"
    environment:
      - VITE_API_URL=https://localhost:7266
    depends_on:
      - backend
    networks:
      - app-network
    restart: unless-stopped

  # Backend (.NET API)
  backend:
    build:
      context: ./Backend
      dockerfile: Dockerfile
    ports:
      - "7266:8080"
    environment:
      - ASPNETCORE_ENVIRONMENT=Production
      - ConnectionStrings__DefaultConnection=Server=database;Database=SchoolManagementDB;User=sa;Password=${DB_PASSWORD};TrustServerCertificate=True
      - Jwt__SecretKey=${JWT_SECRET}
      - Redis__ConnectionString=redis:6379
    depends_on:
      - database
      - redis
    networks:
      - app-network
    restart: unless-stopped

  # SQL Server Database
  database:
    image: mcr.microsoft.com/mssql/server:2022-latest
    environment:
      - ACCEPT_EULA=Y
      - SA_PASSWORD=${DB_PASSWORD}
      - MSSQL_PID=Developer
    ports:
      - "1433:1433"
    volumes:
      - sqlserver-data:/var/opt/mssql
    networks:
      - app-network
    restart: unless-stopped

  # Redis Cache
  redis:
    image: redis:7-alpine
    ports:
      - "6379:6379"
    volumes:
      - redis-data:/data
    command: redis-server --appendonly yes
    networks:
      - app-network
    restart: unless-stopped

volumes:
  sqlserver-data:
  redis-data:

networks:
  app-network:
    driver: bridge
```

### **Frontend Dockerfile**

```dockerfile
# Build stage
FROM node:20-alpine AS build

WORKDIR /app

# Copy package files
COPY package.json bun.lockb ./

# Install dependencies
RUN npm install -g bun
RUN bun install

# Copy source code
COPY . .

# Build application
RUN bun run build

# Production stage
FROM nginx:alpine

# Copy custom nginx config
COPY nginx.conf /etc/nginx/nginx.conf

# Copy built app from build stage
COPY --from=build /app/dist /usr/share/nginx/html

# Expose port
EXPOSE 80 443

CMD ["nginx", "-g", "daemon off;"]
```

### **Backend Dockerfile**

```dockerfile
# Build stage
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build

WORKDIR /src

# Copy solution and project files
COPY SMSPrototype1.sln ./
COPY SMSPrototype1/SMSPrototype1.csproj SMSPrototype1/
COPY SMSServices/SMSServices.csproj SMSServices/
COPY SMSRepository/SMSRepository.csproj SMSRepository/
COPY SMSDataContext/SMSDataContext.csproj SMSDataContext/
COPY SMSDataModel/SMSDataModel.csproj SMSDataModel/

# Restore dependencies
RUN dotnet restore

# Copy source code
COPY . .

# Build application
WORKDIR /src/SMSPrototype1
RUN dotnet build -c Release -o /app/build

# Publish stage
FROM build AS publish
RUN dotnet publish -c Release -o /app/publish

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final

WORKDIR /app

# Copy published app
COPY --from=publish /app/publish .

# Expose port
EXPOSE 8080

ENTRYPOINT ["dotnet", "SMSPrototype1.dll"]
```

---

## 🚀 **CI/CD Pipeline (GitHub Actions)**

### **Complete Workflow**

```yaml
name: Deploy to Production

on:
  push:
    branches: [main]
  pull_request:
    branches: [main]

env:
  AZURE_WEBAPP_NAME: school-management-api
  AZURE_STATIC_APP_NAME: school-management-frontend
  DOTNET_VERSION: '9.0'
  NODE_VERSION: '20'

jobs:
  # Job 1: Backend Tests
  backend-tests:
    runs-on: ubuntu-latest
    
    steps:
      - uses: actions/checkout@v4
      
      - name: Setup .NET
        uses: actions/setup-dotnet@v4
        with:
          dotnet-version: ${{ env.DOTNET_VERSION }}
      
      - name: Restore dependencies
        run: dotnet restore
        working-directory: ./Backend
      
      - name: Build
        run: dotnet build --no-restore --configuration Release
        working-directory: ./Backend
      
      - name: Run unit tests
        run: dotnet test --no-build --configuration Release --verbosity normal
        working-directory: ./Backend
      
      - name: Run integration tests
        run: dotnet test --filter Category=Integration
        working-directory: ./Backend

  # Job 2: Frontend Tests
  frontend-tests:
    runs-on: ubuntu-latest
    
    steps:
      - uses: actions/checkout@v4
      
      - name: Setup Node.js
        uses: actions/setup-node@v4
        with:
          node-version: ${{ env.NODE_VERSION }}
      
      - name: Install Bun
        run: curl -fsSL https://bun.sh/install | bash
      
      - name: Install dependencies
        run: bun install
        working-directory: ./Frontend
      
      - name: Run linter
        run: bun run lint
        working-directory: ./Frontend
      
      - name: Run type check
        run: bun run type-check
        working-directory: ./Frontend
      
      - name: Build
        run: bun run build
        working-directory: ./Frontend

  # Job 3: Security Scan
  security-scan:
    runs-on: ubuntu-latest
    
    steps:
      - uses: actions/checkout@v4
      
      - name: Run Trivy vulnerability scanner
        uses: aquasecurity/trivy-action@master
        with:
          scan-type: 'fs'
          scan-ref: '.'
          format: 'sarif'
          output: 'trivy-results.sarif'
      
      - name: Upload Trivy results to GitHub Security
        uses: github/codeql-action/upload-sarif@v2
        with:
          sarif_file: 'trivy-results.sarif'

  # Job 4: Build and Push Docker Images
  build-images:
    needs: [backend-tests, frontend-tests, security-scan]
    runs-on: ubuntu-latest
    if: github.ref == 'refs/heads/main'
    
    steps:
      - uses: actions/checkout@v4
      
      - name: Login to Docker Hub
        uses: docker/login-action@v3
        with:
          username: ${{ secrets.DOCKER_USERNAME }}
          password: ${{ secrets.DOCKER_PASSWORD }}
      
      - name: Build and push backend image
        uses: docker/build-push-action@v5
        with:
          context: ./Backend
          push: true
          tags: |
            ${{ secrets.DOCKER_USERNAME }}/school-management-backend:latest
            ${{ secrets.DOCKER_USERNAME }}/school-management-backend:${{ github.sha }}
      
      - name: Build and push frontend image
        uses: docker/build-push-action@v5
        with:
          context: ./Frontend
          push: true
          tags: |
            ${{ secrets.DOCKER_USERNAME }}/school-management-frontend:latest
            ${{ secrets.DOCKER_USERNAME }}/school-management-frontend:${{ github.sha }}

  # Job 5: Deploy to Staging
  deploy-staging:
    needs: build-images
    runs-on: ubuntu-latest
    environment: staging
    
    steps:
      - name: Deploy to Azure App Service (Staging)
        uses: azure/webapps-deploy@v2
        with:
          app-name: ${{ env.AZURE_WEBAPP_NAME }}-staging
          publish-profile: ${{ secrets.AZURE_WEBAPP_PUBLISH_PROFILE_STAGING }}
          images: ${{ secrets.DOCKER_USERNAME }}/school-management-backend:${{ github.sha }}
      
      - name: Deploy to Azure Static Web Apps (Staging)
        uses: Azure/static-web-apps-deploy@v1
        with:
          azure_static_web_apps_api_token: ${{ secrets.AZURE_STATIC_WEB_APPS_API_TOKEN_STAGING }}
          repo_token: ${{ secrets.GITHUB_TOKEN }}
          action: "upload"
          app_location: "/Frontend"
          output_location: "dist"

  # Job 6: Smoke Tests (Staging)
  smoke-tests-staging:
    needs: deploy-staging
    runs-on: ubuntu-latest
    
    steps:
      - uses: actions/checkout@v4
      
      - name: Wait for deployment
        run: sleep 30
      
      - name: Health check
        run: |
          response=$(curl -s -o /dev/null -w "%{http_code}" https://school-management-staging.azurewebsites.net/health)
          if [ $response -ne 200 ]; then
            echo "Health check failed with status $response"
            exit 1
          fi
      
      - name: Run smoke tests
        run: |
          npm install -g newman
          newman run ./Backend/performance-tests/smoke-tests.json \
            --env-var "baseUrl=https://school-management-staging.azurewebsites.net"

  # Job 7: Deploy to Production
  deploy-production:
    needs: smoke-tests-staging
    runs-on: ubuntu-latest
    environment: production
    
    steps:
      - name: Deploy to Azure App Service (Production)
        uses: azure/webapps-deploy@v2
        with:
          app-name: ${{ env.AZURE_WEBAPP_NAME }}
          publish-profile: ${{ secrets.AZURE_WEBAPP_PUBLISH_PROFILE }}
          images: ${{ secrets.DOCKER_USERNAME }}/school-management-backend:${{ github.sha }}
      
      - name: Deploy to Azure Static Web Apps (Production)
        uses: Azure/static-web-apps-deploy@v1
        with:
          azure_static_web_apps_api_token: ${{ secrets.AZURE_STATIC_WEB_APPS_API_TOKEN }}
          repo_token: ${{ secrets.GITHUB_TOKEN }}
          action: "upload"
          app_location: "/Frontend"
          output_location: "dist"
      
      - name: Notify deployment
        run: |
          curl -X POST ${{ secrets.SLACK_WEBHOOK_URL }} \
            -H 'Content-Type: application/json' \
            -d '{"text":"🚀 Deployed to production: ${{ github.sha }}"}'

  # Job 8: Smoke Tests (Production)
  smoke-tests-production:
    needs: deploy-production
    runs-on: ubuntu-latest
    
    steps:
      - uses: actions/checkout@v4
      
      - name: Production health check
        run: |
          response=$(curl -s -o /dev/null -w "%{http_code}" https://school-management.azurewebsites.net/health)
          if [ $response -ne 200 ]; then
            echo "❌ Production health check failed!"
            # Trigger rollback
            exit 1
          fi
          echo "✅ Production is healthy"
```

---

## 🌍 **Production Infrastructure (Azure)**

### **Resource Group Structure**

```
schoolmanagement-rg
│
├── App Services
│   ├── schoolmanagement-api (Backend)
│   │   ├── Scaling: Auto-scale (1-5 instances)
│   │   ├── Deployment slots: Staging, Production
│   │   └── Always On: Enabled
│   │
│   └── schoolmanagement-frontend (Static Web App)
│       ├── CDN: Enabled
│       └── Custom domain: app.schoolms.com
│
├── Database
│   └── schoolmanagement-sqlserver
│       ├── SQL Database: SchoolManagementDB
│       ├── Tier: Standard S2 (50 DTUs)
│       ├── Backup: Automated (7-day retention)
│       └── Geo-replication: Enabled (West India → South India)
│
├── Cache
│   └── schoolmanagement-redis
│       ├── Tier: Standard C1 (1GB)
│       └── Persistence: AOF enabled
│
├── Storage
│   └── schoolmanagementstorage
│       ├── Blob containers: school-files, backups
│       ├── Redundancy: LRS (Locally Redundant Storage)
│       └── Encryption: Enabled
│
├── Monitoring
│   ├── Application Insights: schoolmanagement-ai
│   ├── Log Analytics Workspace: schoolmanagement-logs
│   └── Alerts: Performance, Errors, Security
│
└── Networking
    ├── Virtual Network: schoolmanagement-vnet
    ├── Subnet: app-subnet, db-subnet
    └── Network Security Groups: Firewall rules
```

### **Cost Estimate (50 Schools, 25K Students)**

| Resource | Tier | Monthly Cost |
|----------|------|--------------|
| App Service (Backend) | Standard S2 | ₹6,000 |
| Static Web App (Frontend) | Standard | ₹800 |
| SQL Database | Standard S2 | ₹8,000 |
| Redis Cache | Standard C1 | ₹3,500 |
| Blob Storage | 100GB | ₹200 |
| Application Insights | Pay-as-you-go | ₹1,500 |
| Bandwidth | 500GB/month | ₹2,000 |
| **Total** | | **₹22,000/month** |

**Revenue at 50 schools (₹50/student):** ₹6.25L/month  
**Gross Margin:** ~96%

---

## 🔄 **Zero-Downtime Deployment**

### **Blue-Green Deployment Strategy**

```
┌─────────────────────────────────────────┐
│       Azure App Service                  │
│                                          │
│  ┌────────────────┐  ┌────────────────┐ │
│  │  Production    │  │    Staging     │ │
│  │  Slot (Blue)   │  │  Slot (Green)  │ │
│  │                │  │                │ │
│  │  Current Code  │  │   New Code     │ │
│  │  v1.0.0        │  │   v1.1.0       │ │
│  └────────┬───────┘  └────────┬───────┘ │
└───────────┼──────────────────┼─────────┘
            │                  │
            │ 1. Deploy to     │
            │    Staging       │
            │                  │
            │ 2. Test Staging  │
            │    (smoke tests) │
            │                  │
            │ 3. SWAP SLOTS ◄──┘
            │    (instant)     
            │                  
            ▼                  
      ┌─────────┐              
      │  Users  │              
      │ See new │              
      │ version │              
      └─────────┘              
```

**Swap takes < 1 second, zero downtime**

**Rollback:** Just swap slots again (instant)

---

## 📊 **Auto-Scaling Configuration**

### **Backend Scaling Rules**

```json
{
  "scaleRules": [
    {
      "metricTrigger": {
        "metricName": "CpuPercentage",
        "operator": "GreaterThan",
        "threshold": 70,
        "timeWindow": "PT5M"
      },
      "scaleAction": {
        "direction": "Increase",
        "type": "ChangeCount",
        "value": 1,
        "cooldown": "PT10M"
      }
    },
    {
      "metricTrigger": {
        "metricName": "CpuPercentage",
        "operator": "LessThan",
        "threshold": 30,
        "timeWindow": "PT10M"
      },
      "scaleAction": {
        "direction": "Decrease",
        "type": "ChangeCount",
        "value": 1,
        "cooldown": "PT10M"
      }
    },
    {
      "metricTrigger": {
        "metricName": "HttpQueueLength",
        "operator": "GreaterThan",
        "threshold": 100,
        "timeWindow": "PT5M"
      },
      "scaleAction": {
        "direction": "Increase",
        "type": "ChangeCount",
        "value": 2,
        "cooldown": "PT5M"
      }
    }
  ],
  "minInstances": 1,
  "maxInstances": 5
}
```

**Scaling Triggers:**
- CPU > 70% for 5 min → Scale up
- CPU < 30% for 10 min → Scale down
- HTTP queue > 100 requests → Scale up urgently

---

## 🔐 **Environment Variables & Secrets**

### **Azure Key Vault Integration**

```csharp
// Program.cs
builder.Configuration.AddAzureKeyVault(
    new Uri("https://schoolmanagement-kv.vault.azure.net/"),
    new DefaultAzureCredential()
);

// Access secrets
var jwtSecret = builder.Configuration["Jwt--SecretKey"]; // From Key Vault
var dbConnection = builder.Configuration["ConnectionStrings--DefaultConnection"];
```

**Secrets in Key Vault:**
- `Jwt--SecretKey`: JWT signing key
- `ConnectionStrings--DefaultConnection`: Database connection
- `SendGrid--ApiKey`: Email service API key
- `Twilio--AccountSid`: SMS service credentials
- `Azure--StorageConnectionString`: Blob storage access

**Never commit secrets to Git!**

---

## 🛡️ **Security Hardening**

### **Network Security**

```hcl
# Network Security Group Rules
resource "azurerm_network_security_rule" "allow_https" {
  name                        = "AllowHTTPS"
  priority                    = 100
  direction                   = "Inbound"
  access                      = "Allow"
  protocol                    = "Tcp"
  source_port_range           = "*"
  destination_port_range      = "443"
  source_address_prefix       = "*"
  destination_address_prefix  = "*"
}

resource "azurerm_network_security_rule" "block_direct_db" {
  name                        = "BlockDirectDB"
  priority                    = 200
  direction                   = "Inbound"
  access                      = "Deny"
  protocol                    = "Tcp"
  source_port_range           = "*"
  destination_port_range      = "1433"
  source_address_prefix       = "*"
  destination_address_prefix  = "db-subnet"
}
```

**Firewall Rules:**
- Allow HTTPS (443) from anywhere
- Block direct SQL Server access (1433) except from app subnet
- Allow SSH/RDP only from trusted IPs (your office/home)

---

## 📚 **Deployment Checklist**

**Pre-Deployment:**
- [ ] All tests passing (unit + integration)
- [ ] Security scan clean (no critical vulnerabilities)
- [ ] Database migrations reviewed
- [ ] Environment variables configured
- [ ] Secrets in Key Vault
- [ ] Monitoring alerts configured
- [ ] Backup verified (can restore)

**During Deployment:**
- [ ] Deploy to staging first
- [ ] Run smoke tests on staging
- [ ] Review Application Insights for errors
- [ ] Load test staging (simulate traffic)
- [ ] Get approval from stakeholder
- [ ] Swap to production
- [ ] Monitor production for 30 minutes

**Post-Deployment:**
- [ ] Verify all features working
- [ ] Check error rates (should be < 0.1%)
- [ ] Review performance metrics
- [ ] Send deployment notification
- [ ] Document any issues
- [ ] Update changelog

---

## 📚 **Next Steps**

1. **Performance:** [13_PERFORMANCE_OPTIMIZATION.md](./13_PERFORMANCE_OPTIMIZATION.md)
2. **Disaster Recovery:** [14_DISASTER_RECOVERY.md](./14_DISASTER_RECOVERY.md)
3. **Monitoring:** [17_MONITORING_LOGGING.md](./17_MONITORING_LOGGING.md)

---

**Document Status:** ✅ Complete  
**Infrastructure:** 🟡 Development (not yet in Azure)  
**Target:** Production deployment by Feb 13, 2026
