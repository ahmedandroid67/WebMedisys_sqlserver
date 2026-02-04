# Cabinet Medical Management System - Project Overview

## 📋 Project Summary

**Cabinet** is a comprehensive medical practice management web application built with **ASP.NET Core 8.0** using **Razor Pages**. It provides a complete solution for managing patients, consultations, appointments, medications, prescriptions, inventory, and staff in a medical office environment.

## 🏗️ Technology Stack

### Backend
- **Framework**: ASP.NET Core 8.0 (Razor Pages)
- **Language**: C# with nullable reference types enabled
- **ORM**: Entity Framework Core 8.0
- **Database**: SQL Server (LocalDB)
- **Authentication**: Cookie-based authentication

### Frontend
- **UI Framework**: Bootstrap 5
- **Icons**: Font Awesome 6.5.1
- **JavaScript**: jQuery + Vanilla JS
- **Styling**: Custom CSS with dark/light theme support

### Database Connection
```
Server: DESKTOP-BNISB42
Database: Cabinetweb
Authentication: Windows Authentication (Trusted_Connection)
```

## 📁 Project Structure

```
Cabinet/
├── Data/
│   └── ApplicationDbContext.cs          # EF Core DbContext
├── Models/                               # Domain entities
│   ├── Patient.cs                        # Patient information
│   ├── Consultation.cs                   # Medical consultations
│   ├── Rendezvous.cs                     # Appointments
│   ├── Employer.cs                       # Staff/employees
│   ├── Medicament.cs                     # Medication database
│   ├── Ordonnance.cs                     # Prescriptions
│   ├── OrdonnanceMedicament.cs          # Prescription-medication junction
│   ├── Service.cs                        # Medical services/procedures
│   ├── Stock.cs                          # Inventory items
│   ├── StockMovement.cs                  # Inventory movements
│   └── CabinetInfo.cs                    # Practice information
├── Pages/                                # Razor Pages
│   ├── Account/                          # Authentication
│   │   ├── Login.cshtml                  # Login page
│   │   └── Logout.cshtml                 # Logout handler
│   ├── Consultations/                    # Consultation management
│   │   ├── Index.cshtml                  # List consultations
│   │   ├── Create.cshtml                 # New consultation
│   │   ├── Edit.cshtml                   # Edit consultation
│   │   └── PrintOrdonnance.cshtml        # Print prescription
│   ├── Patients/                         # Patient management
│   │   ├── Index.cshtml                  # Patient list
│   │   ├── Create.cshtml                 # Add patient
│   │   └── Edit.cshtml                   # Edit patient
│   ├── Rendezvous/                       # Appointment scheduling
│   ├── Employers/                        # Staff management
│   ├── Medicaments/                      # Medication database
│   ├── services/                         # Medical services
│   ├── Stock/                            # Inventory management
│   ├── Settings/                         # Practice settings
│   ├── Shared/
│   │   └── _Layout.cshtml                # Main layout template
│   └── Index.cshtml                      # Dashboard
├── Migrations/                           # EF Core migrations
├── wwwroot/                              # Static files
│   ├── css/
│   │   └── site.css                      # Custom styles + theming
│   ├── js/
│   │   ├── site.js                       # General scripts
│   │   └── theme.js                      # Theme switcher
│   └── lib/                              # Client libraries
├── Program.cs                            # Application entry point
└── appsettings.json                      # Configuration
```

## 🗄️ Database Schema

### Core Entities

#### **Patient** (`patient` table)
- Patient demographics and contact information
- Fields: IdPatient, Nom, Prenom, CIN, Email, Phone, DateNaiss, Sexe, Adresse
- Relationships: One-to-Many with Consultations and Ordonnances

#### **Consultation** (`Consultation` table)
- Medical consultation records with vitals and notes
- Fields: IdConsultation, PatientId, Service, PrixConsul, Remise, DateConsultation, Etat
- Medical Notes: Signe, Diagnostique, Conduite
- Vitals: TGly, TTension, TPoid, TTaille, TSpo, TImc, TTemp, TFvc, TFev, TLdl
- Workflow States: "Reception" → "Visite" → "Terminer"

#### **Rendezvous** (`rendezvous` table)
- Appointment scheduling
- Fields: IdRdv, Nom, Prenom, DateHeure, Service, Sexe, Phone
- Uses DateTime for proper date/time handling

