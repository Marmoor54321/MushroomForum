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
        private readonly string _ApiIdKey;
        private const string ApiUrl = "https://mushroom.kindwise.com/api/v1/identification";


        public MushroomIdService(IHttpClientFactory httpClientFactory, ILogger<MushroomIdService> logger, IConfiguration configuration)
        {
            _httpClientFactory = httpClientFactory;
            _logger = logger;
            _ApiIdKey = configuration["ApiSettings:ApiIdKey"] ?? throw new InvalidOperationException("API key not found in configuration");
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

                // Tworzenie URL z parametrami szczegółów
                var detailsParams = "?details=common_names,url,description,edibility,psychoactive&language=pl";
                var fullUrl = ApiUrl + detailsParams;

                // Tworzenie JSON
                var requestData = new
                {
                    images = new[] { base64Image },
                    similar_images = true // Opcjonalne, zgodnie z dokumentacją
                };
                var jsonContent = JsonSerializer.Serialize(requestData);
                var content = new StringContent(jsonContent, System.Text.Encoding.UTF8, "application/json");

                // Dodanie nagłówka Api-Key
                client.DefaultRequestHeaders.Add("Api-Key", _ApiIdKey);

                var response = await client.PostAsync(fullUrl, content);
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
                    var mushroomSuggestion = new MushroomSuggestion
                    {
                        Id = suggestion.TryGetProperty("id", out var id) ? id.GetString() : null,
                        Name = suggestion.GetProperty("name").GetString(),
                        Probability = suggestion.GetProperty("probability").GetDouble()
                    };

                    // Parsowanie szczegółów (details)
                    if (suggestion.TryGetProperty("details", out var details))
                    {
                        _logger.LogInformation("Details section found: {Details}", details.ToString());

                        // Common names
                        if (details.TryGetProperty("common_names", out var commonNames))
                        {
                            if (commonNames.ValueKind == JsonValueKind.Array)
                            {
                                foreach (var name in commonNames.EnumerateArray())
                                {
                                    if (name.ValueKind == JsonValueKind.String)
                                    {
                                        mushroomSuggestion.CommonNames.Add(name.GetString());
                                    }
                                }
                            }
                            else if (commonNames.ValueKind == JsonValueKind.Null)
                            {
                                _logger.LogInformation("common_names is null");
                            }
                            else
                            {
                                _logger.LogWarning("common_names has unexpected type: {Type}", commonNames.ValueKind);
                            }
                        }

                        // URL
                        SafeGetStringProperty(details, "url", value => mushroomSuggestion.Url = value, _logger);

                        // Description (po polsku)
                        SafeGetStringProperty(details, "description", value => mushroomSuggestion.Description = value, _logger);

                        // Edibility
                        SafeGetStringProperty(details, "edibility", value => mushroomSuggestion.Edibility = value, _logger);

                        // Psychoactive
                        if (details.TryGetProperty("psychoactive", out var psychoactive))
                        {
                            if (psychoactive.ValueKind == JsonValueKind.True || psychoactive.ValueKind == JsonValueKind.False)
                            {
                                mushroomSuggestion.Psychoactive = psychoactive.GetBoolean();
                            }
                            else if (psychoactive.ValueKind == JsonValueKind.Null)
                            {
                                _logger.LogInformation("psychoactive is null");
                            }
                            else
                            {
                                _logger.LogWarning("psychoactive has unexpected type: {Type}", psychoactive.ValueKind);
                            }
                        }
                    }
                    else
                    {
                        _logger.LogInformation("No details section found in suggestion");
                    }

                    // Parsowanie similar_images
                    if (suggestion.TryGetProperty("similar_images", out var similarImages) &&
                        similarImages.ValueKind == JsonValueKind.Array)
                    {
                        foreach (var imageElement in similarImages.EnumerateArray())
                        {
                            var similarImage = new SimilarImage();

                            SafeGetStringProperty(imageElement, "id", value => similarImage.Id = value, _logger);
                            SafeGetStringProperty(imageElement, "url", value => similarImage.Url = value, _logger);
                            SafeGetStringProperty(imageElement, "url_small", value => similarImage.UrlSmall = value, _logger);
                            SafeGetStringProperty(imageElement, "license_name", value => similarImage.LicenseName = value, _logger);
                            SafeGetStringProperty(imageElement, "license_url", value => similarImage.LicenseUrl = value, _logger);
                            SafeGetStringProperty(imageElement, "citation", value => similarImage.Citation = value, _logger);

                            if (imageElement.TryGetProperty("similarity", out var similarity) &&
                                similarity.ValueKind == JsonValueKind.Number)
                            {
                                similarImage.Similarity = similarity.GetDouble();
                            }

                            mushroomSuggestion.SimilarImages.Add(similarImage);
                        }
                    }

                    result.Suggestions.Add(mushroomSuggestion);
                }

                // Opcjonalnie: Parsowanie is_mushroom
                if (doc.RootElement.TryGetProperty("result", out var resultElem) &&
                    resultElem.TryGetProperty("is_mushroom", out var isMushroom))
                {
                    if (isMushroom.TryGetProperty("binary", out var binary) &&
                        (binary.ValueKind == JsonValueKind.True || binary.ValueKind == JsonValueKind.False))
                    {
                        result.IsMushroom = binary.GetBoolean();
                    }

                    if (isMushroom.TryGetProperty("probability", out var probability) &&
                        probability.ValueKind == JsonValueKind.Number)
                    {
                        result.MushroomProbability = probability.GetDouble();
                    }
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

        private static void SafeGetStringProperty(JsonElement element, string propertyName, Action<string> setValue, ILogger logger)
        {
            if (element.TryGetProperty(propertyName, out var property))
            {
                if (property.ValueKind == JsonValueKind.String)
                {
                    setValue(property.GetString());
                }
                else if (property.ValueKind == JsonValueKind.Null)
                {
                    logger.LogInformation("{PropertyName} is null", propertyName);
                }
                else
                {
                    logger.LogWarning("{PropertyName} has unexpected type: {Type}", propertyName, property.ValueKind);
                }
            }
        }
    }
}