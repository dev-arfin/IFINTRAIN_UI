

using Microsoft.AspNetCore.Components;
using iFinancing360.UI.Components;
using iFinancing360.UI.Helper.APIClient;
using System.Text.Json.Nodes;

namespace IFinancing360_TRAINING_UI.Components.BookHistoryComponent
{
  public partial class BookHistoryForm
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
      var res = await IFINTEMPLATEClient.GetRow<JsonObject>("MasterBook", "GetRowByID", new { ID = ID });

      if (res?.Data != null)
      {
        row = res.Data;
      }
      Loading.Close();
    }
    #endregion


    #region Back
    private void Back()
    {
      NavigationManager.NavigateTo($"/inquiry/inquirybook");
    }
    #endregion
  }
}