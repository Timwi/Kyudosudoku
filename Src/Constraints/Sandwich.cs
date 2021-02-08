using System;
using System.Collections.Generic;
using System.Linq;
using PuzzleSolvers;
using RT.Util;
using RT.Util.ExtensionMethods;

namespace KyudosudokuWebsite
{
    [KyuConstraintInfo("Sandwich")]
    sealed class Sandwich : KyuRowColConstraint
    {
        public override string Description => $"Within this {(IsCol ? "column" : "row")}, the digits sandwiched between the {Digit1} and the {Digit2} must add up to {Sum}. The {Digit1} and {Digit2} can occur in either order.";
        public override double ExtraTop => IsCol ? .25 : 0;
        public override bool ShownTopLeft => true;
        public static readonly Example Example = new Example
        {
            Constraints = { new Sandwich(false, 0, 3, 7, 17) },
            Cells = { 0, 1, 2, 3, 4, 5, 6, 7, 8 },
            Good = { 4, 1, 5, 3, 9, 8, 7, 6, 2 },
            Bad = { 4, 1, 5, 3, 9, 8, 6, 7, 2 },
            Reason = "The digits between the 3 and 7 are 9+8+6 = 23.",
            Wide = true
        };

        public Sandwich(bool isCol, int rowCol, int digit1, int digit2, int sum) : base(isCol, rowCol)
        {
            Digit1 = digit1;
            Digit2 = digit2;
            Sum = sum;
        }
        private Sandwich() { }    // for Classify

        public int Digit1 { get; private set; }
        public int Digit2 { get; private set; }
        public int Sum { get; private set; }

        protected override IEnumerable<Constraint> getConstraints() { yield return new SandwichUniquenessConstraint(Digit1, Digit2, Sum, Ut.NewArray(9, x => IsCol ? (RowCol + 9 * x) : (x + 9 * RowCol))); }

        public override bool Verify(int[] grid)
        {
            var numbers = Ut.NewArray(9, x => grid[IsCol ? (RowCol + 9 * x) : (x + 9 * RowCol)]);
            var p1 = Array.IndexOf(numbers, Digit1);
            var p2 = Array.IndexOf(numbers, Digit2);
            if (p1 == -1 || p2 == -1)
                return false;
            var pLeft = Math.Min(p1, p2);
            var pRight = Math.Max(p1, p2);
            var s = 0;
            for (var i = pLeft + 1; i < pRight; i++)
                s += numbers[i];
            return s == Sum;
        }

