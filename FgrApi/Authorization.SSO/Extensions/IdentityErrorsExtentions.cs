namespace Authorization.SSO.Extensions
{
    public static class IdentityErrorsExtentions
    {
        public static IActionResult ErrorResponse(this IEnumerable<IdentityError> errors) => StringResponse(errors.Select(e => e.Description));
        public static IActionResult ErrorResponse(this IEnumerable<string> errors) => StringResponse(errors);

        private static IActionResult StringResponse(IEnumerable<string> errors)
        {
            var i = 0;
            var errorModel = errors.Aggregate(new ModelStateDictionary(), (p, a) => { p.AddModelError($"Error #{++i}", a); return p; });
            return new BadRequestObjectResult(errorModel);
        }
    }
}
