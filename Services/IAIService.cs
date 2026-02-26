using ChatAPI.DTO;

namespace ChatAPI.Services
{
    public interface IAIService
    {
        Task<string> GetResponseAsync(List<AIMessageDto> messages);
       public string CleanAiResponse(string message);
    }
}
