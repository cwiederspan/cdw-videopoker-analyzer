using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace VideoPoker.Api.Models {

    public interface IScoreSheet {

        int RoyalFlush { get; }

        int StraightFlush { get; }

        int FourOfAKind { get; }

        int FullHouse { get; }

        int Flush { get; }

        int Straight { get; }

        int ThreeOfAKind { get; }

        int TwoPair { get; }

        int JacksOrBetter { get; }
    }
}
