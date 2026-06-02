using System.ComponentModel;

namespace Autobarn.Client;

public class VehicleDto {

	public string Registration { get; set; } = "";

	public string ModelCode { get; set; } = "";

	public string Color { get; set; } = "";

	public int Year { get; set; }
}