# Cabinet Medical Management System - Technical Architecture

## 🏛️ Architecture Overview

The Cabinet application follows a **monolithic architecture** using the **Razor Pages** pattern, which is well-suited for form-based web applications with server-side rendering.

### Architecture Pattern: Razor Pages (MVVM-like)

```
┌─────────────────────────────────────────────────────────────┐
│                        Browser (Client)                      │
│  ┌──────────────┐  ┌──────────────┐  ┌──────────────┐      │
│  │   HTML/CSS   │  │  JavaScript  │  │  Bootstrap   │      │
│  └──────────────┘  └──────────────┘  └──────────────┘      │
└─────────────────────────────────────────────────────────────┘
                            ↕ HTTP/HTTPS
┌─────────────────────────────────────────────────────────────┐
│                    ASP.NET Core Web Server                   │
│  ┌──────────────────────────────────────────────────────┐  │
│  │              Middleware Pipeline                      │  │
│  │  ┌──────┐ ┌──────┐ ┌──────┐ ┌──────┐ ┌──────┐       │  │
│  │  │Static│→│Auth  │→│Route │→│Razor │→│Error │       │  │
│  │  │Files │ │      │ │      │ │Pages │ │Handle│       │  │
│  │  └──────┘ └──────┘ └──────┘ └──────┘ └──────┘       │  │
│  └──────────────────────────────────────────────────────┘  │
│                                                              │
│  ┌──────────────────────────────────────────────────────┐  │
│  │                 Razor Pages Layer                     │  │
│  │  ┌─────────────┐  ┌─────────────┐  ┌─────────────┐  │  │
│  │  │ .cshtml     │  │ .cshtml.cs  │  │  Models     │  │  │
│  │  │ (View)      │←→│ (PageModel) │←→│ (Binding)   │  │  │
│  │  └─────────────┘  └─────────────┘  └─────────────┘  │  │
│  └──────────────────────────────────────────────────────┘  │
│                            ↕                                 │
│  ┌──────────────────────────────────────────────────────┐  │
│  │            Entity Framework Core (ORM)                │  │
│  │  ┌──────────────┐  ┌──────────────┐                  │  │
│  │  │  DbContext   │  │   DbSet<T>   │                  │  │
│  │  └──────────────┘  └──────────────┘                  │  │
│  └──────────────────────────────────────────────────────┘  │
└─────────────────────────────────────────────────────────────┘
                            ↕ ADO.NET
┌─────────────────────────────────────────────────────────────┐
│                      SQL Server Database                     │
│  ┌──────────┐ ┌──────────┐ ┌──────────┐ ┌──────────┐       │
│  │ Patient  │ │Consulta- │ │Rendezvous│ │ Employer │       │
│  │          │ │  tion    │ │          │ │          │       │
│  └──────────┘ └──────────┘ └──────────┘ └──────────┘       │
└─────────────────────────────────────────────────────────────┘
```

## 🔧 Core Components

### 1. Program.cs - Application Bootstrap

**Purpose**: Configure services and middleware pipeline

```csharp
Key Configurations:
├── Services
│   ├── Razor Pages with Authorization
│   ├── DbContext with SQL Server
│   └── Cookie Authentication
└── Middleware Pipeline
    ├── HTTPS Redirection
    ├── Static Files
    ├── Routing
    ├── Authentication
    ├── Authorization
    └── Razor Pages Endpoint Mapping
```

**Authentication Setup**:
- Scheme: Cookie-based
- Login Path: `/Account/Login`
- Expiration: 30 minutes
- Persistent: Yes (survives browser close)

**Authorization**:
- Default: All pages require authentication
- Exception: `/Account/Login` allows anonymous access

### 2. Data Layer - ApplicationDbContext

**File**: `Data/ApplicationDbContext.cs`

**DbSets** (Database Tables):
```csharp
public DbSet<Employer> Employer { get; set; }
public DbSet<Consultation> Consultation { get; set; }
public DbSet<ConsultationService> ConsultationService { get; set; }
public DbSet<Patient> Patient { get; set; }
public DbSet<Medicament> Medicament { get; set; }
public DbSet<Ordonnance> Ordonnance { get; set; }
public DbSet<OrdonnanceMedicament> OrdonnanceMedicament { get; set; }
public DbSet<Rendezvous> Rendezvous { get; set; }
public DbSet<Service> Service { get; set; }
public DbSet<Stock> Stocks { get; set; }
public DbSet<CategoryStock> CategoryStocks { get; set; }
public DbSet<StockMovement> StockMovements { get; set; }
public DbSet<CabinetInfo> CabinetInfo { get; set; }
```

