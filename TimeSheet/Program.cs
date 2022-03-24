using Microsoft.TeamFoundation.Client;
using Microsoft.TeamFoundation.WorkItemTracking.Client;
using System;
using System.Linq;
using TimeSheet.Entities;

namespace TimeSheet
{
    class Program
    {
        const string TfsUrl = "https://tfs.example.com/";
        const string UserName = "Пушкин Александр Сергеевич";

        static void Main(string[] args)
        {
            GetCompletedWork();
        }

        static void GetCompletedWork()
        {

            var startWeek = DateTime.Now.StartOfWeek(DayOfWeek.Monday);
            var endWeek = startWeek.AddDays(7).AddSeconds(-1);

            using (var client = new TfsTeamProjectCollection(new Uri(TfsUrl))) 
            {
                var queryStr = GetChangedItemQuery(startWeek, endWeek);
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
            }
        }

        static string GetChangedItemQuery(DateTime dateFrom, DateTime dateTo) => $"SELECT [System.Id], [System.WorkItemType]" +
            $" FROM WorkItems WHERE [System.ChangedDate] >= '{dateFrom}' AND [System.ChangedDate] <= '{dateTo}'" +
            $" AND ([System.WorkItemType] = 'Bug' OR [System.WorkItemType] = 'Issue' OR [System.WorkItemType] Contains 'Task')" +
            $" AND ([System.ChangedBy] EVER @Me)";
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
