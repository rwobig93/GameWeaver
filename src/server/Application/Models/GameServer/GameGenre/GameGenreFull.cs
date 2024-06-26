﻿using Application.Models.GameServer.Game;

namespace Application.Models.GameServer.GameGenre;

public class GameGenreFull
{
    public Guid Id { get; set; }
    public Guid GameId { get; set; }
    public string Name { get; set; } = "";
    public string Description { get; set; } = "";
    public List<GameSlim> Games { get; set; } = [];
}