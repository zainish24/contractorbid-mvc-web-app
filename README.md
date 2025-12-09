# 🏗️ Contractor Bidding Dashboard - ASP.NET MVC

![ASP.NET MVC](https://img.shields.io/badge/ASP.NET-MVC%20Web%20App-purple)
![.NET Core](https://img.shields.io/badge/.NET-6.0-blue)
![SQL Server](https://img.shields.io/badge/Database-SQL%20Server-red)
![Contractor Platform](https://img.shields.io/badge/Platform-Contractor%20Bidding-green)

A secure **ASP.NET MVC Web Application** for contractors to calculate and submit bids on construction jobs using intelligent backend calculations.

## ✨ Features

### 👷 Contractor Features
- **Secure Registration & Login** - JWT-based authentication
- **Bid Settings Management** - Customize labor rates, material margins, travel costs
- **Job Browsing** - View available jobs with filtering
- **Smart Bid Calculation** - Automated bid calculation using backend logic
- **Bid History** - Track submitted bids and their status
- **Profile Management** - Update company details and preferences

### 💼 Job Management
- **Job Listings** - View available construction projects
- **Budget Ranges** - Filter by project budgets
- **Location-based Search** - Find jobs in preferred areas
- **Bid Submission** - Submit competitive bids securely

### ⚙️ Admin Features
- **Job Posting** - Create and manage construction job listings
- **Bid Monitoring** - Track all contractor submissions
- **Contractor Management** - Oversee platform users
- **Analytics Dashboard** - Platform performance insights

## 🛠️ Built With

- **Framework**: ASP.NET Core MVC
- **Backend**: C# .NET 8
- **Database**: SQL Server + Entity Framework Core
- **Frontend**: Razor Views, Bootstrap 5, JavaScript
- **Authentication**: JWT Tokens
- **APIs**: RESTful Web API

## 🚀 Quick Start

### Prerequisites
- .NET 8.0 SDK or higher
- SQL Server
- Visual Studio 2022 / VS Code

### Installation & Run
```bash
# Clone the repository
git clone https://github.com/zainish24/contractorbid-mvc-web-app.git

# Navigate to project directory
cd contractorbid-mvc-web-app

# Update database connection in appsettings.json
# Run database migrations
dotnet ef database update

# Launch the application
dotnet run
```

## 📁 Project Structure
```
ContractorBiddingDashboard/
├── Controllers/          # MVC Controllers (Auth, Dashboard, Jobs, Bids)
├── Models/              # Data Models (Contractor, Job, Bid, Settings)
├── Views/               # Razor Pages (Dashboard, Jobs, Settings)
├── Migrations/          # Entity Framework Migrations
├── wwwroot/             # Static Files (CSS, JS, Images)
├── Services/            # Business Logic (Bid Calculation, Auth)
└── Data/                # DbContext & Database Configuration
```

## ⚙️ Configuration

1. **Update database connection** in `appsettings.json`:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=your-database-here;Trusted_Connection=true;"
  },
  "JwtSettings": {
    "Secret": "your-secret-key-here",
    "ExpiryInHours": 24
  }
}
```

2. **For development**, use User Secrets for sensitive data:
```bash
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "your-connection-string"
```

## 🔐 Security Features

- **JWT Authentication** - Secure token-based auth
- **Password Hashing** - BCrypt for password security
- **Input Validation** - Prevent SQL injection & XSS
- **Role-based Access** - Different permissions per user type
- **Secure API Calls** - Protected backend communication

## 🎯 Key Technical Features

- **MVC Architecture** - Clean separation of concerns
- **Entity Framework Core** - Efficient ORM with migrations
- **Repository Pattern** - Data access abstraction
- **Dependency Injection** - Loosely coupled components
- **Responsive Design** - Mobile-friendly Bootstrap 5 UI
- **RESTful APIs** - Clean API design for frontend communication

## 📊 Database Schema

```sql
-- Main Tables:
-- Contractors, ContractorSettings, Jobs, Bids
-- Supports bid calculations, job tracking, and contractor management
```

## 🚀 Deployment

### Local Development
```bash
dotnet build
dotnet run
```

### Publish to Production
```bash
dotnet publish -c Release
```

## 🤝 Contributing

1. Fork the repository
2. Create your feature branch (`git checkout -b feature/AmazingFeature`)
3. Commit your changes (`git commit -m 'Add some AmazingFeature'`)
4. Push to the branch (`git push origin feature/AmazingFeature`)
5. Open a Pull Request

## 📝 License

This project is licensed under the MIT License.

---

**Repository**: `contractorbid-mvc-web-app`  
**Technology**: ASP.NET Core MVC Web Application  
**Database**: SQL Server with Entity Framework Core  
**Status**: In Development  

**Built with ❤️ for Construction Industry Professionals | Secure Bidding Platform**  
**⭐ If this project helps contractors streamline their bidding process, please give it a star!**

---

*Empowering contractors with smart bidding solutions since 2024* 🏗️🚀
