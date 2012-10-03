namespace DynamicValidation
{
	public interface INamedPredicate
	{
		bool Matches(object actual, out string message);
	}
}