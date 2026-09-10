using Dapper;
using DisparoApi.Data;
using DisparoApi.Dtos;

namespace DisparoApi.Repositories;

public class TemplateRepository : ITemplateRepository
{
    private readonly IDbConnectionFactory _factory;

    public TemplateRepository(IDbConnectionFactory factory)
    {
        _factory = factory;
    }

    public async Task<List<TemplateResponse>> ListAsync()
    {
        using var conn = _factory.Create();
        var sql = @"SELECT id, nome, mensagem, created_at, updated_at FROM templates ORDER BY id DESC";
        var result = await conn.QueryAsync<TemplateResponse>(sql);
        return result.ToList();
    }

    public async Task<TemplateResponse?> GetByIdAsync(int id)
    {
        using var conn = _factory.Create();
        var sql = @"SELECT id, nome, mensagem, created_at, updated_at FROM templates WHERE id = @id";
        return await conn.QueryFirstOrDefaultAsync<TemplateResponse>(sql, new { id });
    }

    public async Task<TemplateResponse> CreateAsync(string nome, string mensagem)
    {
        using var conn = _factory.Create();
        var sql = @"INSERT INTO templates (nome, mensagem) VALUES (@nome, @mensagem);
                   SELECT id, nome, mensagem, created_at, updated_at FROM templates WHERE id = LAST_INSERT_ID()";
        return await conn.QueryFirstAsync<TemplateResponse>(sql, new { nome, mensagem });
    }

    public async Task<TemplateResponse?> UpdateAsync(int id, string nome, string mensagem)
    {
        using var conn = _factory.Create();
        var sql = @"UPDATE templates SET nome = @nome, mensagem = @mensagem WHERE id = @id;
                   SELECT id, nome, mensagem, created_at, updated_at FROM templates WHERE id = @id";
        var result = await conn.QueryFirstOrDefaultAsync<TemplateResponse>(sql, new { id, nome, mensagem });
        return result;
    }

    public async Task DeleteAsync(int id)
    {
        using var conn = _factory.Create();
        var sql = @"DELETE FROM templates WHERE id = @id";
        await conn.ExecuteAsync(sql, new { id });
    }
}
