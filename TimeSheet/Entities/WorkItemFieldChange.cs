﻿namespace TimeSheet.Entities
{
	class WorkItemFieldChange<T>
	{
		public bool IsChangedInRevision { get; set; }
		public T OldValue { get; set; }
		public T NewValue { get; set; }
	}
}