**Connection String**:
```
Server=DESKTOP-BNISB42;
Database=Cabinetweb;
Trusted_Connection=True;
TrustServerCertificate=True
```

### 3. Models Layer - Domain Entities

#### Entity Relationships

```
Patient (1) ──────< (M) Consultation
   │                      │
   │                      └──> (1) Service
   │
   └──────< (M) Ordonnance
                  │
                  └──────< (M) OrdonnanceMedicament >──────┐
                                                            │
Medicament (1) ────────────────────────────────────────────┘

Stock (M) >────── (1) CategoryStock

Stock (1) ──────< (M) StockMovement

Rendezvous (standalone - no FK relationships)

Employer (standalone - authentication entity)

Service (standalone - lookup table)

CabinetInfo (standalone - settings)
```

#### Key Model Attributes

**Data Annotations Used**:
- `[Table("name")]` - Maps to specific database table
- `[Key]` - Primary key designation
- `[DatabaseGenerated(DatabaseGeneratedOption.Identity)]` - Auto-increment
- `[Column("name")]` - Maps to specific column
- `[StringLength(n)]` - Maximum string length
- `[Required]` - Not null constraint
- `[ForeignKey("PropertyName")]` - Foreign key relationship
- `[NotMapped]` - Exclude from database
- `[EmailAddress]` - Email validation
- `[DataType(DataType.Password)]` - Password field
- `[Compare("PropertyName")]` - Property comparison validation

### 4. Pages Layer - Razor Pages

#### Page Structure Pattern

Each functional area follows this pattern:
```
FeatureName/
├── Index.cshtml          # List/Grid view
├── Index.cshtml.cs       # List page model
├── Create.cshtml         # Create form
├── Create.cshtml.cs      # Create logic
├── Edit.cshtml           # Edit form
└── Edit.cshtml.cs        # Edit logic
```

#### PageModel Lifecycle

```csharp
Request → OnGet/OnGetAsync()
          ↓
          Load data from database
          ↓
          Render .cshtml view
          ↓
          User interaction (form submit)
          ↓
          OnPost/OnPostAsync()
          ↓
          Validate ModelState
          ↓
          Save to database
          ↓
          RedirectToPage() or return Page()
```

### 5. View Components

**Location**: `ViewComponents/`

**LowStockCountViewComponent**:
- Purpose: Display low stock badge in navigation
- Invoked: `@await Component.InvokeAsync("LowStockCount")`
- Returns: Badge with count of items below alarm threshold

## 🔐 Security Architecture

### Authentication Flow

```
1. User visits protected page
   ↓
2. Middleware checks authentication cookie
   ↓
3. If not authenticated → Redirect to /Account/Login
   ↓
4. User submits credentials
   ↓
5. LoginModel.OnPostAsync() validates against database
   ↓
6. If valid:
   - Create ClaimsIdentity with user info
   - Sign in with CookieAuthenticationDefaults
   - Create encrypted authentication cookie
   - Redirect to requested page
   ↓
7. Subsequent requests include cookie
   ↓
8. Middleware validates cookie and populates User.Identity
```

### Claims Structure

```csharp
new Claim(ClaimTypes.Name, user.Email)
new Claim("FullName", $"{user.Nom} {user.Prenom}")
new Claim(ClaimTypes.Role, user.Role)
```

**Usage in Views**:
```csharp
@User.Identity.Name                    // Email
@User.FindFirst("FullName")?.Value     // Full name
@User.IsInRole("Admin")                // Role check
```

### Authorization Patterns

**Page-level**:
```csharp
[Authorize]                           // Requires any authenticated user
[Authorize(Roles = "Admin")]          // Requires specific role
```

**Global**:
```csharp
// In Program.cs
options.Conventions.AuthorizeFolder("/");
options.Conventions.AllowAnonymousToPage("/Account/Login");
```

