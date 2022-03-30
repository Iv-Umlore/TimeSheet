using CheckingTimeFilling.TimeSheet;
using System;
using System.Configuration;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CheckingTFSTimeUtilit
{
    public partial class FillCompletedWork : Form
    {
        private const int TimeInterval = 15 * 60 * 1000;
        private TimeSpan TimeToStartNotifications { get; set; }
        private double MinimalHourWork { get; set; }
        private bool IsFilledToday { get; set; } = false;
        private DateTime LastFillingDate { get; set; } = DateTime.MinValue;
        private string TfsUrl { get; set; }
        private string UserName { get; set; }
        private bool IsConstantsCorrect { get; set; }

        private TfsInformation TfsInfo;

        public FillCompletedWork()
        {
            InitializeComponent();

            this.WindowState = FormWindowState.Normal;
            ShowIcon = true;
            this.ShowInTaskbar = true;

            IntervalChecking.RunWorkerAsync(StartWork());
        }

        private Task StartWork()
        {
            while (true)
            {
                FillConstants();
                if (IsConstantsCorrect)
                {
                    TfsInfo = new TfsInformation(TfsUrl, UserName);
                    if (!IsFilledToday && DateTime.Now.TimeOfDay > TimeToStartNotifications)
                    {
                        var completedWork = TfsInfo.GetCompletedWork();
                        if (completedWork < MinimalHourWork && !completedWork.Equals(MinimalHourWork))
                        {
                            GetWarning("Заполните Completed Work за сегодня!");
                        }
                        else
                        {
                            IsFilledToday = true;
                            LastFillingDate = DateTime.Today;
                        }
                    }
                }
                else
                {
                    GetWarning("Пожалуйста, укажите актуальные значения TFS Url и Username в конфигурации." +
                        "\r\nДополнительно можете указать время начала показа уведомлений(предположительно за час до конца рабочего дня), а так же минимальное число часов в день для работы над проектом.");
                }

                Thread.Sleep(TimeInterval);
                if (LastFillingDate.Date < DateTime.Now.Date)
                {
                    IsFilledToday = false;
                }
            }
        }

        private void FillConstants()
        {
            IsConstantsCorrect = true;

            TimeToStartNotifications = TimeSpan.TryParse(ConfigurationManager.AppSettings["TimeToStartNotifications"], out var tmpTime)
                ? tmpTime
                : new TimeSpan(hours: 17, minutes: 00, seconds: 00);

            MinimalHourWork = int.TryParse(ConfigurationManager.AppSettings["MinimalHourWork"], out var tmpMinWorkTime)
                ? tmpMinWorkTime
                : 8;

            var urlStr = ConfigurationManager.AppSettings["TfcUrl"];
            if (!string.IsNullOrEmpty(urlStr))
            {
                TfsUrl = urlStr;
            }
            else
            {
                TfsUrl = string.Empty;
                GetWarning("Укажите TFS Url!");
                IsConstantsCorrect = false;
            }

            var usernameStr = ConfigurationManager.AppSettings["UserName"];
            if (!string.IsNullOrEmpty(usernameStr))
            {
                UserName = usernameStr;
            }
            else
            {
                UserName = string.Empty;
                GetWarning("Укажите Username такой же как в TFS");
                IsConstantsCorrect = false;
            }
        }

        private void GetWarning(string text)
        {
            MessageBox.Show(text);
        }
    }
}
