using CheckingTimeFilling.TimeSheet.Entities;
using Microsoft.TeamFoundation.Client;
using Microsoft.TeamFoundation.WorkItemTracking.Client;
using System;
using System.Linq;

namespace CheckingTimeFilling.TimeSheet
{
    public class TfsInformation
    {
        private string TfsUrl;
        private string UserName;

        public TfsInformation(string tfsUrl, string userName)
        {
            TfsUrl = tfsUrl;
            UserName = userName;
        }

        /// <summary>
        /// Вернуть число часов указанных за этот день
        /// </summary>
        [Obsolete]
        public double GetCompletedWork()
        {
            var startWeek = new DateTime(year: 2022, month: 3, day: 17);
            var endWeek = new DateTime(year: 2022, month: 3, day: 17, hour: 23, minute:59, second:59);

            using (var client = new TfsTeamProjectCollection(new Uri(TfsUrl))) 
            {
                var queryStr = GetChangedItemQuery(startWeek, endWeek);
                //var queryStr = GetItemChangedByMe(DateTime.Now);
                var query = new Query(client.GetService<WorkItemStore>(), queryStr, null, false);

                var request = query.BeginQuery();

                var workItems = query.EndQuery(request)
                    .AsParallel()
                    .Cast<WorkItem>()
                    .SelectMany(workItem => workItem.Revisions.Cast<Revision>().Select(t => new WorkItemChange<double?>(t, "Completed Work")))
                    .Where(x => x.DateOfChange > startWeek && x.DateOfChange <= endWeek)
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

        static string GetItemChangedByMe(DateTime Today) =>
            $"SELECT [System.Id], [System.WorkItemType] " +
            $" FROM WorkItems WHERE [System.ChangedDate] >= '{Today.Date}' AND [System.ChangedDate] <= '{Today.Date.AddDays(1).AddSeconds(-1)}'" +
            $" AND ([System.WorkItemType] = 'Bug' OR [System.WorkItemType] = 'Issue' OR [System.WorkItemType] Contains 'Task')" +
            $" AND ([System.ChangedBy] EVER @Me)";

        static string GetChangedItemQuery(DateTime dateFrom, DateTime dateTo) => $"SELECT * " + //
            $" FROM WorkItems WHERE [System.Id] = 2691945 AND [System.ChangedDate] >= '{dateFrom}' AND [System.ChangedDate] <= '{dateTo}'";/*  */
            /*$"[System.ChangedDate] >= '{dateFrom}' AND [System.ChangedDate] <= '{dateTo}'" +
            $" AND ([System.WorkItemType] = 'Bug' OR [System.WorkItemType] = 'Issue' OR [System.WorkItemType] Contains 'Task')";// +
            //$" AND ([System.ChangedBy] EVER @Me)";*/

    }

    static class DateTimeExtensions
    {
        public static DateTime StartOfWeek(this DateTime dt, DayOfWeek startOfWeek)
        {
            int diff = (7 + (dt.DayOfWeek - startOfWeek)) % 7;
            return dt.AddDays(-1 * diff).Date;
        }
    } 
}