## 🎨 Frontend Architecture

### Layout Hierarchy

```
_Layout.cshtml (Master Template)
├── <head>
│   ├── Bootstrap CSS
│   ├── Font Awesome
│   ├── site.css (Custom styles)
│   └── theme.js (Theme initialization)
├── <header>
│   └── Navigation Bar
│       ├── Brand
│       ├── Menu Items (if authenticated)
│       ├── Theme Toggle
│       └── User Dropdown
├── <main>
│   └── @RenderBody() ← Page content injected here
└── <footer>
    └── Copyright info

Individual Pages (.cshtml)
├── @page directive
├── @model directive
├── @section Styles { } (optional)
├── Page content
└── @section Scripts { } (optional)
```

### CSS Architecture

**File**: `wwwroot/css/site.css`

**Structure**:
```css
1. Base Styles (html, body)
2. CSS Variables
   ├── :root (Light theme)
   └── [data-theme="dark"] (Dark theme)
3. Component Styles
   ├── Navbar
   ├── Dropdown
   ├── Cards
   ├── Footer
   └── Theme Toggle
4. Utility Classes
```

**Theme System**:
- Uses CSS custom properties (variables)
- JavaScript toggles `data-theme` attribute on `<html>`
- Preference stored in `localStorage`
- Smooth transitions on theme change

### JavaScript Architecture

**theme.js**:
```javascript
Purpose: Theme management
- Loads saved preference on page load
- Toggles theme on button click
- Saves preference to localStorage
- Updates data-theme attribute
```

**site.js**:
```javascript
Purpose: General site functionality
- Form validation helpers
- Dynamic UI interactions
- AJAX calls (if any)
```

## 💾 Data Access Patterns

### Repository Pattern (Not Implemented)

Currently, the application uses **direct DbContext access** in PageModels:

```csharp
public class IndexModel : PageModel
{
    private readonly ApplicationDbContext _context;
    
    public IndexModel(ApplicationDbContext context)
    {
        _context = context;
    }
    
    public async Task OnGetAsync()
    {
        Patients = await _context.Patient.ToListAsync();
    }
}
```

**Pros**:
- Simple and straightforward
- Less code overhead
- Good for small to medium applications

**Cons**:
- Tight coupling to EF Core
- Harder to unit test
- Business logic mixed with presentation

### Recommended Pattern (Future Enhancement)

```csharp
// Repository Interface
public interface IPatientRepository
{
    Task<List<Patient>> GetAllAsync();
    Task<Patient?> GetByIdAsync(int id);
    Task AddAsync(Patient patient);
    Task UpdateAsync(Patient patient);
    Task DeleteAsync(int id);
}

// Service Layer
public class PatientService
{
    private readonly IPatientRepository _repository;
    
    public async Task<List<Patient>> GetActivePatients()
    {
        // Business logic here
    }
}

// PageModel
public class IndexModel : PageModel
{
    private readonly IPatientService _patientService;
    
    public async Task OnGetAsync()
    {
        Patients = await _patientService.GetActivePatients();
    }
}
```

## 🔄 Entity Framework Core Patterns

### Eager Loading (Include)

```csharp
// Load related entities in single query
var consultations = await _context.Consultation
    .Include(c => c.Patient)
    .ToListAsync();
```

### Lazy Loading (Not Enabled)

Virtual navigation properties exist but lazy loading is not configured:
```csharp
public virtual ICollection<Consultation> Consultations { get; set; }
```

### Explicit Loading (Not Used)

Could be used for on-demand loading:
```csharp
await _context.Entry(patient)
    .Collection(p => p.Consultations)
    .LoadAsync();
```

## 📊 Query Optimization Patterns

### Dashboard Queries (Index.cshtml.cs)

**Statistics** - Efficient aggregation:
```csharp
TotalPatients = await _context.Patient.CountAsync();
TodayRevenue = await _context.Consultation
    .Where(c => c.DateConsultation.Value.Date == today)
    .SumAsync(c => (c.PrixConsul ?? 0) - (c.Remise ?? 0));
```

**Lists** - Filtered with includes:
```csharp
WaitingList = await _context.Consultation
    .Include(c => c.Patient)
    .Where(c => c.Etat == "Reception" || c.Etat == "Visite")
    .OrderBy(c => c.DateConsultation)
    .ToListAsync();
```

