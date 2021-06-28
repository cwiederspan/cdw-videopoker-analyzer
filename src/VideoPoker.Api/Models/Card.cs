using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace VideoPoker.Api.Models {

    public record Card {

        public readonly SuitType Suit;

        public readonly RankType Rank;

        public readonly string Code;

        public int Number { get; private set; }

        public Card(int cardNumber) {

            if (cardNumber < 0 || cardNumber > 51) {
                throw new ApplicationException("Invalid card number");
            }

            this.Number = cardNumber;
            this.Suit = (SuitType)(this.Number / 13);
            this.Rank = (RankType)(this.Number % 13);
            this.Code = this.GetCardCode(cardNumber);
        }

        private string GetCardCode(int cardNumber) {

            string code = "";

            switch (this.Rank) {

                case RankType.Two:
                    code = "2";
                    break;

                case RankType.Three:
                    code = "3";
                    break;

                case RankType.Four:
                    code = "4";
                    break;

                case RankType.Five:
                    code = "5";
                    break;

                case RankType.Six:
                    code = "6";
                    break;

                case RankType.Seven:
                    code = "7";
                    break;

                case RankType.Eight:
                    code = "8";
                    break;

                case RankType.Nine:
                    code = "9";
                    break;

                case RankType.Ten:
                    code = "T";
                    break;

                case RankType.Jack:
                    code = "J";
                    break;

                case RankType.Queen:
                    code = "Q";
                    break;

                case RankType.King:
                    code = "K";
                    break;

                case RankType.Ace:
                    code = "A";
                    break;

            }

            switch (this.Suit) {
                case SuitType.Clubs:
                    code += "C";
                    break;

                case SuitType.Diamonds:
                    code += "D";
                    break;

                case SuitType.Hearts:
                    code += "H";
                    break;

                case SuitType.Spades:
                    code += "S";
                    break;
            }

            return code;
        }
    }
}
