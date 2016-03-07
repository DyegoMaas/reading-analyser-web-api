using Nancy;
using Nancy.ModelBinding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReadingAnalyser.WebApi.Modules
{
    public class AudiobookModule : NancyModule
    {
        public AudiobookModule()
        {
            Post["/audiobook/get-remaining-percentage"] = _ =>
            {
                var modelo = this.Bind<Teste>();
                var timeCompleted = TimeSpan.Parse(modelo.TimeCompleted);
                var timeRemaining = TimeSpan.Parse(modelo.TimeRemaining);

                var percentageRemaining = BookStatistics.Audiobook.GetRemainingPercentage(timeCompleted, timeRemaining);
                return new { RemainingPercentage = percentageRemaining };
            };
        }

        public class Teste
        {
            public string TimeCompleted { get; set; }
            public string TimeRemaining { get; set; }
        }
    }
}
