using System.Globalization;
using OrcaFacil.Application.Interfaces;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace OrcaFacil.Infrastructure.Pdf;

public class QuestPdfQuoteGenerator : IQuotePdfGenerator
{
    // Mesma razão do WhatsAppMessageBuilder: formatação explícita em pt-BR,
    // para não depender da cultura do sistema operacional onde a API roda.
    private static readonly CultureInfo PtBr = CultureInfo.GetCultureInfo("pt-BR");

    static QuestPdfQuoteGenerator()
    {
        // Licença Community: gratuita para empresas com menos de US$ 1 milhão
        // de receita bruta anual. Ver README (seção FASE 7) antes de usar
        // isto num produto comercial em maior escala.
        QuestPDF.Settings.License = LicenseType.Community;
    }

    public byte[] Generate(QuotePdfData data)
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

    private static void ComposeHeader(IContainer container, QuotePdfData data)
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

            // Só tenta desenhar o logo se for um arquivo local existente —
            // não há upload de logo implementado ainda (isso é FASE 10), então
            // por ora LogoUrl só funciona se apontar para um caminho no disco
            // do servidor. Sem isso, o cabeçalho segue só com texto.
            if (!string.IsNullOrWhiteSpace(data.CompanyLogoPath) && File.Exists(data.CompanyLogoPath))
            {
                row.ConstantItem(80).Image(data.CompanyLogoPath);
            }
        });
    }

    private static void ComposeContent(IContainer container, QuotePdfData data)
    {
        container.PaddingTop(15).Column(column =>
        {
            column.Spacing(10);

            column.Item().Text($"ORÇAMENTO Nº {data.Number.ToString("D4", PtBr)}").FontSize(14).Bold();

            column.Item().Row(row =>
            {
                row.RelativeItem().Text($"Data de emissão: {data.IssueDate.ToString("dd/MM/yyyy", PtBr)}");
                row.RelativeItem().AlignRight().Text($"Válido até: {data.ValidUntil.ToString("dd/MM/yyyy", PtBr)}");
            });

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

            column.Item().AlignRight().Column(totalsColumn =>
            {
                totalsColumn.Item().Text($"Subtotal: R$ {data.Subtotal.ToString("N2", PtBr)}");
                totalsColumn.Item().Text($"Desconto: R$ {data.DiscountAmount.ToString("N2", PtBr)}");
                totalsColumn.Item().Text($"Total: R$ {data.Total.ToString("N2", PtBr)}").FontSize(13).Bold();
            });

            if (!string.IsNullOrWhiteSpace(data.Notes))
            {
                column.Item().Column(notesColumn =>
                {
                    notesColumn.Item().Text("Observações").Bold();
                    notesColumn.Item().Text(data.Notes);
                });
            }

            column.Item().Text(
                    "Condições: os valores apresentados estão sujeitos a alteração após a data de validade " +
                    "informada acima. A aprovação deste orçamento implica concordância com os valores e " +
                    "condições aqui descritos.")
                .FontSize(8).Italic();

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
                    c.Item().AlignCenter().Text("Assinatura do Cliente").FontSize(9);
                });
            });
        });
    }

    private static void ComposeItemsTable(IContainer container, QuotePdfData data)
    {
        container.Table(table =>
        {
            table.ColumnsDefinition(columns =>
            {
                columns.RelativeColumn(4);
                columns.RelativeColumn(1);
                columns.RelativeColumn(2);
                columns.RelativeColumn(2);
                columns.RelativeColumn(2);
            });

            table.Header(header =>
            {
                header.Cell().Text("Descrição").Bold();
                header.Cell().Text("Qtd.").Bold();
                header.Cell().Text("Preço unit.").Bold();
                header.Cell().Text("Desconto").Bold();
                header.Cell().Text("Total").Bold();
                header.Cell().ColumnSpan(5).PaddingTop(3).BorderBottom(1);
            });

            foreach (var item in data.Items)
            {
                table.Cell().Text(item.Description);
                table.Cell().Text(item.Quantity.ToString("0.##", PtBr));
                table.Cell().Text($"R$ {item.UnitPrice.ToString("N2", PtBr)}");
                table.Cell().Text($"R$ {item.DiscountAmount.ToString("N2", PtBr)}");
                table.Cell().Text($"R$ {item.LineTotal.ToString("N2", PtBr)}");
            }
        });
    }
}
