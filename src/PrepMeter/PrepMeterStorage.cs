using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace TransparentClock.PrepMeter
{
    /// <summary>
    /// Service class for persisting and retrieving Prep Meter data using JSON serialization.
    /// Handles all file I/O operations for the Prep Meter feature.
    /// </summary>
    public static class PrepMeterStorage
    {
        private const string AppDataFolderName = "TransparentClock";
        private const string ProfileFileName = "PrepMeterProfile.json";
        private const string CommitmentsFileName = "PrepMeterData.json";

        private static readonly JsonSerializerOptions JsonOptions = new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNameCaseInsensitive = true,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };

        /// <summary>
        /// Gets the application data folder path for storing Prep Meter data.
        /// </summary>
        private static string GetDataFolderPath()
        {
            string appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            string prepMeterPath = Path.Combine(appDataPath, AppDataFolderName);

            // Ensure the directory exists
            if (!Directory.Exists(prepMeterPath))
            {
                Directory.CreateDirectory(prepMeterPath);
            }

            return prepMeterPath;
        }

        /// <summary>
        /// Saves the user's exam preparation profile.
        /// </summary>
        /// <param name="profile">The PrepProfile object to save.</param>
        /// <exception cref="ArgumentNullException">Thrown when profile is null.</exception>
        /// <exception cref="IOException">Thrown when file operations fail.</exception>
        public static void SaveProfile(PrepProfile profile)
        {
            if (profile == null)
            {
                throw new ArgumentNullException(nameof(profile), "Profile cannot be null.");
            }

            try
            {
                string folderPath = GetDataFolderPath();
                string filePath = Path.Combine(folderPath, ProfileFileName);

                string json = JsonSerializer.Serialize(profile, JsonOptions);
                File.WriteAllText(filePath, json);
            }
            catch (Exception ex)
            {
                throw new IOException($"Failed to save Prep Meter profile to {ProfileFileName}.", ex);
            }
        }

        /// <summary>
        /// Loads the user's exam preparation profile.
        /// </summary>
        /// <returns>The loaded PrepProfile object, or a new empty profile if the file doesn't exist.</returns>
        /// <exception cref="IOException">Thrown when file operations fail.</exception>
        /// <exception cref="JsonException">Thrown when JSON deserialization fails.</exception>
        public static PrepProfile LoadProfile()
        {
            try
            {
                string folderPath = GetDataFolderPath();
                string filePath = Path.Combine(folderPath, ProfileFileName);

                if (!File.Exists(filePath))
                {
                    return new PrepProfile();
                }

                string json = File.ReadAllText(filePath);
                PrepProfile? profile = JsonSerializer.Deserialize<PrepProfile>(json, JsonOptions);

                return profile ?? new PrepProfile();
            }
            catch (JsonException ex)
            {
                throw new IOException("Failed to deserialize Prep Meter profile. The file may be corrupted.", ex);
            }
            catch (Exception ex)
            {
                throw new IOException($"Failed to load Prep Meter profile from {ProfileFileName}.", ex);
            }
        }

        /// <summary>
        /// Saves a single day's commitment log.
        /// </summary>
        /// <param name="log">The DailyCommitment object to save.</param>
        /// <exception cref="ArgumentNullException">Thrown when log is null.</exception>
        /// <exception cref="IOException">Thrown when file operations fail.</exception>
        public static void SaveDailyCommitment(DailyCommitment log)
        {
            if (log == null)
            {
                throw new ArgumentNullException(nameof(log), "Daily commitment log cannot be null.");
            }

            try
            {
                string folderPath = GetDataFolderPath();
                string filePath = Path.Combine(folderPath, CommitmentsFileName);

                // Load existing commitments
                List<DailyCommitment> commitments = LoadAllCommitments();

                // Find and replace or add the new commitment
                int existingIndex = commitments.FindIndex(c => c.Date.Date == log.Date.Date);
                if (existingIndex >= 0)
                {
                    commitments[existingIndex] = log;
                }
                else
                {
                    commitments.Add(log);
                }

                // Sort by date in descending order (newest first)
                commitments.Sort((a, b) => b.Date.CompareTo(a.Date));

                string json = JsonSerializer.Serialize(commitments, JsonOptions);
                File.WriteAllText(filePath, json);
            }
            catch (Exception ex)
            {
                throw new IOException($"Failed to save daily commitment log to {CommitmentsFileName}.", ex);
            }
        }

        /// <summary>
        /// Loads all saved daily commitment logs.
        /// </summary>
        /// <returns>A list of all DailyCommitment objects, sorted by date in descending order. Returns an empty list if the file doesn't exist.</returns>
        /// <exception cref="IOException">Thrown when file operations fail.</exception>
        /// <exception cref="JsonException">Thrown when JSON deserialization fails.</exception>
        public static List<DailyCommitment> LoadAllCommitments()
        {
            try
            {
                string folderPath = GetDataFolderPath();
                string filePath = Path.Combine(folderPath, CommitmentsFileName);

                if (!File.Exists(filePath))
                {
                    return new List<DailyCommitment>();
                }

                string json = File.ReadAllText(filePath);
                List<DailyCommitment>? commitments = JsonSerializer.Deserialize<List<DailyCommitment>>(json, JsonOptions);

                return commitments ?? new List<DailyCommitment>();
            }
            catch (JsonException ex)
            {
                throw new IOException("Failed to deserialize commitment logs. The file may be corrupted.", ex);
            }
            catch (Exception ex)
            {
                throw new IOException($"Failed to load commitment logs from {CommitmentsFileName}.", ex);
            }
        }

        /// <summary>
        /// Retrieves the most recent daily commitment log.
        /// </summary>
        /// <returns>The most recent DailyCommitment, or null if no logs exist.</returns>
        public static DailyCommitment? LoadLatestCommitment()
        {
            try
            {
                List<DailyCommitment> commitments = LoadAllCommitments();
                return commitments.Count > 0 ? commitments[0] : null;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Retrieves a commitment log for a specific date.
        /// </summary>
        /// <param name="date">The date to search for.</param>
        /// <returns>The DailyCommitment for the specified date, or null if not found.</returns>
        public static DailyCommitment? LoadCommitmentForDate(DateTime date)
        {
            try
            {
                List<DailyCommitment> commitments = LoadAllCommitments();
                return commitments.Find(c => c.Date.Date == date.Date);
            }
            catch
            {
                return null;
            }
        }
    }
}
