namespace McMaster.Extensions.CommandLineUtils.Conventions
{
    internal interface IConventionBuilder
    {
        void AddConvention(IAppConvention convention);
    }
}
