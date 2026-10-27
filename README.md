# Apartment Maintenance Management System

A secure, scalable, enterprise-grade web application to manage apartment maintenance operations, employees, vendors, assets, purchase orders with a 3-tier sequential committee approval workflow, parking allocations, residents, and audit logging.

Built to comfortably support **5,000+ registered residents and staff** with role-based and policy-based access control (RBAC), optimistic concurrency, soft deletion, and automated audit logging.

---

## 1. System Architecture

The project adheres to **Clean Architecture** and **Domain-Driven Design (DDD)** principles, separating business domain logic from infrastructure and presentation concerns:

```
ApartmentMaintenance/
├── src/
│   ├── ApartmentMaintenance.Domain/          # Core entities, Enums, Value Objects, Domain Events
│   ├── ApartmentMaintenance.Application/     # DTOs, interfaces, validation rules (FluentValidation)
│   ├── ApartmentMaintenance.Infrastructure/  # EF Core 10 (MySQL & SQLite), Serilog, BCrypt, JWT, AuditLog
│   └── ApartmentMaintenance.Api/             # ASP.NET Core 10 Controllers, Swagger, Middlewares, Healthchecks
├── tests/
│   └── ApartmentMaintenance.UnitTests/       # xUnit tests (Workflow, Password Hashing, JWT, DTO Validators)
├── client/                                   # React 19 + TypeScript + Material-UI (MUI) SPA
├── docker-compose.yml                        # Production-ready multi-container orchestration
├── Dockerfile.api                            # Multi-stage .NET 10 Web API image
├── Dockerfile.client                         # Multi-stage Vite build + Nginx alpine reverse proxy
└── .env.example                              # Environment configuration template
```

