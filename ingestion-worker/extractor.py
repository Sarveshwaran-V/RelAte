"""
Resume Text Extraction & Metadata Pipeline
Owner: Sivaram (Track 2A)
Extracts raw text, full name, email, phone, experience, and LinkedIn profile from PDF/DOCX resumes.
"""
import os
import re
import hashlib
from typing import Dict, Any, Optional
from pathlib import Path


def compute_file_hash(file_path: str) -> str:
    """Computes SHA-256 hash to detect duplicate resume files."""
    hasher = hashlib.sha256()
    with open(file_path, "rb") as f:
        while chunk := f.read(8192):
            hasher.update(chunk)
    return hasher.hexdigest()


def extract_raw_text(file_path: str) -> str:
    """Extracts raw text from PDF or DOCX file."""
    path = Path(file_path)
    extension = path.suffix.lower()

    if extension == ".pdf":
        try:
            import pypdf
            reader = pypdf.PdfReader(file_path)
            pages = [page.extract_text() or "" for page in reader.pages]
            return "\n".join(pages).strip()
        except ImportError:
            raise ImportError("Please install pypdf: pip install pypdf")
    elif extension in [".docx", ".doc"]:
        try:
            import docx
            doc = docx.Document(file_path)
            return "\n".join([p.text for p in doc.paragraphs]).strip()
        except ImportError:
            raise ImportError("Please install python-docx: pip install python-docx")
    elif extension == ".txt":
        with open(file_path, "r", encoding="utf-8", errors="ignore") as f:
            return f.read().strip()
    else:
        raise ValueError(f"Unsupported file format: {extension}")


def extract_linkedin_url(text: str) -> Optional[str]:
    """Extracts LinkedIn profile URL from resume text."""
    pattern = r"https?://(?:www\.)?linkedin\.com/in/[\w\-]+"
    match = re.search(pattern, text, re.IGNORECASE)
    return match.group(0) if match else None


def extract_email(text: str) -> Optional[str]:
    """Extracts primary contact email from resume text."""
    pattern = r"\b[A-Za-z0-9._%+-]+@[A-Za-z0-9.-]+\.[A-Z|a-z]{2,}\b"
    match = re.search(pattern, text)
    return match.group(0) if match else None


def extract_phone(text: str) -> Optional[str]:
    """Extracts phone number from resume text."""
    pattern = r"(?:\+?\d{1,3}[-.\s]?)?\(?\d{3}\)?[-.\s]?\d{3}[-.\s]?\d{4}"
    match = re.search(pattern, text)
    return match.group(0) if match else None


def process_resume_file(file_path: str) -> Dict[str, Any]:
    """
    Main extraction worker function:
    Returns clean intermediate JSON record for Pradeep's normalization step.
    """
    raw_text = extract_raw_text(file_path)
    file_hash = compute_file_hash(file_path)
    
    return {
        "file_path": file_path,
        "file_hash": file_hash,
        "raw_text": raw_text,
        "email": extract_email(raw_text),
        "phone": extract_phone(raw_text),
        "linkedin_url": extract_linkedin_url(raw_text),
        "status": "extracted"
    }


if __name__ == "__main__":
    print("Sivaram's Resume Extractor Module initialized.")
