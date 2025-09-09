using Pro4Soft.BackgroundWorker.Dto.P4W.Entities;
using Pro4Soft.BackgroundWorker.Execution.Common;

namespace Tester;

internal class Program
{
    static void Main(string[] args)
    {
        try
        {
            var po = Utils.DeserializeFromJson<PurchaseOrderP4>(Utils.ReadTextFile("po.json"));
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
    }
}