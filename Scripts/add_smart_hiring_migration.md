# Adding Smart Hiring Migration

Run this command in Package Manager Console to add the new HiringOutcome entity:

```powershell
Add-Migration AddSmartHiringEntities
Update-Database
```

Or using .NET CLI:

```bash
dotnet ef migrations add AddSmartHiringEntities
dotnet ef database update
```

This will create the HiringOutcomes table to track ML model performance and hiring outcomes for continuous learning.



