using UnityEngine;
using System.Collections;
using System.Collections.Generic;



namespace AudioVisualizer
{
    /// <summary>
    /// Samples the audio across multiple audio sources.
    /// All other AudioVisualizer classes rely on this class to sample the audio data.
    /// There should just be 1 instance of this in each scene.
    /// </summary>
    public class AudioSampler : MonoBehaviour
    {


        public static AudioSampler instance; //singleton static instance
        public List<AudioSource> audioSources; // list of audio sources used for audio input.
        public bool debug = false; // if true, show audio data being sampled

        // used for drawing the debug chart.
        private Texture2D drawTexture; 
        private Color startColor = Color.magenta;
        private Color endColor = Color.blue;
        private Gradient gradient;
        private float fMax;// = (float)AudioSettings.outputSampleRate/2;
        private List<string> debugLables = new List<string>() { "SubBass", "Bass", "LowMid", "Mid", "UpperMid", "High", "VeryHigh", "Decibal" };
        private int samplesToTake = 1024; // how many audio samples should we take?

        //singleton logic
        void OnEnable()
        {
            if (instance == null)
            {
                instance = this;
            }
        }
        void OnDisable()
        {
            instance = null;
        }

        void Awake()
        {
            drawTexture = Texture2D.whiteTexture; // get an empty white texture
            gradient = PanelWaveform.GetColorGradient(startColor, endColor); // get a color gradient.
            if (audioSources.Count == 0) // if we haven't assigned any audio sources
            {
                if (this.GetComponent<AudioSource>() != null) // try to grab one from this gameobject
                {
                    audioSources.Add(this.GetComponent<AudioSource>());
                }
                else
                {
                    Debug.LogError("Error! no audio sources attached to AudioSampler.css");
                }
            }
        }

        void Start()
        {
            //get max frequency
            fMax = (float)AudioSettings.outputSampleRate / 2;
        }


   

        //draw the debug chart if enabled.
        void OnGUI()
        {
            if (debug)
            {

                //rms
                // avg
                // 7 Frequency Range volumes
                int heightPerSource = 100;
                for (int s = 0; s < audioSources.Count; s++)
                {
                    int width = (int)(Screen.width * .5f);
                    int height = heightPerSource * (s + 1);
                    int headerFooter = (int)(height * .2f);
                    int spacing = (int)(width / debugLables.Count);
                    int barWidth = 10;
                    int yBottom = height - headerFooter;
                    int yTop = headerFooter;
                    int indent = 40; //left indent

                    GUI.color = Color.white;
                    GUI.Label(new Rect(0, yBottom, 60, 20), "Source: " + s);
                    for (int j = 0; j < debugLables.Count; j++)
                    {
                        float percent = (float)j / (debugLables.Count - 1);
                        int x = indent + spacing + spacing * j;
                        Vector2 start = new Vector2(x, yBottom);
                        float volume = Mathf.Clamp(GetFrequencyVol(s, (FrequencyRange)j) * 10, 0, .5f);
                        float y = yBottom - heightPerSource * volume;
                        //Debug.Log(i + " vol: " + volume + " y: " + y); 
                        Vector2 end = new Vector2(x, y);
                        DrawLine(start, end, barWidth, gradient.Evaluate(percent));
                        GUI.Label(new Rect(x, yBottom, 60, 20), debugLables[j]);
                        GUI.Label(new Rect(x, yBottom + 20, 40, 20), volume.ToString("F3"));
                    }
                }

            }
        }
        //draw a line of the debug chart.
        private void DrawLine(Vector2 start, Vector2 end, int width, Color color)
        {
            GUI.color = color;
            Vector2 d = end - start;
            float a = Mathf.Rad2Deg * Mathf.Atan(d.y / d.x);
            if (d.x < 0)
                a += 180;

            int width2 = (int)Mathf.Ceil(width / 2);

            if (Vector2.Distance(start, end) > .1)
            {
                GUIUtility.RotateAroundPivot(a, start);
                GUI.DrawTexture(new Rect(start.x, start.y - width2, d.magnitude, width), drawTexture);
                GUIUtility.RotateAroundPivot(-a, start);
            }
        }


        //get an array of output data (decibals)
        public float[] GetAudioSamples(int audioSourceIndex)
        {
            if (!audioSources[audioSourceIndex].mute) // if not muted
            {
                float[] samples = audioSources[audioSourceIndex].GetOutputData(samplesToTake, 0); // grab samples!
                //normalize the samples
                float[] normSamples = NormalizeArray(samples);
                //multiply by volume.
                for (int i = 0; i < samples.Length; i++)
                {
                    normSamples[i] = normSamples[i] * audioSources[audioSourceIndex].volume;
                }
                return normSamples;
            }

            return new float[samplesToTake];
        }

