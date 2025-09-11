using Microsoft.AspNetCore.Mvc.Filters;

namespace PersonsListExample.Filters
{
    public class SkipFilter : Attribute, IFilterMetadata
    {
    }
}
