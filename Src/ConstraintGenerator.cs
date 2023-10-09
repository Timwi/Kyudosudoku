using System;
using System.Collections.Generic;
using RT.Util;
using SvgPuzzleConstraints;

namespace KyudosudokuWebsite
{
    record struct ConstraintGenerator(int? probability, Type type, Func<int[], int[][], IList<SvgConstraint>> generator)
    {
        // The numbers balance the relative probabilities of each constraint occurring so that they each occur reasonably similarly often.
        public static readonly ConstraintGenerator[] All = Ut.NewArray<ConstraintGenerator>(
            // Cell constraints
            (null, typeof(AntiBishop), (s, ur) => AntiBishop.Generate(s)),
            (null, typeof(AntiKing), (s, ur) => AntiKing.Generate(s)),
            (null, typeof(AntiKnight), (s, ur) => AntiKnight.Generate(s)),
            (null, typeof(NoConsecutive), (s, ur) => NoConsecutive.Generate(s)),
            (null, typeof(MaximumCell), (s, ur) => MaximumCell.Generate(s)),
            (null, typeof(FindThe9), (s, ur) => FindThe9.Generate(s)),
            (null, typeof(OddEven), (s, ur) => OddEven.Generate(s)),
            (null, typeof(Means), (s, ur) => Means.Generate(s)),

            // Area constraints
            (20, typeof(Arrow), (s, ur) => Arrow.Generate(s)),
            (20, typeof(KillerCage), KillerCage.Generate),
            (15, typeof(Palindrome), (s, ur) => Palindrome.Generate(s)),
            (20, typeof(CappedLine), (s, ur) => CappedLine.Generate(s)),
            (20, typeof(RenbanCage), RenbanCage.Generate),
            (17, typeof(Snowball), (s, ur) => Snowball.Generate(s)),
            (30, typeof(Thermometer), (s, ur) => Thermometer.Generate(s)),
            (15, typeof(GermanWhisper), (s, ur) => GermanWhisper.Generate(s)),

            // Row/column constraints
            (37, typeof(Battlefield), (s, ur) => Battlefield.Generate(s)),
            (null, typeof(Binairo), (s, ur) => Binairo.Generate(s)),
            (20, typeof(Sandwich), (s, ur) => Sandwich.Generate(s)),
            (47, typeof(Skyscraper), (s, ur) => Skyscraper.Generate(s)),
            (20, typeof(SkyscraperSum), (s, ur) => SkyscraperSum.Generate(s)),
            (20, typeof(ToroidalSandwich), (s, ur) => ToroidalSandwich.Generate(s)),
            (20, typeof(XSum), (s, ur) => XSum.Generate(s)),
            (15, typeof(YSum), (s, ur) => YSum.Generate(s)),

            // Four-cell constraints
            (50, typeof(Clockface), (s, ur) => Clockface.Generate(s)),
            (40, typeof(Inclusion), (s, ur) => Inclusion.Generate(s)),
            (60, typeof(Battenburg), (s, ur) => Battenburg.Generate(s)),

            // Other
            (null, typeof(ConsecutiveNeighbors), (s, ur) => ConsecutiveNeighbors.Generate(s)),
            (null, typeof(DoubleNeighbors), (s, ur) => DoubleNeighbors.Generate(s)),
            (13, typeof(LittleKiller), (s, ur) => LittleKiller.Generate(s))
        );

        public static implicit operator (int? probability, Type type, Func<int[], int[][], IList<SvgConstraint>> generator)(ConstraintGenerator value) => (value.probability, value.type, value.generator);
        public static implicit operator ConstraintGenerator((int? probability, Type type, Func<int[], int[][], IList<SvgConstraint>> generator) value) => new(value.probability, value.type, value.generator);
    }
}
