using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using VideoPoker.Api.Models;

namespace VideoPoker.Api.Services {
    
    public class ScoringService {

        public List<Hand> ScoreAllHands(IScoreSheet scoreSheet) {

            // 2,598,960 = COMBIN(52, 5) in Excel and is the number of all possible 
            // combinations for a 5 card hand
            List<Hand> allHands = new List<Hand>(2598960);

            for (int i0 = 51; i0 >= 0; i0--) {

                for (int i1 = i0 - 1; i1 >= 0; i1--) {

                    for (int i2 = i1 - 1; i2 >= 0; i2--) {

                        for (int i3 = i2 - 1; i3 >= 0; i3--) {

                            for (int i4 = i3 - 1; i4 >= 0; i4--) {

                                var cards = new List<Card>(5) {
                                    new Card(i0),
                                    new Card(i1),
                                    new Card(i2),
                                    new Card(i3),
                                    new Card(i4)
                                };

                                var score = this.ScoreHand(scoreSheet, cards);

                                allHands.Add(new Hand(cards, score));
                            }
                        }
                    }
                }
            }

            return allHands;
        }

        private int ScoreHand(IScoreSheet scoreSheet, List<Card> cards) {

            System.Diagnostics.Debug.Assert(cards.Count == 5);

            int score = 0;

            int[] values = (
                from x in cards
                orderby (int)x.Rank
                select (int)x.Rank
            ).ToArray<int>();

            // By checking for pairs, performance can be enhanced by skipping certain scoring components
            bool hasSomePair = this.HasSomePair(values);

            // This is the core logic for finding matches
            bool hasFlush = !hasSomePair && this.HasFlush(cards);
            bool hasStraight = !hasSomePair && this.HasStraight(values);
            bool hasRoyalFlush = hasStraight && hasFlush && cards.Min(x => (int)x.Rank) == (int)RankType.Ten;

            bool hasThreeOfAKind = hasSomePair && this.HasThreeOfAKind(values);
            bool hasFourOfAKind = hasThreeOfAKind && this.HasFourOfAKind(values);
            bool hasFullHouse = hasThreeOfAKind && !hasFourOfAKind && this.HasFullHouse(values);
            bool hasTwoPair = hasSomePair && !hasThreeOfAKind && !hasFullHouse && this.HasTwoPair(values);
            bool hasJacksOrBetter = hasSomePair && !hasThreeOfAKind && !hasTwoPair && this.HasJacksOrBetter(values);

            // Check for Royal Flush and Straight Flush
            if (hasRoyalFlush) { score = scoreSheet.RoyalFlush; }
            else if (hasFlush && hasStraight) { score = scoreSheet.StraightFlush; }
            else if (hasFourOfAKind) { score = scoreSheet.FourOfAKind; }
            else if (hasFullHouse) { score = scoreSheet.FullHouse; }      // Could be 8???
            else if (hasFlush == true) { score = scoreSheet.Flush; }
            else if (hasStraight == true) { score = scoreSheet.Straight; }
            else if (hasThreeOfAKind == true) { score = scoreSheet.ThreeOfAKind; }
            else if (hasTwoPair == true) { score = scoreSheet.TwoPair; }
            else if (hasJacksOrBetter == true) { score = scoreSheet.JacksOrBetter; }

            // Return the value, too.
            return score;
        }

        private bool HasSomePair(int[] values) {

            // This is simply a helper method to determine whether there's at least one pair

            bool hasSomePair = false;

            // Check to see if there are any pairs
            if (
                (values[0] == values[1]) ||
                (values[1] == values[2]) ||
                (values[2] == values[3]) ||
                (values[3] == values[4])
            ) {

                // Here's the result
                hasSomePair = true;
            }

            return hasSomePair;
        }