        public override string Svg => $@"<g transform='translate({(IsCol ? RowCol : -1.025)}, {(IsCol ? -.85 : RowCol)}) scale(.01)' font-size='23'>
  <linearGradient id='sandwich-6' x1='21.124' x2='124.084' y1='39.244' y2='39.244' gradientTransform='rotate(2.56 -380.023 -142.253)' gradientUnits='userSpaceOnUse'>
    <stop offset='.005' stop-color='#fbc02d'/>
    <stop offset='.081' stop-color='#fcca30'/>
    <stop offset='.209' stop-color='#fdd534'/>
    <stop offset='.36' stop-color='#fdd835'/>
    <stop offset='.69' stop-color='#fff59d'/>
    <stop offset='1' stop-color='#fdd835'/>
  </linearGradient>
  <g stroke-width='2.331' transform='translate(11, 9) scale(.8)'>
    <path fill='#df8826' d='M47.217 68.421c-1.222-1.21-2.814-1.51-4.526-2.115-4.89-1.36-9.416-2.415-14.187-4.38-1.347-.455-2.69-.605-4.036-.905a8.262 8.262 0 01-4.205-2.814c-.085.304-.133.639-.12 1.004.082 2.316 0 4.028.57 4.83.571.807 17.448 5.736 21.854 7.199 3.067 1.017 3.788 1.759 5.105 1.634-.167-1.694.725-3.307-.455-4.453z'/>
    <path fill='#f6d9a4' d='M66.458 52.218c.206-1.913-.244-2.012-2.608-2.518-2.364-.502-17.366-5.084-19.648-5.689-2.283-.605-3.587-1.107-5.054-.605s-3.097 1.51-3.097 1.51c-3.668 2.72-13.698 10.871-13.698 10.871s-1.622.785-2.081 2.42a8.262 8.262 0 004.204 2.814c1.347.3 2.69.455 4.037.905 4.77 1.965 9.296 3.02 14.187 4.38 1.643.58 3.363 1.068 4.565 2.162.489-.519 1.304-.399 1.823-.879 15.954-12.681 17.361-15.272 17.37-15.371z'/>
    <path fill='#d76208' d='M48.93 67.113c-.52.48-1.721 1.231-1.713 1.308.155 1.588.193 4.573.335 4.53 1.386-.403 4.728-3.144 7.34-5.057 2.609-1.914 11.497-10.43 11.742-11.137.236-.682.086-1.738.06-2.45a56.597 56.597 0 00-.189-3.303c.103 1.128-.635 1.879-1.265 2.47-5.127 4.793-10.335 8.817-16.31 13.639z'/>
    <path fill='#ea8a77' d='M20.31 55.487c-.613.678-.536 1.214.06 1.913 1.12 1.309 4.222 2.917 8.744 3.222 4.521.305 8.562-1.184 12.046-8.357 3.483-7.173-4.582-11.476-4.582-11.476S20.924 54.81 20.311 55.487z'/>
    <path fill='#a72e18' d='M20.963 55.839c-.683.63-.125 2.265 7.584 3.826 0 0-2.732-2.316-4.526-3.423-1.793-1.107-2.406-1.008-3.058-.403z'/>
    <path fill='#eaa88e' d='M27.77 57.602c-.613.678-.535 1.214.06 1.913 1.12 1.309 4.222 2.917 8.744 3.222 4.526.3 8.563-1.184 12.046-8.357 3.484-7.173-4.581-11.476-4.581-11.476S28.384 56.924 27.77 57.602z'/>
    <path fill='#a72e18' d='M28.423 57.954c-.682.63-.125 2.265 7.585 3.826 0 0-2.733-2.316-4.526-3.423-1.794-1.107-2.407-1.008-3.06-.403z'/>
    <path fill='#ea8a77' d='M36.625 60.184c-.682.756-.596 1.352.069 2.133 1.248 1.458 4.702 3.251 9.743 3.59 5.04.335 9.536-1.317 13.423-9.309 3.887-7.992-5.11-12.784-5.11-12.784s-17.443 15.611-18.125 16.37z'/>
    <path fill='#a72e18' d='M37.355 60.575c-.76.703-.138 2.522 8.447 4.26 0 0-3.042-2.578-5.041-3.814-2-1.236-2.681-1.12-3.406-.446z'/>
    <path fill='#9abc00' d='M67.9 45.719c.643-.155 1.14-.966 1.085-1.772-.172.399-.712.219-.88-.18-.167-.4-.111-.88-.141-1.33-.026-.45-.206-.97-.57-1.034-.31-.056-.58.249-.884.33-1.03.279-1.704-1.93-2.729-1.609-.279.086-.493.356-.72.58-1.065 1.038-2.57 1.12-3.887.716-1.317-.403-2.523-1.223-3.78-1.849-.67-.33-1.356-.605-2.05-.828 2.175 4.393 4.187 8.919 8.348 10.498 1.643.553 3.904.913 6.07.88.035-.194.052-.391.03-.589-.583-.24-1.308-.883-1.008-1.544.18-.39.652-.442.97-.219.317.219.519.627.707 1.013.532-.974.258-2.471-.562-3.063zM23.833 44.204a60.763 60.763 0 00-3.093 2.613c-1.017.935-1.309.793-2.463.279a2.214 2.214 0 00-.317.673c.145.168.308.288.476.425l-.828.905c-.125-.214-.326-.36-.54-.373-.04.776-.082 1.553-.12 2.325.347.133.737.082 1.054-.137-.171.652.013 1.428.447 1.857.356-.291.592-.802.609-1.33.369-.004.733.228.952.601l-.772 1.201c.215.472.716.712 1.133.536.205.653.562 1.227 1.016 1.64-.025-.365-.055-.734-.081-1.099l1.085-.296c-.026.485-.043 1.008.155 1.429.197.42.699.66 1 .343-.12.575.136 1.154.527 1.518.562-6.186 2.741-11.754 5.156-17.202-1.81 1.33-3.62 2.672-5.396 4.092z'/>
    <path fill='#c6d500' d='M39.783 38.172c-1.768-.596-3.514-1.32-5.307-1.02-1.54.257-2.921 1.256-4.256 2.235l-.986.72c-2.416 5.453-4.6 11.017-5.157 17.203.12.112.249.202.382.266.583.27 1.231.172 1.849.052.197-.562-.013-1.206-.022-1.82-.008-.613.464-1.346.893-1.038-.292 1.146.682 2.133 1.523 2.737.309.22.656.451 1 .335.158-.051.3-.176.463-.214.673-.15.94 1.145 1.574 1.454.665.322 1.317-1.013.777-1.587.785-.554 1.595.665 1.969 1.681.261.712.733 1.519 1.265 1.75 4.007-5.714 4.093-14.77 4.033-22.754z'/>
    <path fill='#9abc00' d='M55.231 61.201a1.263 1.263 0 01-.87-1.124v-.008c-.323-.455-.61-.91-.82-1.3-3.36-7.255-7.079-13.608-10.931-19.94-.957-.069-1.896-.348-2.827-.66.06 7.983-.026 17.043-4.037 22.753a.674.674 0 00.7-.073c-.147-.502.415-.905.84-.81.425.094.807.433 1.236.437.665.004 1.188-1 .905-1.742.476-.527 1.244-.06 1.656.55.407.609.738 1.394 1.347 1.634.51.202 1.05-.051 1.578-.184.524-.138 1.176-.09 1.455.476.189.382.15.905.39 1.235.253.343.678.326 1.047.404.828.175 1.48.926 2.214 1.437.729.51 1.767.716 2.325-.06-.494-.515-.464-1.61.06-2.077.523-.467 1.377-.158 1.617.584.069.214.095.459.223.63.249.33.7.16 1-.094.515-.433.914-1.055 1.158-1.763-.09-.099-.176-.202-.266-.305z'/>
    <path fill='#9abc00' d='M55.231 61.201c.086.103.172.202.258.3.03-.085.064-.163.09-.248a1.102 1.102 0 01-.348-.052zM47.303 38.224c-1.527.219-3.041.7-4.577.618-.039 0-.077-.009-.116-.009 3.852 6.332 7.572 12.686 10.93 19.94.211.39.498.845.82 1.3-.038-.7.665-1.33 1.137-.944-.283-.703-.575-1.45-.515-2.23.06-.781.63-1.554 1.253-1.4.618.155.991 1.103 1.617 1.116.335.004.6-.253.854-.553-.528-6.07-6.345-12.094-9.785-17.967-.532.009-1.077.052-1.618.129z'/>
    <path fill='#7a8e00' d='M61.692 49.236c-4.157-1.578-6.17-6.104-8.348-10.497a13.744 13.744 0 00-4.415-.644c3.436 5.869 9.254 11.896 9.786 17.967.347-.412.677-.901 1.136-.897.284.004.567.21.841.116.438-.15.532-.922.94-1.167.527-.317 1.291.408 1.69-.124.369-.494-.154-1.227-.618-1.575-.227-.836.704-1.493 1.403-1.334.704.159 1.334.72 2.046.725.768.004 1.455-.776 1.61-1.69-2.167.034-4.428-.326-6.071-.88z'/>
    <g transform='matrix(.429 0 0 .429 15.36 20.425)'>
      <linearGradient id='sandwich-1' x1='17.181' x2='45.393' y1='71.738' y2='73.757' gradientUnits='userSpaceOnUse'>
        <stop offset='0' stop-color='#ff3d2a'/>
        <stop offset='.041' stop-color='#f63424'/>
        <stop offset='.174' stop-color='#dd1d14'/>
        <stop offset='.314' stop-color='#cc0d09'/>
        <stop offset='.464' stop-color='#c10302'/>
        <stop offset='.64' stop-color='#be0000'/>
        <stop offset='.99' stop-color='#ff1500'/>
      </linearGradient>
      <ellipse cx='30.6' cy='72.7' fill='url(#sandwich-1)' rx='15.73' ry='8.68'/>
      <ellipse cx='31.02' cy='70.64' fill='#e44000' rx='15.3' ry='6.63'/>
      <ellipse cx='31.83' cy='69.76' fill='#891301' rx='14.49' ry='5.75'/>
    </g>
    <g transform='matrix(.429 0 0 .429 15.36 20.425)'>
      <linearGradient id='sandwich-2' x1='5.446' x2='33.749' y1='156.275' y2='158.301' gradientTransform='rotate(-9.633 -419.545 -82.064)' gradientUnits='userSpaceOnUse'>
        <stop offset='0' stop-color='#ff3d2a'/>
        <stop offset='.041' stop-color='#f63424'/>
        <stop offset='.174' stop-color='#dd1d14'/>
        <stop offset='.314' stop-color='#cc0d09'/>
        <stop offset='.464' stop-color='#c10302'/>
        <stop offset='.64' stop-color='#be0000'/>
        <stop offset='.99' stop-color='#ff1500'/>
      </linearGradient>
      <path fill='url(#sandwich-2)' d='M68.24 77.31c.65 4.73-5.77 10.01-14.33 11.81-8.56 1.8-16.03-.59-16.68-5.31C36.58 79.08 43 73.79 51.56 72c8.56-1.79 16.03.58 16.68 5.31z'/>
      <path fill='#e44000' d='M67.96 75.29c.5 3.61-5.86 7.95-14.19 9.69-8.33 1.74-15.49.23-15.98-3.38-.5-3.61 5.86-7.95 14.19-9.69 8.33-1.74 15.49-.23 15.98 3.38z'/>
      <path fill='#891301' d='M67.85 74.42c.43 3.13-5.62 7.01-13.51 8.66-7.89 1.65-14.64.45-15.07-2.68-.43-3.13 5.62-7.01 13.51-8.66 7.89-1.65 14.64-.45 15.07 2.68z'/>
    </g>
    <g transform='matrix(.429 0 0 .429 15.36 20.425)'>
      <linearGradient id='sandwich-3' x1='64.886' x2='93.097' y1='78.777' y2='80.796' gradientUnits='userSpaceOnUse'>
        <stop offset='0' stop-color='#ff3d2a'/>
        <stop offset='.041' stop-color='#f63424'/>
        <stop offset='.174' stop-color='#dd1d14'/>
        <stop offset='.314' stop-color='#cc0d09'/>
        <stop offset='.464' stop-color='#c10302'/>
        <stop offset='.64' stop-color='#be0000'/>
        <stop offset='.99' stop-color='#ff1500'/>
      </linearGradient>
      <ellipse cx='78.3' cy='79.74' fill='url(#sandwich-3)' rx='15.73' ry='8.68'/>
      <ellipse cx='78.73' cy='77.68' fill='#e44000' rx='15.3' ry='6.63'/>
      <ellipse cx='78.39' cy='76.8' fill='#891301' rx='13.54' ry='5.51'/>
    </g>
    <g transform='matrix(.429 0 0 .429 15.36 20.425)'>
      <linearGradient id='sandwich-4' x1='76.479' x2='104.69' y1='68.922' y2='70.941' gradientUnits='userSpaceOnUse'>
        <stop offset='0' stop-color='#ff3d2a'/>
        <stop offset='.041' stop-color='#f63424'/>
        <stop offset='.174' stop-color='#dd1d14'/>
        <stop offset='.314' stop-color='#cc0d09'/>
        <stop offset='.464' stop-color='#c10302'/>
        <stop offset='.64' stop-color='#be0000'/>
        <stop offset='.99' stop-color='#ff1500'/>
      </linearGradient>
      <ellipse cx='89.89' cy='69.88' fill='url(#sandwich-4)' rx='15.73' ry='8.68'/>
      <ellipse cx='90.32' cy='67.83' fill='#e44000' rx='15.3' ry='6.63'/>
      <ellipse cx='89.99' cy='66.95' fill='#891301' rx='13.54' ry='5.51'/>
    </g>
    <g transform='matrix(.429 0 0 .429 15.36 20.425)'>
      <linearGradient id='sandwich-5' x1='93.584' x2='121.796' y1='54.139' y2='56.158' gradientUnits='userSpaceOnUse'>
        <stop offset='0' stop-color='#ff3d2a'/>
        <stop offset='.041' stop-color='#f63424'/>
        <stop offset='.174' stop-color='#dd1d14'/>
        <stop offset='.314' stop-color='#cc0d09'/>
        <stop offset='.464' stop-color='#c10302'/>
        <stop offset='.64' stop-color='#be0000'/>
        <stop offset='.99' stop-color='#ff1500'/>
      </linearGradient>
      <ellipse cx='107' cy='55.1' fill='url(#sandwich-5)' rx='15.73' ry='8.68'/>
      <ellipse cx='107.43' cy='53.05' fill='#e44000' rx='15.3' ry='6.63'/>
      <ellipse cx='107.09' cy='52.17' fill='#891301' rx='13.54' ry='5.51'/>
    </g>
    <path fill='url(#sandwich-6)' d='M13.89 60.54c-.62.57-1.28 1.2-1.52 2.11-.41 1.52.55 3.16 1.72 3.87 1.17.72 2.51.82 3.76 1.26 6 2.08 8.96 11.14 14.9 13.48 4.19 1.65 8.66-.48 13.01-1.24 5.66-.98 11.59.47 16.51 4.06 4.01 2.92 8.1 7.41 12.64 6.13 3.02-.85 5.14-4.05 7.51-6.52 3.86-4.03 8.85-6.38 13.91-6.53 1.56-.05 3.16.1 4.66-.44 2.46-.89 4.28-3.53 5.43-6.36 1.15-2.83 1.79-5.94 2.82-8.85 1.38-3.92 3.47-7.45 6.07-10.27.27-.29.59-2.06.68-2.48.14-.71-.37.09-.83-.37-3.1-3.08-7.27-3.8-11.16-4.84-10.3-2.73-19.95-8.33-30.08-11.88-10.13-3.55-21.39-4.91-30.91.66-2.52 1.48-3.88 3.38-5.54 5.98-1.85 2.89-4.65 4.87-7.03 7.05-5.53 5.08-11.04 10.13-16.55 15.18z' transform='matrix(.429 0 0 .429 15.36 20.425)'/>
    <g transform='matrix(.429 0 0 .429 15.36 20.425)'>
      <linearGradient id='sandwich-7' x1='9.732' x2='74.285' y1='67.527' y2='67.527' gradientUnits='userSpaceOnUse'>
        <stop offset='0' stop-color='#ffc044'/>
        <stop offset='.847' stop-color='#d95f23'/>
      </linearGradient>
      <path fill='url(#sandwich-7)' d='M72.84 74.22c-2.85-2.82-6.56-3.52-10.55-4.93-11.4-3.17-21.95-5.63-33.07-10.21-3.14-1.06-6.27-1.41-9.41-2.11-3.74-1.09-7.13-3.22-9.8-6.56-.2.71-.31 1.49-.28 2.34.19 5.4 0 9.39 1.33 11.26C12.4 65.89 51.74 77.39 62 80.79c7.15 2.37 8.83 4.1 11.9 3.81-.4-3.95 1.69-7.71-1.06-10.38z'/>
      <linearGradient id='sandwich-8' x1='10.014' x2='117.791' y1='44.887' y2='44.887' gradientUnits='userSpaceOnUse'>
        <stop offset='0' stop-color='#fff2d9'/>
        <stop offset='.847' stop-color='#f7d398'/>
      </linearGradient>
      <path fill='url(#sandwich-8)' d='M117.69 36.45c.48-4.46-.57-4.69-6.08-5.87-5.51-1.17-40.48-11.85-45.8-13.26-5.32-1.41-8.36-2.58-11.78-1.41-3.42 1.17-7.22 3.52-7.22 3.52-8.55 6.34-31.93 25.34-31.93 25.34s-3.78 1.83-4.85 5.64c2.67 3.34 6.06 5.47 9.8 6.56 3.14.7 6.27 1.06 9.41 2.11 11.12 4.58 21.67 7.04 33.07 10.21 3.83 1.35 7.84 2.49 10.64 5.04 1.14-1.21 3.04-.93 4.25-2.05 37.19-29.56 40.46-35.6 40.49-35.83z'/>
      <linearGradient id='sandwich-9' x1='72.836' x2='118.397' y1='59.201' y2='59.201' gradientUnits='userSpaceOnUse'>
        <stop offset='0' stop-color='#ffb219'/>
        <stop offset='.847' stop-color='#ca4300'/>
      </linearGradient>
      <path fill='url(#sandwich-9)' d='M76.83 71.17c-1.21 1.12-4.01 2.87-3.99 3.05.36 3.7.45 10.66.78 10.56 3.23-.94 11.02-7.33 17.11-11.79 6.08-4.46 26.8-24.31 27.37-25.96.55-1.59.2-4.05.14-5.71-.09-2.56-.2-5.15-.44-7.7.24 2.63-1.48 4.38-2.95 5.76-11.95 11.17-24.09 20.55-38.02 31.79z'/>
    </g>
  </g>
  <text x='20' y='47.192' text-anchor='end'>{Digit1}</text>
  <text x='20' y='67.84' text-anchor='end'>{Digit2}</text>
  <text x='70' y='57.608' text-anchor='start'>{Sum}</text>
</g>";

        public static IList<KyuConstraint> Generate(int[] sudoku)
        {
            var constraints = new List<KyuConstraint>();
            foreach (var isCol in new[] { false, true })
                for (var rowCol = 0; rowCol < 9; rowCol++)
                    for (var digit1 = 1; digit1 <= 9; digit1++)
                        for (var digit2 = digit1 + 1; digit2 <= 9; digit2++)
                        {
                            var digits = Enumerable.Range(0, 9).Select(x => sudoku[isCol ? (rowCol + 9 * x) : (x + 9 * rowCol)]).ToArray();
                            var p1 = Array.IndexOf(digits, digit1);
                            var p2 = Array.IndexOf(digits, digit2);
                            var pLeft = Math.Min(p1, p2);
                            var pRight = Math.Max(p1, p2);
                            var sandwichSum = 0;
                            for (var i = pLeft + 1; i < pRight; i++)
                                sandwichSum += digits[i];
                            constraints.Add(new Sandwich(isCol, rowCol, digit1, digit2, sandwichSum));
                        }
            return constraints;
        }
    }
}
