namespace Autobarn.Messages; 

public class NewVehiclePriceMessage : NewVehicleMessage {
	public string CurrencyCode { get; set; } = "missing";
	public int Price { get; set; }
}
