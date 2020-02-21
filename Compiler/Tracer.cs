using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Compiler
{
    public class Tracer
    {
        private readonly Stopwatch stopwatch = new Stopwatch();
        private readonly List<(string name, TimeSpan time)> epochs = new List<(string, TimeSpan)>();
        private TimeSpan lastTime = TimeSpan.Zero;

        public void Restart()
        {
            stopwatch.Restart();
        }

        public void AddEpoch(string epochName)
        {
            var currentTime = stopwatch.Elapsed;
            var deltaTime = currentTime - lastTime;
            lastTime = currentTime;

            epochs.Add((epochName, deltaTime));
        }

        public void PrintEpochs()
        {
            stopwatch.Stop();

            Console.WriteLine($"Total Time: {stopwatch.Elapsed.TotalSeconds} seconds");

            foreach (var (name, time) in epochs)
            {
                Console.WriteLine($"\t{time.TotalSeconds.ToString("N8", CultureInfo.CurrentCulture)}: {name}");
            }
        }
    }
}