        //get an array of output data, averaged into 'numBins' bins
        public float[] GetAudioSamples(int audioSourceIndex, int numBins, bool absoluteVal)
        {
            if (!audioSources[audioSourceIndex].mute) // if not muted
            {
                float[] samples = audioSources[audioSourceIndex].GetOutputData(numBins, 0); // grab samples!
                //normalize the samples
                float[] normSamples = NormalizeArray(samples);
                //multiply by volume.
                for (int i = 0; i < samples.Length; i++)
                {
                    if (absoluteVal)
                    {
                        normSamples[i] = Mathf.Abs(samples[i]) * audioSources[audioSourceIndex].volume;
                    }
                    else
                    {
                        normSamples[i] = samples[i] * audioSources[audioSourceIndex].volume;
                    }
                }

                return normSamples;
            }

            return new float[numBins];
        }


        //sample the audio, square each value, and sum them all to get instant energy (the current 'energy' in the audio)
        public float GetInstantEnergy(int audioSourceIndex)
        {
            if (!audioSources[audioSourceIndex].mute)
            {
                float[] audioSamples = GetAudioSamples(audioSourceIndex);
                float sum = 0;
                //sum up the audio samples
                foreach (float f in audioSamples)
                {
                    sum += (f * f);
                }
                return sum * audioSources[audioSourceIndex].volume;
            }

            return 0;
        }

        //Get the RMS value of the audio (Root means squared) value 0-1
        //An average "noise" value of the audio at this point in time, using samplesToTake audio samples, and the passed in sensitivity.
        public float GetRMS(int audioSourceIndex)
        {
            if (!audioSources[audioSourceIndex].mute)
            {
                //grab output data (decibals)
                float[] audioSamples = audioSources[audioSourceIndex].GetOutputData(samplesToTake, 0); // fill array with samples
                //float[] normSamples = NormalizeArray(audioSamples);
                int i;
                float sum = 0;
                for (i = 0; i < samplesToTake; i++)
                {
                    sum += audioSamples[i] * audioSamples[i]; // sum squared samples
                }
                float rmsValue = Mathf.Sqrt(sum / samplesToTake) * audioSources[audioSourceIndex].volume; // rms = square root of average

                return rmsValue;
            }

            return 0;
        }

        //like GetAvg or GetRMS, but inside a given frequency range
        public float GetFrequencyVol(int audioSourceIndex, FrequencyRange freqRange)
        {
            if (!audioSources[audioSourceIndex].mute) // if not muted
            {
                Vector2 range = GetFreqForRange(freqRange);
                float fLow = range.x;//Mathf.Clamp (range.x, 20, fMax); // limit low...
                float fHigh = range.y;//Mathf.Clamp (range.y, fLow, fMax); // and high frequencies
                // get spectrum
                float[] freqData = new float[samplesToTake];
                audioSources[audioSourceIndex].GetSpectrumData(freqData, 0, FFTWindow.BlackmanHarris);
                int n1 = (int)Mathf.Floor(fLow * samplesToTake / fMax);
                int n2 = (int)Mathf.Floor(fHigh * samplesToTake / fMax);
                float sum = 0;
                // Debug.Log("Smapling freq: " + n1 + "-" + n2);
                // average the volumes of frequencies fLow to fHigh
                for (int i = n1; i <= n2; i++)
                {
                    if(i < freqData.Length)
                        sum += Mathf.Abs(freqData[i]);
                }
                sum = sum * audioSources[audioSourceIndex].volume;
                return sum / (n2 - n1 + 1);
            }

            return 0;
        }

        //return the raw spectrum data i nthe given frequency range.
        public float[] GetFrequencyData(int audioSourceIndex, FrequencyRange freqRange)
        {
            if (!audioSources[audioSourceIndex].mute) // if not muted
            {
                Vector2 range = GetFreqForRange(freqRange);
                float fLow = range.x;//Mathf.Clamp (range.x, 20, fMax); // limit low...
                float fHigh = range.y;//Mathf.Clamp (range.y, fLow, fMax); // and high frequencies
                // get spectrum
                float[] freqData = new float[samplesToTake];
                audioSources[audioSourceIndex].GetSpectrumData(freqData, 0, FFTWindow.BlackmanHarris);
                int n1 = (int)Mathf.Floor(fLow * samplesToTake / fMax);
                int n2 = (int)Mathf.Floor(fHigh * samplesToTake / fMax);
                float sum = 0;
                // Debug.Log("Smapling freq: " + n1 + "-" + n2);
                // average the volumes of frequencies fLow to fHigh

                List<float> validData = new List<float>();
                for (int i = n1; i <= n2; i++)
                {
                    validData.Add(freqData[i] * audioSources[audioSourceIndex].volume);
                }

                float[] normData = NormalizeArray(validData.ToArray());

                return normData;
            }

            Debug.LogWarning("warning: Audio Source: " + audioSourceIndex + " is muted");
            return new float[samplesToTake];
        }

