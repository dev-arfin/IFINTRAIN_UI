

using Microsoft.AspNetCore.Components;
using iFinancing360.UI.Components;
using iFinancing360.UI.Helper.APIClient;
using System.Text.Json.Nodes;

namespace IFinancing360_TRAINING_UI.Components.MasterBorrowerComponent
{
  public partial class MasterBorrowerForm
  {
    #region Service
    [Inject] IFINTEMPLATEClient IFINTEMPLATEClient { get; set; } = null!;
    #endregion

    #region Parameter
    [Parameter, EditorRequired] public string? ID { get; set; }

    #endregion

    #region Component Field
    #endregion

    #region Field
    public JsonObject row = new();
    #endregion

    #region OnInitialized
    protected override async Task OnParametersSetAsync()
    {
      if (ID != null)
      {
        await GetRow();
      }
      else
      {
      }
      await base.OnParametersSetAsync();
    }
    #endregion

    #region GetRow
    public async Task GetRow()
    {
      Loading.Show();
      var res = await IFINTEMPLATEClient.GetRow<JsonObject>("MasterBorrower", "GetRowByID", new { ID = ID });

      if (res?.Data != null)
      {
        row = res.Data;
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
        var res = await IFINTEMPLATEClient.Post("MasterBorrower", "Insert", data);

        if (res?.Data != null)
        {
          ID = res.Data["ID"]?.GetValue<string>();
          NavigationManager.NavigateTo($"/setting/borrower/{ID}");
        }
      }
      #endregion

      #region Update
      else
      {
        var res = await IFINTEMPLATEClient.Put("MasterBorrower", "UpdateByID", data);
      }
      #endregion

      Loading.Close();
      StateHasChanged();
    }
    #endregion

    #region Back
    private void Back()
    {
      NavigationManager.NavigateTo($"/setting/borrower");
    }
    #endregion
  }
}