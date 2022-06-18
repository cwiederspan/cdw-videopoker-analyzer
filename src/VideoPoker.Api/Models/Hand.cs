using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace VideoPoker.Api.Models {

    public record Hand {

        public readonly List<Card> Cards;

        public readonly int Score;

        public readonly string Label;
        
        public Hand(List<Card> cards, int score) {
            this.Cards = cards;
            this.Score = score;
        }
        
        public Hand(List<Card> cards, int score, string label) {
            this.Cards = cards;
            this.Score = score;
            this.Label = label;
        }

        public bool Contains(string cardCode) {

            return this.Cards.Exists(x => x.Code == cardCode);
        }

        public override string ToString() {

            return $"{String.Join(",", this.Cards.Select(c => c.Code))}_{this.Label}_{this.Score}";
        }
    }
}
