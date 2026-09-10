using System.Globalization;
using OrcaFacil.Application.Interfaces;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace OrcaFacil.Infrastructure.Pdf;

public class QuestPdfWorkOrderGenerator : IWorkOrderPdfGenerator
{
    private static readonly CultureInfo PtBr = CultureInfo.GetCultureInfo("pt-BR");

    static QuestPdfWorkOrderGenerator()
    {
        QuestPDF.Settings.License = LicenseType.Community;
    }

    public byte[] Generate(WorkOrderPdfData data)
    {
        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(2, Unit.Centimetre);
                page.DefaultTextStyle(style => style.FontSize(10));

                page.Header().Element(c => ComposeHeader(c, data));
                page.Content().Element(c => ComposeContent(c, data));
                page.Footer().AlignCenter().Text(text =>
                {
                    text.Span("Página ");
                    text.CurrentPageNumber();
                    text.Span(" de ");
                    text.TotalPages();
                });
            });
        });

        return document.GeneratePdf();
    }

    private static void ComposeHeader(IContainer container, WorkOrderPdfData data)
    {
        container.Row(row =>
        {
            row.RelativeItem().Column(column =>
            {
                column.Item().Text(data.CompanyName).FontSize(16).Bold();

                if (!string.IsNullOrWhiteSpace(data.CompanyCnpj))
                {
                    column.Item().Text($"CNPJ: {data.CompanyCnpj}").FontSize(9);
                }

                if (!string.IsNullOrWhiteSpace(data.CompanyPhone))
                {
                    column.Item().Text($"Telefone: {data.CompanyPhone}").FontSize(9);
                }

                if (!string.IsNullOrWhiteSpace(data.CompanyEmail))
                {
                    column.Item().Text($"E-mail: {data.CompanyEmail}").FontSize(9);
                }

                if (!string.IsNullOrWhiteSpace(data.CompanyAddress))
                {
                    column.Item().Text(data.CompanyAddress).FontSize(9);
                }
            });

            // Mesma limitação do PDF de orçamento: só desenha se for um
            // arquivo local existente (sem upload de logo até a FASE 10).
            if (!string.IsNullOrWhiteSpace(data.CompanyLogoPath) && File.Exists(data.CompanyLogoPath))
            {
                row.ConstantItem(80).Image(data.CompanyLogoPath);
            }
        });
    }

    private static void ComposeContent(IContainer container, WorkOrderPdfData data)
    {
        container.PaddingTop(15).Column(column =>
        {
            column.Spacing(10);

            column.Item().Text($"ORDEM DE SERVIÇO Nº {data.Id.ToString()[..8].ToUpperInvariant()}").FontSize(14).Bold();

            column.Item().Row(row =>
            {
                row.RelativeItem().Text($"Data de abertura: {data.CreatedAt.ToString("dd/MM/yyyy", PtBr)}");
                row.RelativeItem().AlignRight().Text(
                    data.ScheduledDate.HasValue
                        ? $"Data prevista: {data.ScheduledDate.Value.ToString("dd/MM/yyyy", PtBr)}"
                        : "Data prevista: a definir");
            });

            if (data.QuoteNumber.HasValue)
            {
                column.Item().Text($"Gerada a partir do orçamento #{data.QuoteNumber.Value.ToString("D4", PtBr)}").FontSize(9);
            }

            column.Item().Text($"Status: {data.Status}").Bold();

            if (!string.IsNullOrWhiteSpace(data.AssignedUserName))
            {
                column.Item().Text($"Responsável: {data.AssignedUserName}");
            }

            column.Item().BorderBottom(1).PaddingBottom(10).Column(customerColumn =>
            {
                customerColumn.Item().Text("Cliente").Bold();
                customerColumn.Item().Text(data.CustomerName);

                if (!string.IsNullOrWhiteSpace(data.CustomerPhone))
                {
                    customerColumn.Item().Text($"Telefone: {data.CustomerPhone}");
                }

                if (!string.IsNullOrWhiteSpace(data.CustomerEmail))
                {
                    customerColumn.Item().Text($"E-mail: {data.CustomerEmail}");
                }

                if (!string.IsNullOrWhiteSpace(data.CustomerAddress))
                {
                    customerColumn.Item().Text($"Endereço: {data.CustomerAddress}");
                }
            });

            column.Item().Element(c => ComposeItemsTable(c, data));

            column.Item().AlignRight().Text($"Total: R$ {data.Total.ToString("N2", PtBr)}").FontSize(13).Bold();

            if (!string.IsNullOrWhiteSpace(data.Notes))
            {
                column.Item().Column(notesColumn =>
                {
                    notesColumn.Item().Text("Observações").Bold();
                    notesColumn.Item().Text(data.Notes);
                });
            }

            // Mesmo bloco em destaque do PDF de orçamento — ver comentário lá.
            if (!string.IsNullOrWhiteSpace(data.TechnicalObservations))
            {
                column.Item().Background(Colors.Yellow.Lighten4).Padding(10).Column(obsColumn =>
                {
                    obsColumn.Item().Text("Observações / Ressalvas Técnicas:").Bold();
                    obsColumn.Item().Text(data.TechnicalObservations);
                });
            }

            column.Item().PaddingTop(30).Row(signatureRow =>
            {
                signatureRow.RelativeItem().Column(c =>
                {
                    c.Item().LineHorizontal(1);
                    c.Item().AlignCenter().Text("Assinatura da Empresa").FontSize(9);
                });

                signatureRow.ConstantItem(30);

                signatureRow.RelativeItem().Column(c =>
                {
                    c.Item().LineHorizontal(1);

                    var clientCaption = !string.IsNullOrWhiteSpace(data.TechnicalObservations)
                        ? "Declaro estar ciente dos serviços realizados e das ressalvas acima descritas."
                        : "Assinatura do Cliente";

                    c.Item().AlignCenter().Text(clientCaption).FontSize(9);
                });
            });
        });
    }

    private static void ComposeItemsTable(IContainer container, WorkOrderPdfData data)
    {
        container.Table(table =>
        {
            table.ColumnsDefinition(columns =>
            {
                columns.RelativeColumn(5);
                columns.RelativeColumn(1);
                columns.RelativeColumn(2);
                columns.RelativeColumn(2);
            });

            table.Header(header =>
            {
                header.Cell().Text("Descrição").Bold();
                header.Cell().Text("Qtd.").Bold();
                header.Cell().Text("Preço unit.").Bold();
                header.Cell().Text("Total").Bold();
                header.Cell().ColumnSpan(4).PaddingTop(3).BorderBottom(1);
            });

            foreach (var item in data.Items)
            {
                table.Cell().Text(item.Description);
                table.Cell().Text(item.Quantity.ToString("0.##", PtBr));
                table.Cell().Text($"R$ {item.UnitPrice.ToString("N2", PtBr)}");
                table.Cell().Text($"R$ {item.LineTotal.ToString("N2", PtBr)}");
            }
        });
    }
}
