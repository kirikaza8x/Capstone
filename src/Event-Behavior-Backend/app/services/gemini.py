import json
from google import genai
from google.genai import types
from app.config import GEMINI_API_KEY

# Initialize Gemini client with your API key
client = genai.Client(api_key=GEMINI_API_KEY)

async def generate_json(
    prompt: str,
    expected_schema: dict,
    model_name: str = "gemini-2.5-flash"
) -> dict:
    """
    Generate JSON output from Gemini model based on a prompt and expected schema.
    """

    try:
        response = await client.aio.models.generate_content(
            model=model_name,
            contents=[types.Part.from_text(text=prompt)],
            config=types.GenerateContentConfig(
                temperature=0,                # deterministic
                top_p=0.95,                   # nucleus sampling cutoff
                top_k=20,                     # consider top 20 tokens
                response_mime_type="application/json",  # force JSON output
                response_json_schema=expected_schema,   # enforce schema validation
                max_output_tokens=1024        # cap response length
            ),
        )
        return response.parsed
    except Exception as e:
        raise RuntimeError(f"Gemini error: {str(e)}")


async def generate_text(
    prompt: str,
    model_name: str = "gemini-2.5-flash"
) -> str:
    """
    Generate plain text output from Gemini model based on a prompt.
    """

    try:
        response = await client.aio.models.generate_content(
            model=model_name,
            contents=[types.Part.from_text(text=prompt)],  # keyword argument
            config=types.GenerateContentConfig(
                temperature=0.7,                     # more creative
                top_p=0.95,                          # probability cutoff
                top_k=40,                            # consider top 40 tokens
                response_mime_type="text/plain",     # plain text output
                max_output_tokens=512                # cap response length
            ),
        )
        return response.text.strip()
    except Exception as e:
        raise RuntimeError(f"Gemini error: {str(e)}")
