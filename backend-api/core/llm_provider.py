"""
LLM Provider Abstraction Layer
Supports Gemini Flash (Dev/Validation) and Azure OpenAI (Prod Cutover)
"""
from abc import ABC, abstractmethod
from typing import List, Optional, Dict, Any
import json
import logging
from core.config import settings

logger = logging.getLogger(__name__)


class LLMProvider(ABC):
    """Abstract Base Class for LLM and Embedding Providers."""

    @abstractmethod
    def generate_text(self, prompt: str, system_instruction: Optional[str] = None, temperature: float = 0.2) -> str:
        """Generate text completion from prompt."""
        pass

    @abstractmethod
    def generate_structured(self, prompt: str, system_instruction: Optional[str] = None) -> Dict[str, Any]:
        """Generate parsed JSON output conforming to expected schema."""
        pass

    @abstractmethod
    def generate_embedding(self, text: str) -> List[float]:
        """Generate 1536-dimensional embedding vector for input text."""
        pass


class GeminiFlashProvider(LLMProvider):
    """Google Gemini Flash Provider (Dev Free Tier)."""

    def __init__(self, api_key: Optional[str] = None):
        self.api_key = api_key or settings.GEMINI_API_KEY
        try:
            import google.generativeai as genai
            genai.configure(api_key=self.api_key)
            self._client = genai
        except Exception as e:
            logger.warning(f"Failed to initialize google.generativeai: {e}")
            self._client = None

    def generate_text(self, prompt: str, system_instruction: Optional[str] = None, temperature: float = 0.2) -> str:
        if not self._client or not self.api_key:
            raise ValueError("GEMINI_API_KEY is not configured or google-generativeai is not installed.")
        
        model = self._client.GenerativeModel(
            model_name=settings.GEMINI_MODEL,
            system_instruction=system_instruction
        )
        response = model.generate_content(
            prompt,
            generation_config={"temperature": temperature}
        )
        return response.text.strip()

    def generate_structured(self, prompt: str, system_instruction: Optional[str] = None) -> Dict[str, Any]:
        raw_text = self.generate_text(prompt, system_instruction, temperature=0.0)
        # Strip potential markdown code fences ```json ... ```
        clean_text = raw_text.strip()
        if clean_text.startswith("```json"):
            clean_text = clean_text[7:]
        elif clean_text.startswith("```"):
            clean_text = clean_text[3:]
        if clean_text.endswith("```"):
            clean_text = clean_text[:-3]
        return json.loads(clean_text.strip())

    def generate_embedding(self, text: str) -> List[float]:
        if not self._client or not self.api_key:
            raise ValueError("GEMINI_API_KEY is not configured.")
        result = self._client.embed_content(
            model=settings.GEMINI_EMBEDDING_MODEL,
            content=text,
            task_type="semantic_similarity"
        )
        embedding = result.get("embedding", [])
        return embedding


class AzureOpenAIProvider(LLMProvider):
    """Azure OpenAI Provider (Production Deployment)."""

    def __init__(self):
        try:
            from openai import AzureOpenAI
            self._client = AzureOpenAI(
                api_key=settings.AZURE_OPENAI_API_KEY,
                api_version=settings.AZURE_OPENAI_API_VERSION,
                azure_endpoint=settings.AZURE_OPENAI_ENDPOINT
            )
        except Exception as e:
            logger.warning(f"Failed to initialize AzureOpenAI client: {e}")
            self._client = None

    def generate_text(self, prompt: str, system_instruction: Optional[str] = None, temperature: float = 0.2) -> str:
        if not self._client:
            raise ValueError("Azure OpenAI client is not configured.")
        messages = []
        if system_instruction:
            messages.append({"role": "system", "content": system_instruction})
        messages.append({"role": "user", "content": prompt})

        response = self._client.chat.completions.create(
            model=settings.AZURE_OPENAI_CHAT_DEPLOYMENT,
            messages=messages,
            temperature=temperature
        )
        return response.choices[0].message.content.strip()

    def generate_structured(self, prompt: str, system_instruction: Optional[str] = None) -> Dict[str, Any]:
        raw_text = self.generate_text(prompt, system_instruction, temperature=0.0)
        clean_text = raw_text.strip()
        if clean_text.startswith("```json"):
            clean_text = clean_text[7:]
        elif clean_text.startswith("```"):
            clean_text = clean_text[3:]
        if clean_text.endswith("```"):
            clean_text = clean_text[:-3]
        return json.loads(clean_text.strip())

    def generate_embedding(self, text: str) -> List[float]:
        if not self._client:
            raise ValueError("Azure OpenAI client is not configured.")
        response = self._client.embeddings.create(
            input=text,
            model=settings.AZURE_OPENAI_EMBEDDING_DEPLOYMENT
        )
        return response.data[0].embedding


class MockLLMProvider(LLMProvider):
    """Mock Provider for local testing and offline execution."""

    def generate_text(self, prompt: str, system_instruction: Optional[str] = None, temperature: float = 0.2) -> str:
        return "Mock response generated by MockLLMProvider."

    def generate_structured(self, prompt: str, system_instruction: Optional[str] = None) -> Dict[str, Any]:
        return {
            "role_title": "Senior .NET Backend Engineer",
            "mandatory_skills": ["C#", ".NET Core", "SQL Server"],
            "optional_skills": ["Azure", "Docker"],
            "min_experience_years": 3
        }

    def generate_embedding(self, text: str) -> List[float]:
        # Return synthetic 1536-dimensional mock embedding
        return [0.01 * (i % 10) for i in range(1536)]


def get_llm_provider() -> LLMProvider:
    """Factory function returning the active LLM provider based on settings."""
    provider_type = settings.LLM_PROVIDER.lower()
    if provider_type == "azure_openai":
        return AzureOpenAIProvider()
    elif provider_type == "gemini":
        return GeminiFlashProvider()
    elif provider_type == "mock":
        return MockLLMProvider()
    else:
        logger.warning(f"Unknown provider '{provider_type}', falling back to MockLLMProvider.")
        return MockLLMProvider()
