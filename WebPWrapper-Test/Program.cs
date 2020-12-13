using System;


namespace WebPWrapper_Test
{
    class Program
    {
        static void Main(string[] args)
        {
            using (var test = new Test_ProgressiveDecode())
            {
                test.Run(args);
            }
        }
    }
}
