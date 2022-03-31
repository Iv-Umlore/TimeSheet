using Microsoft.TeamFoundation.WorkItemTracking.Client;
using System;

namespace CheckingTimeFilling.TimeSheet.Entities
{
	[Obsolete("Revision and Field are obsolete classes")]
	public class WorkItemChange<T>
	{
		public int WorkItemId { get; set; }

		/// <summary>
		/// Основное поле данной задачи. Содержит в себе "слепок" состояния задачи на момент времени
		/// Содержит информацию о всех полях связанных с задачей.
		/// </summary>
		public Revision Revision { get; set; }
		public DateTime DateOfChange { get; set; }
		public string UserName { get; set; }
		public WorkItemFieldChange<T> Field { get; set; }

		public WorkItemChange(Revision revision, string fieldName) : this(revision, revision.Fields[fieldName]) { }

		public WorkItemChange(Revision revision, CoreField fieldName) : this(revision, revision.Fields[fieldName]) { }

		private WorkItemChange(Revision revision, Field field)
		{
			WorkItemId = revision.WorkItem.Id;
			Revision = revision;
			DateOfChange = (DateTime)revision.Fields[CoreField.ChangedDate].Value;
			UserName = (string)revision.Fields[CoreField.ChangedBy].Value;
			Field = new WorkItemFieldChange<T>
			{
				IsChangedInRevision = field.IsChangedInRevision,
				OldValue = (T)field.OriginalValue,
				NewValue = (T)field.Value,
			};
		}
    }
}
