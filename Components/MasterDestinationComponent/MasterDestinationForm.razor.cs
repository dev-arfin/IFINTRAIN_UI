// MasterDeta

using Microsoft.AspNetCore.Components;
using iFinancing360.UI.Components;
using iFinancing360.UI.Helper.APIClient;
using System.Text.Json.Nodes;

namespace IFinancing360_TRAINING_UI.Components.MasterDestinationComponent
{
  public partial class MasterDestinationForm
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
      var res = await IFINTEMPLATEClient.GetRow<JsonObject>("MasterDestination", "GetRowByID", new { ID = ID });

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
        var res = await IFINTEMPLATEClient.Post("MasterDestination", "Insert", data);

        if (res?.Data != null)
        {
          
          NavigationManager.NavigateTo($"/setting/destination/{res.Data["ID"]}");
        }
      }
      #endregion

      #region Update
      else
      {
        var res = await IFINTEMPLATEClient.Put("MasterDestination", "UpdateByID", data);
      }
      #endregion

      Loading.Close();
      StateHasChanged();
    }
    #endregion

    #region Back
    private void Back()
    {
      NavigationManager.NavigateTo($"/setting/destination/");
    }
    #endregion
  }
}