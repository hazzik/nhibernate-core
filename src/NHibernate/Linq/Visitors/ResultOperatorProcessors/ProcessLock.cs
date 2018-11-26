namespace NHibernate.Linq.Visitors.ResultOperatorProcessors
{
	internal class ProcessLock : IResultOperatorProcessor<LockResultOperator>
	{
		public void Process(LockResultOperator resultOperator, QueryModelVisitor queryModelVisitor, IntermediateHqlTree tree)
		{
			tree.AddAdditionalCriteria((q, p) => q.SetLockMode(resultOperator.Alias, (LockMode) resultOperator.LockMode.Value));
		}
	}
}
