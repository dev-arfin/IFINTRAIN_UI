using Microsoft.AspNetCore.Components;
using iFinancing360.UI.Components;
using iFinancing360.UI.Helper.APIClient;
using System.Text.Json.Nodes;

namespace IFinancing360_TRAINING_UI.Components.TransactionTicketSellComponent
{
  public partial class TransactionTicketSellDocForm
  {
    #region Service
    [Inject] IFINTEMPLATEClient IFINTEMPLATEClient { get; set; } = null!;
    [Inject] NavigationManager NavigationManager { get; set; } = null!;
    #endregion

    #region Parameter
    // ID dokumen (null saat Add)
    [Parameter, EditorRequired] public string? ID { get; set; }
    // ID header TransactionTicketSell
    [Parameter] public string? TransactionID { get; set; }

    // [Parameter] public string? FileName { get; set; }
    [Parameter] public string? ParentMenuURL { get; set; }
    #endregion

    #region Element Reference
    FormFieldFileUpload fileUpload = null!;
    #endregion

    #region Field
    public JsonObject row = new();
     public JsonObject rowTransaction { get; set; } = [];
    #endregion

    #region OnParametersSetAsync
    protected override async Task OnParametersSetAsync()
    {

        if (!string.IsNullOrWhiteSpace(TransactionID))
    {
        await GetRowTransaction();
    }

      if (!string.IsNullOrWhiteSpace(ID))
      {
        await GetRow();
      }
      else
      {
        row = new JsonObject
        {
          ["ID"]            = null,
          ["TransactionID"] = TransactionID,
          ["DocumentName"]  = string.Empty,
          ["Remark"]        = string.Empty,
          ["FileName"]      = string.Empty,
          ["FilePath"]      = string.Empty
        };
      }

      await base.OnParametersSetAsync();
    }
    #endregion

    private bool IsLocked =>
    rowTransaction["Status"]?.GetValue<string>() is "POST" or "CANCEL";

    #region GetRow
    public async Task GetRow()
    {
      Loading.Show();
      try
      {
        var res = await IFINTEMPLATEClient.GetRow<JsonObject>("TransactionTicketSellDoc","GetRowByID",new { ID = ID });
        if (res?.Data != null)
        {
          row = res.Data;
        }
      }
      finally
      {
        Loading.Close();
      }
    }
    #endregion

    #region OnSubmit
private async void OnSubmit(JsonObject data)
{
    Loading.Show();

    data = SetAuditInfo(data);
    data = row.Merge(data);

    // Inject TransactionID dari header
    if (!string.IsNullOrWhiteSpace(TransactionID))
    {
        data["TransactionID"] = TransactionID;
    }

    // ===== INSERT =====
    if (ID == null)
    {
        // Daftarkan input file ke PostAsClient
        var fileInputs = new Dictionary<string, ElementReference>();

        if (fileUpload != null)
        {
            // "File" = HARUS sama dengan Name="File" di FormFieldFileUpload
            fileInputs.Add("File", fileUpload.InputElement);
        }

        var res = await IFINTEMPLATEClient.PostAsClient(
            "TransactionTicketSellDoc",
            "Insert",
            data,
            fileInputs
        );

        if (res?.Data != null)
        {
            ID = res.Data["ID"]?.GetValue<string>();

            NavigationManager.NavigateTo(
                $"/borrowtransaction/transactionticket/{TransactionID}/sellingdocument/{ID}"
            );
        }
    }
    // ===== UPDATE =====
    else
    {
        data["ID"] = ID;

        var fileInputs = new Dictionary<string, ElementReference>
        {
            { "File", fileUpload.InputElement }
        };

        var res = await IFINTEMPLATEClient.PutAsClient(
            "TransactionTicketSellDoc",
            "UpdateByID",
            data,
            fileInputs
        );

        await fileUpload.Clear();
        await GetRow();
        StateHasChanged();
    }

    Loading.Close();
    StateHasChanged();
}
#endregion



    #region Back
    private void Back()
    {
      if (!string.IsNullOrWhiteSpace(TransactionID))
      {
        NavigationManager.NavigateTo(
          $"/borrowtransaction/transactionticket/{TransactionID}/sellingdocument");
      }
      else if (!string.IsNullOrWhiteSpace(ParentMenuURL))
      {
        NavigationManager.NavigateTo(ParentMenuURL);
      }
      else
      {
        // fallback generic
        NavigationManager.NavigateTo("/borrowtransaction/transactionticket");
      }
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

          var res = await IFINTEMPLATEClient.PostAsClient("TransactionTicketSellDoc", "UploadFile", data, fileInputElements);

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

      var res = await IFINTEMPLATEClient.GetFileAsync(
        "TransactionTicketSellDoc",
        "GetFile",
        new { ID = ID });

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
            "TransactionTicketSellDoc","DeleteFile",row);

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

    #region GetRowTransaction
    public async Task GetRowTransaction()
    {
      Loading.Show();
      var res = await IFINTEMPLATEClient.GetRow<JsonObject>("TransactionTicketSell", "GetRowByID", new
      {
        ID = TransactionID
      });

      if (res?.Data != null)
      {
        rowTransaction = res.Data;
      }

      Loading.Close();
      StateHasChanged();
    }
    #endregion
  }
}
