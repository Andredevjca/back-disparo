using DisparoApi.Dtos;

namespace DisparoApi.Repositories;

public interface ITemplateRepository
{
    Task<List<TemplateResponse>> ListAsync();
    Task<TemplateResponse?> GetByIdAsync(int id);
    Task<TemplateResponse> CreateAsync(string nome, string mensagem);
    Task<TemplateResponse?> UpdateAsync(int id, string nome, string mensagem);
    Task DeleteAsync(int id);
}
