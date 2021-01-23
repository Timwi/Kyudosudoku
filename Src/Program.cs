using RT.PropellerApi;

namespace KyudosudokuWebsite
{
    class Program
    {
        static void Main(string[] args)
        {
            PropellerUtil.RunStandalone(@"D:\Daten\Config\Kyudosudoku.config.json", new KyudosudokuPropellerModule(),
#if DEBUG
                true
#else
                false
#endif
            );
        }
    }
}