For detailed Solution Architecture diagrams and Entity Relationship Diagrams (ERD), refer to the [Architecture and ERD Documentation](file:///C:/Users/ShreyaLap/.gemini/antigravity/brain/1f4f31a7-e4b3-4818-a86b-f829f02aef2d/architecture_and_erd.md).

---

## 2. Technology Stack

- **Backend**:
  - ASP.NET Core Web API with **.NET 10 LTS**
  - **Entity Framework Core 10** (`MySql.EntityFrameworkCore 10.0.9` official Oracle provider & SQLite support)
  - **Authentication**: JWT Bearer tokens (access + refresh tokens) with **BCrypt** password hashing
  - **Authorization**: Role-based (`Admin`, `President`, `Secretary`, `Treasurer`, `Staff`, `Resident`) and committee-policy-based
  - **Logging**: Structured logging with **Serilog** (console & rotating file logs)
  - **Validation**: FluentValidation with automatic model state error formatting
  - **API Documentation**: OpenAPI / Swagger UI with JWT Bearer authorization support
- **Frontend**:
  - **React 19** with **TypeScript** and **Vite**
  - **Material-UI (MUI)** responsive component framework
  - **Axios** with automatic token injection, interceptors, and 401 token refresh handling
  - **React Router v7** with protected route guards and role-based route access
- **Testing**:
  - **xUnit** backend unit tests with approval state machine transition testing
- **Database & Containerization**:
  - **MySQL 8.4 LTS** with UTF8MB4 charset, indexes, foreign keys, and soft deletes
  - **Docker** and **Docker Compose** multi-container setup with healthchecks

---

## 3. Key Modules & Features

### 3.1. 3-Tier Committee Approval Workflow
- **Sequential Lifecycle**: `Draft` → `Pending Approval` (assigned to **President**) → (approved) → assigned to **Secretary** → (approved) → assigned to **Treasurer** → (approved) → **Approved**.
- **Rejection & Change Requests**: Any approver can reject (terminating the workflow as `Rejected`) or request modifications (returning status to `Change Requested` for requester edits).
- **PO Conversion**: Once fully approved, authorized members can generate a binding **Purchase Order (PO)** with automated tracking of delivery status.
- **Audit Logging**: Every single transition records approver ID, timestamp, decision, and comments.

### 3.2. Role-Based Access Control (RBAC)
| Role | Capabilities |
| :--- | :--- |
| **System Administrator** | Full system configuration, towers, flats, parking master data, user & role management, audit logs. |
| **President** | Review and approve/reject Tier-1 purchase requests, view committee operations, assets, and announcements. |
| **Secretary** | Manage residents, employees, vendors; review and approve/reject Tier-2 purchase requests. |
| **Treasurer** | Review budgets, invoices, amounts; approve/reject Tier-3 purchase requests; view purchase orders and financial records. |
| **Maintenance Staff** | View operational tasks, update asset maintenance logs, and manage scheduled service activities. |
| **Resident / Owner** | View assigned flats, allocated parking spots, announcements, and maintenance request updates. |

### 3.3. Asset & Vendor Management
- **Vendor Registry**: Tracks service categories (Security, Lifts, Plumbing, Electrical, Fire Safety, etc.), contracts, tax IDs, and renewal dates.
- **Asset Inventory**: Lifts, generators, water treatment pumps, CCTV, electrical panels, warranty tracking, and scheduled maintenance dates.

### 3.4. Parking & Resident Allocation
- Master parking lot registry with tower association, parking type (Covered/Open/EV).
- Enforces strict one-active-allocation-per-slot constraint to prevent double-booking.
- Multi-flat ownership and co-occupant tracking.

---

## 4. Default Seed Accounts & Credentials

The system seeds essential roles, users, towers, flats, vendors, and sample workflows automatically upon initial launch:

| Role | Email Address | Password | Initial Responsibilities |
| :--- | :--- | :--- | :--- |
| **Admin** | `admin@apartment.com` | `Admin@123456` | Global settings, tower & parking setup, audit trail |
| **President** | `president@apartment.com` | `Admin@123456` | 1st-stage PO Approvals |
| **Secretary** | `secretary@apartment.com` | `Admin@123456` | 2nd-stage PO Approvals, staff & resident records |
| **Treasurer** | `treasurer@apartment.com` | `Admin@123456` | 3rd-stage PO Approvals, financial accounts |
| **Staff** | `maintenance@apartment.com` | `Admin@123456` | Asset maintenance log updates |
| **Resident** | `resident1@apartment.com` | `Admin@123456` | Resident self-service portal |

---

## 5. Local Development Setup

### Prerequisites
- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- [Node.js 20+ LTS](https://nodejs.org/) & `npm`
- [MySQL 8.4+](https://dev.mysql.com/downloads/) (or run MySQL via Docker)

### Step 1: Configure Database Connection
Edit `src/ApartmentMaintenance.Api/appsettings.json` or create `appsettings.Development.json`:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Port=3306;Database=ApartmentDb;User=root;Password=YourPassword;TreatTinyAsBoolean=true;"
  }
}
```
*(Alternatively, you can test with SQLite by setting `"UseInMemoryDatabase": true` or configuring SQLite in `ApplicationDbContext` if MySQL is not locally running).*

### Step 2: Run Backend Web API
```bash
# Restore and build the solution
dotnet restore
dotnet build

# Run the API (DbInitializer seeds roles and sample records on startup)
dotnet run --project src/ApartmentMaintenance.Api
```
- API will start on: `http://localhost:5000` (or `https://localhost:5001`)
- Interactive Swagger UI: `http://localhost:5000/swagger`
- Health check endpoint: `http://localhost:5000/health`

### Step 3: Run Frontend React Application
Open a new terminal:
```bash
cd client
npm install
npm run dev
```
- Client will start on: `http://localhost:5173`
- The Vite dev server automatically proxies `/api` calls to `http://localhost:5000`.

---

## 6. Docker Container Deployment

A turnkey `docker-compose.yml` is provided at the repository root to start MySQL 8.4, the ASP.NET Core API, and the React Nginx web client in isolated containers.

```bash
# Copy and configure environment variables
cp .env.example .env

# Build and launch all containers in detached mode
docker compose up --build -d

# Check service status
docker compose ps

# View live logs
docker compose logs -f api
```

### Published Ports:
- **Frontend Web App**: `http://localhost:3000`
- **Backend Web API**: `http://localhost:5000`
- **Swagger Documentation**: `http://localhost:5000/swagger`
- **MySQL Database**: `localhost:3306`

To shut down and clean up:
```bash
docker compose down -v
```

---

## 7. Running Backend Unit Tests

The test suite validates password hashing, JWT token lifecycle, sequential 3-tier approval state transitions, rejection termination, and FluentValidation rules:

```bash
dotnet test tests/ApartmentMaintenance.UnitTests
```

---

## 8. Database Backup & Disaster Recovery Procedures

### 8.1. Manual Full Backup
```bash
# Export full database schema and data to compressed SQL dump
mysqldump -u root -p \
  --single-transaction \
  --quick \
  --routines \
  --triggers \
  ApartmentDb | gzip > "backup_ApartmentDb_$(date +%Y%m%d_%H%M%S).sql.gz"
```

### 8.2. Automated Daily Backup (Cron Example on Linux / Task Scheduler on Windows)
```bash
#!/bin/bash
BACKUP_DIR="/var/backups/apartment_db"
mkdir -p "$BACKUP_DIR"
TIMESTAMP=$(date +"%Y%m%d_%H%M%S")
FILENAME="$BACKUP_DIR/apartment_db_$TIMESTAMP.sql.gz"

mysqldump -h localhost -u db_backup_user -p'StrongPassword' \
  --single-transaction --quick ApartmentDb | gzip > "$FILENAME"

# Retain backups for 30 days
find "$BACKUP_DIR" -type f -name "*.sql.gz" -mtime +30 -delete
```

### 8.3. Database Restore Procedure
```bash
# 1. Unzip the archive
gunzip < backup_ApartmentDb_20261026_000000.sql.gz > restore.sql

# 2. Re-create the database and restore data
mysql -u root -p -e "CREATE DATABASE IF NOT EXISTS ApartmentDb CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;"
mysql -u root -p ApartmentDb < restore.sql
```

---

## 9. Load & Performance Testing Plan (5,000 Registered Users)

### 9.1. Workload Sizing & Concurrency Assumptions
- **User Population**: 5,000 registered residents and employees.
- **Peak Concurrency Factor**: 10% – 15% concurrent active users = **500 to 750 concurrent virtual users (VUs)**.
- **Throughput Target**: 1,200 – 1,800 requests per second (RPS) during peak hours (morning & evening announcements, maintenance logging).
- **Latency SLO**:
  - `GET` endpoints (Dashboard, Flats, Parking): p95 < 150 ms, p99 < 300 ms.
  - `POST` / `PUT` endpoints (Approvals, PO creation): p95 < 250 ms, p99 < 500 ms.
  - Error rate: < 0.01% under normal load; 0% data corruption under concurrent approval attempts.

### 9.2. k6 Load Testing Script (`load-test.js`)
```javascript
import http from 'k6/http';
import { check, sleep } from 'k6';

export const options = {
  stages: [
    { duration: '2m', target: 100 },  // Ramp-up to 100 users
    { duration: '5m', target: 500 },  // Stay at 500 concurrent users
    { duration: '2m', target: 750 },  // Stress spike to 750 users
    { duration: '2m', target: 0 },    // Ramp-down
  ],
  thresholds: {
    http_req_duration: ['p(95)<250'], // 95% requests must finish under 250ms
    http_req_failed: ['rate<0.01'],    // Less than 1% failure
  },
};

const BASE_URL = 'http://localhost:5000/api';

export default function () {
  // 1. Authenticate user
  const loginRes = http.post(`${BASE_URL}/auth/login`, JSON.stringify({
    email: 'resident1@apartment.com',
    password: 'Admin@123456'
  }), { headers: { 'Content-Type': 'application/json' } });

  check(loginRes, { 'login status is 200': (r) => r.status === 200 });
  const token = loginRes.json('token');

  const authHeaders = {
    headers: {
      'Authorization': `Bearer ${token}`,
      'Content-Type': 'application/json',
    },
  };

  // 2. Fetch dashboard summary
  const dashRes = http.get(`${BASE_URL}/dashboard/summary`, authHeaders);
  check(dashRes, { 'dashboard status is 200': (r) => r.status === 200 });

  // 3. Paginated assets search
  const assetsRes = http.get(`${BASE_URL}/assets?page=1&pageSize=20`, authHeaders);
  check(assetsRes, { 'assets status is 200': (r) => r.status === 200 });

  sleep(1);
}
```

Run test:
```bash
k6 run load-test.js
```

### 9.3. Backend Tuning for Scale
1. **Connection Pooling**: Set MySQL connection pool minimum: 20, maximum: 200 in connection string (`Max Pool Size=200;Min Pool Size=20`).
2. **EF Core Read-Only Queries**: Use `AsNoTracking()` on all read-only listing endpoints (already implemented in controllers) to eliminate memory tracking overhead.
3. **Database Indexing**: Compound indexes on `(TowerId, FlatNumber)`, `(AssignedToRoleId, Status)` for approval queues, and `(IsActive, FlatId)` for parking allocations.
4. **JWT Verification Caching**: In-memory caching for public keys and committee role claims to minimize database hit frequency during high-velocity authorization checks.
