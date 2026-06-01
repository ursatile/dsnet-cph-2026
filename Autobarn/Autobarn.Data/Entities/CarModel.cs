using System.Dynamic;
using System.Text.Json.Serialization;

namespace Autobarn.Data.Entities;

public partial class CarModel
{
	public string Code { get; set; } = "";
	public Make Make { get; set; } = default!;

	public string MakeCode { get; set; } = "";

	public string Name { get; set; } = "";

	[JsonIgnore]
	public virtual ICollection<Vehicle> Vehicles { get; set; } = new HashSet<Vehicle>();
}
