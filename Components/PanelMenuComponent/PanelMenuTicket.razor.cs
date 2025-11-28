using Microsoft.AspNetCore.Components;

namespace IFinancing360_TRAINING_UI.Components.PanelMenuComponent
{
	public partial class PanelMenuTicket
	{
		[Parameter] public RenderFragment? ChildContent { get; set; }
		[Parameter] public string? UserID { get; set; }


		List<Menu> menus = [
		];

		protected override async Task OnParametersSetAsync()
		{

			if (!string.IsNullOrWhiteSpace(UserID))
			{
				string BasePath = $"borrowtransaction/transactionticket/{UserID}";

				menus.AddRange([
					new Menu { Title = "Info", Url = BasePath, Exact = true },
					new Menu { Title = "Selling Detail", Url = $"{BasePath}/sellingdetail" },
					new Menu { Title = "Selling Document", Url = $"{BasePath}/sellingdocument" },
				]);

				menus = menus.DistinctBy(x => x.Title).ToList();
			}
			await base.OnParametersSetAsync();
		}
	}
}