**Potential Optimizations**:
- Add indexes on frequently queried columns (DateConsultation, Etat)
- Use `AsNoTracking()` for read-only queries
- Implement pagination for large result sets
- Cache dashboard statistics

## 🧪 Testing Strategy (Not Implemented)

### Recommended Testing Pyramid

```
                    ┌─────────┐
                    │   E2E   │  ← Selenium/Playwright
                    └─────────┘
                  ┌─────────────┐
                  │ Integration │  ← WebApplicationFactory
                  └─────────────┘
              ┌───────────────────┐
              │   Unit Tests      │  ← xUnit/NUnit
              └───────────────────┘
```

**Unit Tests** - Test business logic:
```csharp
[Fact]
public void CalculateConsultationTotal_WithDiscount_ReturnsCorrectAmount()
{
    // Arrange
    var consultation = new Consultation 
    { 
        PrixConsul = 500, 
        Remise = 50 
    };
    
    // Act
    var total = consultation.PrixConsul - consultation.Remise;
    
    // Assert
    Assert.Equal(450, total);
}
```

**Integration Tests** - Test database operations:
```csharp
[Fact]
public async Task CreatePatient_SavesToDatabase()
{
    // Arrange
    var options = new DbContextOptionsBuilder<ApplicationDbContext>()
        .UseInMemoryDatabase("TestDb")
        .Options;
    
    using var context = new ApplicationDbContext(options);
    
    // Act
    context.Patient.Add(new Patient { Nom = "Test" });
    await context.SaveChangesAsync();
    
    // Assert
    Assert.Equal(1, await context.Patient.CountAsync());
}
```

## 🚀 Deployment Architecture

### Development Environment
```
Developer Machine
├── Visual Studio 2022 / VS Code
├── .NET 8.0 SDK
├── SQL Server LocalDB
└── IIS Express (Development Server)
```

### Production Deployment Options

**Option 1: IIS on Windows Server**
```
Windows Server
├── IIS 10+
├── .NET 8.0 Runtime
├── SQL Server (Full or Express)
└── Application Pool (No Managed Code)
```

**Option 2: Azure App Service**
```
Azure Cloud
├── App Service (Windows/Linux)
├── Azure SQL Database
└── Application Insights (Monitoring)
```

**Option 3: Docker Container**
```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY publish/ .
ENTRYPOINT ["dotnet", "Cabinet.dll"]
```

## 📈 Performance Considerations

### Current Performance Characteristics

**Strengths**:
- Server-side rendering (fast initial page load)
- Minimal JavaScript (lightweight client)
- Direct database access (low latency)

**Bottlenecks**:
- No caching (repeated database queries)
- No pagination (large result sets load all data)
- Synchronous operations in some areas
- No CDN for static assets

### Recommended Optimizations

1. **Response Caching**:
```csharp
[ResponseCache(Duration = 60)]
public async Task OnGetAsync() { }
```

2. **Memory Caching**:
```csharp
services.AddMemoryCache();
// In PageModel
_cache.GetOrCreateAsync("key", async entry => 
{
    entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5);
    return await _context.Patient.ToListAsync();
});
```

3. **Database Indexes**:
```sql
CREATE INDEX IX_Consultation_DateConsultation ON Consultation(date_consultation);
CREATE INDEX IX_Consultation_Etat ON Consultation(etat);
CREATE INDEX IX_Rendezvous_DateHeure ON rendezvous(dateheure);
```

4. **AsNoTracking for Read-Only**:
```csharp
var patients = await _context.Patient
    .AsNoTracking()
    .ToListAsync();
```

## 🔍 Monitoring & Logging (Not Implemented)

### Recommended Logging Strategy

**Serilog Configuration**:
```csharp
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.File("logs/cabinet-.txt", rollingInterval: RollingInterval.Day)
    .WriteTo.Seq("http://localhost:5341")
    .CreateLogger();
```

**Application Insights** (Azure):
```csharp
services.AddApplicationInsightsTelemetry();
```

---

**Document Version**: 1.0  
**Last Updated**: February 4, 2026  
**Maintained By**: Development Team
