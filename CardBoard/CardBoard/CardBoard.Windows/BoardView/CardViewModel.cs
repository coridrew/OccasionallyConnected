using CardBoard.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CardBoard.BoardView
{
    public class CardViewModel
    {
        private readonly Card _card;
        private readonly Column _column;
        
        public CardViewModel(Card card, Column column)
        {
            _card = card;
            _column = column;
        }

        public Card Card
        {
            get { return _card; }
        }

        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
            {
                return false;
            }

            var that = (CardViewModel)obj;
            return
                Object.Equals(this._card, that._card) &&
                this._column == that._column;
        }

        public override int GetHashCode()
        {
            return _card.GetHashCode() * 3 + (int)_column;
        }

    }
}
