using Microsoft.TeamFoundation.Client;
using Microsoft.TeamFoundation.WorkItemTracking.Client;
using System;
using System.Linq;
using TimeSheet.Entities;

namespace TimeSheet
{
    class Program
    {
        //const string TfsUrl = "http://ikiselev:8080/tfs";
        const string TfsUrl = "https://tfs.mtsit.com/STS";
        const string UserName = "Горятнин Павел";

        static void Main(string[] args)
        {
            var completedWorkToday = GetCompletedWork();
            if (completedWorkToday < 7.0)
            {
                MakeNotification(completedWorkToday);
            }


        }

        /// <summary>
        /// Вернуть число часов указанных за этот день
        /// </summary>
        [Obsolete]
        static double GetCompletedWork()
        {
            var startWeek = new DateTime(year: 2022, month: 3, day: 17);// DateTime.Now.StartOfWeek(DayOfWeek.Monday);
            var endWeek = new DateTime(year: 2022, month: 3, day: 17, hour: 23, minute:59, second:59);// 
            // var endWeek = startWeek.AddDays(7).AddSeconds(-1);

            using (var client = new TfsTeamProjectCollection(new Uri(TfsUrl))) 
            {
                var queryStr = GetChangedItemQuery(startWeek, endWeek);
                //var queryStr = GetItemChangedByMe(startWeek, endWeek);
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

        static void MakeNotification(double completeWorkNow)
        {
            // Уведомление о том, что вы плохо сделали
        }

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
