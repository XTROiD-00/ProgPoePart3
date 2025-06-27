using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace ProgPoePart3
{
    public class get_reminder
    {
        private List<string> descriptions = new List<string>();
        private List<string> dates = new List<string>();

        // Validate input is not empty
        public string validate_input(string user_input)
        {
            return string.IsNullOrWhiteSpace(user_input) ? "Please add a task" : "found";
        }

        // Extract day value from string input
        public string get_days(string day)
        {
            string get_day_in = Regex.Replace(day, @"[^\d]", "");
            return get_day_in != "0" ? get_day_in : "today";
        }

        // Add today's reminder
        public string today_date(string description, string date)
        {
            if (date == "today")
            {
                string format_date = DateTime.Now.ToString("yyyy-MM-dd");
                descriptions.Add(description);
                dates.Add(format_date);
                return "Nice, will remind you today.";
            }
            return "error";
        }

        // Add future reminder based on day offset
        public string get_remindDate(string description, string date)
        {
            if (int.TryParse(date, out int addDays))
            {
                DateTime future = DateTime.Now.AddDays(addDays);
                string store_date = future.ToString("yyyy-MM-dd");
                descriptions.Add(description);
                dates.Add(store_date);
                return $"Reminder set for {store_date}";
            }
            return "Invalid date input.";
        }

        // Get today's reminders
        public string reminder()
        {
            string now_date = DateTime.Now.ToString("yyyy-MM-dd");
            string found_remind = "";

            for (int i = 0; i < dates.Count; i++)
            {
                if (dates[i] == now_date)
                    found_remind += $"\n Due Today: {descriptions[i]}";
            }

            return string.IsNullOrEmpty(found_remind)
                ? "No reminders for today."
                : found_remind;
        }
    }
}
