using Microsoft.AspNetCore.Components;
using iFinancing360.UI.Components;
using iFinancing360.UI.Helper.APIClient;
using System.Globalization;
using System.Text.Json.Nodes;
using System.Data;

namespace IFinancing360_TRAINING_UI.Components.BorrowTransactionComponent
{
    public partial class BorrowTransactionForm
    {
        #region Service
        [Inject] IFINTEMPLATEClient IFINTEMPLATEClient { get; set; } = null!;
        #endregion

        #region Parameter
        [Parameter, EditorRequired] public string? ID { get; set; }
        [Parameter] public EventCallback<JsonObject> RowChanged { get; set; }
        
        [Parameter] public string? FileName { get; set;} = null;

        #endregion

        #region Element Refrence

        FormFieldFileUpload fileUpload = null;

        #endregion

        #region State
        public JsonObject row = new();
        SingleSelectLookup<JsonObject> borrowerlookup = null!;
        private string? isSave;
        #endregion

        #region OnInitialized
        protected override async Task OnParametersSetAsync()
        {
            if (ID != null && row["ID"] == null)
            {
                await GetRow();
            }
            else if (ID == null)
            {
                row["Status"] = "HOLD";
                row["TotalRentAmount"] = 0m;
                row["TotalLateChargeAmount"] = 0m;
            }
            await base.OnParametersSetAsync();
        }
        #endregion

        #region GetRow
        public async Task GetRow()
        {
            Loading.Show();
            var res = await IFINTEMPLATEClient.GetRow<JsonObject>("BorrowTransaction", "GetRowByID", new { ID = ID });

            if (res?.Data != null)
            {
                row = res.Data;
                FileName = row["FileName"]?.GetValue<string>() ?? "";
                await RowChanged.InvokeAsync(row);
            }
            Loading.Close();
        }
        #endregion

        #region OnSubmit
        private async void OnSubmit(JsonObject data)
        {
            Loading.Show();
            data = SetAuditInfo(data);
            data = row.Merge(data);         

            #region Insert
            if (ID == null)
            {
                var res = await IFINTEMPLATEClient.Post("BorrowTransaction", "Insert", data);
                if (res?.Data != null)
                {
                    NavigationManager.NavigateTo($"/borrowtransaction/transaction/{res.Data["ID"]}");
                }
            }
            #endregion

            #region Update
            else
            {
                // var res = await IFINTEMPLATEClient.Put("BorrowTransaction", "UpdateByID", data);

                if (isSave == "Save")
                {
                    Loading.Show();
                    var res = await IFINTEMPLATEClient.Put("BorrowTransaction", "UpdateByID", data);
                    if (res != null)
                    {
                        if (res.Result > 0) await GetRow();
                    }
                }
                else if (isSave == "Return")
                {
                    bool? result = await Confirm();
                    if (result == true)
                    {
                        Loading.Show();
                        var res = await IFINTEMPLATEClient.Put("BorrowTransaction", "Return", data);
                        if (res != null)
                        {
                        if (res.Result > 0) await GetRow();
                        }
                    }
                }
            }
                #endregion
                Loading.Close();
                StateHasChanged();
            }
            #endregion


        #region Back
        private void Back()
        {
            NavigationManager.NavigateTo($"/borrowtransaction/transaction");
        }
        #endregion

        #region Cancel
        public async Task Cancel()
        {
            bool? result = await Confirm();

            if (result == true)
            {
                Loading.Show();
                await IFINTEMPLATEClient.Put("BorrowTransaction", "Cancel", row);
                await GetRow();

                Loading.Close();
                StateHasChanged();
            }
        }
        #endregion

        #region Borrow
        public async Task Borrow()
        {
            bool? result = await Confirm();

            if (result == true)
            {
                
                Loading.Show();
                await IFINTEMPLATEClient.Put("BorrowTransaction", "Borrow", row);
                await GetRow();

                Loading.Close();
                StateHasChanged();
            }
        }
        #endregion

        #region Return
        public async Task Return()
        {
            bool? result = await Confirm();

            if (result == true)
            {
                Loading.Show();
                await IFINTEMPLATEClient.Put("BorrowTransaction", "Return", row);
                await GetRow();

                Loading.Close();
                StateHasChanged();
            }
        }
        #endregion

         #region UpdateTotalRentAmount
        public async Task UpdateTotalRentAmount()
        {
            bool? result = await Confirm();

            if (result == true)
            {
                Loading.Show();
                await IFINTEMPLATEClient.Put("BorrowTransaction", "UpdateTotalRentAmount", row);
                await GetRow();

                Loading.Close();
                StateHasChanged();
            }
        }
        #endregion
        #region LoadBorrowerLookUp
        protected async Task<List<JsonObject>?> LoadBorrowerLookUp(DataGridLoadArgs args)
            {
            var res = await IFINTEMPLATEClient.GetRows<JsonObject>("MasterBorrower", "GetRowsForLookup", new
            {
                Keyword = args.Keyword,
                Offset = args.Offset,
                Limit = args.Limit,
            });
            return res?.Data;
            }
        #endregion




    #region Upload
        private async Task Upload(FileInput file)
        {
            Loading.Show();
            try
            {
                if (ID != null)
                {
                    var data = new JsonObject
                    {
                        ["ID"] = row["ID"]?.GetValue<string>()
                    };

                    Dictionary<string, ElementReference> fileInputElements = new()
                    {
                        { "file", fileUpload.InputElement }
                    };

                    var res = await IFINTEMPLATEClient.PostAsClient("BorrowTransaction","UploadFile",data,fileInputElements);

                    await GetRow();
                    StateHasChanged();
                }
                else
                {
                    row["FileContent"]  = file.Content.ToJsonNode();
                    row["FileMimeType"] = file.MimeType;
                    row["FileName"]     = file.Name;

                    StateHasChanged();
                }
            }
            finally
            {
                Loading.Close();
            }
        }
    #endregion

    #region Preview
    private async Task<FileInput?> Preview()
    {
        if (ID == null)
        {
            await fileUpload.PreviewFromInput();
            return null;
        }

        var res = await IFINTEMPLATEClient.GetFileAsync("BorrowTransaction", "GetFile", new { ID = ID });
        return res;
    }
    #endregion

    #region Delete
    private async Task<bool> Delete()
    {
        Loading.Show();
        try
        {
            if (ID != null)
            {
                var res = await IFINTEMPLATEClient.Delete(
                    "BorrowTransaction", "DeleteFile", row);

                await fileUpload.Clear();
                await GetRow();

                return res?.Result > 0;
            }

            row["FileContent"]  = null;
            row["FileMimeType"] = null;
            row["FileName"]     = null;

            await fileUpload.Clear();
            StateHasChanged();
            return true;
        }
        finally
        {
            Loading.Close();
        }
    }
    #endregion

   #region GetReportHTML
    private async Task<string> GetHTML()
    {
      Loading.Show();

      var result = await IFINTEMPLATEClient.GetRow("BorrowTransaction","GetHTMLForReport", ID);

      string html = result?.Data["HTML"]?.GetValue<string>() ?? "<p>Default screen</p>";

      Loading.Close();

      return html;
    }
    #endregion

     #region PrintByID
        private async Task PrintByID(string MimeType)
        {
            if (ID != null)
            {
                var file = await IFINTEMPLATEClient.GetRow<JsonObject>("BorrowTransaction","PrintDocument", new {MimeType = MimeType, ID = ID});

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

