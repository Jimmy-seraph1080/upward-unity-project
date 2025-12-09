using TMPro;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public class Leaderboard : MonoBehaviour
{
    [Header("UI")]
    //reference to the TextMeshProUGUI where the leaderboard text is displayed
    public TextMeshProUGUI timeDisplayText;

    [Header("Display")]
    //maximum number of leaderboard entries to show
    [SerializeField] private int maxEntriesToShow = 10;
    //fixed width for the player name column
    [SerializeField] private int nameColumnWidth = 12;

    //when this object is enabled, refresh the leaderboard display
    private void OnEnable() => refreshDisplay();
    //on start, also refresh the leaderboard display
    private void Start() => refreshDisplay();

    //refresh the leaderboard display from online or local data
    public void refreshDisplay()
    {
        //if no time text is displayed then do nothing
        if (timeDisplayText == null) 
            return;

        //if FirebaseManager instance exist then attempt to load online data
        if (FirebaseManager.Instance != null)
        {
            //show loading message while waiting for Firebase response
            timeDisplayText.text = "Loading...";
            //request leaderboard entries from Firebase with a maximum number of entries
            FirebaseManager.Instance.getLeaderboardEntries(onOnlineLoaded, maxEntriesToShow);
        }
        else
        {
            //if Firebase is not available, use only local data
            showLocalOnly();
        }
    }

    //callback when online leaderboard data has been loaded
    private void onOnlineLoaded(List<LeaderboardEntry> online)
    {
        //if we received valid online data with at least one entry and no nulls
        if (online != null && online.Count > 0)
        {
            //render the leaderboard using online entries
            renderEntries(online, "Leaderboard");
        }
        else
        {
            //if no online data, fallback to local PlayerPrefs data by calling showLocalOnly
            showLocalOnly();
        }
    }

    //show only local leaderboard data from PlayerPrefs
    private void showLocalOnly()
    {
        //get local leaderboard entries from PlayerPrefs
        List<LeaderboardEntry> local = getLocalEntries();

        //if there are no local entries, show a message and stop
        if (local.Count == 0)
        {
            //display message that no times have been recorded
            timeDisplayText.text = "No times recorded yet.";
            return;
        }

        //render the leaderboard using local entries
        renderEntries(local, "Leaderboard");
    }

    //convert PlayerPrefs data into entries (time in seconds)
    private List<LeaderboardEntry> getLocalEntries()
    {
        //create a new list to hold leaderboard entries
        var result = new List<LeaderboardEntry>();

        //read how many completion records have been saved
        int count = PlayerPrefs.GetInt("LevelCompletionCount", 0);
        //iterate through all saved completion records
        for (int i = 0; i < count; i++)
        {
            //read completion time in seconds for this index
            float timeSec = PlayerPrefs.GetFloat("Level Completion Times" + i, -1f);
            //read player name for this index
            string name = PlayerPrefs.GetString("Level Completion Names" + i, "");

            //skip entries with invalid time
            if (timeSec < 0f) 
                continue;
            //if name is empty, use default name
            if (string.IsNullOrEmpty(name))
                name = "Player";

            //add a new leaderboard entry to the result list
            result.Add(new LeaderboardEntry
            {
                name = name,
                time = timeSec
            });
        }

        //read LevelTimes which stores run times in milliseconds as a string
        string raw = PlayerPrefs.GetString("Level Times", string.Empty);
        //if legacy data exists, parse and convert it
        if (!string.IsNullOrEmpty(raw))
        {
            //split the raw string by '|' to get individual tokens
            foreach (var token in raw.Split('|'))
            {
                //try parsing each token as an integer number of milliseconds
                if (int.TryParse(token, out int ms))
                {
                    //convert milliseconds to seconds and add as a legacy entry
                    result.Add(new LeaderboardEntry
                    {
                        name = "Player",
                        time = ms / 1000f
                    });
                }
            }
        }

        //return the combined list of current and legacy entries
        return result;
    }

    //render the given leaderboard entries as formatted text
    private void renderEntries(List<LeaderboardEntry> entries, string title)
    {
        //filter out null entries, order by time ascending, and take only the top entries
        var ordered = entries
            .Where(e => e != null)
            .OrderBy(e => e.time)
            .Take(maxEntriesToShow)
            .ToList();

        //if there are no entries after filtering, show a message
        if (ordered.Count == 0)
        {
            //display message that no times have been recorded
            timeDisplayText.text = "No times recorded yet.";
            return;
        }

        //use a build in function StringBuilder to assemble the leaderboard text
        var sb = new StringBuilder();
        //append the leaderboard title on the first line
        sb.AppendLine(title);

        //iterate through each ordered entry and format a leaderboard line
        for (int i = 0; i < ordered.Count; i++)
        {
            //format the rank number as a string " 1."... "10."
            string rank = formatRank(i + 1);
            //sanitize the player name and pad or trim it to the fixed column width
            string name = padOrTrimName(sanitizeName(ordered[i].name));

            //convert time in seconds to milliseconds (rounded)
            int ms = Mathf.RoundToInt(ordered[i].time * 1000f);
            //format milliseconds as mm:ss:msmsms
            string timeText = formatMillis(ms);

            //append rank, name, and time text to the StringBuilder
            sb.Append(rank)
              .Append("  ")
              .Append(name)
              .Append("  ")
              .Append(timeText)
              .Append('\n');
        }

        //set the TextMeshProUGUI text to string
        timeDisplayText.text = sb.ToString();
    }


    //clean up player name
    private string sanitizeName(string name)
    {
        //if name is null, empty, or whitespace, return the default name Player
        if (string.IsNullOrWhiteSpace(name))
            return "Player";

        //replace newline characters with spaces and trim surrounding spaces
        return name.Replace('\n', ' ')
                   .Replace('\r', ' ')
                   .Trim();
    }

    //ensure the player name fits within the configured column width
    private string padOrTrimName(string name)
    {
        //if the name is longer than the column width, trim it
        if (name.Length > nameColumnWidth)
            name = name.Substring(0, nameColumnWidth);
        //pad the name with spaces on the right to match the column width
        return name.PadRight(nameColumnWidth);
    }

    //format the rank number so single digits have a leading space
    private static string formatRank(int rank)
        => rank < 10 ? $" {rank}." : $"{rank}.";

    //convert total milliseconds into a formatted time string mm:ss:msmsms
    private static string formatMillis(int totalMs)
    {
        //calculate minutes from total milliseconds
        int minutes = totalMs / 60000;
        //calculate remaining seconds after removing minutes
        int seconds = (totalMs % 60000) / 1000;
        //calculate remaining milliseconds after removing seconds
        int millis = totalMs % 1000;
        //return formatted time string with leading zeros
        return $"{minutes:00}:{seconds:00}:{millis:000}";
    }
}
