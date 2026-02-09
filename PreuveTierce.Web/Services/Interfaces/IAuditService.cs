namespace PreuveTierce.Web.Services.Interfaces
{
    public interface IAuditService
    {
        public Task SaveLogAsync(string action, string docHash, string status, HttpContext context);
    }
}
