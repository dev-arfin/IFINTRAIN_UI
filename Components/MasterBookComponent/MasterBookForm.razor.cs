using Microsoft.AspNetCore.Components;
using iFinancing360.UI.Components;
using iFinancing360.UI.Helper.APIClient;
using System.Globalization;
using System.Text.Json.Nodes;

namespace IFinancing360_TRAINING_UI.Components.MasterBookComponent
{
  public partial class MasterBookForm
  {
    #region Service
    [Inject] IFINTEMPLATEClient IFINTEMPLATEClient { get; set; } = null!;
    #endregion

    #region Parameter
    [Parameter, EditorRequired] public string? ID { get; set; }
    #endregion

    #region State
    public JsonObject row = new();
    #endregion

    #region OnInitialized
    protected override async Task OnParametersSetAsync()
    {
      if (ID != null)
      {
        await GetRow();
      }
      await base.OnParametersSetAsync();
    }
    #endregion

    #region GetRow
    public async Task GetRow()
    {
      Loading.Show();
      var res = await IFINTEMPLATEClient.GetRow<JsonObject>("MasterBook", "GetRowByID", new { ID = ID });

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
        var res = await IFINTEMPLATEClient.Post("Masterbook", "Insert", data);

        if (res?.Data != null)
        {
          ID = res.Data["ID"]?.GetValue<string>();
          NavigationManager.NavigateTo($"/setting/book/{ID}");
        }
      }
      #endregion

          #region Update
      else
      {
        var res = await IFINTEMPLATEClient.Put("MasterBook", "UpdateByID", data);
      }
      #endregion

      Loading.Close();
      StateHasChanged();
    }
      #endregion


    #region Back
    private void Back()
    {
      NavigationManager.NavigateTo($"/setting/book");
    }
    #endregion

  }
}