#### **Employer** (`Employer` table)
- Staff and user accounts
- Fields: IdEmployer, Nom, Prenom, Email, MotPasse, Role, Fonction, Telephone, Adresse
- **Note**: Passwords are currently stored in plain text (security improvement needed)

#### **Medicament** (`medicaments` table)
- Comprehensive medication database
- Fields: ID, CODE, NOM, DCI1, DOSAGE1, UNITE_DOSAGE1, FORME, PRESENTATION, PPV, PH, PRIX_BR, PRINCEPS_GENERIQUE, TAUX_REMBOURSEMENT

#### **Ordonnance** (`Ordonnance` table)
- Prescription records
- Links to Patient and contains multiple medications

#### **Service** (`services` table)
- Medical services/procedures catalog
- Fields: IdService, NomService, Prix, Obs

#### **Stock** (`stock` table)
- Inventory management
- Fields: Id, Nom, Observation, Quantite, Alarme, CategoryId
- Includes low-stock alerting system

#### **CategoryStock** (`category_stock` table)
- Stock categorization
- Fields: Id, Nom, Icone

#### **StockMovement** (`StockMovements` table)
- Inventory movement tracking
- Records stock additions and removals

#### **CabinetInfo** (`CabinetInfo` table)
- Medical practice information
- Practice details for documents and settings

## 🔐 Authentication & Authorization

### Authentication System
- **Type**: Cookie-based authentication
- **Login Path**: `/Account/Login`
- **Session Duration**: 30 minutes
- **Persistent Sessions**: Enabled (stays logged in after browser close)

### Authorization
- All pages require authentication except `/Account/Login`
- Role-based claims stored in authentication cookie
- User claims include: Email, FullName, Role

### Security Considerations
⚠️ **Current Security Issues**:
1. Passwords stored in plain text (should use hashing)
2. No password complexity requirements enforced at database level
3. No account lockout mechanism
4. No two-factor authentication

## 🎨 UI/UX Features

### Theme System
- **Light Mode**: Clean, professional white theme
- **Dark Mode**: Modern dark theme with proper contrast
- **Toggle**: Persistent theme preference stored in localStorage
- **CSS Variables**: Comprehensive theming system using CSS custom properties

### Design Highlights
- Modern, card-based dashboard layout
- Responsive Bootstrap 5 grid system
- Font Awesome icons throughout
- Smooth transitions and hover effects
- Color-coded status badges
- Professional gradient accents

### Dashboard Features
1. **Statistics Cards**:
   - Total Patients count
   - Today's Appointments count
   - Today's Revenue (in DH - Moroccan Dirham)

2. **Waiting Room**:
   - Real-time patient queue
   - Shows consultation state (Reception/Visite)
   - Quick access to patient files

3. **Today's Agenda**:
   - Chronological appointment list
   - Patient contact information
   - Time-based organization

4. **Low Stock Alerts**:
   - Automatic alerts when stock ≤ alarm threshold
   - Category-based organization
   - Direct link to inventory management

## 🔄 Workflow

### Patient Consultation Flow
1. **Appointment Booking** → Patient schedules via Rendezvous
2. **Reception** → Patient arrives, consultation created with "Reception" state
3. **Waiting Room** → Appears on dashboard waiting list
4. **Consultation** → Doctor opens file, state changes to "Visite"
5. **Medical Exam** → Record vitals, symptoms, diagnosis, treatment plan
6. **Prescription** → Create Ordonnance with medications
7. **Completion** → State changes to "Terminer", payment recorded
8. **Print** → Generate prescription document

### Stock Management Flow
1. **Categories** → Organize items (Consommables, Outils, Médicaments)
2. **Items** → Add products with alarm thresholds
3. **Movements** → Track additions/removals
4. **Alerts** → Dashboard shows low stock warnings
5. **Reorder** → Manage inventory based on alerts

## 📊 Key Features

### ✅ Implemented Features
- ✅ Patient registration and management
- ✅ Appointment scheduling
- ✅ Consultation workflow with state management
- ✅ Vital signs recording (10+ parameters)
- ✅ Medical notes (symptoms, diagnosis, treatment)
- ✅ Prescription generation
- ✅ Medication database search
- ✅ Staff/employee management
- ✅ Service catalog with pricing
- ✅ Inventory management with categories
- ✅ Stock movement tracking
- ✅ Low stock alerting
- ✅ Dashboard with real-time statistics
- ✅ Dark/Light theme toggle
- ✅ Responsive design
- ✅ Print-friendly prescription format
- ✅ Practice information settings

