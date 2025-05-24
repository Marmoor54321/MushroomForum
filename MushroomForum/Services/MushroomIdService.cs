using System.Net.Http.Headers;
using System.Text.Json;
using MushroomForum.Models;
using Microsoft.Extensions.Logging;
using MushroomForum.ViewModels;

namespace MushroomForum.Services
{
    public class MushroomIdService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<MushroomIdService> _logger;
        private const string ApiKey = "KLUCZ API"; //Później zaimplementuje korzystanie z API nie ujawniając klucza
        private const string ApiUrl = "https://mushroom.kindwise.com/api/v1/identification";

        public MushroomIdService(IHttpClientFactory httpClientFactory, ILogger<MushroomIdService> logger)
        {
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }

        public async Task<MushroomIdResultViewModel> IdentifyAsync(IFormFile image)
        {
            var result = new MushroomIdResultViewModel();
            try
            {
                // Walidacja formatu obrazu
                if (!new[] { "image/jpeg", "image/png" }.Contains(image.ContentType))
                {
                    result.Success = false;
                    result.ErrorMessage = "Only JPEG or PNG images are supported";
                    _logger.LogError("Invalid image format: {ContentType}", image.ContentType);
                    return result;
                }

                var client = _httpClientFactory.CreateClient();

                // Konwersja obrazu na Base64
                string base64Image;
                using (var memoryStream = new MemoryStream())
                {
                    await image.CopyToAsync(memoryStream);
                    byte[] imageBytes = memoryStream.ToArray();
                    base64Image = $"data:{image.ContentType};base64,{Convert.ToBase64String(imageBytes)}";
                }

                // Tworzenie JSON
                var requestData = new
                {
                    images = new[] { base64Image },
                    similar_images = true // Opcjonalne, zgodnie z dokumentacją
                };
                var jsonContent = JsonSerializer.Serialize(requestData);
                var content = new StringContent(jsonContent, System.Text.Encoding.UTF8, "application/json");

                // Dodanie nagłówka Api-Key
                client.DefaultRequestHeaders.Add("Api-Key", ApiKey);

                var response = await client.PostAsync(ApiUrl, content);
                if (!response.IsSuccessStatusCode)
                {
                    result.Success = false;
                    result.ErrorMessage = $"API error: {response.StatusCode}";
                    _logger.LogError("API error: {StatusCode}", response.StatusCode);
                    return result;
                }

                var json = await response.Content.ReadAsStringAsync();
                _logger.LogInformation("API response: {JsonResponse}", json);

                using var doc = JsonDocument.Parse(json);

                // Sprawdzenie statusu
                if (!doc.RootElement.TryGetProperty("status", out var status) || status.GetString() != "COMPLETED")
                {
                    result.Success = false;
                    result.ErrorMessage = "API request did not complete successfully";
                    _logger.LogError("Invalid status in response: {Status}. Full response: {JsonResponse}",
                        status.GetString() ?? "null", json);
                    return result;
                }

                // Sprawdzenie klucza 'result.classification.suggestions'
                if (!doc.RootElement.TryGetProperty("result", out var resultElement) ||
                    !resultElement.TryGetProperty("classification", out var classification) ||
                    !classification.TryGetProperty("suggestions", out var suggestions))
                {
                    result.Success = false;
                    result.ErrorMessage = "Response does not contain 'result.classification.suggestions'";
                    _logger.LogError("Missing 'result.classification.suggestions' in response. Full response: {JsonResponse}", json);
                    return result;
                }

                foreach (var suggestion in suggestions.EnumerateArray())
                {
                    result.Suggestions.Add(new MushroomSuggestion
                    {
                        Id = suggestion.TryGetProperty("id", out var id) ? id.GetString() : null,
                        Name = suggestion.GetProperty("name").GetString(),
                        Probability = suggestion.GetProperty("probability").GetDouble(),
                        Description = suggestion.TryGetProperty("details", out var details) &&
                                      details.TryGetProperty("language", out var lang) &&
                                      lang.GetString() == "en" ? "English description available" : null
                        // Możesz dodać obsługę similar_images, jeśli potrzebna
                    });
                }

                // Opcjonalnie: Parsowanie is_mushroom
                if (doc.RootElement.TryGetProperty("result", out var resultElem) &&
                    resultElem.TryGetProperty("is_mushroom", out var isMushroom))
                {
                    result.IsMushroom = isMushroom.GetProperty("binary").GetBoolean();
                    result.MushroomProbability = isMushroom.GetProperty("probability").GetDouble();
                }

                result.Success = true;
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.ErrorMessage = ex.Message;
                _logger.LogError(ex, "Error during mushroom identification: {Message}. InnerException: {InnerMessage}",
                    ex.Message, ex.InnerException?.Message ?? "No InnerException");
            }
            return result;
        }
    }
}