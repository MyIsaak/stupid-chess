﻿using System.Linq;
using System.Collections.ObjectModel;

namespace ChessDotNet.Pieces
{
    public class Queen : Piece
    {
        public override Player Owner
        {
            get;
            set;
        }

        public Queen(Player owner)
        {
            Owner = owner;
        }

        public override char GetFenCharacter()
        {
            return Owner == Player.White ? 'Q' : 'q';
        }

		public override byte GetImportance ()
		{
			return 9;
		}

        public override bool IsValidMove(Move move, ChessGame game)
        {
            ChessUtilities.ThrowIfNull(move, "move");
            ChessUtilities.ThrowIfNull(game, "game");
            return new Bishop(Owner).IsValidMove(move, game) || new Rook(Owner).IsValidMove(move, game);
        }

        public override ReadOnlyCollection<Move> GetValidMoves(Position from, bool returnIfAny, ChessGame game)
        {
            ChessUtilities.ThrowIfNull(from, "from");
            ReadOnlyCollection<Move> horizontalVerticalMoves = new Rook(Owner).GetValidMoves(from, returnIfAny, game);
            if (returnIfAny && horizontalVerticalMoves.Count > 0)
                return horizontalVerticalMoves;
            ReadOnlyCollection<Move> diagonalMoves = new Bishop(Owner).GetValidMoves(from, returnIfAny, game);
            return new ReadOnlyCollection<Move>(horizontalVerticalMoves.Concat(diagonalMoves).ToList());
        }
    }
}
