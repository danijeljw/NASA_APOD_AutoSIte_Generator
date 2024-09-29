using System;
using System.IO;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace NASA_APOD_AutoSite_Generator;
class Program
{
    static async Task Main(string[] args)
    {
        string apiKey = "DEMO_KEY";  // Use your NASA API key here if needed.
        string apiUrl = $"https://api.nasa.gov/planetary/apod?api_key={apiKey}";
        string outputFolder = "output";

        try
        {
            // Ensure output folder exists
            if (!Directory.Exists(outputFolder))
            {
                Directory.CreateDirectory(outputFolder);
            }

            using (HttpClient client = new HttpClient())
            {
                // Step 1: Make request to NASA API
                HttpResponseMessage response = await client.GetAsync(apiUrl);
                response.EnsureSuccessStatusCode();
                
                string jsonResponse = await response.Content.ReadAsStringAsync();
                
                // Step 2: Parse the JSON response
                var apodData = JsonSerializer.Deserialize<ApodResponse>(jsonResponse);
                Console.WriteLine("Fetched APOD data successfully.");

                // Step 3: Download the image
                string imageUrl = apodData?.hdurl ?? apodData?.url ?? string.Empty;
                if (string.IsNullOrEmpty(imageUrl))
                {
                    return;
                }
                string imageFileName = Path.Combine(outputFolder, "nasa_apod_image.jpg");

                byte[] imageBytes = await client.GetByteArrayAsync(imageUrl);
                await File.WriteAllBytesAsync(imageFileName, imageBytes);
                Console.WriteLine($"Image downloaded to {imageFileName}");

                // Step 4: Generate HTML file
                string htmlContent = GenerateHtml(apodData!, "nasa_apod_image.jpg");
                string htmlFilePath = Path.Combine(outputFolder, "index.html");
                await File.WriteAllTextAsync(htmlFilePath, htmlContent);
                Console.WriteLine($"HTML file generated at {htmlFilePath}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
    }

    // HTML generator
    static string GenerateHtml(ApodResponse apodData, string localImagePath)
    {
        return $@"
<!DOCTYPE html>
<html lang='en'>
<head>
    <meta charset='UTF-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
    <title>{apodData.title}</title>
    <link href='https://cdn.jsdelivr.net/npm/bootstrap@5.3.0-alpha1/dist/css/bootstrap.min.css' rel='stylesheet'>
</head>
<body>
    <div class='container mt-5'>
        <h1 class='text-center'>{apodData.title}</h1>
        <img src='{localImagePath}' class='img-fluid mx-auto d-block' alt='{apodData.title}'>
        <p class='text-center'><strong>Date:</strong> {apodData.date}</p>
        <p>{apodData.explanation}</p>
        <footer class='text-center mt-5'>
            <p>NASA APOD Service Version: {apodData.service_version}</p>
        </footer>
    </div>
</body>
</html>
";
    }
}

// Class to deserialize JSON response from NASA API
public class ApodResponse
{
    public string? date { get; set; }
    public string? explanation { get; set; }
    public string? hdurl { get; set; }
    public string? title { get; set; }
    public string? service_version { get; set; }
    public string? url { get; set; }
}
