Atendimento: imagens e recebimento

As imagens enviadas pelo sistema sao recuperadas de envio_imagens usando o vinculo
com o detalhe do disparo ou o evolution_id. Nao exige nova tabela.
GET /api/atendimento/conversas/{id}/mensagens/{mensagemId}/imagem exige login e acesso a conversa.
O historico retorna tem_imagem, sem repetir base64 nas consultas automaticas.
Imagens de mensagens externas que nao foram enviadas pelo sistema nao fazem parte desse armazenamento.

Recebimento em tempo real:
- Evolution precisa encaminhar MESSAGES_UPSERT para a URL publica do backend terminada em /api/evolution/webhook.
- Sao aceitos data objeto, data array e data.messages; rotas por evento tambem sao aceitas.
- MESSAGES_UPDATE atualiza status. remoteJidAlt e usado quando o remetente chega como @lid.
- A chave recebida precisa corresponder a Evolution:ApiKey (header apikey ou x-api-key, query ou campo apikey do envelope).
- A URL precisa ser acessivel a partir do servidor/container Evolution.
- Para recuperar mensagens antigas que nao entraram antes da correcao, usar Buscar historicos ou Forcar re-sincronia.
Referencia: https://docs.evolutionfoundation.com.br/evolution-api/configuration/webhooks

Frontend: lista a cada 5s, conversa aberta a cada 3s, sem requisicoes sobrepostas.
Consultas e respostas nao acionam loading global. Somente POST sincronizar aciona loading.
A sincronizacao nao e iniciada automaticamente ao entrar na tela; o status continua no painel.

Validacao local: dotnet run --project tests/EnvioImagem.Checks (HTTP/repositorios simulados).
A entrega real do webhook e o acesso ao banco precisam ser confirmados no ambiente conectado.
