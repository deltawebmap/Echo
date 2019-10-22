using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace EchoReader.Entities
{
    public class PerformanceReport
    {
        private DateTime lastCheckpoint;
        private string lastCheckpointName;

        public DateTime start;
        public List<PerformanceReportStep> steps;

        public PerformanceReport()
        {
            steps = new List<PerformanceReportStep>();
            start = DateTime.UtcNow;
        }

        private void SaveLastCheckpoint()
        {
            steps.Add(new PerformanceReportStep
            {
                name = lastCheckpointName,
                time = (DateTime.UtcNow - lastCheckpoint).TotalMilliseconds
            });
        }

        public void StartStep(string name)
        {
            //Add last, if any
            if (lastCheckpointName != null)
                SaveLastCheckpoint();

            //Reset
            lastCheckpointName = name;
            lastCheckpoint = DateTime.UtcNow;
        }

        public void End()
        {
            SaveLastCheckpoint();
        }

        public string GetString()
        {
            return JsonConvert.SerializeObject(this);
        }
    }

    public class PerformanceReportStep
    {
        public double time;
        public string name;
    }
}
