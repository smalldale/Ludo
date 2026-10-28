using Ludo.Core.Domain;
using Ludo.Core.Domain.Enums;
using Ludo.Core.Domain.Interfaces;

namespace Ludo.Core.Tests.TestData;

public static class PiecesFactory
{
    public static IPiece Create(
        int id,
        PlayerColor color)
    {
        return new Piece(id, color);
    }

    public static List<IPiece> CreateMany(
        int count,
        PlayerColor color,
        int firstPieceId = 1)
    {
        return Enumerable
            .Range(firstPieceId, count)
            .Select(id => Create(id, color))
            .ToList();
    }

    public static Dictionary<PlayerColor, List<IPiece>>
        CreateForPlayers(
            List<IPlayer> players,
            int piecesPerPlayer = 4)
    {
        var piecesByColor =
            new Dictionary<PlayerColor, List<IPiece>>();

        int nextPieceId = 1;

        foreach (IPlayer player in players)
        {
            piecesByColor[player.Color] = CreateMany(
                piecesPerPlayer,
                player.Color,
                nextPieceId);

            nextPieceId += piecesPerPlayer;
        }

        return piecesByColor;
    }
}