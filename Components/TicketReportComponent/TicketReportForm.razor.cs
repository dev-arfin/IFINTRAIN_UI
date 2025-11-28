using System.Text.Json.Nodes;
using iFinancing360.UI.Components;
using iFinancing360.UI.Helper.APIClient;
using Microsoft.AspNetCore.Components;

namespace IFinancing360_TRAINING_UI.Components.TicketReportComponent
{
    public partial class TicketReportForm
    {
        #region Service
        [Inject] IFINTEMPLATEClient IFINTEMPLATEClient { get; set; } = null!;
        #endregion

        #region Parameter
        // Kalau nanti butuh, ID masih disediakan; saat ini tidak dipakai.
        [Parameter] public EventCallback<JsonObject?> IDChanged { get; set; }
        [Parameter, EditorRequired] public string? ID { get; set; }
        #endregion

        #region Field
        public JsonObject row = new();
        #endregion

        #region OnInitialized
        protected override async Task OnInitializedAsync()
        {
            // Default: kalau form dipakai untuk membuat report baru
            if (ID is null)
            {
                var systemDate = GetSystemDate();
                row["StartDate"] = systemDate;
                row["EndDate"]   = systemDate;
            }

            await base.OnInitializedAsync();
        }
        #endregion

        #region GetHtml (Preview HTML Report Periode)
        private async Task<string> GetHTML()
        {
            // Validasi: StartDate & EndDate wajib diisi
            if (!row.ContainsKey("StartDate") || row["StartDate"] is null)
                throw new Exception("Start Date is required");

            if (!row.ContainsKey("EndDate") || row["EndDate"] is null)
                throw new Exception("End Date is required");

            var startDate = row["StartDate"]!.GetValue<DateTime>();
            var endDate   = row["EndDate"]!.GetValue<DateTime>();

            // Panggil API TransactionTicketSell/GetHTMLPeriodPreview
            var file = await IFINTEMPLATEClient.GetRow<JsonObject>(
                "TransactionTicketSell",
                "GetHTMLPeriodPreview",
                new
                {
                    startDate,
                    endDate
                }
            );

            if (file?.Data == null)
            {
                return "No records found";
            }

            return file.Data["HTML"]?.GetValue<string>() ?? "";
        }
        #endregion

        #region Print (Download Docx / PDF Report Periode)
        private async Task Print(string mimeType)
        {
            // Validasi tanggal sebelum kirim request
            if (!row.ContainsKey("StartDate") || row["StartDate"] is null)
                throw new Exception("Start Date is required");

            if (!row.ContainsKey("EndDate") || row["EndDate"] is null)
                throw new Exception("End Date is required");

            var startDate = row["StartDate"]!.GetValue<DateTime>();
            var endDate   = row["EndDate"]!.GetValue<DateTime>();

            // Panggil API TransactionTicketSell/PrintPeriodDocument
            var file = await IFINTEMPLATEClient.GetRow<JsonObject>(
                "TransactionTicketSell",
                "PrintPeriodDocument",
                new
                {
                    MimeType = mimeType,
                    startDate,
                    endDate
                }
            );

            if (file?.Data != null)
            {
                var data     = file.Data;
                var content  = data["Content"]?.GetValueAsByteArray();
                var fileName = data["Name"]?.GetValue<string>();
                var mt       = data["MimeType"]?.GetValue<string>();

                PreviewFile(content, fileName, mt);
            }
        }
        #endregion
    }
}
