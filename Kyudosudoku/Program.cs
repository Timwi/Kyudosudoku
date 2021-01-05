using RT.PropellerApi;

namespace KyudosudokuWebsite
{
    class Program
    {
        static void Main(string[] args)
        {
            PropellerUtil.RunStandalone("KyudosudokuSettings.json", new KyudosudokuPropellerModule(),
#if DEBUG
                true
#else
                false
#endif
            );
        }
    }
}
