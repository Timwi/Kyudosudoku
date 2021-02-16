using System;
using System.Collections.Generic;
using System.Linq;
using PuzzleSolvers;
using RT.Util;
using RT.Util.ExtensionMethods;

namespace KyudosudokuWebsite
{
    [KyuConstraintInfo("Toroidal sandwich")]
    sealed class ToroidalSandwich : KyuRowColConstraint
    {
        public override string Description => $"Within this {(IsCol ? "column" : "row")}, the digits after the {Digit1} and before the {Digit2}, wrapping around the edges of the grid if necessary, must add up to {Sum}.";
        public override double ExtraTop => IsCol ? .25 : 0;
        public override double ExtraLeft => IsCol ? 0 : .5;
        public override bool ShownTopLeft => true;
        public static readonly Example Example = new Example
        {
            Constraints = { new ToroidalSandwich(false, 0, 3, 7, 17) },
            Cells = { 0, 1, 2, 3, 4, 5, 6, 7, 8 },
            Good = { 4, 1, 5, 3, 9, 8, 7, 6, 2 },
            Bad = { 4, 1, 5, 7, 9, 8, 6, 3, 2 },
            Reason = "The digits after the 3 and before the 7 are 2+4+1+5 = 12.",
            Wide = true
        };

        public ToroidalSandwich(bool isCol, int rowCol, int digit1, int digit2, int sum) : base(isCol, rowCol)
        {
            Digit1 = digit1;
            Digit2 = digit2;
            Sum = sum;
        }
        private ToroidalSandwich() { }    // for Classify

        public int Digit1 { get; private set; }
        public int Digit2 { get; private set; }
        public int Sum { get; private set; }

        protected override IEnumerable<Constraint> getConstraints() { yield return new SandwichWraparoundUniquenessConstraint(Digit1, Digit2, Sum, Ut.NewArray(9, x => IsCol ? (RowCol + 9 * x) : (x + 9 * RowCol))); }

        public override bool Verify(int[] grid)
        {
            var numbers = Ut.NewArray(9, x => grid[IsCol ? (RowCol + 9 * x) : (x + 9 * RowCol)]);
            var p1 = Array.IndexOf(numbers, Digit1);
            var p2 = Array.IndexOf(numbers, Digit2);
            if (p1 == -1 || p2 == -1)
                return false;
            var s = 0;
            var i = (p1 + 1) % numbers.Length;
            while (i != p2)
            {
                s += numbers[i];
                i = (i + 1) % numbers.Length;
            }
            return s == Sum;
        }

