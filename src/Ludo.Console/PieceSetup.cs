using Ludo.Core.Domain;
using Ludo.Core.Domain.Enums;
using Ludo.Core.Domain.Interfaces;

public static class PieceSetup
{
    public static Dictionary<PlayerColor, List<IPiece>>
        CreatePieces(IEnumerable<IPlayer> players)
    {
        var piecesByColor =
            new Dictionary<PlayerColor, List<IPiece>>();

        foreach (IPlayer player in players)
        {
            var pieces = new List<IPiece>();

            for (int i = 0; i < 4; i++)
            {
                pieces.Add(
                    new Piece((player.Id - 1) * 4 + i + 1, player.Color));
            }

            piecesByColor.Add(player.Color, pieces);
        }

        return piecesByColor;
    }
}