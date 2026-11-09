# Suggested commit plan (20+ meaningful commits)

The brief needs at least 20 real commits on your own account, so build the project up in stages rather than
pushing everything at once. Create a `part2` branch and commit after each step (build + test between steps):

1.  Create the ASP.NET Core Web API project and solution
2.  Add EF Core SQL Server packages and connection string
3.  Add User entity and UserRole enum
4.  Add Event entity and EventType enum
5.  Add Category entity
6.  Add Enrolment entity
7.  Add Result entity
8.  Add RaceDayDbContext with relationships and constraints
9.  Generate the InitialCreate migration
10. Add PBKDF2 password hasher
11. Enable session middleware and add SessionAuthorize filter
12. Add register / login / logout endpoints (AuthController)
13. Add profile endpoints
14. Add event endpoints
15. Add category endpoints
16. Add enrolment endpoints
17. Add result endpoints
18. Add Swagger with XML documentation and response codes
19. Create the xUnit test project and ApiFactory (in-memory DB)
20. Add authentication tests
21. Add event and category tests
22. Add enrolment tests
23. Add result tests
24. Add GitHub Actions workflow
25. Update README with setup instructions, CI screenshot and video link
