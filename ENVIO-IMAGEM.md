Envio com imagem

Os endpoints unitario, massa e importacoes/{id}/enviar aceitam imagem opcional:
{ "imagem": { "base64": "...", "mimeType": "image/png", "nomeArquivo": "imagem.png" } }

PNG/JPEG: limite de 5 MB. Enviar somente o base64, sem prefixo data:.
O texto personalizado segue como caption no endpoint message/sendMedia da Evolution v2.
Referencia: https://doc.evolution-api.com/v2/api-reference/message-controller/send-media

Ao iniciar a API, DatabaseInitializer cria envio_imagens se ainda nao existir.
Uma imagem e armazenada por envio, sem duplicar para cada contato.
A conta MySQL precisa de permissao CREATE TABLE. Nenhuma tabela existente e removida.
Esta alteracao de schema ainda nao foi executada em um banco real nesta sessao.

Validacao local: dotnet run --project tests/EnvioImagem.Checks
O teste simula HTTP; nao envia mensagens reais nem acessa o banco.
Frontend: npm.cmd run build -- --configuration development
Para confirmar a integracao real, usar uma conta Evolution conectada e um numero de teste.

Templates com imagem

POST/PUT /api/modelos aceitam o mesmo objeto imagem opcional. GET devolve a imagem salva.
PUT com imagem null remove a imagem do template. Nome e mensagem continuam obrigatorios.
A inicializacao cria template_imagens sem alterar os templates existentes.
Texto e imagem do template sao gravados na mesma transacao.

Nos tres endpoints de envio, imagem informada substitui a do template apenas no disparo.
Sem imagem informada, usarImagemTemplate (padrao true) carrega a imagem do template.
Para enviar apenas texto, passar imagem: null e usarImagemTemplate: false.
As telas enviam a imagem exibida na previa com usarImagemTemplate: false, respeitando a remocao.
Selecionar outro template carrega a imagem desse novo template.
