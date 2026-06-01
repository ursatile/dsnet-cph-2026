using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Autobarn.Website.Models;

public partial class VehicleDto {

	///<summary>The code identifying the make &amp; model, e.g. "dmc-delorean"</summary>
	[HiddenInput(DisplayValue = false)]
	public string? ModelCode { get; set; }

	public string? ModelName { get; set; }


	[GeneratedRegex("[^A-Z0-9]")]
	private static partial Regex VehicleRegistrationRegex();

	private static string? NormalizeRegistration(string? reg) {
		return reg == null ? reg : VehicleRegistrationRegex().Replace(reg.ToUpperInvariant(), "");
	}

	///<summary>The registration code that identifies this vehicle, e.g. "OUTATIME"</summary>
	[Required]
	[DisplayName("Registration Plate")]
	public string? Registration {
		get => NormalizeRegistration(field);
		set;
	}

	///<summary>The year this vehicle was registered, e.g. 1985</summary>
	[Required]
	[DisplayName("Year of first registration")]
	[Range(1950, 2026)]
	public int Year { get; set; }

	///<summary>What color is this vehicle, e.g. "Silver"</summary>
	[Required]
	[DisplayName("Colour")]
	public string Color { get; set; } = String.Empty;

	private static readonly string[] colors = [
		"Black", "Blue", "Gold", "Green", "Grey", "Orange",
		"Purple", "Red", "Silver", "Turquoise", "White", "Yellow"
	];

	private static readonly SelectListItem blankSelectListItem = new("select...", String.Empty);

	public static IEnumerable<SelectListItem> ListColors(string selectedColor) {
		var items = new List<SelectListItem> { blankSelectListItem };
		items.AddRange(colors.Select(c => new SelectListItem(c, c, c == selectedColor)));
		return items;
	}
}
