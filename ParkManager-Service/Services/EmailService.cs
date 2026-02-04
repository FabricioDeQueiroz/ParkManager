using System.Globalization;
using Microsoft.EntityFrameworkCore;
using MimeKit;
using MimeKit.Utils;
using ParkManager_Service.Data;
using ParkManager_Service.Models;
using ParkManager_Service.Services.Interfaces;

namespace ParkManager_Service.Services
{
    public class EmailService(AppDbContext db) : IEmail
    {
        private readonly string _smtpEmail = Environment.GetEnvironmentVariable("SMTP__EMAIL")
            ?? throw new InvalidOperationException("SMTP__EMAIL não definido nas variáveis de ambiente.");

        private readonly string _smtpPassword = Environment.GetEnvironmentVariable("SMTP__PASSWORD")
            ?? throw new InvalidOperationException("SMTP__PASSWORD não definido nas variáveis de ambiente.");

        public async Task SendMailAsync()
        {
            // Pegar todos Usuários Gerente do DB:
            var gerentes = await db.Usuarios
                .Where(u => u.Tipo == TipoUsuario.Gerente)
                .ToListAsync()
                .ConfigureAwait(false);

            if (gerentes.Count == 0) return;

            foreach (var gerente in gerentes)
            {
                // Pega os Estacionamentos do usuário:
                var estacionamentos = await db.Estacionamentos
                    .Where(e => e.IdGerente == gerente.Id)
                    .ToListAsync()
                    .ConfigureAwait(false);

                if (estacionamentos.Count == 0) continue;

                string nomeGerente = gerente.Nome.Split(' ').FirstOrDefault() ?? gerente.Nome;
                string emailGerente = gerente.Email!;

                string dataAtual = DateTime.UtcNow.AddDays(-5).ToString("MMMM yyyy", new CultureInfo("pt-BR"));

                string faturamentoTotal = estacionamentos.Sum(e => e.Faturamento).ToString("C", new CultureInfo("pt-BR"));
                string retornoContratanteTotal = estacionamentos.Sum(e => e.RetornoContratante * e.Faturamento).ToString("C", new CultureInfo("pt-BR"));

                var detalhesEstacionamentos = new List<(string nomeEstacionamento, string nomeContratante, string faturamento, string porcentagem, string retornoContratante)>();

                foreach (var estacionamento in estacionamentos)
                {
                    detalhesEstacionamentos.Add((
                        estacionamento.Nome,
                        estacionamento.NomeContratante,
                        (estacionamento.Faturamento).ToString("C", new CultureInfo("pt-BR")),
                        (estacionamento.RetornoContratante).ToString("P2", new CultureInfo("pt-BR")),
                        (estacionamento.RetornoContratante * estacionamento.Faturamento).ToString("C", new CultureInfo("pt-BR"))
                    ));
                }

                var message = new MimeMessage();
                message.From.Add(MailboxAddress.Parse(_smtpEmail));
                message.To.Add(MailboxAddress.Parse(emailGerente));
                message.Subject = $"Relatório Finaceiro Mensal de {dataAtual} - ParkManager";

                var builder = new BodyBuilder();

                var parkManagerLogoPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "logo.png");
                var logo = await builder.LinkedResources.AddAsync(parkManagerLogoPath).ConfigureAwait(false);
                logo.ContentId = MimeUtils.GenerateMessageId();

                var estacionamentosHtml = string.Join(Environment.NewLine, detalhesEstacionamentos.Select(e =>
                    "<tr>" +
                        $"<td style='border: 1px solid #ddd; padding: 8px;'>{e.nomeEstacionamento}</td>" +
                        $"<td style='border: 1px solid #ddd; padding: 8px;'>{e.nomeContratante}</td>" +
                        $"<td style='border: 1px solid #ddd; padding: 8px;'> {e.faturamento}</td>" +
                        $"<td style='border: 1px solid #ddd; padding: 8px;'>{e.porcentagem}</td>" +
                        $"<td style='border: 1px solid #ddd; padding: 8px;'> {e.retornoContratante}</td>" +
                    "</tr>"
                ));

                builder.HtmlBody = $@"
                    <!DOCTYPE html>
                    <html>
                    <head>
                        <meta charset='UTF-8'>
                        <meta name='viewport' content='width=device-width, initial-scale=1.0'>
                        <title>Relatório Financeiro Mensal - ParkManager</title>
                        <style>
                            @import url('https://fonts.googleapis.com/css2?family=Inter:ital,opsz,wght@0,14..32,100..900;1,14..32,100..900&display=swap');
                            body {{
                                font-family: 'Inter', Arial, sans-serif;
                                color: #333;
                                line-height: 1.6;
                                margin: 0;
                                padding: 0;
                                background-color: #F7FAFC;
                            }}
                            .container {{
                                width: 100%;
                                max-width: 820px;
                                margin: 20px auto;
                                background-color: #F7FAFC;
                                padding: 32px;
                                border-radius: 8px;
                                box-shadow: 0 0 10px rgba(0, 0, 0, 0.1);
                            }}
                            h1, h2, h3 {{
                                color: #2E86C1;
                            }}
                            p {{
                                margin-bottom: 10px;
                            }}
                            .header-logo {{
                                width: 200px;
                                display: block;
                                margin: 0 0 10px 0;
                            }}
                            hr {{
                                border: 0;
                                border-top: 1px solid #eee;
                                margin: 25px 0;
                            }}
                            .date-info {{
                                text-align: right;
                                font-size: 1.1em;
                                color: #777;
                                margin-bottom: 15px;
                            }}
                            .summary-box {{
                                gap: 20px;
                                background-color: #EBF8FF;
                                border: 1px solid #CEEDFF;
                                padding: 15px;
                                border-radius: 8px;
                                margin-bottom: 20px;
                            }}
                            .summary-item {{
                                font-size: 1.3em;
                                font-weight: bold;
                                padding-bottom: 5px;
                                border-bottom: 1px dashed #cce7ff;
                            }}
                            .summary-item:last-child {{
                                border-bottom: none;
                                padding-bottom: 0;
                            }}
                            .summary-item span:first-child {{
                                color: #555;
                            }}
                            .summary-item span:last-child {{
                                color: #007bff;
                                text-align: right;
                            }}
                            .summary-item .positive {{
                                color: #276749;
                            }}
                            .summary-item .negative {{
                                color: #9B2C2C;
                            }}
                            table {{
                                width: 100%;
                                border-collapse: collapse;
                                margin-top: 20px;
                                font-size: 1.1em;
                                border: none;
                            }}
                            th, td {{
                                border: none;
                                padding: 8px 0;
                                text-align: center;
                            }}
                            th {{
                                background-color: #f2f2f2;
                                color: #555;
                                font-weight: bold;
                                padding-left: 5px;
                                padding-bottom: 15px;
                            }}
                            table thead tr th:first-child {{
                                border-top-left-radius: 8px;
                            }}
                            table thead tr th:last-child {{
                                border-top-right-radius: 8px;
                            }}
                            td {{
                                padding-left: 5px;
                            }}
                            tr:nth-child(even) {{
                                background-color: #ffffff;
                            }}
                            .footer {{
                                margin-top: 30px;
                                text-align: center;
                                font-size: 1.1em;
                                color: #777;
                            }}
                            .message {{
                                font-size: 1.13em;
                                text-align: justify;
                                hyphens: auto;
                                word-break: break-word;
                            }}
                        </style>
                    </head>
                    <body>
                        <div class='container'>
                            <img src='cid:{logo.ContentId}' alt='Logo ParkManager' class='header-logo'/>

                            <hr>

                            <div style='background:#FDECEA; color:#611A15; padding:14px; border-radius:6px; font-size:14px;'>
                                <strong>Aviso:</strong> Este e-mail foi enviado como parte de um projeto acadêmico para fins de estudo e demonstração técnica.
                            </div>

                            <br>

                            <h2 style='color: #2E86C1;'>Olá, {nomeGerente}!</h2>
                            <p class='message'>Esperamos que esteja tudo bem. Segue abaixo o seu relatório financeiro detalhado referente ao mês anterior, com os valores totais apurados e o valor devido a cada contratante.</p>
                            <p class='message'>Este relatório é gerado no primeiro dia de cada mês para auxiliar no seu controle financeiro.</p>

                            <br>
                            <p class='date-info'>Relatório de {dataAtual}</p>

                            <h3>Resumo Geral</h3>
                            <div class='summary-box'>
                                <div class='summary-item'>
                                    <span>Faturamento Total: </span>
                                    <span class='positive'> {faturamentoTotal}</span>
                                </div>
                                <br>
                                <div class='summary-item'>
                                    <span>Total Contratantes: </span>
                                    <span class='negative'> {retornoContratanteTotal}</span>
                                </div>
                            </div>

                            <h3>Detalhes por Estacionamento</h3>
                            <table>
                                <thead>
                                    <tr>
                                        <th style='border: 1px solid #ddd; padding: 8px;'>Estacionamento</th>
                                        <th style='border: 1px solid #ddd; padding: 8px;'>Contratante</th>
                                        <th style='border: 1px solid #ddd; padding: 8px;'>Faturamento</th>
                                        <th style='border: 1px solid #ddd; padding: 8px;'>Retorno (%)</th>
                                        <th style='border: 1px solid #ddd; padding: 8px;'>Retorno Contratante</th>
                                    </tr>
                                </thead>
                                <tbody>
                                    {estacionamentosHtml}
                                </tbody>
                            </table>

                            <div class='footer'>
                                <p>Atenciosamente,<br/>Equipe ParkManager</p>
                                <p style='margin-top: 10px;'>Este é um e-mail automático. Por favor, não responda.</p>
                            </div>
                        </div>
                    </body>
                    </html>
                ";

                message.Body = builder.ToMessageBody();

                using var smtp = new MailKit.Net.Smtp.SmtpClient();
                await smtp.ConnectAsync("smtp.gmail.com", 587, MailKit.Security.SecureSocketOptions.StartTls).ConfigureAwait(false);
                await smtp.AuthenticateAsync(_smtpEmail, _smtpPassword).ConfigureAwait(false);
                await smtp.SendAsync(message).ConfigureAwait(false);
                await smtp.DisconnectAsync(true).ConfigureAwait(false);
            }
        }
    }
}
