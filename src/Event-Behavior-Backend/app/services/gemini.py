import google.generativeai as genai
from app.config import GEMINI_API_KEY
import json

genai.configure(api_key=GEMINI_API_KEY)
model = genai.GenerativeModel("gemini-1.5-flash")

async def generate_json(prompt: str, expected_schema: str) -> dict:
    full_prompt = f"""
{prompt}

Return ONLY valid JSON matching this exact structure:
{expected_schema}

No markdown, no explanation, no extra text.
"""
    try:
        response = model.generate_content(full_prompt)
        text = response.text.strip()
        if text.startswith("```json"):
            text = text.split("```json")[1].split("```")[0].strip()
        return json.loads(text)
    except Exception as e:
        raise RuntimeError(f"Gemini error: {str(e)}")