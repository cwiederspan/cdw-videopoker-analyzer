using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace VideoPoker.Api.Models {

    public record Hand {

        public readonly List<Card> Cards;

        public readonly int Score;
        
        public Hand(List<Card> cards, int score) {
            this.Cards = cards;
            this.Score = score;
        }


        public bool Contains(string cardCode) {

            return this.Cards.Exists(x => x.Code == cardCode);
        }
    }
}
