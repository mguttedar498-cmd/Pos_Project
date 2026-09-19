using Azure;
using HMS_360_PMS.EntitiesModels.KOT;
using HMS_360_PMS.HMS_360_PMS.EntitiesModels.POS;
using HMS_360_PMS.HMS_360_PMS.ServiceLayer.POS.DTOs;
using HMS_360_PMS.HMS_360_PMS.ServiceLayer.POS.Interfaces;
using HMS_360_PMS.ProjectEntitiesModels.KOT;
using HMS_360_PMS.ProjectServiceLayer.KOT.Interfaces;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace HMS_360_PMS.ProjectServiceLayer.GeneralSettings.Services
{
    public class EmailSender_Service
    {
        public readonly IBasicSettingsManager _basicsettingManager;
        private readonly IPOSReports_Service _reportservice;

        public EmailSender_Service(IBasicSettingsManager basicsettingManager, IPOSReports_Service Service)
        {
            _basicsettingManager = basicsettingManager;
            _reportservice = Service;
        }

        public async Task<bool> SendEmailDailyClose(CloseDayRequest request, string branchName)
        {
            try
            {
                var fromDate = request.POSEntryDate.Date;
                var toDate = request.SystemTime.Date;
                var Outlet = "All";
                var branchcode = request.BranchCode;

                var dailySales = await _reportservice.GetDailySales(fromDate, toDate, Outlet);
                var dailySalesReport = BuildDailySalesReport(dailySales);
                byte[] pdf1 = GeneratePdf(dailySalesReport, fromDate, toDate);

                var chanceSheet = await _reportservice.GetChancesheet(fromDate, toDate, Outlet, branchcode);
                var chanceSheetReport = BuildChanceSheetReport(chanceSheet, branchName);
                byte[] pdf2 = GenerateChanceSheetPDF(chanceSheetReport, fromDate, toDate);

                var EmailSenderData = await _basicsettingManager.GetEmailSenderData();
                var smtpHost = EmailSenderData.Host;
                int smtpPort = EmailSenderData.Port;
                var senderEmail = EmailSenderData.SenderEmail;
                var senderName = EmailSenderData.SenderName;
                var securityType = EmailSenderData.SecurityType?.Trim();
                var appPassword = EmailSenderData.Password?.Replace(" ", "").Trim();
                var EmailReceiverData = await _basicsettingManager.GetEmailReceiverData();
                //var recipientEmail = EmailReceiverData?.EmailId;

                var message = new MimeMessage();
                message.From.Add(new MailboxAddress(senderName ?? "Support", senderEmail));
                //message.To.Add(new MailboxAddress("Recipient Client", "cogwavekarthik@gmail.com" ?? " "));
                message.Subject = $"HMS 360 PMS | Daily Sales Report and Chance Sheet Report for {branchName} Branch | {fromDate:dd-MM-yyyy}";

                foreach (var cc in EmailReceiverData)
                {
                    if (!string.IsNullOrWhiteSpace(cc.EmailId))
                    {
                        message.To.Add(MailboxAddress.Parse(cc.EmailId.Trim()));
                    }
                }

                var bodyBuilder = new BodyBuilder();
                bodyBuilder.HtmlBody = $@" <p>Dear Valued Customer,</p> 
                <p>Greetings from <b>HMS 360 PMS</b>.</p> 
                <p>Please find attached your <b>Daily Sales Report</b> and <b>Chance Sheet Report</b> for {branchcode} the period <b>{fromDate:dd/MM/yyyy} - {toDate:dd/MM/yyyy}</b>.</p> 
                <p>The attached reports contain key business insights, including sales, billing, tax, settlement, and operational details to help you monitor your daily performance.</p> 
                <p>Please review the attached reports at your convenience. Feel free to contact us if you need any assistance.</p> 
                <p>Thanks & Regards,<br/> 
                <b>HMS 360 PMS Support Team</b></p>";
                bodyBuilder.Attachments.Add($"DailySales_{branchName}.pdf", pdf1, ContentType.Parse("application/pdf"));
                bodyBuilder.Attachments.Add($"ChanceSheet_{branchName}.pdf", pdf2, ContentType.Parse("application/pdf"));
                message.Body = bodyBuilder.ToMessageBody();
                using (var client = new SmtpClient())
                {
                    if (!Enum.TryParse<SecureSocketOptions>(securityType, true, out var socketOptions))
                    {
                        socketOptions = SecureSocketOptions.Auto;
                    }
                    await client.ConnectAsync(smtpHost, smtpPort, socketOptions);
                    await client.AuthenticateAsync(senderEmail, appPassword);
                    await client.SendAsync(message);
                    await client.DisconnectAsync(true);
                    //Console.WriteLine(response);

                }
                
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return false;
            }
        }

        public async Task<bool> SentEmailNotfication(EmailNotficationModel request)
        {
            try
            {
                var reportTypes = request.ReportType?.Split(',').Select(x => x.Replace(" ", "").Trim().ToLower()).ToList();
                if (reportTypes == null || !reportTypes.Any())
                {
                    return false;
                }
                var bodyBuilder = new BodyBuilder();
                bodyBuilder.HtmlBody = request.EmailBody;

                foreach (var reportType in reportTypes)
                {
                    byte[] pdfBytes = null;
                    PdfReportModel? report = null;

                    if (reportType == "dailysales")
                    {
                        if (request.DailySalesData == null || !request.DailySalesData.FromDate.HasValue || !request.DailySalesData.ToDate.HasValue) continue;
                        var fromDate = request.DailySalesData.FromDate.Value.Date;
                        var toDate = request.DailySalesData.ToDate.Value.Date;
                        var outlet = request.DailySalesData.Outlet;

                        var result = await _reportservice.GetDailySales(fromDate, toDate, outlet);
                        report = BuildDailySalesReport(result);
                        pdfBytes = GeneratePdf(report, fromDate, toDate);
                    }
                    //else if (reportType == "chancesheet")
                    //{
                    //    if (request.ChanceSheetData == null || !request.ChanceSheetData.FromDate.HasValue || !request.ChanceSheetData.ToDate.HasValue) continue;
                    //    var fromDate = request.ChanceSheetData.FromDate.Value.Date;
                    //    var toDate = request.ChanceSheetData.ToDate.Value.Date;
                    //    var outlet = request.ChanceSheetData.Outlet;

                    //    var result = await _reportservice.GetChancesheet(fromDate, toDate, outlet, request.Branchcode);
                    //    report = BuildChanceSheetReport(result);
                    //    pdfBytes = GenerateChanceSheetPDF(report, fromDate, toDate);
                    //}
                    else if (reportType == "voidkot")
                    {
                        if (request.VoidKotModelData == null || !request.VoidKotModelData.FromDate.HasValue || !request.VoidKotModelData.ToDate.HasValue) continue;
                        var fromDate = request.VoidKotModelData.FromDate.Value.Date;
                        var toDate = request.VoidKotModelData.ToDate.Value.Date;
                        var outlet = request.VoidKotModelData.Outlet;

                        var result = await _reportservice.GetVoidData(fromDate, toDate, outlet);
                        report = BuildVoidKotReport(result);
                        pdfBytes = GeneratePdf(report, fromDate, toDate);
                    }
                    else if (reportType == "nckot")
                    {
                        if (request.NCKotModelData == null || !request.NCKotModelData.FromDate.HasValue || !request.NCKotModelData.ToDate.HasValue) continue;
                        var fromDate = request.NCKotModelData.FromDate.Value.Date;
                        var toDate = request.NCKotModelData.ToDate.Value.Date;
                        var outlet = request.NCKotModelData.Outlet;

                        var result = await _reportservice.GetNCData(fromDate, toDate, outlet);
                        report = BuildNCKotReport(result);
                        pdfBytes = GeneratePdf(report, fromDate, toDate);
                    }
                    else if (reportType == "getitemsales")
                    {
                        if (request.GroupedItemSalesData == null || !request.GroupedItemSalesData.FromDate.HasValue || !request.GroupedItemSalesData.ToDate.HasValue) continue;
                        var fromDate = request.GroupedItemSalesData.FromDate.Value.Date;
                        var toDate = request.GroupedItemSalesData.ToDate.Value.Date;
                        var outlet = request.GroupedItemSalesData.Outlet;

                        var result = await _reportservice.GetItemSales(fromDate, toDate, outlet);
                        report = BuildGroupedItemSalesReport(result);
                        pdfBytes = GeneratePdf(report, fromDate, toDate);
                    }
                    else if (reportType == "kotcancellation")
                    {
                        if (request.KotCancellationModelData == null) continue;
                        var result = await _reportservice.GetKotCancellation(request.KotCancellationModelData);
                        report = BuildKotCancelReport(result);
                        pdfBytes = GeneratePdf(report, request.KotCancellationModelData.FromDate.Date, request.KotCancellationModelData.ToDate.Date);
                    }
                    else if (reportType == "billcancellation")
                    {
                        if (request.BillCancellationModelData == null) continue;
                        var result = await _reportservice.GetBillCancellation(request.BillCancellationModelData);
                        report = BuildBillCancelReport(result);
                        pdfBytes = GeneratePdf(report, request.BillCancellationModelData.FromDate.Date, request.BillCancellationModelData.ToDate.Date);
                    }
                    else if (reportType == "creditoutstanding")
                    {
                        if (request.CreditOutstandingModelData == null) continue;
                        var result = await _reportservice.GetCreditOutstanding(request.CreditOutstandingModelData);
                        report = BuildCreditOutstandingReport(result);
                        pdfBytes = GeneratePdf(report, request.CreditOutstandingModelData.FromDate.Date, request.CreditOutstandingModelData.ToDate.Date);
                    }
                    else if (reportType == "dailysalecategorywise")
                    {
                        if (request.DailySaleCategorywiseModelData == null) continue;
                        var result = await _reportservice.GetDailysaleCategorywise(request.DailySaleCategorywiseModelData);
                        report = BuildDailySaleCategorywiseReport(result);
                        pdfBytes = GeneratePdf(report, request.DailySaleCategorywiseModelData.FromDate.Date, request.DailySaleCategorywiseModelData.ToDate.Date);
                    }
                    else if (reportType == "korregister")
                    {
                        if (request.KOTRegisterModelData == null) continue;
                        var result = await _reportservice.GetKotRegister(request.KOTRegisterModelData);
                        report = BuildKOTRegisterReport(result);
                        pdfBytes = GeneratePdf(report, request.KOTRegisterModelData.FromDate.Date, request.KOTRegisterModelData.ToDate.Date);
                    }

                    if (pdfBytes != null)
                    {
                        bodyBuilder.Attachments.Add($"{reportType}.pdf", pdfBytes, ContentType.Parse("application/pdf"));
                    }
                }

                var emailSenderData = await _basicsettingManager.GetEmailSenderData();

                var message = new MimeMessage();
                message.From.Add(new MailboxAddress(emailSenderData.SenderName ?? "Support", emailSenderData.SenderEmail));
                message.To.Add(MailboxAddress.Parse(request.ToEmail));

                var ccList = (request.CCEmail ?? "").Split(',');
                foreach (var cc in ccList)
                {
                    if (!string.IsNullOrWhiteSpace(cc))
                    {
                        message.Cc.Add(MailboxAddress.Parse(cc.Trim()));
                    }
                }

                message.Subject = request.Subject;
                message.Body = bodyBuilder.ToMessageBody();

                using (var client = new SmtpClient())
                {
                    if (!Enum.TryParse<SecureSocketOptions>(emailSenderData.SecurityType?.Trim(), true, out var socketOptions))
                    {
                        socketOptions = SecureSocketOptions.Auto;
                    }

                    await client.ConnectAsync(emailSenderData.Host, emailSenderData.Port, socketOptions);
                    await client.AuthenticateAsync(emailSenderData.SenderEmail, emailSenderData.Password?.Trim());
                    await client.SendAsync(message);
                    await client.DisconnectAsync(true);
                }

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return false;
            }
        }

        private byte[] GeneratePdf(PdfReportModel report, DateTime fromDate, DateTime toDate)
        {
            QuestPDF.Settings.License = LicenseType.Community;

            return Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4.Landscape());
                    page.Margin(10);

                    page.Header()
                        .Text($"{report.ReportName} - {fromDate:dd/MM/yyyy} - {toDate:dd/MM/yyyy}")
                        .Bold()
                        .FontSize(16)
                        .AlignCenter();

                    page.Content().Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            for (int i = 0; i < report.Headers.Count; i++)
                                columns.RelativeColumn();
                        });

                        void AddHeader()
                        {
                            foreach (var header in report.Headers)
                            {
                                table.Cell()
                                    .Border(1)
                                    .Padding(3)
                                    .Text(header)
                                    .Bold()
                                    .FontSize(8);
                            }
                        }

                        void AddNormalCell(string text)
                        {
                            table.Cell()
                                .Border(1)
                                .Padding(3)
                                .Text(text ?? "")
                                .FontSize(7);
                        }

                        void AddOutletHeader(string text)
                        {
                            table.Cell()
                                .ColumnSpan((uint)report.Headers.Count)
                                .Border(1)
                                .Padding(4)
                                .Text(text)
                                .Bold()
                                .FontSize(10);
                        }

                        void AddSeparator()
                        {
                            table.Cell()
                                .ColumnSpan((uint)report.Headers.Count)
                                .BorderBottom(1)
                                .Padding(0)
                                .Text("");
                        }

                        void AddTotalRow(List<string> row)
                        {
                            foreach (var cell in row)
                            {
                                table.Cell()
                                    .Border(1)
                                    .Background(Colors.Grey.Lighten2)
                                    .Padding(3)
                                    .Text(cell ?? "")
                                    .Bold()
                                    .FontSize(8);
                            }
                        }

                        foreach (var row in report.Rows)
                        {
                            if (row == null || row.Count == 0)
                                continue;

                            if (row[0].StartsWith("Outlet:"))
                            {
                                AddOutletHeader(row[0]);
                                continue;
                            }

                            if (row[0] == "HEADER_ROW")
                            {
                                AddHeader();
                                continue;
                            }

                            if (row[0] == "SEPARATOR")
                            {
                                AddSeparator();
                                continue;
                            }

                            if (row[0] == "SUB TOTAL")
                            {
                                AddTotalRow(row);
                                continue;
                            }

                            foreach (var cell in row)
                                AddNormalCell(cell);
                        }

                        AddSeparator();

                        if (report.TotalRow != null && report.TotalRow.Count > 0)
                            AddTotalRow(report.TotalRow);
                    });
                });
            }).GeneratePdf();
        }

        private byte[] GenerateChanceSheetPDF(PdfChancesheetReportModel report, DateTime fromDate, DateTime toDate)
        {
            QuestPDF.Settings.License = LicenseType.Community;

            return Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A3.Landscape());
                    page.Margin(10);

                    page.Header()
                        .Text($"{report.ReportName} - {fromDate:dd/MM/yyyy} - {toDate:dd/MM/yyyy}")
                        .Bold()
                        .FontSize(16)
                        .AlignCenter();

                    page.Content().Column(column =>
                    {
                        column.Item().Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                for (int i = 0; i < report.Headers.Count; i++)
                                    columns.RelativeColumn();
                            });

                            void AddHeader()
                            {
                                foreach (var header in report.Headers)
                                {
                                    table.Cell()
                                        .Border(1)
                                        .Background(Colors.LightBlue.Medium)
                                        .Padding(3)
                                        .Text(header)
                                        .Bold()
                                        .FontSize(8);
                                }
                            }

                            void AddNormalCell(string text)
                            {
                                table.Cell()
                                    .Border(1)
                                    .Padding(3)
                                    .Text(text ?? "")
                                    .FontSize(7);
                            }

                            void AddOutletHeader(string text)
                            {
                                table.Cell()
                                    .ColumnSpan((uint)report.Headers.Count)
                                    .Border(1)
                                    .Padding(4)
                                    .Text(text)
                                    .Bold()
                                    .FontSize(10);
                            }

                            void AddSeparator()
                            {
                                table.Cell()
                                    .ColumnSpan((uint)report.Headers.Count)
                                    .BorderBottom(1)
                                    .Padding(0)
                                    .Text("");
                            }

                            void AddTotalRow(List<string> row)
                            {
                                foreach (var cell in row)
                                {
                                    table.Cell()
                                        .Border(1)
                                        .Background(Colors.LightGreen.Accent1)
                                        .Padding(3)
                                        .Text(cell ?? "")
                                        .Bold()
                                        .FontSize(8);
                                }
                            }

                            foreach (var row in report.Rows)
                            {
                                if (row == null || row.Count == 0)
                                    continue;

                                if (row[0].StartsWith("Outlet:"))
                                {
                                    AddOutletHeader(row[0]);
                                    continue;
                                }

                                if (row[0] == "HEADER_ROW")
                                {
                                    AddHeader();
                                    continue;
                                }

                                if (row[0] == "SEPARATOR")
                                {
                                    AddSeparator();
                                    continue;
                                }

                                if (row[0] == "SUB TOTAL")
                                {
                                    AddTotalRow(row);
                                    continue;
                                }

                                foreach (var cell in row)
                                    AddNormalCell(cell);
                            }
                        });

                        column.Item().PaddingTop(20);

                        column.Item()
                            .PaddingTop(10)
                            .Border(1)
                            .Padding(10)
                            .Column(c =>
                            {
                                c.Item().Text("Overall Total")
                                    .Bold()
                                    .FontSize(14);

                                c.Item().Table(table =>
                                {
                                    table.ColumnsDefinition(columns =>
                                    {
                                        columns.RelativeColumn(3);
                                        columns.RelativeColumn();
                                    });

                                    // Data
                                    table.Cell().Border(1).Padding(3).Text("Tax");
                                    table.Cell().Border(1).Padding(3).AlignRight().Text(report.OverallSummary.Tax.ToString("N2"));

                                    table.Cell().Border(1).Padding(3).Text("CGST");
                                    table.Cell().Border(1).Padding(3).AlignRight().Text(report.OverallSummary.CGST.ToString("N2"));

                                    table.Cell().Border(1).Padding(3).Text("SGST");
                                    table.Cell().Border(1).Padding(3).AlignRight().Text(report.OverallSummary.SGST.ToString("N2"));

                                    table.Cell().Border(1).Padding(3).Text("Total");
                                    table.Cell().Border(1).Padding(3).AlignRight().Text(report.OverallSummary.Total.ToString("N2"));

                                    table.Cell().Border(1).Padding(3).Text("Discount");
                                    table.Cell().Border(1).Padding(3).AlignRight().Text(report.OverallSummary.Discount.ToString("N2"));

                                    table.Cell().Border(1).Padding(3).Text("Grand");
                                    table.Cell().Border(1).Padding(3).AlignRight().Text(report.OverallSummary.Grand.ToString("N2"));

                                    //table.Cell().Border(1).Padding(3).Text("Cash");
                                    //table.Cell().Border(1).Padding(3).AlignRight().Text(report.OverallSummary.Cash.ToString("N2"));

                                    //table.Cell().Border(1).Padding(3).Text("Card");
                                    //table.Cell().Border(1).Padding(3).AlignRight().Text(report.OverallSummary.Card.ToString("N2"));

                                    //table.Cell().Border(1).Padding(3).Text("Online");
                                    //table.Cell().Border(1).Padding(3).AlignRight().Text(report.OverallSummary.Online.ToString("N2"));

                                    ////c.Item().Text($"Sale : {report.OverallSummary.Sale}");
                                    //c.Item().Text($"Tax : {report.OverallSummary.Tax}");
                                    //c.Item().Text($"CGST : {report.OverallSummary.CGST}");
                                    //c.Item().Text($"SGST : {report.OverallSummary.SGST}");
                                    //c.Item().Text($"Grand : {report.OverallSummary.Grand}");
                                    //c.Item().Text($"Cash : {report.OverallSummary.Cash}");
                                    //c.Item().Text($"Card : {report.OverallSummary.Card}");
                                    //c.Item().Text($"Online : {report.OverallSummary.Online}");
                                });
                            });

                        column.Item()
                         .PaddingTop(10)
                         .Border(1)
                         .Padding(10)
                         .Column(c =>
                         {
                             c.Item().Text("OutletWise Summary")
                                 .Bold()
                                 .FontSize(14);

                             c.Item().Table(table =>
                             {
                                 table.ColumnsDefinition(columns =>
                                 {
                                     columns.RelativeColumn(3);
                                     columns.RelativeColumn();
                                 });

                                 // Header
                                 table.Cell().Border(1).Padding(4).Text("Outlet").Bold();
                                 table.Cell().Border(1).Padding(4).AlignRight().Text("Amount").Bold();

                                 // Data
                                 foreach (var outlet in report.OutletWiseSummary)
                                 {
                                     table.Cell().Border(1).Padding(3).Text(outlet.OutletName);
                                     table.Cell().Border(1).Padding(3).AlignRight().Text(outlet.TotalAmount.ToString("N2"));
                                 }
                             });
                         });

                        column.Item()
                            .PaddingTop(10)
                            .Border(1)
                            .Padding(10)
                            .Column(c =>
                            {
                                c.Item().Text("Remarks Summary")
                                    .Bold()
                                    .FontSize(14);

                                c.Item().Table(table =>
                                {
                                    table.ColumnsDefinition(columns =>
                                    {
                                        columns.RelativeColumn(3);
                                        columns.RelativeColumn();
                                    });

                                    // Header
                                    table.Cell().Border(1).Padding(4).Text("Particulars").Bold();
                                    table.Cell().Border(1).Padding(4).AlignRight().Text("Amount").Bold();

                                    // Data
                                    foreach (var item in report.RemarksSummary)
                                    {
                                        table.Cell().Border(1).Padding(3).Text(item.Particulars);
                                        table.Cell().Border(1).Padding(3).AlignRight().Text(item.Amount.ToString("N2"));
                                    }
                                });
                            });

                    });
                });
            }).GeneratePdf();
        }

        private PdfReportModel BuildDailySalesReport(List<DailySalesModel> data)
        {
            var report = new PdfReportModel
            {
                ReportName = "Daily Sales Report",
                Headers = new List<string>
                {
                    "BillNo", "BillDate", "BillTime", "TableNo",
                    "BillAmount", "Discount", "Tax",
                    "RoundOff", "CGST", "SGST", "Total"
                }
            };

            var groupedData = data.GroupBy(x => x.OltName);

            foreach (var group in groupedData)
            {
                // Outlet Heading
                report.Rows.Add(new List<string> { $"Outlet: {group.Key}" });

                // Header marker
                report.Rows.Add(new List<string> { "HEADER_ROW" });

                // Data rows
                foreach (var item in group)
                {
                    report.Rows.Add(new List<string>
                    {
                        item.BillNo,
                        item.BillDate.ToString("dd/MM/yyyy"),
                        item.BillTime?.ToString() ?? "",
                        item.TableNo,
                        item.BillAmount.ToString("0.00"),
                        item.Discount.ToString("0.00"),
                        item.Tax.ToString("0.00"),
                        item.RoundOff.ToString("0.00"),
                        item.CGST.ToString("0.00"),
                        item.SGST.ToString("0.00"),
                        item.Total.ToString("0.00")
                    });
                }

                // Subtotal (NO separator before this)
                report.Rows.Add(new List<string>
                {
                    "SUB TOTAL","","","",
                    group.Sum(x => x.BillAmount).ToString("0.00"),
                    group.Sum(x => x.Discount).ToString("0.00"),
                    group.Sum(x => x.Tax).ToString("0.00"),
                    group.Sum(x => x.RoundOff).ToString("0.00"),
                    group.Sum(x => x.CGST).ToString("0.00"),
                    group.Sum(x => x.SGST).ToString("0.00"),
                    group.Sum(x => x.Total).ToString("0.00")
                });

                // Separator AFTER subtotal only
                report.Rows.Add(new List<string> { "SEPARATOR" });
            }

            // Grand Total
            report.TotalRow = new List<string>
            {
                "GRAND TOTAL","","","",
                data.Sum(x => x.BillAmount).ToString("0.00"),
                data.Sum(x => x.Discount).ToString("0.00"),
                data.Sum(x => x.Tax).ToString("0.00"),
                data.Sum(x => x.RoundOff).ToString("0.00"),
                data.Sum(x => x.CGST).ToString("0.00"),
                data.Sum(x => x.SGST).ToString("0.00"),
                data.Sum(x => x.Total).ToString("0.00")
            };

            return report;
        }

        private PdfChancesheetReportModel BuildChanceSheetReport(ChanceSheetResponse data, string branchName)
        {
            var report = new PdfChancesheetReportModel
            {
                ReportName = $"Chance Sheet Report for {branchName} Branch",
                Headers = new List<string>
                {
                    "BillNo",
                    "ItemSale",
                    "CGST",
                    "SGST",
                    "Total",
                    "Discount",
                    "Net Amt", 
                    "Cash",
                    "Card",
                    "UPI",
                    "Online",
                    "NEFT",
                    "Pluxee",
                    "Cheque",
                    "Credit",
                    "Room",
                    "KBSRefName"
                    //"BillDate",
                    //"BillTime",
                    //"Tax",
                    //"RoundOff",
                }
            };

            var groupedData = data.Data.GroupBy(x => x.OltName);

            foreach (var group in groupedData)
            {
                // Outlet heading
                report.Rows.Add(new List<string> { $"Outlet: {group.Key}" });

                // Header marker
                report.Rows.Add(new List<string> { "HEADER_ROW" });

                // Data rows
                foreach (var item in group)
                {
                    report.Rows.Add(new List<string>
                    {
                        item.BillNo,
                        item.ItemSale.ToString("0.00"),
                        item.CGST.ToString("0.00"),
                        item.SGST.ToString("0.00"),
                        item.Total.ToString("0.00"),
                        item.Dis.ToString("0.00"),
                        item.Grand.ToString("0.00"),
                        item.Cash.ToString("0.00"),
                        item.Card.ToString("0.00"),
                        item.UPI.ToString("0.00"),
                        item.Online.ToString("0.00"),
                        item.NEFT.ToString("0.00"),
                        item.Pluxee.ToString("0.00"),
                        item.Cheque.ToString("0.00"),
                        item.Credit.ToString("0.00"),
                        item.Room.ToString("0"),
                        item.KBSRefName

                        //item.Date,
                        //item.BillTime.ToString() ?? "",
                        //item.Tax.ToString("0.00"),
                        //item.RoundOff.ToString("0.00"),
                    });
                }

                // Subtotal per outlet
                report.Rows.Add(new List<string>
                {
                    "SUB TOTAL",
                    group.Sum(x => x.ItemSale).ToString("0.00"),
                    group.Sum(x => x.CGST).ToString("0.00"),
                    group.Sum(x => x.SGST).ToString("0.00"),
                    group.Sum(x => x.Total).ToString("0.00"),
                    group.Sum(x => x.Dis).ToString("0.00"),
                    group.Sum(x => x.Grand).ToString("0.00"),
                    group.Sum(x => x.Cash).ToString("0.00"),
                    group.Sum(x => x.Card).ToString("0.00"),
                    group.Sum(x => x.UPI).ToString("0.00"),
                    group.Sum(x => x.Online).ToString("0.00"),
                    group.Sum(x => x.NEFT).ToString("0.00"),
                    group.Sum(x => x.Pluxee).ToString("0.00"),
                    group.Sum(x => x.Cheque).ToString("0.00"),
                    group.Sum(x => x.Credit).ToString("0.00"),
                    group.Sum(x => x.Room).ToString("0.00"),
                    ""
                });

                report.Rows.Add(new List<string> { "SEPARATOR" });
            }

            // Overall Summary
            report.OverallSummary = data.Summary;

            // Remarks Summary
            report.RemarksSummary = data.RemarksSummary;

            // Outlet Wise Summary
            report.OutletWiseSummary = data.OutletWiseSummary;

            return report;
        }

        private PdfReportModel BuildVoidKotReport(List<VoidKotModel> data)
        {
            var report = new PdfReportModel
            {
                ReportName = "Void KOT Report",
                Headers = new List<string>
                {
                    "KOTNO",
                    "KOTDate",
                    "KOTTime",
                    "ItemName",
                    "ItemCode",
                    "ItemQty",
                    "CancelQty",
                    "CancelRate",
                    "RefKotNo",
                    "Remarks"
                }
            };

            var groupedData = data.GroupBy(x => x.OltName);

            foreach (var group in groupedData)
            {
                // Outlet Heading
                report.Rows.Add(new List<string>
                {
                    $"Outlet: {group.Key}"
                });

                // Header marker
                report.Rows.Add(new List<string>
                {
                    "HEADER_ROW"
                });

                // Data rows
                foreach (var item in group)
                {
                    report.Rows.Add(new List<string>
                    {
                        item.KOTNO.ToString(),
                        item.KOTDate.ToString("dd/MM/yyyy"),
                        item.KOTTime,
                        item.ItemName,
                        item.ItemCode,
                        item.ItemQty.ToString(),
                        item.CancelQty.ToString(),
                        item.CancelRate.ToString("0.00"),
                        item.RefKotNo,
                        item.Remarks
                    });
                }

                // Subtotal (only CancelRate)
                report.Rows.Add(new List<string>
        {
            "SUB TOTAL",   // KOTNO
            "",            // KOTDate
            "",            // KOTTime
            "",            // ItemName
            "",            // ItemCode
            "",            // ItemQty
            "",            // CancelQty
            group.Sum(x => x.CancelRate).ToString("0.00"), // CancelRate
            "",            // RefKotNo
            ""             // Remarks
        });

                report.Rows.Add(new List<string> { "SEPARATOR" });
            }

            // Grand Total (only CancelRate)
            report.TotalRow = new List<string>
    {
        "GRAND TOTAL",
        "",
        "",
        "",
        "",
        "",
        "",
        data.Sum(x => x.CancelRate).ToString("0.00"),
        "",
        ""
    };

            return report;
        }

        private PdfReportModel BuildNCKotReport(List<NCKotModel> data)
        {
            var report = new PdfReportModel
            {
                ReportName = "NC KOT Report",
                Headers = new List<string>
        {
            "KOTNO",
            "KOTDate",
            "KOTTime",
            "ItemName",
            "KOTDQty",
            "NCKOT_Particulars",
            "KOTDRate",
            "NCDepName"
        }
            };

            var groupedData = data.GroupBy(x => x.OltName);

            foreach (var group in groupedData)
            {
                // Outlet Heading
                report.Rows.Add(new List<string>
        {
            $"Outlet: {group.Key}"
        });

                // Header marker
                report.Rows.Add(new List<string>
        {
            "HEADER_ROW"
        });

                // Data rows
                foreach (var item in group)
                {
                    report.Rows.Add(new List<string>
            {
                item.KOTNO.ToString(),
                item.KOTDate.ToString("dd/MM/yyyy"),
                item.KOTTime.ToString("HH:mm:ss"),
                item.ItemName,
                item.KOTDQty.ToString(),
                item.NCKOT_Particulars,
                item.KOTDRate.ToString("0.00"),
                item.NCDepName
            });
                }

                // Sub Total (only KOTDRate)
                report.Rows.Add(new List<string>
        {
            "SUB TOTAL",   // KOTNO
            "",            // KOTDate
            "",            // KOTTime
            "",            // ItemName
            "",            // KOTDQty
            "",            // NCKOT_Particulars
            group.Sum(x => x.KOTDRate).ToString("0.00"), // KOTDRate
            ""             // NCDepName
        });

                report.Rows.Add(new List<string> { "SEPARATOR" });
            }

            // Grand Total (only KOTDRate)
            report.TotalRow = new List<string>
    {
        "GRAND TOTAL",
        "",
        "",
        "",
        "",
        "",
        data.Sum(x => x.KOTDRate).ToString("0.00"),
        ""
    };

            return report;
        }

        private PdfReportModel BuildGroupedItemSalesReport(List<GroupedItemSalesResponse> data)
        {
            var report = new PdfReportModel
            {
                ReportName = "Grouped Item Sales Report",
                Headers = new List<string>
        {
            "Item Name",
            "Rate",
            "Quantity",
            "Total",
            "Outlet Name"
        }
            };

            foreach (var group in data)
            {
                // Group heading (must match 5 columns)
                report.Rows.Add(new List<string>
        {
            $"Group: {group.GroupName}",
            "",
            "",
            "",
            ""
        });

                // Header marker
                report.Rows.Add(new List<string> { "HEADER_ROW" });

                // Data rows
                foreach (var item in group.Items)
                {
                    report.Rows.Add(new List<string>
            {
                item.ItemName,
                item.Rate.ToString("0.00"),
                item.Quantity.ToString("0.##"),
                item.Total.ToString("0.00"),
                item.OutletName
            });
                }

                // Subtotal row (5 columns)
                report.Rows.Add(new List<string>
        {
            "SUB TOTAL",
            "",
            group.Items.Sum(x => x.Quantity).ToString("0.##"),
            group.Items.Sum(x => x.Total).ToString("0.00"),
            ""
        });

                // Separator row
                report.Rows.Add(new List<string>
        {
            "SEPARATOR",
            "",
            "",
            "",
            ""
        });
            }

            // Grand Total
            var allItems = data.SelectMany(x => x.Items).ToList();

            report.TotalRow = new List<string>
    {
        "GRAND TOTAL",
        "",
        allItems.Sum(x => x.Quantity).ToString("0.##"),
        allItems.Sum(x => x.Total).ToString("0.00"),
        ""
    };

            return report;
        }

        private PdfReportModel BuildKotCancelReport(List<KotCancelReportDto> data)
        {
            var report = new PdfReportModel
            {
                ReportName = "KOT Cancel Report",
                Headers = new List<string>
        {
            "KotNo", "KotDate", "KotTime", "TotalAmount"
        }
            };

            var groupedData = data.GroupBy(x => x.Outlet);

            foreach (var group in groupedData)
            {
                // Outlet heading
                report.Rows.Add(new List<string> { $"Outlet: {group.Key}" });

                // Header marker
                report.Rows.Add(new List<string> { "HEADER_ROW" });

                // Data rows
                foreach (var item in group)
                {
                    report.Rows.Add(new List<string>
            {
                item.KotNo,
                item.KotDate.ToString("dd/MM/yyyy"),
                item.KotTime ?? "",
                item.TotalAmount.ToString("0.00")
            });
                }

                // Sub Total
                report.Rows.Add(new List<string>
        {
            "SUB TOTAL", "", "",
            group.Sum(x => x.TotalAmount).ToString("0.00")
        });

                // Separator
                report.Rows.Add(new List<string> { "SEPARATOR" });
            }

            // Grand Total
            report.TotalRow = new List<string>
    {
        "GRAND TOTAL", "", "",
        data.Sum(x => x.TotalAmount).ToString("0.00")
    };

            return report;
        }

        private PdfReportModel BuildBillCancelReport(List<BillCancelReportDto> data)
        {
            var report = new PdfReportModel
            {
                ReportName = "Bill Cancel Report",
                Headers = new List<string>
        {
            "Bill No",
            "Bill Date",
            "Total Amount",
            "Reason"
        }
            };

            // Header marker
            report.Rows.Add(new List<string> { "HEADER_ROW" });

            // Data rows
            foreach (var item in data)
            {
                report.Rows.Add(new List<string>
        {
            item.KSMBillNo,
            item.KSMBillDate.ToString("dd/MM/yyyy"),
            item.TotalAmount.ToString("0.00"),
            item.Reason
        });
            }

            // Grand Total
            report.TotalRow = new List<string>
    {
        "GRAND TOTAL",
        "",
        data.Sum(x => x.TotalAmount).ToString("0.00"),
        ""
    };

            return report;
        }

        private PdfReportModel BuildCreditOutstandingReport(List<CreditOutstandingReportDto> data)
        {
            var report = new PdfReportModel
            {
                ReportName = "Credit Outstanding Report",
                Headers = new List<string>
        {
            "Bill No",
            "Bill Date",
            "Total Amount",
            "IP Address",
            "Department",
            "Guest Name"
        }
            };

            // Header marker
            report.Rows.Add(new List<string> { "HEADER_ROW" });

            // Data rows
            foreach (var item in data)
            {
                report.Rows.Add(new List<string>
        {
            item.BillNo,
            item.BillDate.ToString("dd/MM/yyyy"),
            item.TotalAmount.ToString("0.00"),
            item.IpAddress,
            item.Department,
            item.GuestName
        });
            }

            // Grand Total
            report.TotalRow = new List<string>
    {
        "GRAND TOTAL",
        "",
        data.Sum(x => x.TotalAmount).ToString("0.00"),
        "",
        "",
        ""
    };

            return report;
        }

        private PdfReportModel BuildDailySaleCategorywiseReport(DailySaleCategorywiseResponseDto data)
        {
            var report = new PdfReportModel
            {
                ReportName = "Daily Sale Categorywise Report",
                Headers = new List<string>
        {
            "Item Name",
            "Quantity",
            "Rate",
            "Category"
        }
            };

            var groupedData = data.Items.GroupBy(x => x.Category);

            foreach (var group in groupedData)
            {
                // Category Heading
                report.Rows.Add(new List<string>
        {
            $"Category: {group.Key}",
            "",
            "",
            ""
        });

                // Header marker
                report.Rows.Add(new List<string> { "HEADER_ROW" });

                // Data rows
                foreach (var item in group)
                {
                    report.Rows.Add(new List<string>
            {
                item.ItemName,
                item.Qty.ToString("0.##"),
                item.Rate.ToString("0.00"),
                item.Category
            });
                }

                // Sub Total
                report.Rows.Add(new List<string>
        {
            "SUB TOTAL",
            group.Sum(x => x.Qty).ToString("0.##"),
            "",
            ""
        });

                // Separator
                report.Rows.Add(new List<string>
        {
            "SEPARATOR",
            "",
            "",
            ""
        });
            }

            // Grand Total Row
            report.TotalRow = new List<string>
    {
        "GRAND TOTAL",
        data.Summary.TotalQuantity.ToString("0.##"),
        "",
        ""
    };

            // Summary Rows
            report.Rows.Add(new List<string>
    {
        "TOTAL TAX",
        data.Summary.TotalTax.ToString("0.00"),
        "",
        ""
    });

            report.Rows.Add(new List<string>
    {
        "TOTAL AMOUNT",
        data.Summary.TotalAmount.ToString("0.00"),
        "",
        ""
    });

            report.Rows.Add(new List<string>
    {
        "GRAND AMOUNT",
        data.Summary.GrandAmount.ToString("0.00"),
        "",
        ""
    });

            return report;
        }

        private PdfReportModel BuildKOTRegisterReport(List<KOTRegisterResponseDto> data)
        {
            var report = new PdfReportModel
            {
                ReportName = "KOT Register Report",
                Headers = new List<string>
        {
            "KOTNO",
            "IssueTime",
            "ItemName",
            "Qty",
            "UserId",
            "Steward",
            "TableNo",
            "TotalAmount",
            "BillNo"
        }
            };

            var groupedData = data.GroupBy(x => x.Steward);

            foreach (var group in groupedData)
            {
                // Group Heading
                report.Rows.Add(new List<string>
        {
            $"Steward: {group.Key}"
        });

                // Header marker
                report.Rows.Add(new List<string>
        {
            "HEADER_ROW"
        });

                // Data rows
                foreach (var item in group)
                {
                    report.Rows.Add(new List<string>
            {
                item.KOTNO,
                item.IssueTime,
                item.ItemName,
                item.Qty.ToString("0.##"),
                item.UserId,
                item.Steward,
                item.TableNo,
                item.TotalAmount.ToString("0.00"),
                item.BillNo
            });
                }

                // Sub Total
                report.Rows.Add(new List<string>
        {
            "SUB TOTAL",  // KOTNO
            "",           // IssueTime
            "",           // ItemName
            group.Sum(x => x.Qty).ToString("0.##"),
            "",           // UserId
            "",           // Steward
            "",           // TableNo
            group.Sum(x => x.TotalAmount).ToString("0.00"),
            ""            // BillNo
        });

                report.Rows.Add(new List<string> { "SEPARATOR" });
            }

            // Grand Total
            report.TotalRow = new List<string>
    {
        "GRAND TOTAL",
        "",
        "",
        data.Sum(x => x.Qty).ToString("0.##"),
        "",
        "",
        "",
        data.Sum(x => x.TotalAmount).ToString("0.00"),
        ""
    };

            return report;
        }
    }
}
