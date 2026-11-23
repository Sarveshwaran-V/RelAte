# Azure SQL & Vector Storage Setup

This directory contains database DDL scripts and stored procedures for the RelAte candidate intelligence engine.

## Schema Overview

1. **`dbo.candidates`**: Stores raw resume content, parsed demographic information (Name, Email, Phone, LinkedIn URL, Experience, Role).
2. **`dbo.candidate_skills`**: Stores canonical normalized skill tags and mentions extracted from resumes.
3. **`dbo.candidate_embeddings`**: Stores 1536-dimensional vector embeddings generated using Gemini / Azure OpenAI embedding models, using Azure SQL's native `VECTOR(1536, FLOAT32)` data type.

## Execution Order

1. Connect to your Azure SQL database instance via Azure Data Studio, SSMS, or VS Code SQL extension.
2. Run `schema.sql` to create all tables and relational indexes.
3. Run `hybrid_retrieval.sql` to install the hybrid search stored procedure `usp_HybridRetrieveCandidates`.
4. (Optional for local dev) Run `seed_mock.sql` to populate sample candidates.

## Connecting from Backend

Set the connection string in your `.env`:
```env
AZURE_SQL_CONNECTION_STRING="Driver={ODBC Driver 18 for SQL Server};Server=tcp:<your-server>.database.windows.net,1433;Database=relate_db;Uid=<user>;Pwd=<password>;Encrypt=yes;TrustServerCertificate=no;Connection Timeout=30;"
```
