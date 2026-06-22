#nullable enable
using System;
using System.ComponentModel.DataAnnotations;

namespace NeutronData.Models;

public abstract class Expiration
{
    [MaxLength(100)]
    public string? LotNumber { get; set; }
    public DateTime? ExpirationDate { get; set; }
}