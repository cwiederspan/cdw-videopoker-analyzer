using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace VideoPoker.Api.Models {

    public class StandardNineSixScoreSheet : IScoreSheet {

        // *** Payouts *** //
        // Royal Flush = x800 (if playing 5 coins, so use that. Otherwise x250)
        // Straight Flush = x50
        // Four of a Kind = x25
        // Full House = x9 (or often x8)
        // Flush = x6
        // Straight = x4
        // Three of a Kind = x3
        // Two Pairs =  x2
        // Jacks or Better = x1

        public int RoyalFlush { get { return 800; } }
        // public int RoyalFlush { get { return 250; } }

        public int StraightFlush { get { return 50; } }

        public int FourOfAKind { get { return 25; } }

        public int FullHouse { get { return 9; } }

        public int Flush { get { return 6; } }

        public int Straight { get { return 4; } }

        public int ThreeOfAKind { get { return 3; } }

        public int TwoPair { get { return 2; } }

        public int JacksOrBetter { get { return 1; } }
    }
}
