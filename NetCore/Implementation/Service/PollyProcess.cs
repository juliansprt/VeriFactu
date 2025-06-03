using Polly;
using Polly.Retry;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using VeriFactu.Business;
using VeriFactu.Business.Operations;
using VeriFactu.Net.Core.Implementation.Exceptions;
using VeriFactu.Qrcode;

namespace VeriFactu.Net.Core.Implementation.Service
{
    public class PollyProcess
    {
        protected readonly ResilienceOptions _resilienceOptions;
        protected readonly Settings _settings;
        protected readonly ILogger _logger;
        protected readonly ICertificateService _certificateService;
        public PollyProcess(ISettingsServices settingsService, ILogger logger, ICertificateService certificateService)
        {
            _settings = settingsService.GetSettings();
            _resilienceOptions = _settings.ResilenceOptions;
            _logger = logger;
            _certificateService = certificateService;
        }

        public IAsyncPolicy<string> GetAsyncPolicy()
        {


            //var invoiceQuery = new InvoiceQuery(invoice.SellerID, invoice.CompanyId, invoice.SellerName, _settings, _certificateService);

            var retry = Policy<string>
                .Handle<VerifactuEnviandoRequestException>()
                .WaitAndRetryAsync(
                    _resilienceOptions.RetryCount,
                    att => TimeSpan.FromSeconds(Math.Pow(_resilienceOptions.RetryBaseDelaySeconds, att))
                           + TimeSpan.FromMilliseconds(new Random().Next(_resilienceOptions.JitterMilliseconds)),
                    onRetryAsync: (ex, ts, cnt, ctx) =>
                    {
                        _logger.Information(ex.Exception, "Retry {RetryCount} tras {Delay}s", cnt, ts.TotalSeconds);
                        return Task.CompletedTask;
                    });

            var breaker = Policy<string>
                .Handle<Exception>()
                .CircuitBreakerAsync(
                    _resilienceOptions.CircuitBreaker.FailureThreshold,
                    TimeSpan.FromSeconds(_resilienceOptions.CircuitBreaker.DurationOfBreakSeconds),
                    onBreak: (ex, _) => 
                    {
                        _logger.Warning(ex.Exception, "Circuito abierto"); 
                    },
                    onReset: () => 
                    { 
                        _logger.Information("Circuito cerrado"); 
                    },
                    onHalfOpen: () => 
                    { 
                        _logger.Information("Circuito half-open"); 
                    });


            var fallback = Policy<string>
                .Handle<Exception>()
                .FallbackAsync<string>(
                    // 1) fallbackAction: recibe resultado previo, Context y CancellationToken
                    async (outcome, context, cancellationToken) =>
                    {
                        _logger.Information("Fallback: consultando estado de factura");

                        // Recupera los datos del Context
                        string sellerId = (string)context[nameof(Invoice.SellerID)];
                        int companyId = (int)context[nameof(Invoice.CompanyId)];
                        string sellerName = (string)context[nameof(Invoice.SellerName)];
                        string invoiceId = (string)context[nameof(Invoice.InvoiceID)];
                        DateTime invoiceDate = (DateTime)context[nameof(Invoice.InvoiceDate)];

                        _settings.SimulateTimeout = false; // Desactiva el timeout simulado para la consulta de estado
                        // Consulta estado o genera valor alternativo
                        var invoiceQuery = new InvoiceQuery(sellerId, companyId, sellerName, _settings, _certificateService);
                        var invoice = invoiceQuery.GetInvoice(
                            invoiceDate.Year.ToString(),
                            invoiceDate.Month.ToString("D2"),
                            invoiceId);

                        return invoice.ToString();
                    },
                    // 2) onFallbackAsync: callback para logging
                    async (outcome, context) =>
                    {
                        _logger.Error(
                            outcome.Exception,
                            "Entrando en fallback para invoice {InvoiceID}",
                            context[nameof(Invoice.InvoiceID)]);
                    }
                );

            return Policy.WrapAsync(fallback, breaker, retry);

        }

    }
}
