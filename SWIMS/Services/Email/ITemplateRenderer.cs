using System.Threading.Tasks;

namespace SWIMS.Services.Email;

public interface ITemplateRenderer
{
    Task<EmailTemplate> RenderAsync(string templateKey, object model);
}
