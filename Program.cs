// Usage
await QuranAudioDownloader.DownloadAudioFilesAsync();

public class QuranAudioDownloader
{
    private const string BaseUrl = "https://server13.mp3quran.net/husr/";
    private static readonly string FolderPath = Path.Combine(Directory.GetParent(AppContext.BaseDirectory).Parent.Parent.Parent.FullName, "QuranAudio");
    private static readonly HttpClient Client = new HttpClient { Timeout = TimeSpan.FromMinutes(8) };

    public static async Task DownloadAudioFilesAsync()
    {
        // Step 1: Create a folder to store downloaded audio files
        Directory.CreateDirectory(FolderPath);

        // Step 2: Initialize the list of Surah numbers that failed to download
        List<int> notDownloaded = new();

        // Step 3: Loop through Surah numbers (001 to 114) and download
        for (int i = 1; i <= 114; i++)
        {
            bool success = await TryDownloadSurah(i);
            if (!success)
            {
                notDownloaded.Add(i);
            }
        }

        // Step 4: Retry mechanism for failed downloads
        if (notDownloaded.Count > 0)
        {
            await RetryFailedDownloads(notDownloaded);
        }

        // Final report
        PrintFinalReport(notDownloaded);
    }

    private static async Task<bool> TryDownloadSurah(int surahNumber)
    {
        string fileName = $"{surahNumber:D3}.mp3";
        string fileUrl = $"{BaseUrl}{fileName}";
        string savePath = Path.Combine(FolderPath, fileName);

        try
        {
            Console.WriteLine($"Downloading Surah {surahNumber:D3}...");
            bool success = await DownloadFileAsync(fileUrl, savePath);
            if (success)
            {
                Console.WriteLine($"Downloaded: {fileName}");
            }
            else
            {
                Console.WriteLine($"Failed to download Surah {surahNumber:D3}");
            }
            return success;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error downloading Surah {surahNumber:D3}: {ex.Message}");
            return false;
        }
    }

    private static async Task RetryFailedDownloads(List<int> notDownloaded)
    {
        Console.WriteLine("\nRetrying failed downloads...");
        int maxRetries = 3;
        List<int> retryList = new List<int>(notDownloaded);

        foreach (var surahNumber in retryList)
        {
            bool success = await RetryDownloadSurah(surahNumber, maxRetries);
            if (success)
            {
                notDownloaded.Remove(surahNumber); // Remove from failed list if successful
            }
        }
    }

    private static async Task<bool> RetryDownloadSurah(int surahNumber, int maxRetries)
    {
        string fileName = $"{surahNumber:D3}.mp3";
        string fileUrl = $"{BaseUrl}{fileName}";
        string savePath = Path.Combine(FolderPath, fileName);

        int retries = 0;
        while (retries < maxRetries)
        {
            try
            {
                Console.WriteLine($"Retrying Surah {surahNumber:D3}, Attempt {retries + 1}/{maxRetries}...");
                bool success = await DownloadFileAsync(fileUrl, savePath);
                if (success)
                {
                    Console.WriteLine($"Successfully downloaded Surah {surahNumber:D3} on attempt {retries + 1}.");
                    return true;
                }
            }
            catch (Exception ex)
            {
                retries++;
                Console.WriteLine($"Error downloading Surah {surahNumber:D3}: {ex.Message}");
                if (retries == maxRetries)
                {
                    Console.WriteLine($"Failed to download Surah {surahNumber:D3} after {maxRetries} attempts.");
                }
            }
        }
        return false;
    }

    private static async Task<bool> DownloadFileAsync(string fileUrl, string savePath)
    {
        try
        {
            using HttpResponseMessage response = await Client.GetAsync(fileUrl);
            response.EnsureSuccessStatusCode();

            using FileStream fileStream = new FileStream(savePath, FileMode.Create, FileAccess.Write, FileShare.None);
            await response.Content.CopyToAsync(fileStream);
            return true;
        }
        catch
        {
            return false;
        }
    }

    private static void PrintFinalReport(List<int> notDownloaded)
    {
        if (notDownloaded.Count == 0)
        {
            Console.WriteLine("All downloads completed!");
        }
        else
        {
            Console.WriteLine($"Failed to download Surahs: {string.Join(", ", notDownloaded)}");
        }
    }
}
