using System;
using System.IO;
using OpenAI_API;

public static class OpenAIConfig
{
    public static OpenAIAPI CreateAPI()
    {
        string apiKeyFilePath = Environment.GetEnvironmentVariable("OPENAI_API_KEY_FILE");
        if (string.IsNullOrWhiteSpace(apiKeyFilePath))
            throw new InvalidOperationException("Set OPENAI_API_KEY_FILE to a local key file outside this repository.");

        if (!File.Exists(apiKeyFilePath))
            throw new FileNotFoundException("OpenAI key file is missing at " + apiKeyFilePath);

        string apiKey = File.ReadAllText(apiKeyFilePath).Trim();
        if (string.IsNullOrEmpty(apiKey))
            throw new InvalidOperationException("OpenAI key file is empty at " + apiKeyFilePath);

        return new OpenAIAPI(apiKey);
    }
}
