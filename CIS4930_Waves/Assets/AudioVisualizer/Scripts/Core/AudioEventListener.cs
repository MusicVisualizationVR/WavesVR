using UnityEngine;
using UnityEngine.Events;
using System.Collections;

/// <summary>
/// Listen for beats in the given frequencyRange.
/// Can either automatically detect beats or be be fine tuned for a song by hand.
/// </summary>
namespace AudioVisualizer
{
    
    //Unity OnChange float event
    [System.Serializable]
    public class OnFrequencyEvent : UnityEvent<float> {
    };

    //custom class for onChange event, and min/max value
    [System.Serializable]
    public class OnFrequencyChanged
    {
        public OnFrequencyEvent onChange; // hook in a public float variable here
        //it will be changed between these min/max values with the audio frequency.
        public float minValue = 0;
        public float maxValue = 1;
    }

    /// <summary>
    /// Listen for audio events.
    /// </summary>
    public class AudioEventListener : MonoBehaviour
    {

        public int audioSource = 0; // index into audioSampler audioSource array. Determines which audio source we want to sample
        public FrequencyRange frequencyRange = FrequencyRange.Decibal; // what frequency will we listen to? 
        public int sampleBufferSize = 40;
        public float beatThreshold = 1.3f; // audio threshold, if current is > avg*threshold, we have a beat
        public bool automatic = true; // automatically detects and moves the beatThreshold
        public bool debug = false; // print debug statements?
        public UnityEvent OnBeat; // call these method when a beat hits


        public OnFrequencyChanged onFrequencyChanged; //custom class for onChange<float> event between a min and max value.
        //delegate, so we can listen for beat events. When an event is detected, we'll call the public OnBeat() methods.
        public delegate void BeatEvent();
        public static BeatEvent BeatDetected;


        // public DebugChart avgChart;

        private bool canDetect = true; // flag indicating if we can detect another beat or not
        private float lastFreq = 0; // the frequency of the last detected beat
        private float lastVariance = 0;// the variance of the last frame
        private float[] sampleBuffer; //buffers the audio samples taken
        private int index = 0; // our index into the sample buffer.
        float avgEnergy; // compute the current average
        float variance; // compare everything in the sample buffer to the current average
        float varyPercent; //how much variance are we seeing?
        float frequency; // the current frequency!

        void Awake()
        {
            sampleBuffer = new float[sampleBufferSize]; //buffers the audio samples taken
            //initialize our array
            for (int i = 0; i < sampleBuffer.Length; i++)
            {
                sampleBuffer[i] = 0;
            }

        }

        // Update is called once per frame
        void FixedUpdate()
        {

            if (automatic)
            {
                AutomaticDetection(); // automatic input
            }
            else
            {
                CustomDetection(); // custom input
            }

            HandleFrequencyEvents();

        }

        //set float values equal to the normalized frequency of the audio
        //normalized frequency ranges 0 to 1
        //output value = minValue + (maxValue-minValue)*normalizedFrequency
        void HandleFrequencyEvents()
        {
            //get a value between min-max, using the frequency.
            if (onFrequencyChanged != null)
            {
                float delta = onFrequencyChanged.maxValue - onFrequencyChanged.minValue; //delta = max-min
                float scaledValue = onFrequencyChanged.minValue + delta * GetNormalizedFrequency(); //min + delta*frequency
                onFrequencyChanged.onChange.Invoke(scaledValue);
            }
        }

        void CustomDetection()
        {

            if (index >= sampleBuffer.Length)
            {
                index = 0;
            }
            frequency = AudioSampler.instance.GetFrequencyVol(audioSource, frequencyRange); // get the root means squared value of the audio right now
            sampleBuffer[index] = frequency; // replace the oldest sample in our runningAvg array with the new sample
            avgEnergy = GetAvgEnergy(); // compute the current average


            //if instantEnergy is > beatThreshold*avgEnergy
            if (frequency > beatThreshold * avgEnergy)
            {
                OnBeat.Invoke(); // call the public OnBeat methods

                if (BeatDetected != null) // if we have a listener for this event
                {
                    BeatDetected(); // send the event
                    if (debug)
                    {
                        Debug.Log("Beat Detected");
                    }
                }


            }

            if (debug)
            {
                Debug.Log("FreqVolume: " + frequency + " beatThreshold: " + beatThreshold * avgEnergy);
            }

            index++;
        }

       

        void AutomaticDetection()
        {
            if (index >= sampleBuffer.Length)
            {
                //canDetect = true;
                index = 0;
            }

            frequency = AudioSampler.instance.GetFrequencyVol(audioSource, frequencyRange); // get the root means squared value of the audio right now
            sampleBuffer[index] = frequency; // replace the oldest sample in our runningAvg array with the new sample
            avgEnergy = GetAvgEnergy(); // compute the current average
            variance = GetAvgVariance(avgEnergy); // compare everything in the sample buffer to the current average
            varyPercent = 1 - ((avgEnergy - variance) / avgEnergy); //how much variance are we seeing?
            beatThreshold = 1 + varyPercent; //beatThreshold in range 1-2


            //if we can't detect beats
            if (!canDetect)
            {
                //check to see if we should be able to detect them again.
                //we can start looking for new beats once freq drops
                if (frequency < (2 - beatThreshold) * avgEnergy)
                {
                    canDetect = true;
                }
            }

            //if instantEnergy is > beatThreshold*avgEnergy
            if (frequency > beatThreshold * avgEnergy && canDetect)
            {
                //Debug.Log("\n Beat Detected, (" + freq + " > " + beatThreshold*avgEnergy + " ) \n");
                canDetect = false; //reset the flag. we can't detect another beat, until freq drops below 'lastFreq'
                lastFreq = frequency;  //record the frequency of the last beat
                lastVariance = varyPercent; // record the variancePercent of the last beat

                OnBeat.Invoke(); // call the public OnBeat methods

                if (BeatDetected != null) // if we have a listener for this event
                {
                    BeatDetected(); // send the event
                    if (debug)
                    {
                        Debug.Log("Beat Detected");
                    }
                }

            }

            if (debug)
            {
                Debug.Log("Freq: " + frequency + " beatThreshold: " + beatThreshold * avgEnergy);
            }

            index++;
        }

        //Get the max frequency in the sample buffer, divide the current frequency by it.
        //returns a 0-1 value.
        public float GetNormalizedFrequency()
        {
            //find the highest sample
            float max = -Mathf.Infinity;
            foreach(float sampleFreq in sampleBuffer)
            {
                max = Mathf.Max(max, sampleFreq);
            }

            //return currentSample/maxSample
            return (frequency / max);
        }

        //average all the samples in the sample buffer.
        float GetAvgEnergy()
        {
            float sum = 0;
            for (int i = 0; i < sampleBuffer.Length; i++)
            {
                sum += sampleBuffer[i];
            }

            float avg = sum / sampleBuffer.Length;
            return avg;
        }

        //get the variance of samples in the sample buffer.
        float GetAvgVariance(float avg)
        {
            float sum = 0;
            for (int i = 0; i < sampleBuffer.Length; i++)
            {
                float variance = (sampleBuffer[i] - avg);  //compare each sample in our buffer, to the avg. 
                //Debug.Log("Variance: " + i + " = " + sampleBuffer[i] + "-" + avg);
                sum += Mathf.Abs(variance); //sum up all variances, to get the avgVariance
            }

            float avgVariance = sum / sampleBuffer.Length;
            return avgVariance;
        }
    }
}
