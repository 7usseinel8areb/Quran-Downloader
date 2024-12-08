// Step 1: Create a folder to store downloaded audio files
string folderPath = Path.Combine(Directory.GetParent(AppContext.BaseDirectory).Parent.Parent.Parent.FullName, "QuranAudio");
Directory.CreateDirectory(folderPath);

// Step 2: Define the base URL for the audio files
string baseUrl = "https://server11.mp3quran.net/yasser/";

// Step 3: Initialize HttpClient
using HttpClient client = new HttpClient();

// Step 4: Loop through Surah numbers (001 to 114)
for (int i = 1; i <= 114; i++)
{
    // Format the Surah number to a three-digit format
    string surahNumber = i.ToString("D3");
    string fileName = $"{surahNumber}.mp3";
    string fileUrl = $"{baseUrl}{fileName}";
    string savePath = Path.Combine(folderPath, fileName);

    try
    {
        Console.WriteLine($"Downloading Surah {surahNumber}...");

        // Step 5: Download the file using HttpClient
        using HttpResponseMessage response = await client.GetAsync(fileUrl);
        response.EnsureSuccessStatusCode();

        // Step 6: Save the file locally
        await using FileStream fileStream = new FileStream(savePath, FileMode.Create, FileAccess.Write, FileShare.None);
        await response.Content.CopyToAsync(fileStream);

        Console.WriteLine($"Downloaded: {fileName}");
    }
    catch (Exception ex)
    {
        // Step 7: Handle errors gracefully
        Console.WriteLine($"Error downloading Surah {surahNumber}: {ex.Message}");
    }
}

Console.WriteLine("All downloads completed!");