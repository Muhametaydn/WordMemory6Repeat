using System.Text.Json;
using Microsoft.AspNetCore.Http;
using WordMemoryApp.Models;

namespace WordMemoryApp.Extensions
{
    public static class SessionExtensions
    {
        private const string Key = "PuzzleGame";

        public static void SaveGame(this ISession session, PuzzleState state) =>
            session.SetString(Key, JsonSerializer.Serialize(state));

        public static PuzzleState? LoadGame(this ISession session) =>
            session.GetString(Key) is { } json ? JsonSerializer.Deserialize<PuzzleState>(json) : null;

        public static void ClearGame(this ISession session) => session.Remove(Key);
    }
}