        //return the raw spectrum data i nthe given frequency range, using the specified number of bins
        public float[] GetFrequencyData(int audioSourceIndex, FrequencyRange freqRange, int numBins, bool abs)
        {
            if (!audioSources[audioSourceIndex].mute) // if not muted
            {
                Vector2 range = GetFreqForRange(freqRange);
                float fLow = range.x;//Mathf.Clamp (range.x, 20, fMax); // limit low...
                float fHigh = range.y;//Mathf.Clamp (range.y, fLow, fMax); // and high frequencies
                // get spectrum
                float[] freqData = new float[samplesToTake];
                audioSources[audioSourceIndex].GetSpectrumData(freqData, 0, FFTWindow.BlackmanHarris);
                int n1 = (int)Mathf.Floor(fLow * samplesToTake / fMax);
                int n2 = (int)Mathf.Floor(fHigh * samplesToTake / fMax);
                float sum = 0;
                // Debug.Log("Smapling freq: " + n1 + "-" + n2);
                // average the volumes of frequencies fLow to fHigh

                //Debug.Log("Valid Freq Data: (" + n1 + "-" + n2 + ")/" + samplesToTake);
                List<float> validData = new List<float>();
                for (int i = n1; i <= n2; i++)
                {
                    float frequency = freqData[i];
                    if (abs)
                    {
                        frequency = Mathf.Abs(freqData[i]);
                    }

                    validData.Add(frequency * audioSources[audioSourceIndex].volume);
                }

                float[] binnedArray = GetBinnedArray(validData.ToArray(), numBins);
                float[] normalizedArray = NormalizeArray(binnedArray);
                return normalizedArray;
            }

            Debug.LogWarning("warning: Audio Source: " + audioSourceIndex + " is muted");
            return new float[numBins];
        }

        //take an array, and bin the values. 
        // if numBins is > intput.Length, duplicate input values
        // if numBins is < input.Length, average input values
        float[] GetBinnedArray(float[] input, int numBins)
        {
            float[] output = new float[numBins];

            if(numBins == input.Length)
            {
                return input;
            }
            // if numBins is > intput.Length, duplicate input values
            if(numBins > input.Length)
            {
                int binsPerInput = numBins/input.Length;
                for(int b = 0; b < numBins; b++) 
                {
                    int inputIndex = (b+1)%binsPerInput;
                    output[b] = input[inputIndex];
                }
            }

            // if numBins is < input.Length, average input values
            if (numBins < input.Length)
            {
                int inputsPerBin = input.Length/numBins;
                for (int b = 0; b < numBins; b++)
                {
                    float avg = 0;
                    for (int i = 0; i < inputsPerBin; i++)
                    {
                        int index = b * inputsPerBin + i;
                        avg += input[index];
                    }
                    avg = avg / inputsPerBin;

                    output[b] = avg;
                }
            }

            return output;
        }

        //normalize array values to be in the range 0-1
        float[] NormalizeArray(float[] input)
        {
            float[] output = new float[input.Length];
            float max = -Mathf.Infinity;
            //get the max value in the array
            for (int i = 0; i < input.Length; i++)
            {
                max = Mathf.Max(max, Mathf.Abs(input[i]));
            }

            //divide everything by the max value
            for (int i = 0; i < input.Length; i++)
            {
                output[i] = input[i] / max;
            }

            return output;
        }

        public static Vector2 GetFreqForRange(FrequencyRange freqRange)
        {
            switch (freqRange)
            {
                case FrequencyRange.SubBase:
                    return new Vector2(20, 60);
                    break;
                case FrequencyRange.Bass:
                    return new Vector2(60, 250);
                    break;
                case FrequencyRange.LowMidrange:
                    return new Vector2(250, 500);
                    break;
                case FrequencyRange.Midrange:
                    return new Vector2(500, 2000);
                    break;
                case FrequencyRange.UpperMidrange:
                    return new Vector2(2000, 4000);
                    break;
                case FrequencyRange.High:
                    return new Vector2(4000, 6000);
                    break;
                case FrequencyRange.VeryHigh:
                    return new Vector2(6000, 20000);
                    break;
                case FrequencyRange.Decibal:
                    return new Vector2(0, 20000);
                default:
                    break;
            }

            return Vector2.zero;
        }
    }

    public enum FrequencyRange
    {
        SubBase, // 20-60 Hz
        Bass, // 60-250 Hz
        LowMidrange, //250-500 Hz
        Midrange, //500-2,000 Hz
        UpperMidrange, //2,000-4,000 Hz
        High, //4,000-6000 Hz
        VeryHigh, //6,000-20,000 Hz
        Decibal // use output data instead of frequency data
    };



}
