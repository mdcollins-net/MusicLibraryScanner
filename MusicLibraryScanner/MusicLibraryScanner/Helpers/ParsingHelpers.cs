// Helpers/ParsingHelpers.cs
using System;
using System.IO;
using System.Text.RegularExpressions;

namespace MusicLibraryScanner.Helpers
{
    public static partial class ParsingHelpers
    {
        /// <summary>
        /// Extracts the artist name from a given directory path.
        /// Example: "C:\Music\Artists\The Beatles" => "The Beatles"
        /// </summary>
        public static string ParseArtistName(string artistPath)
        {
            return Path.GetFileName(artistPath.TrimEnd(Path.DirectorySeparatorChar));
        }

        /// <summary>
        /// Extracts the album year and title from a folder name.
        /// Example: "1967 - Sgt. Pepper's Lonely Hearts Club Band" => (1967, "Sgt. Pepper's Lonely Hearts Club Band")
        /// </summary>
        public static (int year, string title) ParseAlbumFolder(string albumFolder)
        {
            var folderName = Path.GetFileName(albumFolder.TrimEnd(Path.DirectorySeparatorChar));
            var match = Regex.Match(folderName, @"^(?<year>\d{4})\s*-\s*(?<title>.+)$");
            if (!match.Success)
                throw new FormatException($"Album folder name '{folderName}' is not in the expected format '<YYYY> - <Album Title>'");

            int year = int.Parse(match.Groups["year"].Value);
            string title = match.Groups["title"].Value.Trim();
            return (year, title);
        }

        /// <summary>
        /// Extracts the track number and title from a file name.
        /// Example: "01 - Sgt. Peppers Lonely Hearts Club Band.flac" => (1, "Sgt. Peppers Lonely Hearts Club Band")
        /// </summary>
        public static (int trackNumber, string title) ParseTrackFile(string trackFile)
        {
            var fileName = Path.GetFileNameWithoutExtension(trackFile);
            var match = TrackRegex().Match(fileName);
            if (!match.Success)
                throw new FormatException($"Track file name '{fileName}' is not in the expected format '<XX> - <Track Title>'");

            var trackNumber = int.Parse(match.Groups["track"].Value);
            var title = match.Groups["title"].Value.Trim();
            return (trackNumber, title);
        }

        [GeneratedRegex(@"^(?<track>\d{2})\s*-\s*(?<title>.+)$")]
        private static partial Regex TrackRegex();
    }
}