### 🔧 Potential Improvements

#### Security
- [ ] Implement password hashing (BCrypt/Argon2)
- [ ] Add password complexity requirements
- [ ] Implement account lockout after failed attempts
- [ ] Add HTTPS enforcement
- [ ] Implement CSRF protection
- [ ] Add audit logging for sensitive operations

#### Features
- [ ] Patient medical history timeline
- [ ] Appointment reminders (SMS/Email)
- [ ] Financial reporting and analytics
- [ ] Multi-doctor support with scheduling
- [ ] Patient portal for self-service
- [ ] Document management (upload medical files)
- [ ] Backup and restore functionality
- [ ] Export data to PDF/Excel
- [ ] Advanced search and filtering
- [ ] Calendar view for appointments

#### Technical
- [ ] Add API layer for mobile app integration
- [ ] Implement caching for performance
- [ ] Add comprehensive error handling
- [ ] Implement logging (Serilog)
- [ ] Add unit and integration tests
- [ ] Implement database backup automation
- [ ] Add data validation at multiple layers
- [ ] Optimize database queries
- [ ] Implement pagination for large lists

#### UX/UI
- [ ] Add loading indicators
- [ ] Implement toast notifications
- [ ] Add keyboard shortcuts
- [ ] Improve mobile responsiveness
- [ ] Add drag-and-drop for file uploads
- [ ] Implement auto-save for forms
- [ ] Add confirmation dialogs for destructive actions

## 🚀 Getting Started

### Prerequisites
- .NET 8.0 SDK
- SQL Server (LocalDB or full version)
- Visual Studio 2022 or VS Code

### Installation Steps

1. **Clone the repository**
   ```bash
   cd c:\Users\Ahmed\Desktop\CabinetWebApp\Cabinet
   ```

2. **Restore dependencies**
   ```bash
   dotnet restore
   ```

3. **Update database connection** (if needed)
   Edit `appsettings.json` to match your SQL Server instance

4. **Apply migrations**
   ```bash
   dotnet ef database update
   ```

5. **Run the application**
   ```bash
   dotnet run
   ```

6. **Access the application**
   - Navigate to `https://localhost:5001` or `http://localhost:5000`
   - Login with an existing employer account

### Creating First User
Since the app requires authentication, you'll need to manually insert a user into the database:

```sql
USE Cabinetweb;

INSERT INTO Employer (Nom, Prenom, Email, MotPasse, Role, Fonction)
VALUES ('Admin', 'System', 'admin@cabinet.com', 'admin123', 'Admin', 'Administrateur');
```

## 📝 Database Migrations History

1. `AddStockAndCategories` - Initial stock management tables
2. `CreateStockTables` - Stock table refinements
3. `addstockmvt` - Stock movement tracking
4. `UpdateMvtk` - Movement updates
5. `FixDateHeureToDateTime` - Fixed DateTime handling in Rendezvous
6. `Fixrendezvous` - Rendezvous table corrections
7. `AddCabinetInfoTable` - Practice information
8. `AddServiceToConsultation` - Service tracking in consultations

## 🌍 Localization

- **Primary Language**: French (France)
- **Currency**: Moroccan Dirham (DH)
- **Date Format**: dd/MM/yyyy
- **Phone Format**: Moroccan format (0XXXXXXXXX)

## 📄 License & Credits

- **Developer**: Laaraichi.com
- **Year**: 2026
- **Framework**: ASP.NET Core (Microsoft)
- **UI Framework**: Bootstrap (Twitter)
- **Icons**: Font Awesome

## 🔗 Related Documentation

- [ASP.NET Core Documentation](https://docs.microsoft.com/aspnet/core)
- [Entity Framework Core](https://docs.microsoft.com/ef/core)
- [Bootstrap 5](https://getbootstrap.com/docs/5.0)
- [Font Awesome](https://fontawesome.com)

---

**Last Updated**: February 4, 2026
**Version**: 1.0
**Status**: Production Ready (with security improvements recommended)
