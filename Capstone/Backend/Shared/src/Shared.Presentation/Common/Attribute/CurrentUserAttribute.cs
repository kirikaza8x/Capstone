using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Shared.Presentation.Common.Attributes
{
    [AttributeUsage(AttributeTargets.Parameter)]
    public class FromCurrentUserAttribute : Attribute, IBindingSourceMetadata
    {
        public BindingSource BindingSource => BindingSource.Custom;
    }
}
