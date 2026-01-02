namespace Chatbot.Api.Models
{
    public class ChatbotConfig
    {
        public string PromptTemplate { get; set; } = string.Empty;
        public Dictionary<string, SiteConfig> Sites { get; set; } = new();
    }

    public class SiteConfig
    {
        public string Context { get; set; } = string.Empty;
    }
}
