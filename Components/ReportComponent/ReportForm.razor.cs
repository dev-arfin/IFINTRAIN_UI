using System.Text.Json.Nodes;

using iFinancing360.UI.Components;
using iFinancing360.UI.Helper.APIClient;
using Microsoft.AspNetCore.Components;

namespace IFinancing360_TRAINING_UI.Components.ReportComponent
{
    public partial class ReportForm
    {
        #region service
        [Inject] IFINTEMPLATEClient IFINTEMPLATEClient {get;set;} = null!;
        #endregion

        #region Parameter
        [Parameter] public EventCallback<JsonObject?> IDChanged {get;set;}
        [Parameter, EditorRequired] public string? ID {get;set;}
        #endregion

        #region Component Field
        #endregion

        #region field
        public JsonObject row = new();
        #endregion
        #region OnInitialized
        protected override async Task OnInitializedAsync()
        {
            if (ID != null)
            {
                
            }
            else
            {
                row["AsOfDate"] = GetSystemDate();
            }
        }
        #endregion

        #region GetHtml
        private async Task<string> GetHTML()
        {
            if (!row.ContainsKey("AsOfDate") || row["AsOfDate"] is null)
                throw new Exception("As Of Date is required");

            var date = row["AsOfDate"].GetValue<DateTime>();

            var file = await IFINTEMPLATEClient.GetRow<JsonObject>("BorrowTransaction","GetHTMLDatePreview", new { AsOfDate = date });

            if (file?.Data == null)
            {
                return "No records found";
                // atau: throw new Exception("Data not found for selected date");
            }

            return file.Data["HTML"]?.GetValue<string>() ?? "";
        }

        #endregion

        #region Print
        private async Task Print(string MimeType)
        {
            var date = row["AsOfDate"].GetValue<DateTime>();
            if (date != null)
            {
                var file = await IFINTEMPLATEClient.GetRow<JsonObject>("BorrowTransaction","PrintDateDocument", new {MimeType = MimeType, AsOfDate = date});

                if (file?.Data != null)
                {
                    var data = file.Data;
                    var content = data["Content"]?.GetValueAsByteArray();
                    var fileName = data["Name"]?.GetValue<string>();
                    var mimeType = data["MimeType"]?.GetValue<string>();

                    PreviewFile(content,fileName,mimeType);
                }
            }
            else
            {
                throw new Exception("ID NOT FOUND");
            }
        }
        #endregion
    }
}