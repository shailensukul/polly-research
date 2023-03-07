using System.Runtime.Serialization;

namespace Client.Model;

[DataContract]
public sealed record ProductContract
{
    [DataMember]
    public Guid Id { get; set; }

    [DataMember]
    public string Description { get; set; }

    [DataMember]
    public string ItemCode { get; set; }

    [DataMember]
    public decimal Price { get; set; }

    [DataMember]
    public string Category { get; set; }

    [DataMember]
    public string SubCategory { get; set; }
}