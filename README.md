# Student Score Manager  
[![GitHub license](https://img.shields.io/badge/license-MIT-blue.svg?style=flat-square)](https://github.com/ainh01/StudentScoreManager/blob/main/LICENSE)  

A Windows Forms application for managing and analyzing student scores. This system supports roles for students, teachers, and administrators, offering features from score entry to detailed analytical reports.  

## Installing / Getting started  

This is a .NET Framework desktop application. To run it, you need:  
- .NET Framework 4.7.2 or higher installed on your system.  
- SQL Server LocalDB or a compatible SQL Server instance.  

To get started, clone the repository, build the project in Visual Studio, and run the executable.  

```shell  
# Clone the repository  
git clone https://github.com/ainh01/StudentScoreManager.git  

# Navigate to the project directory  
cd StudentScoreManager  

# Open in Visual Studio and build the solution (e.g., StudentScoreManager.sln)  
```  

The application will then compile and create an executable in the `bin/Debug` (or `bin/Release`) folder, which you can run.  

## Developing  

### Built With  
- C#  
- .NET Framework 4.7.2  
- Windows Forms  
- ADO.NET for database interaction  
- Microsoft.Extensions.Configuration for app settings  

### Prerequisites  
- Visual Studio 2019 or later (with .NET desktop development workload)  
- SQL Server (LocalDB recommended for development)  

### Setting up Dev  

```shell  
# Clone the repository  
git clone https://github.com/ainh01/StudentScoreManager.git  

# Navigate to the project directory  
cd StudentScoreManager  

# Open the solution file (.sln) in Visual Studio  
start StudentScoreManager.sln  

# Restore NuGet packages (Visual Studio usually does this automatically)  
# Build the solution (Build > Build Solution or F6)  
```  
After cloning, open the `StudentScoreManager.sln` file in Visual Studio. Visual Studio will automatically restore any required NuGet packages. Building the solution will compile the code and prepare the executable.  

### Building  

The project can be built directly from Visual Studio by selecting `Build > Build Solution`. For command-line building:  

```shell  
# Navigate to the solution directory  
cd StudentScoreManager  

# Use MSBuild to build the solution (adjust path to MSBuild.exe as needed)  
"C:\Program Files\Microsoft Visual Studio\2022\Community\MSBuild\Current\Bin\MSBuild.exe" StudentScoreManager.sln /p:Configuration=Debug /p:Platform="Any CPU"  
```  
This command compiles the entire solution in Debug configuration. You can change `Debug` to `Release` for a production build. The output executables will be in `StudentScoreManager\bin\Debug` or `StudentScoreManager\bin\Release`.  

### Deploying / Publishing  
To deploy, you can create an installer package using Visual Studio's "Publish" feature for Windows Forms applications, or simply distribute the compiled executable and its dependencies from the `bin/Release` folder. Ensure the target machine has the correct .NET Framework version installed.  

```shell  
# In Visual Studio:  
# 1. Right-click on the project in Solution Explorer  
# 2. Select 'Publish...'  
# 3. Follow the wizard to create an installer (e.g., ClickOnce deployment)  
```  

## Versioning  

This project uses semantic versioning (SemVer). The current version is not explicitly tagged but follows a conventional `.NET` assembly versioning scheme. New releases will be marked with appropriate version tags. For available versions, refer to the [tags on this repository](https://github.com/ainh01/StudentScoreManager/tags).  

## Configuration  

The application uses `appsettings.json` and `App.config` for configuration.  

- `appsettings.json`: Used for application-specific settings like connection strings.  
- `App.config`: Used for .NET Framework specific configurations.  

**Database Connection String:**  
The connection string for the database is located in `appsettings.json`.  
```json  
{  
  "ConnectionStrings": {  
    "DefaultConnection": "Data Source=(LocalDB)\\MSSQLLocalDB;AttachDbFilename=|DataDirectory|StudentScoreManager.mdf;Integrated Security=True;Connect Timeout=30"  
  },  
  "AppSettings": {  
    "Secret": "YourVeryLongAndSecureSecretKeyHereWhichShouldBeAtLeast128BitsLongAndDifficultToGuessForJWT"  
  }  
}  
```  
**JWT Secret:**  
The `Secret` key in `AppSettings` is used for JWT token generation and validation. It must be a strong, randomly generated string.  

## Tests  

This project currently does not include automated unit or integration tests. Testing is primarily manual, focusing on functional verification of features such as user authentication, CRUD operations for scores, and analytical reports.  

To test the application:  
1. Run the `StudentScoreManager.exe` executable.  
2. Log in with different user roles (student, teacher, admin) to verify role-based access.  
3. Perform operations like adding, editing, and deleting scores (teacher/admin).  
4. View score reports and analytical dashboards.  
5. Verify data persistence by restarting the application.  

## Style guide  

The project adheres to standard C# coding conventions and best practices.  
- **Naming Conventions**: PascalCase for classes, methods, and properties; camelCase for local variables and parameters.  
- **Code Formatting**: Consistent indentation (4 spaces), brace style.  
- **Comments**: XML documentation comments for public members, inline comments for complex logic.  
- **Modularity**: Separation of concerns using architectural patterns like MVC/layered architecture.  

Code analysis tools within Visual Studio are used to maintain code quality.  

## Api Reference  

This is a desktop application and does not expose a public API for external consumption. Internal communication within the application relies on direct method calls to controllers and repositories. Authentication is handled via a custom `SessionManager` and `AuthController` which uses JWTs internally for session management.  

## Database  

**Database System:**  
SQL Server LocalDB (recommended for development and single-user deployment). Can be configured to connect to a full SQL Server instance.  

**Download:**  
SQL Server LocalDB is typically installed with Visual Studio as part of the .NET desktop development workload. Alternatively, you can download it as part of SQL Server Express: [SQL Server Express](https://www.microsoft.com/en-us/sql-server/sql-server-downloads)  

**Schema:**  
The database schema includes tables for:  
- `Users`: Stores user authentication details and roles.  
- `Teachers`: Information about teachers.  
- `Students`: Information about students.  
- `Classes`: Details of academic classes.  
- `Subjects`: Information about subjects taught.  
- `Scores`: Records of student scores for subjects.  
- `Teaches`: Links teachers to classes and subjects.  

The `|DataDirectory|` placeholder in the connection string (`appsettings.json`) refers to the application's executable directory. The `StudentScoreManager.mdf` file should be located in this directory for LocalDB to work correctly. The database is managed using ADO.NET with direct SQL queries or stored procedures.  

**ERD:**  
- Users: (Id, Username, PasswordHash, Role)  
- Teachers: (Id, UserId, Name, Email, Phone)  
- Students: (Id, UserId, Name, ClassId, DateOfBirth)  
- Classes: (Id, Name, SchoolYear, Semester)  
- Subjects: (Id, Name, Description)  
- Teaches: (TeacherId, ClassId, SubjectId, SchoolYear, Semester)  
- Scores: (Id, StudentId, SubjectId, Score1, Score2, FinalScore, DateRecorded)  

## Licensing  

This project is licensed under the MIT License. You can find the full text of the license in the [LICENSE](LICENSE) file in the root of this repository.
