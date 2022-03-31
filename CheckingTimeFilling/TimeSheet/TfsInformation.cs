using CheckingTimeFilling.TimeSheet.Entities;
using Microsoft.TeamFoundation.Client;
using Microsoft.TeamFoundation.WorkItemTracking.Client;
using System;
using System.Linq;

namespace CheckingTimeFilling.TimeSheet
{
    public class TfsInformation
    {
        private readonly string TfsUrl;
        private readonly string UserName;

        public TfsInformation(string tfsUrl, string userName)
        {
            TfsUrl = tfsUrl;
            UserName = userName;
        }

        /// <summary>
        /// Вернуть число часов указанных за этот день в полях Completed Work во всех задачах TFS для пользователя
        /// </summary>
        public double GetCompletedWorkToday()
        {
            var dayStartTime = DateTime.Today;
            var dayEndTime = DateTime.Today.AddDays(1).AddSeconds(-1);

            using (var client = new TfsTeamProjectCollection(new Uri(TfsUrl))) 
            {
                var queryStr = GetItemChangedByMe(DateTime.Now);
                var query = new Query(client.GetService<WorkItemStore>(), queryStr, null, false);

                var request = query.BeginQuery();
                var workItems = query.EndQuery(request)
                    .AsParallel()
                    .Cast<WorkItem>()
                    .SelectMany(workItem => workItem.Revisions.Cast<Revision>().Select(t => new WorkItemChange<double?>(t, "Completed Work")))
                    .Where(x => x.DateOfChange > dayStartTime && x.DateOfChange <= dayEndTime)
                    .Where(x => x.UserName == UserName)
                    .GroupBy(x => x.WorkItemId)
                    .Select(x => (WI: x.Key, Hours: x.Sum(y => (y.Field.NewValue ?? 0) - (y.Field.OldValue ?? 0))))
                    .ToList();

                foreach(var item in workItems)
                    Console.WriteLine($"{item.WI}: {item.Hours}");

                Console.WriteLine($"total: {workItems.Sum(x => x.Hours)}");
                return workItems.Sum(x => x.Hours);
            }
        }

        /// <summary>
        /// Выгрузка Task, Bug, Issue которые изменялись пользователем за текущий день
        /// </summary>
        /// <param name="Today"> Текущая дата </param>
        static string GetItemChangedByMe(DateTime Today) =>
            $"SELECT [System.Id], [System.WorkItemType] " +
            $" FROM WorkItems WHERE [System.ChangedDate] >= '{Today.Date}' AND [System.ChangedDate] <= '{Today.Date.AddDays(1).AddSeconds(-1)}'" +
            $" AND ([System.WorkItemType] = 'Bug' OR [System.WorkItemType] = 'Issue' OR [System.WorkItemType] Contains 'Task')" +
            $" AND ([System.ChangedBy] EVER @Me)";

        // Метод для теста
        /// <summary>
        /// Возвращает информацию по задаче Горятина Павла
        /// Актуально для TFS https://tfs.mtsit.com/STS
        /// </summary>
        [Obsolete("Удалить при передаче программы в пользование")]
        static string GetChangedItemQuery() 
        {
            var dateFrom = new DateTime(year: 2022, month: 3, day: 17);
            var dateTo = new DateTime(year: 2022, month: 3, day: 17, hour: 23, minute: 59, second: 59);

            return $"SELECT * " + 
            $" FROM WorkItems WHERE [System.Id] = 2691945 AND [System.ChangedDate] >= '{dateFrom}' AND [System.ChangedDate] <= '{dateTo}'";

        }
    }
}
