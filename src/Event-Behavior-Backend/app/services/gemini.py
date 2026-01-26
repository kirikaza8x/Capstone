import google.generativeai as genai
from app.config import GEMINI_API_KEY
import json

genai.configure(api_key=GEMINI_API_KEY)
model = genai.GenerativeModel("gemini-2.5-flash")

async def generate_json(prompt: str, expected_schema: str) -> dict:
    """
    Generate JSON output from Gemini model based on a prompt and expected schema.
    Ensures the response is strictly valid JSON without markdown or extra text.
    """
    full_prompt = f"""
{prompt}

Return ONLY valid JSON matching this exact structure:
{expected_schema}

No markdown, no explanation, no extra text.
"""
    try:
        response = model.generate_content(full_prompt)
        text = response.text.strip()

        # Clean up possible markdown fences
        if text.startswith("```"):
            text = text.strip("`").replace("json", "").strip()

        return json.loads(text)
    except json.JSONDecodeError as e:
        raise ValueError(f"Invalid JSON returned: {text}") from e
    except Exception as e:
        raise RuntimeError(f"Gemini error: {str(e)}")


async def generate_text(prompt: str) -> str:
    """
    Generate plain text output from Gemini model based on a prompt.
    """
    try:
        response = model.generate_content(prompt)
        return response.text.strip()
    except Exception as e:
        raise RuntimeError(f"Gemini error: {str(e)}")
