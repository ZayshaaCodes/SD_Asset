using Newtonsoft.Json;

public class SdProgressData
{
    public double progress { get; set; }
    public double eta_relative { get; set; }
    public SdProgressState state { get; set; }
    public object current_image { get; set; }
    public object textinfo { get; set; }
    
    [JsonIgnore]
    public string Info
    {
        get => $"step {state.sampling_step} / {state.sampling_steps}   \tjob {state.job_no + 1} / {state.job_count}  \t{progress * 100:F2}%";
    }
}

public class SdProgressState
{
    public bool skipped { get; set; }
    public bool interrupted { get; set; }
    public string job { get; set; }
    public int job_count { get; set; }
    public string job_timestamp { get; set; }
    public int job_no { get; set; }
    public int sampling_step { get; set; }
    public int sampling_steps { get; set; }
}