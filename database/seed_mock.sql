-- =========================================================================
-- RelAte: Mock Seed Data for Local Development
-- =========================================================================

DECLARE @cand1 UNIQUEIDENTIFIER = NEWID();
DECLARE @cand2 UNIQUEIDENTIFIER = NEWID();
DECLARE @cand3 UNIQUEIDENTIFIER = NEWID();

-- Insert Sample Candidates
INSERT INTO dbo.candidates (candidate_id, full_name, email, phone, linkedin_url, total_experience_years, current_or_last_role, raw_resume_text)
VALUES
(@cand1, 'Arun Kumar', 'arun.kumar@example.com', '+91 9876543210', 'https://www.linkedin.com/in/arunkumar-dev', 5.2, 'Senior .NET Developer', 
 'Over 5 years of software engineering experience developing enterprise web services using C#, .NET Core 8, and Microsoft SQL Server. Extensive experience configuring Docker containers and deploying microservices on Azure Kubernetes Service and Azure App Services.'),

(@cand2, 'Pooja Sharma', 'pooja.sharma@example.com', '+91 9876543211', 'https://www.linkedin.com/in/poojasharma-tech', 4.0, 'Backend Engineer', 
 '4 years backend development specializing in C#, ASP.NET Core, and Azure Functions. Built high-throughput REST APIs and managed CI/CD with GitHub Actions. Basic familiarity with containerization.'),

(@cand3, 'Rahul Verma', 'rahul.verma@example.com', '+91 9876543212', 'https://www.linkedin.com/in/rahulverma-data', 3.5, 'Full Stack Engineer', 
 '3.5 years working with Python, Django, React, and PostgreSQL. Recently transitioned to .NET microservices and AWS cloud operations.');

-- Insert Normalized Skills for Candidates
INSERT INTO dbo.candidate_skills (candidate_id, canonical_skill, raw_skill_mention, years_experience)
VALUES
(@cand1, 'C#', 'C#', 5.0),
(@cand1, '.NET Core', '.NET Core 8', 4.5),
(@cand1, 'SQL Server', 'Microsoft SQL Server', 4.0),
(@cand1, 'Azure', 'Azure App Services', 3.0),
(@cand1, 'Docker', 'Docker containers', 2.5),

(@cand2, 'C#', 'C#', 4.0),
(@cand2, '.NET Core', 'ASP.NET Core', 3.5),
(@cand2, 'Azure', 'Azure Functions', 2.0),

(@cand3, 'Python', 'Python', 3.5),
(@cand3, 'Django', 'Django', 3.0),
(@cand3, 'React', 'React.js', 2.5),
(@cand3, 'PostgreSQL', 'PostgreSQL', 3.0);
