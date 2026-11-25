import { ParsedJD, CandidateMatchResponse } from './contracts.model';

export const MOCK_PARSED_JD: ParsedJD = {
  role_title: "Senior .NET Backend Engineer",
  mandatory_skills: ["C#", ".NET Core", "SQL Server"],
  optional_skills: ["Azure", "Docker", "Microservices"],
  min_experience_years: 3.0
};

export const MOCK_CANDIDATES: CandidateMatchResponse[] = [
  {
    candidate_id: "6a2f8910-b38c-42e5-b827-557766551111",
    name: "Arun Kumar",
    linkedin_url: "https://www.linkedin.com/in/arunkumar-dev",
    total_experience_years: 5.2,
    confidence_score: 94.0,
    score_breakdown: {
      mandatory_matched: ["C#", ".NET Core", "SQL Server"],
      mandatory_missing: [],
      optional_matched: ["Azure", "Docker"]
    },
    evidence_snippets: [
      "Led development of enterprise financial processing system in .NET Core 8 and C# with Microsoft SQL Server backend.",
      "Automated CI/CD pipelines deploying Docker microservices on Azure App Services."
    ],
    fit_summary: "Exceptional fit matching all mandatory (.NET Core, C#, SQL Server) and optional skills (Azure, Docker) with 5.2 years experience."
  },
  {
    candidate_id: "550e8400-e29b-41d4-a716-446655440000",
    name: "Pooja Sharma",
    linkedin_url: "https://www.linkedin.com/in/poojasharma-tech",
    total_experience_years: 4.0,
    confidence_score: 76.7,
    score_breakdown: {
      mandatory_matched: ["C#", ".NET Core"],
      mandatory_missing: ["SQL Server"],
      optional_matched: ["Azure"]
    },
    evidence_snippets: [
      "Designed and deployed .NET Core REST APIs handling 50k+ daily requests on Azure Functions.",
      "Hands-on with C# asynchronous programming and microservices architecture."
    ],
    fit_summary: "Strong .NET Core and C# backend engineer with Azure deployment experience; lacks explicit SQL Server mention on resume."
  },
  {
    candidate_id: "771b9922-c49d-43f6-a938-668877662222",
    name: "Vikramaditya Roy",
    linkedin_url: "https://www.linkedin.com/in/vikram-roy-dev",
    total_experience_years: 6.5,
    confidence_score: 87.5,
    score_breakdown: {
      mandatory_matched: ["C#", "SQL Server"],
      mandatory_missing: [".NET Core"],
      optional_matched: ["Azure", "Docker", "Microservices"]
    },
    evidence_snippets: [
      "Extensive enterprise C# (.NET Framework & WCF) with complex SQL Server stored procedures and query optimization.",
      "Architected distributed microservices deployed via Docker containers in Azure."
    ],
    fit_summary: "Deep SQL Server and enterprise C# tenure with rich Azure/Docker exposure; primary background in traditional .NET Framework."
  },
  {
    candidate_id: "882c0033-d50e-44a7-ba49-779988773333",
    name: "Sneha Patel",
    linkedin_url: "https://www.linkedin.com/in/snehapatel-eng",
    total_experience_years: 3.5,
    confidence_score: 91.2,
    score_breakdown: {
      mandatory_matched: ["C#", ".NET Core", "SQL Server"],
      mandatory_missing: [],
      optional_matched: ["Microservices"]
    },
    evidence_snippets: [
      "Built resilient C# .NET Core microservices persisting high-volume transactional data to Microsoft SQL Server."
    ],
    fit_summary: "Solid backend profile meeting all mandatory criteria with active .NET Core microservices experience."
  }
];
