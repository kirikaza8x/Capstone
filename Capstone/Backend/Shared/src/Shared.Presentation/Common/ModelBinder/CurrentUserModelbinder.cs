using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Logging;
using Shared.Application.DTOs;
using System.Security.Claims;

namespace Shared.Presentation.Common.ModelBinder
{
    public class CurrentUserModelBinder : IModelBinder
    {
        public Task BindModelAsync(ModelBindingContext bindingContext)
        {
            if (bindingContext == null)
            {
                // throw new ArgumentNullException(nameof(bindingContext));
                return Task.FromResult(0);
            }

            var user = bindingContext.HttpContext.User;

            // If user is not authenticated or identity is missing, fail binding
            if (user?.Identity == null || !user.Identity.IsAuthenticated)
            {
                bindingContext.Result = ModelBindingResult.Failed();
                return Task.CompletedTask;
            }

            // Try to extract claims
            Guid userId = Guid.TryParse(user.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : Guid.Empty;
            string? email = user.FindFirstValue(ClaimTypes.Email);
            string? name = user.FindFirstValue(ClaimTypes.Name);
            var roles = user.FindAll(ClaimTypes.Role).Select(r => r.Value).ToList();
            string? jti = user.FindFirstValue("jti");

            // If critical claim (like UserId) is missing, fail binding
            if (userId == Guid.Empty && string.IsNullOrEmpty(email))
            {
                bindingContext.Result = ModelBindingResult.Failed();
                return Task.CompletedTask;
            }

            var dto = new CurrentUserDto
            {
                UserId = userId,
                Email = email,
                Name = name,
                Roles = roles,
                Jti = jti
            };
            bindingContext.Result = ModelBindingResult.Success(dto);

            return Task.CompletedTask;
        }
    }

    internal static class ClaimsPrincipalExtensions
    {
        public static string? FindFirstValue(this ClaimsPrincipal user, string claimType) =>
            user.FindFirst(claimType)?.Value;
    }
}
