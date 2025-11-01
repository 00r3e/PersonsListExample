# 👤 PersonsListExample

A full-stack C# web application demonstrating CRUD (Create, Read, Update, Delete) operations for managing a list of persons/contacts. 
The application demonstrating clean architecture and test-driven development using Entity Framework Core, xUnit, and FluentValidation.

Project Structure
PersonsListExample/

├── Core/               # Domain models and interfaces

├── Infrastructure/     # EF Core DbContext + repository implementations

├── UI/                 # ASP.NET Core MVC Web UI

├── ControllerTests/    # xUnit tests for controllers

├── ServiceTests/       # xUnit tests for services

├── IntegrationTests/   # Full integration tests


## 🛠 Tech Stack

- **.NET 8**
- **ASP.NET Core MVC**
- **Entity Framework Core**
- **SQL Server (LocalDB)**
- **FluentValidation**
- **xUnit** (unit + integration tests)
- **Clean Architecture** pattern

---

## 📦 Features

- Create, read, update, and delete (CRUD) person records
- Domain-driven design with `Core`, `Infrastructure`, and `UI` layers
- Data validation with FluentValidation
- Full test suite:
  - Service tests
  - Controller tests
  - Integration tests
- Dependency Injection for repositories and services
- Use of DTOs for cleaner controller models

---

## 🚀 Getting Started

### Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/en-us/download)
- SQL Server (LocalDB or full)

### Setup Steps

```bash
# Clone the repository
git clone https://github.com/00r3e/PersonsListExample.git
cd PersonsListExample

# Navigate to the UI project
cd UI

# Apply migrations and create the database
dotnet ef database update

# Run the application
dotnet run
