-- =========================================================================
-- RelAte: Tier 1 Hybrid Retrieval Query & Stored Procedure
-- Owner: Priya (Track 3) & Sarvesh (Track 1)
-- Description: Filters 10,000+ candidates down to top ~100-150 candidates by
-- combining keyword/metadata filtering with VECTOR_DISTANCE cosine similarity.
-- =========================================================================

CREATE OR ALTER PROCEDURE dbo.usp_HybridRetrieveCandidates
    @QueryEmbedding VECTOR(1536, FLOAT32),
    @MinExperience DECIMAL(4, 1) = 0.0,
    @TopK INT = 150
AS
BEGIN
    SET NOCOUNT ON;

    -- Tier 1: Vector similarity + experience filtering
    SELECT TOP (@TopK)
        c.candidate_id,
        c.full_name,
        c.email,
        c.linkedin_url,
        c.total_experience_years,
        c.current_or_last_role,
        c.raw_resume_text,
        VECTOR_DISTANCE('cosine', ce.embedding, @QueryEmbedding) AS cosine_distance,
        (1.0 - VECTOR_DISTANCE('cosine', ce.embedding, @QueryEmbedding)) AS vector_similarity,
        STRING_AGG(cs.canonical_skill, ', ') AS matched_canonical_skills
    FROM dbo.candidates c
    INNER JOIN dbo.candidate_embeddings ce 
        ON c.candidate_id = ce.candidate_id
    LEFT JOIN dbo.candidate_skills cs 
        ON c.candidate_id = cs.candidate_id
    WHERE c.total_experience_years >= (@MinExperience * 0.7) -- Allow reasonable leeway for experience
    GROUP BY 
        c.candidate_id,
        c.full_name,
        c.email,
        c.linkedin_url,
        c.total_experience_years,
        c.current_or_last_role,
        c.raw_resume_text,
        ce.embedding
    ORDER BY cosine_distance ASC;
END
GO
