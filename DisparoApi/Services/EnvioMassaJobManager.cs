using System.Collections.Concurrent;
using DisparoApi.Models;
using DisparoApi.Repositories;

namespace DisparoApi.Services;

public class EnvioMassaJobManager : IEnvioMassaJobManager
{
    private readonly ConcurrentDictionary<int, JobState> _jobs = new();
    private readonly IServiceScopeFactory _scopeFactory;

    public EnvioMassaJobManager(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
    }

    private class JobState
    {
        public CancellationTokenSource Cts { get; } = new();
        public bool PararSolicitado { get; set; }
    }

    public void IniciarProcessamento(int envioId, int intervaloMs, string instance)
    {
        if (_jobs.ContainsKey(envioId)) return;

        var state = new JobState();
        _jobs.TryAdd(envioId, state);

        _ = Task.Run(async () =>
        {
            try
            {
                await ProcessarLoopAsync(envioId, intervaloMs, instance, state);
            }
            finally
            {
                _jobs.TryRemove(envioId, out _);
                state.Cts.Dispose();
            }
        }, state.Cts.Token);
    }

    public bool SolicitarParada(int envioId)
    {
        if (!_jobs.TryGetValue(envioId, out var state)) return false;
        state.PararSolicitado = true;
        state.Cts.Cancel();
        return true;
    }

    public bool EstaProcessando(int envioId) => _jobs.ContainsKey(envioId);

    private async Task ProcessarLoopAsync(int envioId, int intervaloMs, string instance, JobState state)
    {
        using var scope = _scopeFactory.CreateScope();
        var envios = scope.ServiceProvider.GetRequiredService<IEnvioRepository>();
        var evolution = scope.ServiceProvider.GetRequiredService<IEvolutionApiService>();

        try
        {
            while (!state.Cts.IsCancellationRequested)
            {
                if (state.PararSolicitado)
                {
                    await envios.FinalizarEnvioAsync(envioId, StatusEnvio.Parado);
                    break;
                }

                var pendentes = await envios.ListarPendentesMassaAsync(envioId);
                if (pendentes.Count == 0)
                {
                    await envios.FinalizarEnvioAsync(envioId, StatusEnvio.Concluido);
                    break;
                }

                var proximo = pendentes[0];

                try
                {
                    await envios.AtualizarStatusDetalheAsync(proximo.id, StatusDetalhe.Enviando, null, null);

                    var (ok, evolutionId, numeroOrigem, erro) = await evolution.EnviarMensagemAsync(
                        instance, proximo.telefone, proximo.mensagem ?? string.Empty);

                    await envios.AtualizarStatusDetalheAsync(proximo.id,
                        ok ? StatusDetalhe.Enviado : StatusDetalhe.Erro,
                        ok ? null : erro,
                        evolutionId);

                    if (ok && !string.IsNullOrWhiteSpace(numeroOrigem))
                    {
                        using var scope2 = _scopeFactory.CreateScope();
                        var envios2 = scope2.ServiceProvider.GetRequiredService<IEnvioRepository>();
                        await envios2.AtualizarStatusDetalheAsync(proximo.id, StatusDetalhe.Enviado, null, evolutionId);
                    }
                }
                catch (Exception ex)
                {
                    await envios.AtualizarStatusDetalheAsync(proximo.id, StatusDetalhe.Erro, ex.Message, null);
                }

                await envios.AtualizarContagensEnvioAsync(envioId);

                try
                {
                    await Task.Delay(Math.Max(100, intervaloMs), state.Cts.Token);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
            }
        }
        catch
        {
            try
            {
                using var scope3 = _scopeFactory.CreateScope();
                var envios3 = scope3.ServiceProvider.GetRequiredService<IEnvioRepository>();
                await envios3.FinalizarEnvioAsync(envioId, StatusEnvio.Parado);
            }
            catch { }
        }
    }
}
