using CheckingTimeFilling.TimeSheet;
using System;
using System.Configuration;
using System.Threading;
using System.Windows.Forms;

namespace CheckingTFSTimeUtilit
{
    public partial class FillCompletedWork : Form
    {
        /// <summary>
        /// Время между уведомлениями (Считаю оптимальным 12 минут, не считаю необходимым задавать в конфигурации)
        /// </summary>
        private const int TimeInterval = (int)(12 * 60 * 1000);

        /// <summary>
        /// URL основного TFS хранилища (прим. https://tfs.mtsit.com/STS)
        /// (задаётся в конфигурации)
        /// </summary>
        private string TfsUrl { get; set; }

        /// <summary>
        /// Имя пользователя в TFS. Необходимо указать в точности как в TFS (с отчеством)
        /// (задаётся в конфигурации)
        /// </summary>
        private string UserName { get; set; }

        /// <summary>
        /// Время в которое начинается показ уведомлений
        /// (задаётся в конфигурации)
        /// </summary>
        private TimeSpan TimeToStartNotifications { get; set; }

        /// <summary>
        /// Минимальная сумма времени, которая должна быть заполнена в TFS за текущий день
        /// (задаётся в конфигурации)
        /// </summary>
        private double MinimalHourWork { get; set; }

        /// <summary>
        /// Заполнялось ли сегодня TFS
        /// Необходима для оптимизации
        /// </summary>
        private bool IsFilledToday { get; set; } = false;

        /// <summary>
        /// Необходима для оптимизации
        /// </summary>
        private DateTime LastFillingDate { get; set; } = DateTime.MinValue;

        /// <summary>
        /// Корректно ли заданы константы в конфигурации
        /// </summary>
        private bool IsConstantsCorrect { get; set; }

        /// <summary>
        /// Сервис взаимодействия с TFS
        /// </summary>
        private TfsInformation TfsInfo;

        /// <summary>
        /// Основной поток выполнения
        /// </summary>
        private Thread mainCheckingThread;

        private const string statusConst = "Статус";
        private const string problemConst = "TFS utilit problem";
        private const string instructionText = "ЛКМ - запуск приложения,\r\nПКМ - приостановка приложения,\r\nСредняя кнопка мыши - Отключение приложения.\r\n\r\nДля запуска необходимо кликнуть ЛКМ по значку.";

        public FillCompletedWork()
        {
            InitializeComponent();
            HideWindow();

            GetWarning("Управление приложением", instructionText, 20000);
            mainCheckingThread = new Thread(()=> StartWork());
            mainCheckingThread.Start();
        }

        private void Icon_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Middle)
            {
                mainCheckingThread.Abort();
                this.Close();
            }

            if (e.Button == MouseButtons.Left)
            {
                if (mainCheckingThread.ThreadState == ThreadState.WaitSleepJoin)
                {
                    GetWarning(statusConst, "Программа уже запущена");
                }
                if (mainCheckingThread.ThreadState == ThreadState.Aborted)
                {
                    mainCheckingThread = new Thread(() => StartWork());
                    mainCheckingThread.Start();
                    HideWindow();
                    GetWarning(statusConst, "Программа запущена");
                }
            }

            if (e.Button == MouseButtons.Right)
            {
                mainCheckingThread.Abort();
                GetWarning(statusConst, "Программа остановлена");
            }
        }

        private void HideWindow()
        {
            this.WindowState = FormWindowState.Minimized;
            ShowIcon = true;
            this.ShowInTaskbar = true;
        }

        private void StartWork()
        {
            while (true)
            {
                FillConstants();
                if (IsConstantsCorrect)
                {
                    TfsInfo = new TfsInformation(TfsUrl, UserName);
                    if (!IsFilledToday && DateTime.Now.TimeOfDay > TimeToStartNotifications)
                    {
                        var completedWork = TfsInfo.GetCompletedWorkToday();
                        if (completedWork < MinimalHourWork && !completedWork.Equals(MinimalHourWork))
                        {
                            GetWarning(problemConst, "Заполните Completed Work за сегодня!", 30000);
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
                    GetWarning(problemConst, "Пожалуйста, укажите актуальные значения TFS Url и Username в конфигурации." +
                        "\r\nДополнительно можете указать время начала показа уведомлений(предположительно за час до конца рабочего дня),\r\n а так же минимальное число часов в день для работы над проектом.", 40000);
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
                GetWarning(problemConst, "Укажите TFS Url!", 5000);
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
                GetWarning(problemConst, "Укажите Username такой же как в TFS", 5000);
                IsConstantsCorrect = false;
            }
        }

        private void GetWarning(string title, string text, int milliseconds = 1500)
        {
            Icon.BalloonTipText = text;
            Icon.BalloonTipTitle = title;
            Icon.ShowBalloonTip(milliseconds);
        }
    }
}
