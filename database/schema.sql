-- =========================================================================
-- RelAte: Azure SQL Schema DDL (Relational + Vector Storage)
-- Owner: Sarvesh (Track 1)
-- =========================================================================

-- 1. Candidates Table (Structured Candidate Profile)
IF OBJECT_ID('dbo.candidates', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.candidates (
        candidate_id UNIQUEIDENTIFIER DEFAULT NEWID() PRIMARY KEY,
        full_name NVARCHAR(200) NOT NULL,
        email NVARCHAR(255) NULL,
        phone NVARCHAR(50) NULL,
        linkedin_url NVARCHAR(500) NULL,
        total_experience_years DECIMAL(4, 1) NOT NULL DEFAULT 0.0,
        current_or_last_role NVARCHAR(200) NULL,
        raw_resume_text NVARCHAR(MAX) NULL,
        resume_file_path NVARCHAR(500) NULL,
        created_at DATETIME2 DEFAULT SYSUTCDATETIME() NOT NULL,
        updated_at DATETIME2 DEFAULT SYSUTCDATETIME() NOT NULL
    );

    CREATE NONCLUSTERED INDEX IX_candidates_experience 
        ON dbo.candidates(total_experience_years);
    CREATE NONCLUSTERED INDEX IX_candidates_email 
        ON dbo.candidates(email) WHERE email IS NOT NULL;
END
GO

-- 2. Candidate Canonical Skills (Normalized Skill Tags)
IF OBJECT_ID('dbo.candidate_skills', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.candidate_skills (
        skill_id BIGINT IDENTITY(1,1) PRIMARY KEY,
        candidate_id UNIQUEIDENTIFIER NOT NULL,
        canonical_skill NVARCHAR(100) NOT NULL,
        raw_skill_mention NVARCHAR(150) NULL,
        years_experience DECIMAL(3, 1) NULL,
        created_at DATETIME2 DEFAULT SYSUTCDATETIME() NOT NULL,
        CONSTRAINT FK_candidate_skills_candidate FOREIGN KEY (candidate_id) 
            REFERENCES dbo.candidates(candidate_id) ON DELETE CASCADE
    );

    CREATE NONCLUSTERED INDEX IX_candidate_skills_canonical 
        ON dbo.candidate_skills(canonical_skill) INCLUDE (candidate_id);
    CREATE NONCLUSTERED INDEX IX_candidate_skills_candidate 
        ON dbo.candidate_skills(candidate_id);
END
GO

-- 3. Candidate Embeddings (Azure SQL Native Vector Storage)
-- Note: Azure SQL Database supports VECTOR(1536, FLOAT32) for native vector similarity.
-- For local SQL Server (Linux/Docker) without native VECTOR type, VARBINARY(8000) or NVARCHAR(MAX) can be substituted.
IF OBJECT_ID('dbo.candidate_embeddings', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.candidate_embeddings (
        candidate_id UNIQUEIDENTIFIER PRIMARY KEY,
        embedding VECTOR(1536, FLOAT32) NOT NULL,
        model_name NVARCHAR(100) NOT NULL DEFAULT 'text-embedding-004',
        last_updated DATETIME2 DEFAULT SYSUTCDATETIME() NOT NULL,
        CONSTRAINT FK_candidate_embeddings_candidate FOREIGN KEY (candidate_id)
            REFERENCES dbo.candidates(candidate_id) ON DELETE CASCADE
    );
END
GO
