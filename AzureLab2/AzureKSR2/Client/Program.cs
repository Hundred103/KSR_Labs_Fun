using System.Text;
using System.Text.Json;

const string baseUrl = "http://localhost:5008";

var files = new[]
{
    ("plik1.txt", "Ala ma kota i psa."),
    ("plik2.txt", "The quick brown fox jumps over the lazy dog."),
    ("plik3.txt", "Hello World from Azure!"),
    ("plik4.txt", "ROT13 zamienia litery w tekscie."),
    ("plik5.txt", "Testowy plik numer piec.")
};

using var http = new HttpClient { BaseAddress = new Uri(baseUrl) };

Console.WriteLine("=== Wysylanie 5 plikow do /api/processing/encode ===\n");

foreach (var (name, content) in files)
{
    var body = JsonSerializer.Serialize(new { FileName = name, Content = content });
    var response = await http.PostAsync(
        "/api/processing/encode",
        new StringContent(body, Encoding.UTF8, "application/json"));

    Console.WriteLine($"[ENCODE] {name} -> {(int)response.StatusCode} {response.StatusCode}");
}

Console.WriteLine("\nCzekam 4 sekundy na przetworzenie...\n");
await Task.Delay(TimeSpan.FromSeconds(4));

Console.WriteLine("=== Pobieranie zakodowanych plikow ===\n");

foreach (var (name, original) in files)
{
    var response = await http.GetAsync($"/api/processing/download/{name}");

    if (response.IsSuccessStatusCode)
    {
        var encoded = await response.Content.ReadAsStringAsync();
        Console.WriteLine($"[OK]  {name}");
        Console.WriteLine($"      Oryginał: {original}");
        Console.WriteLine($"      ROT13:    {encoded}");
    }
    else
    {
        Console.WriteLine($"[BRAK] {name} -> {(int)response.StatusCode} (jeszcze nie przetworzone)");
    }

    Console.WriteLine();
}

Console.WriteLine("=== Koniec ===");
Console.ReadKey();