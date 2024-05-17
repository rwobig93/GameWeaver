using Domain.Enums;

namespace Application.Models.GameServer;

public class WeaverWorkUpdate
{
    public int Id { get; set; }
    public WeaverWorkState Status { get; set; }
}