        public override string Svg => $@"<g transform='translate({(IsCol ? RowCol : -1.025)}, {(IsCol ? -.85 : RowCol)}) scale(.01)' font-size='23'>
  <linearGradient id='d' x1='9.732' x2='74.285' y1='67.527' y2='67.527' gradientTransform='matrix(.429 0 0 .429 53.625 57.976)' gradientUnits='userSpaceOnUse'>
    <stop offset='0' stop-color='#ffc044'/>
    <stop offset='.849' stop-color='#d95f23'/>
  </linearGradient>
  <linearGradient id='c'>
    <stop offset='0' stop-color='#e09c3f'/>
    <stop offset='1' stop-color='#dd7f00'/>
  </linearGradient>
  <linearGradient id='l' x1='5.446' x2='33.749' y1='156.275' y2='158.301' gradientTransform='rotate(-9.633 -419.545 -82.064)' gradientUnits='userSpaceOnUse' xlink:href='#a'/>
  <linearGradient id='m' x1='93.584' x2='121.796' y1='54.139' y2='56.158' gradientUnits='userSpaceOnUse' xlink:href='#b'/>
  <linearGradient id='k' x1='93.584' x2='121.796' y1='54.139' y2='56.158' gradientUnits='userSpaceOnUse' xlink:href='#b'/>
  <linearGradient id='h' x1='5.446' x2='33.749' y1='156.275' y2='158.301' gradientTransform='rotate(-9.633 -419.545 -82.064)' gradientUnits='userSpaceOnUse' xlink:href='#a'/>
  <linearGradient id='n' x1='47.621' x2='38.754' y1='24.526' y2='52.588' gradientUnits='userSpaceOnUse' xlink:href='#c'/>
  <linearGradient id='e' x1='-69.95' x2='-17.528' y1='10.917' y2='18.766' gradientTransform='matrix(.429 0 0 .429 53.625 57.976)' gradientUnits='userSpaceOnUse' xlink:href='#d'/>
  <filter id='p' color-interpolation-filters='sRGB'>
    <feGaussianBlur result='fbSourceGraphic' stdDeviation='1 1'/>
  </filter>
  <filter id='o' width='1.25' height='1.32' x='-.14' y='-.17' color-interpolation-filters='sRGB'>
    <feGaussianBlur result='blur' stdDeviation='2.7'/>
  </filter>
  <linearGradient id='f' x1='21.124' x2='124.084' y1='39.244' y2='39.244' gradientTransform='matrix(.4836 .01916 -.02162 .42857 8.772 28.87)' gradientUnits='userSpaceOnUse'>
    <stop offset='.005' stop-color='#fbc02d'/>
    <stop offset='.081' stop-color='#fcca30'/>
    <stop offset='.209' stop-color='#fdd534'/>
    <stop offset='.36' stop-color='#fdd835'/>
    <stop offset='.69' stop-color='#fff59d'/>
    <stop offset='1' stop-color='#fdd835'/>
  </linearGradient>
  <g stroke-width='1.214'>
    <path fill='#f6d9a4' d='M27.218 55.088c0 4.595 7.282 9.598 19.975 9.468 12.692-.13 17.5-8.91 16.968-12.611'/>
    <path fill='url(#e)' d='M20.2 53.403c0 5.579 8.842 11.654 24.255 11.496 15.412-.158 21.25-10.818 20.604-15.313 0 0 1.14 16.929-21.708 17.447-22.848.518-23.152-13.63-23.152-13.63z' transform='translate(10.583 11.08) scale(.82353)'/>
    <path fill='#ea8a77' d='M27.309 56.775c-.505.559-.442 1 .049 1.576.922 1.078 3.477 2.402 7.2 2.653 3.724.251 7.052-.975 9.921-6.882 2.869-5.907-3.773-9.45-3.773-9.45S27.814 56.217 27.309 56.774z'/>
    <path fill='#a72e18' d='M27.846 57.065c-.562.52-.103 1.866 6.246 3.151 0 0-2.25-1.907-3.727-2.819-1.477-.911-1.982-.83-2.519-.332z'/>
    <path fill='#eaa88e' d='M33.956 58.685c-.505.559-.44 1 .05 1.576.922 1.078 3.476 2.402 7.2 2.653 3.728.247 7.052-.975 9.92-6.882 2.87-5.907-3.771-9.451-3.771-9.451S34.462 58.127 33.956 58.685z'/>
    <path fill='#a72e18' d='M34.494 58.975c-.562.519-.103 1.865 6.246 3.15 0 0-2.25-1.906-3.727-2.818-1.477-.912-1.982-.83-2.52-.332z'/>
    <path fill='#ea8a77' d='M42.173 60.728c-.561.622-.49 1.113.057 1.756 1.028 1.2 3.872 2.677 8.024 2.957 4.15.276 7.853-1.085 11.054-7.667 3.201-6.581-4.208-10.528-4.208-10.528S42.735 60.103 42.173 60.728z'/>
    <path fill='#a72e18' d='M42.774 61.05c-.625.579-.113 2.077 6.957 3.508 0 0-2.505-2.123-4.152-3.141-1.647-1.018-2.207-.922-2.805-.367z'/>
    <path fill='#9abc00' d='M66.5 48.731c.53-.127.94-.795.894-1.46-.142.33-.586.181-.725-.147-.137-.33-.091-.725-.116-1.096-.021-.37-.17-.798-.47-.851-.255-.046-.477.205-.727.272-.849.23-1.404-1.59-2.248-1.325-.23.07-.406.293-.593.477-.877.855-2.116.923-3.2.59-1.085-.332-2.079-1.007-3.114-1.523a12.418 12.418 0 00-1.688-.682c1.791 3.618 3.448 7.345 6.875 8.646 1.353.455 3.215.752 4.999.724a1.68 1.68 0 00.024-.485c-.48-.197-1.077-.727-.83-1.271.149-.321.537-.364.8-.18.26.18.427.516.581.834.438-.802.213-2.035-.462-2.523zm-36.29-1.247a50.04 50.04 0 00-2.547 2.151c-.838.77-1.078.653-2.029.23a1.823 1.823 0 00-.26.554c.119.139.253.238.391.35l-.682.746c-.103-.176-.268-.297-.444-.307-.033.639-.068 1.279-.1 1.914.287.11.608.068.869-.113-.141.537.01 1.176.368 1.53.293-.24.487-.66.501-1.096.304-.003.604.188.784.495l-.635.99c.177.388.59.586.933.44.169.539.463 1.011.836 1.351-.02-.3-.045-.604-.066-.905l.893-.243c-.021.399-.035.83.128 1.176.162.346.576.544.823.283-.098.473.112.95.434 1.25.463-5.094 2.258-9.68 4.247-14.166-1.491 1.095-2.982 2.2-4.444 3.37z'/>
    <path fill='#c6d500' d='M43.345 42.516c-1.456-.49-2.894-1.087-4.37-.84-1.269.212-2.406 1.034-3.505 1.84l-.812.594c-1.99 4.49-3.789 9.072-4.247 14.167.099.092.205.166.314.219.48.222 1.014.141 1.523.043.162-.463-.01-.994-.018-1.5-.007-.504.382-1.108.735-.854-.24.944.562 1.757 1.255 2.254.254.181.54.371.823.276.13-.042.247-.145.381-.176.555-.124.775.943 1.297 1.197.547.265 1.084-.834.64-1.307.646-.456 1.313.548 1.621 1.384.215.587.604 1.251 1.042 1.442 3.3-4.706 3.37-12.164 3.321-18.739z'/>
    <path fill='#9abc00' d='M56.067 61.481a1.04 1.04 0 01-.716-.926v-.006c-.266-.375-.503-.75-.676-1.07-2.767-5.975-5.83-11.207-9.002-16.422-.788-.057-1.561-.287-2.328-.543.05 6.574-.021 14.035-3.324 18.737a.555.555 0 00.576-.06c-.121-.413.342-.745.692-.667.35.078.664.357 1.018.36.547.003.978-.823.745-1.435.392-.434 1.024-.049 1.364.453.335.502.608 1.148 1.109 1.346.42.166.865-.042 1.3-.151.431-.114.968-.075 1.198.392.155.314.123.745.321 1.017.208.282.558.268.862.332.682.144 1.219.763 1.824 1.184.6.42 1.455.59 1.914-.05-.407-.424-.382-1.326.05-1.71.43-.385 1.134-.13 1.331.48.057.177.079.379.184.52.205.271.576.131.823-.078.425-.356.753-.869.954-1.452-.074-.081-.145-.166-.219-.25z'/>
    <path fill='#9abc00' d='M56.067 61.481c.07.085.142.166.213.247.024-.07.052-.134.074-.204a.908.908 0 01-.287-.043zM49.538 42.56c-1.257.18-2.504.576-3.77.509-.031 0-.063-.008-.095-.008 3.173 5.215 6.236 10.448 9.001 16.422.174.32.41.695.676 1.07-.032-.576.547-1.095.936-.777-.233-.58-.473-1.194-.424-1.837.05-.643.519-1.28 1.032-1.153.509.128.816.909 1.332.92.275.003.494-.209.703-.456-.435-4.999-5.225-9.96-8.058-14.796-.439.007-.887.042-1.333.106z'/>
    <path fill='#7a8e00' d='M61.388 51.628c-3.424-1.3-5.081-5.027-6.875-8.645a11.319 11.319 0 00-3.636-.53c2.83 4.833 7.621 9.796 8.06 14.796.285-.34.557-.742.935-.739.234.004.467.173.692.096.361-.124.438-.76.774-.961.434-.261 1.064.336 1.392-.102.304-.407-.127-1.01-.509-1.297-.187-.689.58-1.23 1.156-1.099.58.13 1.098.593 1.685.597.632.003 1.198-.639 1.326-1.392-1.785.028-3.647-.268-5-.724z'/>
    <path fill='url(#f)' d='M18.755 46.621c-.3.245-.62.515-.736.905-.198.652.266 1.356.833 1.66.566.31 1.215.352 1.82.541 2.904.892 4.337 5.78 7.213 6.783 2.028.708 4.192-.206 6.297-.532 2.74-.42 5.611.202 7.993 1.742 1.94 1.252 3.92 3.179 6.119 2.63 1.461-.365 2.488-1.738 3.635-2.798 1.869-1.728 4.284-2.737 6.734-2.8.755-.022 1.53.042 2.255-.19 1.191-.381 2.072-1.514 2.629-2.728.557-1.214.866-2.548 1.365-3.797a12.552 12.552 0 012.938-4.406c.131-.124.286-.883.33-1.063.067-.305-.18.038-.402-.16-1.5-1.32-3.52-1.63-5.403-2.076-4.986-1.17-9.657-3.573-14.56-5.096-4.904-1.523-10.355-2.106-14.964.283-1.22.635-1.878 1.45-2.682 2.565-.895 1.24-2.25 2.09-3.403 3.025-2.677 2.18-5.344 3.346-8.011 5.512z' transform='translate(10.583 11.08) scale(.82353)'/>
    <g stroke-width='2.83' transform='matrix(.3533 0 0 .3533 21.383 23.212)'>
      <linearGradient id='g' x1='17.181' x2='45.393' y1='71.738' y2='73.757' gradientUnits='userSpaceOnUse'>
        <stop offset='0' stop-color='#ff3d2a'/>
        <stop offset='.041' stop-color='#f63424'/>
        <stop offset='.174' stop-color='#dd1d14'/>
        <stop offset='.314' stop-color='#cc0d09'/>
        <stop offset='.464' stop-color='#c10302'/>
        <stop offset='.64' stop-color='#be0000'/>
        <stop offset='.99' stop-color='#ff1500'/>
      </linearGradient>
      <ellipse cx='30.6' cy='72.7' fill='url(#g)' rx='15.73' ry='8.68'/>
      <ellipse cx='31.02' cy='70.64' fill='#e44000' rx='15.3' ry='6.63'/>
      <ellipse cx='31.83' cy='69.76' fill='#891301' rx='14.49' ry='5.75'/>
    </g>
    <g stroke-width='2.83' transform='matrix(.3533 0 0 .3533 22.728 25.228)'>
      <linearGradient id='a' x1='5.446' x2='33.749' y1='156.275' y2='158.301' gradientTransform='rotate(-9.633 -419.545 -82.064)' gradientUnits='userSpaceOnUse'>
        <stop offset='0' stop-color='#ff3d2a'/>
        <stop offset='.041' stop-color='#f63424'/>
        <stop offset='.174' stop-color='#dd1d14'/>
        <stop offset='.314' stop-color='#cc0d09'/>
        <stop offset='.464' stop-color='#c10302'/>
        <stop offset='.64' stop-color='#be0000'/>
        <stop offset='.99' stop-color='#ff1500'/>
      </linearGradient>
      <path fill='url(#h)' d='M68.24 77.31c.65 4.73-5.77 10.01-14.33 11.81-8.56 1.8-16.03-.59-16.68-5.31C36.58 79.08 43 73.79 51.56 72c8.56-1.79 16.03.58 16.68 5.31z'/>
      <path fill='#e44000' d='M67.96 75.29c.5 3.61-5.86 7.95-14.19 9.69-8.33 1.74-15.49.23-15.98-3.38-.5-3.61 5.86-7.95 14.19-9.69 8.33-1.74 15.49-.23 15.98 3.38z'/>
      <path fill='#891301' d='M67.85 74.42c.43 3.13-5.62 7.01-13.51 8.66-7.89 1.65-14.64.45-15.07-2.68-.43-3.13 5.62-7.01 13.51-8.66 7.89-1.65 14.64-.45 15.07 2.68z'/>
    </g>
    <g stroke-width='2.83' transform='matrix(.3533 0 0 .3533 23.568 25.9)'>
      <linearGradient id='i' x1='64.886' x2='93.097' y1='78.777' y2='80.796' gradientUnits='userSpaceOnUse'>
        <stop offset='0' stop-color='#ff3d2a'/>
        <stop offset='.041' stop-color='#f63424'/>
        <stop offset='.174' stop-color='#dd1d14'/>
        <stop offset='.314' stop-color='#cc0d09'/>
        <stop offset='.464' stop-color='#c10302'/>
        <stop offset='.64' stop-color='#be0000'/>
        <stop offset='.99' stop-color='#ff1500'/>
      </linearGradient>
      <ellipse cx='78.3' cy='79.74' fill='url(#i)' rx='15.73' ry='8.68'/>
      <ellipse cx='78.73' cy='77.68' fill='#e44000' rx='15.3' ry='6.63'/>
      <ellipse cx='78.39' cy='76.8' fill='#891301' rx='13.54' ry='5.51'/>
    </g>
    <g stroke-width='2.83' transform='matrix(.3533 0 0 .3533 24.392 25.9)'>
      <linearGradient id='j' x1='76.479' x2='104.69' y1='68.922' y2='70.941' gradientUnits='userSpaceOnUse'>
        <stop offset='0' stop-color='#ff3d2a'/>
        <stop offset='.041' stop-color='#f63424'/>
        <stop offset='.174' stop-color='#dd1d14'/>
        <stop offset='.314' stop-color='#cc0d09'/>
        <stop offset='.464' stop-color='#c10302'/>
        <stop offset='.64' stop-color='#be0000'/>
        <stop offset='.99' stop-color='#ff1500'/>
      </linearGradient>
      <ellipse cx='89.89' cy='69.88' fill='url(#j)' rx='15.73' ry='8.68'/>
      <ellipse cx='90.32' cy='67.83' fill='#e44000' rx='15.3' ry='6.63'/>
      <ellipse cx='89.99' cy='66.95' fill='#891301' rx='13.54' ry='5.51'/>
    </g>
    <g stroke-width='2.83' transform='matrix(.3533 0 0 .3533 21.921 26.724)'>
      <linearGradient id='b' x1='93.584' x2='121.796' y1='54.139' y2='56.158' gradientUnits='userSpaceOnUse'>
        <stop offset='0' stop-color='#ff3d2a'/>
        <stop offset='.041' stop-color='#f63424'/>
        <stop offset='.174' stop-color='#dd1d14'/>
        <stop offset='.314' stop-color='#cc0d09'/>
        <stop offset='.464' stop-color='#c10302'/>
        <stop offset='.64' stop-color='#be0000'/>
        <stop offset='.99' stop-color='#ff1500'/>
      </linearGradient>
      <ellipse cx='107' cy='55.1' fill='url(#k)' rx='15.73' ry='8.68'/>
      <ellipse cx='107.43' cy='53.05' fill='#e44000' rx='15.3' ry='6.63'/>
      <ellipse cx='107.09' cy='52.17' fill='#891301' rx='13.54' ry='5.51'/>
    </g>
    <g stroke-width='2.83'>
      <path fill='url(#l)' d='M68.24 77.31c.65 4.73-5.77 10.01-14.33 11.81-8.56 1.8-16.03-.59-16.68-5.31C36.58 79.08 43 73.79 51.56 72c8.56-1.79 16.03.58 16.68 5.31z' transform='matrix(.3533 0 0 .3533 34.274 13.128)'/>
      <path fill='#e44000' d='M58.284 39.728c.177 1.274-2.07 2.808-5.014 3.423-2.942.614-5.472.08-5.645-1.194-.176-1.277 2.07-2.808 5.014-3.424 2.942-.615 5.472-.082 5.645 1.195z'/>
      <path fill='#891301' d='M58.245 39.42c.152 1.106-1.985 2.477-4.773 3.06-2.788.582-5.172.159-5.324-.947-.152-1.106 1.986-2.477 4.773-3.06 2.788-.583 5.172-.159 5.324.947z'/>
    </g>
    <g stroke-width='2.83' transform='matrix(.3533 0 0 .3533 .51 23.43)'>
      <ellipse cx='107' cy='55.1' fill='url(#m)' rx='15.73' ry='8.68'/>
      <ellipse cx='107.43' cy='53.05' fill='#e44000' rx='15.3' ry='6.63'/>
      <ellipse cx='107.09' cy='52.17' fill='#891301' rx='13.54' ry='5.51'/>
    </g>
    <path fill='url(#n)' d='M43.028 24.8c-12.275.166-22.754 9.697-22.754 15.618.001 5.92 8.763 12.368 24.035 12.2 15.273-.167 21.059-11.48 20.418-16.25-.63-4.693-9.423-11.732-21.699-11.567zm1.258 12.14c6.939.102 9.285 3.468 9.285 3.468s-2.244 3.571-9.386 3.47c-7.143-.103-9.899-2.96-9.899-2.96s3.061-4.08 10-3.978z' transform='translate(10.583 11.08) scale(.82353)'/>
    <path fill='#f7d398' fill-rule='evenodd' d='M48.946 28.395l-.193.004c-2.08.03-4.246-.068-6.475.048l-.092.006-.091.016c-1.667.296-3.543.421-5.498 1.098-2.34.624-4.198 1.835-5.97 2.619l.231-.08c-1.303.337-2.206 1.187-2.853 1.963-.647.775-1.122 1.518-1.555 1.98l.092-.09c-1.628 1.468-1.587 3.549-1.498 4.967.031 1.244.755 2.237 1.47 2.828.719.594 1.466.947 1.983 1.234l.004.002c1.382.824 2.794 1.788 4.453 2.56l-.248-.147c1.68 1.22 3.595 1.232 4.996 1.347l-.213-.033c1.883.435 3.777.51 5.633.432l-.092.002c1.334.024 2.961.477 4.93.298 1.606.106 3.252-.143 4.77-.894 1.59-.573 2.999-1.38 4.296-2.145l.198-.117.154-.17c1.119-1.238 2.123-2.722 2.617-4.54.346-1.146.852-2.428 1.01-3.942l.004-.041.002-.041c.11-2.244-1.307-3.909-2.633-4.975-1.42-1.41-3.121-2.172-4.736-2.73l-.094-.032-.096-.02c-1.142-.232-2.31-.704-3.625-1.169l-.039-.014-.039-.011c-.206-.06-.411-.116-.617-.166zm-.36 3.006c.102.026.202.05.302.08l.015.004c1.201.427 2.471.956 3.95 1.257l-.192-.052c1.469.507 2.708 1.094 3.658 2.06l.065.067.074.056c.908.719 1.573 1.7 1.549 2.512-.113.99-.51 2.09-.893 3.363l-.006.02-.006.021c-.313 1.16-.971 2.18-1.816 3.147-1.236.725-2.44 1.415-3.652 1.84l-.094.033-.09.045c-1.033.525-2.177.707-3.402.609l-.139-.01-.139.014c-1.296.137-2.796-.282-4.683-.316h-.047l-.045.002c-1.685.071-3.305-.007-4.832-.36l-.106-.023-.107-.01c-1.48-.122-2.757-.259-3.482-.785l-.117-.086-.131-.06c-1.382-.643-2.725-1.55-4.223-2.44l-.02-.012-.019-.01c-.612-.34-1.195-.647-1.531-.925-.337-.279-.385-.364-.389-.592v-.033l-.002-.036c-.082-1.283-.033-2.1.514-2.593l.047-.043.043-.045c.713-.762 1.2-1.547 1.67-2.11.469-.562.837-.86 1.302-.98l.117-.031.114-.05c2.082-.92 3.815-2.025 5.572-2.484l.06-.015.06-.022c1.45-.51 3.133-.642 4.985-.964 1.943-.097 3.976-.02 6.067-.043z' color='#000' enable-background='accumulate' filter='url(#o)' font-family='sans-serif' font-weight='400' overflow='visible' style='line-height:normal;font-variant-ligatures:normal;font-variant-position:normal;font-variant-caps:normal;font-variant-numeric:normal;font-variant-alternates:normal;font-feature-settings:normal;text-indent:0;text-align:start;text-decoration-line:none;text-decoration-style:solid;text-decoration-color:#000;text-transform:none;text-orientation:mixed;white-space:normal;shape-padding:0;isolation:auto;mix-blend-mode:normal;solid-color:#000;solid-opacity:1' transform='translate(10.583 11.08) scale(.82353)'/>
    <path fill='#fff' fill-rule='evenodd' d='M45 26.53c-9.643-.046-15.364 4.704-15.715 9.491-.449 6.13 4.386 10.427 12.858 10.51 8.473.084 16.505-4.696 16.735-9.797.23-5.1-7.397-10.172-13.877-10.203zm-1.252 2.024c4.254.017 14.452 2.56 14.517 7.977-.094 2.753-3.06 8.57-15.612 8.57-12.55 0-12.755-5.305-12.55-9.183 2.539-5.486 9.391-7.38 13.645-7.364z' filter='url(#p)' transform='translate(10.583 11.08) scale(.82353)'/>
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
                            var sum = 0;
                            var i = (p1 + 1) % digits.Length;
                            while (i != p2)
                            {
                                sum += digits[i];
                                i = (i + 1) % digits.Length;
                            }
                            constraints.Add(new ToroidalSandwich(isCol, rowCol, digit1, digit2, sum));
                        }
            return constraints;
        }
    }
}
