﻿using Models;

namespace Services;

public interface ISnapshot
{
    List<Character> Characters { get; set; }
    List<CharacterNpc> Npcs { get; set; }

    HashSet<Guid> ItemsSold { get; set; }
    HashSet<Guid> CharactersImported { get; set; }
}

public class Snapshot : ISnapshot
{
    public List<Character> Characters { get; set; } = [];
    public List<CharacterNpc> Npcs { get; set; } = [];

    public HashSet<Guid> ItemsSold { get; set; } = [];
    public HashSet<Guid> CharactersImported { get; set; } = [];
}
