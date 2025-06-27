using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Media;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace ProgPoePart3
{
    public partial class MainWindow : Window
    {
        // Quiz
        private List<QuizQuestion> quizData;
        private int questionIndex = 0;
        private int currentScore = 0;
        private Button selectedChoice = null;

        // Reminder
        private get_reminder reminder = new get_reminder();
        private string heldTask = "";

        // Memory
        private memory_storage memory = new memory_storage();
        private string userName = "";

        // Cyber bot
        private Dictionary<string, List<string>> topics = new Dictionary<string, List<string>>();
        private Dictionary<string, string> sentiments = new Dictionary<string, string>();
        private List<string> ignoreWords = new List<string>();

        public MainWindow()
        {
            InitializeComponent();
            LoadQuizData();
            ShowQuiz();
            LoadCyberTopics();
            LoadSentiments();
            LoadIgnoreWords();
            ShowLogo();
            PlayAudio();

            var (savedUser, lastInput) = memory.Load();
            if (!string.IsNullOrWhiteSpace(savedUser))
            {
                userName = savedUser;
                combinedChat.Items.Add($"Welcome back, {userName}!");
                if (!string.IsNullOrWhiteSpace(lastInput))
                    combinedChat.Items.Add($"Last time you asked: \"{lastInput}\"");
            }
            else
            {
                userName = "User";
            }
        }

        private void LoadQuizData()
        {
            quizData = new List<QuizQuestion>
            {
                new QuizQuestion { Question = "How to stay protected from phishing?", CorrectChoice = "Exit and report", Choices = new() { "Enter details", "Follow the steps", "Share to others" } },
                new QuizQuestion { Question = "How to stay safe from malware?", CorrectChoice = "Download malware defense software", Choices = new() { "Leave it", "Destroy the device", "Sell the device" } },
                new QuizQuestion { Question = "How to keep passwords safe?", CorrectChoice = "Use strong passwords with multiple characters", Choices = new() { "Use the same password", "Share with friends", "Don't set passwords" } },
                new QuizQuestion { Question = "What is DDOS?", CorrectChoice = "Distributed Denial-of-Service", Choices = new() { "Don't deny offense systems", "Dark device of security", "Demolition death offense system" } },
                new QuizQuestion { Question = "What is privacy?", CorrectChoice = "Personal information that is kept secret", Choices = new() { "Gossiping", "Sharing to others", "Staying on the low" } },
                new QuizQuestion { Question = "How to stay protected from scams?", CorrectChoice = "Exit and report", Choices = new() { "Accept all", "Follow steps", "Share to others" } },
                new QuizQuestion { Question = "What is 2FA?", CorrectChoice = "Adds an extra layer of security", Choices = new() { "Removes all security", "Limits security", "Shares passwords" } },
                new QuizQuestion { Question = "What is browsing?", CorrectChoice = "Being mindful of the websites you visit", Choices = new() { "Entering random websites", "Surfing the net", "Searching the internet" } },
                new QuizQuestion { Question = "Is giving out passwords safe?", CorrectChoice = "No", Choices = new() { "Yes", "I think so", "Maybe" } },
                new QuizQuestion { Question = "How to detect a suspicious email?", CorrectChoice = "Look for poor grammar and suspicious links", Choices = new() { "Open every attachment", "Click all links", "Reply with details" } },
            };
        }

        private void ShowQuiz()
        {
            if (questionIndex >= quizData.Count)
            {
                MessageBox.Show("Quiz complete! Your score: " + currentScore);
                currentScore = 0;
                questionIndex = 0;
                DisplayingScore.Text = "";
                ShowQuiz();
                return;
            }

            selectedChoice = null;
            var currentQuiz = quizData[questionIndex];
            DisplayingQuestion.Text = currentQuiz.Question;

            var shuffled = currentQuiz.Choices.OrderBy(_ => Guid.NewGuid()).ToList();
            Button1.Content = shuffled[0];
            Button2.Content = shuffled[1];
            Button3.Content = shuffled[2];
            Button4.Content = currentQuiz.CorrectChoice;

            foreach (Button btn in new[] { Button1, Button2, Button3, Button4 })
                btn.Background = Brushes.LightGray;
        }

        private void HandleAnswerSelection(object sender, RoutedEventArgs e)
        {
            selectedChoice = sender as Button;
            string chosen = selectedChoice.Content.ToString();
            string correct = quizData[questionIndex].CorrectChoice;
            selectedChoice.Background = chosen == correct ? Brushes.Green : Brushes.Red;
        }

        private void HandleNextQuestion(object sender, RoutedEventArgs e)
        {
            if (selectedChoice == null)
            {
                MessageBox.Show("Please select an answer.");
                return;
            }

            if (selectedChoice.Content.ToString() == quizData[questionIndex].CorrectChoice)
                currentScore++;

            DisplayingScore.Text = $"Score: {currentScore}";
            questionIndex++;
            ShowQuiz();
        }

        private void set_reminder(object sender, RoutedEventArgs e)
        {
            string input = combinedText.Text.Trim();
            if (string.IsNullOrWhiteSpace(input))
            {
                MessageBox.Show("Please enter something.");
                return;
            }

            memory.Save(userName, input);
            combinedChat.Items.Add($"You: {input}");

            string inputLower = input.ToLower();

            if (inputLower.Contains("add task"))
            {
                heldTask = input.Replace("add task", "").Trim();
                combinedChat.Items.Add($"Bot: Task added - \"{heldTask}\". Would you like a reminder?");
            }
            else if (inputLower.Contains("remind"))
            {
                if (!string.IsNullOrWhiteSpace(heldTask))
                {
                    string day = reminder.get_days(input);
                    string response = day == "today" ? reminder.today_date(heldTask, "today") : reminder.get_remindDate(heldTask, day);
                    combinedChat.Items.Add("Bot: " + response);
                }
                else
                {
                    combinedChat.Items.Add("Bot: No task was set to remind you.");
                }
            }
            else if (inputLower.Contains("show reminders"))
            {
                combinedChat.Items.Add("Bot: " + reminder.reminder());
            }
            else
            {
                string response = GenerateChatbotResponse(inputLower);
                combinedChat.Items.Add("Bot: " + response);
            }

            combinedChat.ScrollIntoView(combinedChat.Items[combinedChat.Items.Count - 1]);
            combinedText.Clear();
        }

        private string GenerateChatbotResponse(string input)
        {
            string sentimentResponse = sentiments.FirstOrDefault(kvp => input.Contains(kvp.Key)).Value;
            List<string> filtered = input.Split(' ').Where(word => !ignoreWords.Contains(word)).ToList();
            foreach (var word in filtered)
            {
                if (topics.ContainsKey(word))
                {
                    var responses = topics[word];
                    string topicResponse = responses[new Random().Next(responses.Count)];
                    return sentimentResponse != null ? $"{sentimentResponse} {topicResponse}" : topicResponse;
                }
            }
            return sentimentResponse ?? "I can help with reminders, cybersecurity, phishing, scams, and safe browsing!";
        }

        private void LoadCyberTopics()
        {
            topics["phishing"] = new() {
                "Phishing tricks you into giving personal info through fake emails.",
                "Don't click unknown links or attachments." };
            topics["malware"] = new() {
                "Malware is harmful software. Use antivirus and avoid shady sites." };
            topics["passwords"] = new() {
                "Use strong, unique passwords and avoid reusing them." };
            topics["2fa"] = new() {
                "2FA adds extra login security. Always enable it." };
            topics["privacy"] = new() {
                "Keep your info private. Don’t overshare on social media." };
            topics["scam"] = new() {
                "If it sounds too good to be true, it’s probably a scam." };
        }

        private void LoadSentiments()
        {
            sentiments["worried"] = "It's okay to be worried. Let's look at this together.";
            sentiments["curious"] = "Great question! Here's what you should know.";
            sentiments["confused"] = "Let's simplify this.";
            sentiments["anxious"] = "Don't stress — I've got your back.";
            sentiments["frustrated"] = "Take a breath — you're doing fine.";
        }

        private void LoadIgnoreWords()
        {
            ignoreWords = new() {
                "the", "is", "a", "an", "of", "on", "at", "to", "and", "or", "but", "in",
                "about", "with", "as", "by", "for", "if", "can", "i", "you", "we", "they",
                "are", "was", "were", "have", "has", "do", "does", "did", "how", "what",
                "who", "when", "where", "why", "which", "it", "this", "that", "these",
                "those", "any", "some", "just", "more", "less", "also", "only", "tell", "me", "please"
            };
        }

        private void ShowLogo()
        {  
            string BaseDir =AppDomain.CurrentDomain.BaseDirectory;

            string imagePath = System.IO.Path.Combine(BaseDir, "poeimg.jpg");

            logoImage.Source = new BitmapImage(new Uri(imagePath));
        }

        private void PlayAudio()
        {
            string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "audiosamp.wav");
            if (File.Exists(path))
            {
                try
                {
                    SoundPlayer player = new SoundPlayer(path);
                    player.Play();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error playing audio: " + ex.Message);
                }
            }
        }
    }
}