        private bool HasFlush(List<Card> cards) {

            return (
                (cards[0].Suit == cards[1].Suit) &&
                (cards[1].Suit == cards[2].Suit) &&
                (cards[2].Suit == cards[3].Suit) &&
                (cards[3].Suit == cards[4].Suit)
            );
        }

        private bool HasStraight(int[] values) {

            bool isStraight = false;

            // Check to see if there's a straight
            if (
                (values[0] == values[1] - 1) &&
                (values[1] == values[2] - 1) &&
                (values[2] == values[3] - 1) &&
                (values[3] == values[4] - 1)
            ) {

                // This is a "traditional" straight with 5 cards in a row
                isStraight = true;
            }
            else if (
                (values[0] == 0) &&      // Two
                (values[1] == 1) &&      // Three
                (values[2] == 2) &&      // Four
                (values[3] == 3) &&      // Five
                (values[4] == 12)        // Ace
            ) {

                // This is an Ace-Two-Three-Four-Five straight
                isStraight = true;
            }
            else {

                // Otherwise, there's no straight
                isStraight = false;
            }

            return isStraight;
        }

        private bool HasFourOfAKind(int[] values) {

            bool isFourOfAKind = false;

            // Check to see if there's a straight
            if (
                (values[1] == values[2]) &&
                (values[2] == values[3]) &&
                (values[0] == values[1] || values[3] == values[4])
            ) {

                // This is a "traditional" straight with 5 cards in a row
                isFourOfAKind = true;
            }

            return isFourOfAKind;
        }

        private bool HasThreeOfAKind(int[] values) {

            bool isThreeOfAKind = false;

            // Check to see if there's a straight
            if (
                (values[0] == values[1] && values[1] == values[2]) ||
                (values[1] == values[2] && values[2] == values[3]) ||
                (values[2] == values[3] && values[3] == values[4])
            ) {

                // This is a "traditional" straight with 5 cards in a row
                isThreeOfAKind = true;
            }

            return isThreeOfAKind;
        }

        private bool HasFullHouse(int[] values) {

            bool isFullHouse = false;

            // Check to see if there's a straight
            if (
                (values[0] == values[1]) &&
                (values[1] == values[2] || values[2] == values[3]) &&
                (values[3] == values[4])
            ) {

                // This is a "traditional" straight with 5 cards in a row
                isFullHouse = true;
            }

            return isFullHouse;
        }

        private bool HasTwoPair(int[] values) {

            bool hasTwoPair = false;

            // Because the values are sorted, then there are three cases that can make up two pair.
            // They look like this (0 = outlier [may or may not be paired, as in Four-of-a-kind or full house]):

            // 0 1 2 3 4    // Array Index
            // ---------
            // 0 X X Y Y    // Case #1
            // X X 0 Y Y    // Case #2
            // X X Y Y 0    // Case #3

            // NOTE: A four of a kind would also qualify as two pair using this logic

            // Check for Case #1
            if (
                (values[1] == values[2]) &&
                (values[3] == values[4])
            ) {
                hasTwoPair = true;
            }

            // Check for Case #2
            else if (
                (values[0] == values[1]) &&
                (values[3] == values[4])
            ) {
                hasTwoPair = true;
            }

            // Check for Case #3
            else if (
                (values[0] == values[1]) &&
                (values[2] == values[3])
            ) {
                hasTwoPair = true;
            }

            return hasTwoPair;
        }

        private bool HasJacksOrBetter(int[] values) {

            bool hasSomePair = false;

            // Check to see if there's a straight
            if (
                (values[0] == values[1] && values[0] >= (int)RankType.Jack) ||
                (values[1] == values[2] && values[1] >= (int)RankType.Jack) ||
                (values[2] == values[3] && values[2] >= (int)RankType.Jack) ||
                (values[3] == values[4] && values[3] >= (int)RankType.Jack)
            ) {

                // This is a "traditional" straight with 5 cards in a row
                hasSomePair = true;
            }

            return hasSomePair;
        }
    }
}
