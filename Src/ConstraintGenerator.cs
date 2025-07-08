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
            (null, typeof(AntiBishop), (s, _) => AntiBishop.Generate(s)),
            (null, typeof(AntiKing), (s, _) => AntiKing.Generate(s)),
            (null, typeof(AntiKnight), (s, _) => AntiKnight.Generate(s)),
            (null, typeof(NoConsecutive), (s, _) => NoConsecutive.Generate(s)),
            (null, typeof(MaximumCell), (s, _) => MaximumCell.Generate(s)),
            (null, typeof(MinimumCell), (s, _) => MinimumCell.Generate(s)),
            (null, typeof(FindThe9), (s, _) => FindThe9.Generate(s)),
            (null, typeof(OddEven), (s, _) => OddEven.Generate(s)),
            (null, typeof(Means), (s, _) => Means.Generate(s)),

            // Area constraints
            (20, typeof(Arrow), (s, _) => Arrow.Generate(s)),
            (20, typeof(KillerCage), KillerCage.Generate),
            (12, typeof(Palindrome), (s, _) => Palindrome.Generate(s)),
            (20, typeof(CappedLine), (s, _) => CappedLine.Generate(s)),
            (20, typeof(RenbanCage), RenbanCage.Generate),
            (17, typeof(Snowball), (s, _) => Snowball.Generate(s)),
            (30, typeof(Thermometer), (s, _) => Thermometer.Generate(s)),
            (15, typeof(GermanWhisper), (s, _) => GermanWhisper.Generate(s)),
            (15, typeof(ASum), (s, _) => ASum.Generate(s)),

            // Row/column constraints
            (37, typeof(Battlefield), (s, _) => Battlefield.Generate(s)),
            (null, typeof(Binairo), (s, _) => Binairo.Generate(s)),
            (20, typeof(Sandwich), (s, _) => Sandwich.Generate(s)),
            (47, typeof(Skyscraper), (s, _) => Skyscraper.Generate(s)),
            (20, typeof(SkyscraperSum), (s, _) => SkyscraperSum.Generate(s)),
            (20, typeof(ToroidalSandwich), (s, _) => ToroidalSandwich.Generate(s)),
            (20, typeof(XSum), (s, _) => XSum.Generate(s)),
            (15, typeof(YSum), (s, _) => YSum.Generate(s)),

            // Four-cell constraints
            (50, typeof(Clockface), (s, _) => Clockface.Generate(s)),
            (40, typeof(Inclusion), (s, _) => Inclusion.Generate(s)),
            (60, typeof(Battenburg), (s, _) => Battenburg.Generate(s)),

            // Other
            (null, typeof(ConsecutiveNeighbors), (s, _) => ConsecutiveNeighbors.Generate(s)),
            (null, typeof(DoubleNeighbors), (s, _) => DoubleNeighbors.Generate(s)),
            (13, typeof(LittleKiller), (s, _) => LittleKiller.Generate(s)),
            (13, typeof(LittleSandwich), (s, _) => LittleSandwich.Generate(s))
        );

        public static implicit operator (int? probability, Type type, Func<int[], int[][], IList<SvgConstraint>> generator)(ConstraintGenerator value) => (value.probability, value.type, value.generator);
        public static implicit operator ConstraintGenerator((int? probability, Type type, Func<int[], int[][], IList<SvgConstraint>> generator) value) => new(value.probability, value.type, value.generator);
    }
}
