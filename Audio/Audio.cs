using System;
using System.Collections.Generic;
using System.IO;
using Godot;
using NAudio;
using NAudio.Dsp;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;

public class Audio : Node
{
	private string file;

	private List<Complex[]> fftData = new List<Complex[]>();

	private List<float>[] frequenciesBeatmap;

	private int sampleRate;

	private int totalSamples;

	private List<float> data = new List<float>();

	private int[] logArr;

	private int SIZE = 1024;

	private float WINDOW_SIZE;

	Audio()
	{
		logArr = new int[] {
			0, 2, 5, 8, 12, 16, 21, 26, 31, 37, 43, 49, 55, 61, 68, 75, 82, 89, 96,
			103, 110, 118, 126, 134, 142, 150, 158, 166, 174, 182, 190, 198, 207, 216,
			225, 234, 243, 252, 261, 270, 279, 288, 297, 306, 315, 324, 333, 342, 352,
			362, 372, 382, 392, 402, 412, 422, 432, 442, 452, 462, 472, 482, 492, 502,
			512
		};

		//if (System.IO.File.Exists(file))
		//{
		file = "assets//audio//09 Princess Amagi-ya.mp3";
		//}
	}

	public override void _Ready()
	{
		Godot.Thread audio = new Godot.Thread();
		audio.Start(this, "analyze", file);
		audio.WaitToFinish();
	}

	private void analyze(string file)
	{
		calcMagnitude (file);

		List<float> beatmap = makeBeatmap();

		using (TextWriter tw = new StreamWriter("Output//beatmap.csv"))
		{
			foreach (float data in beatmap)
			{
				tw.WriteLine (data);
			}
			tw.Flush();
			tw.Close();
		}
		using (TextWriter tw = new StreamWriter("Output//multimap.csv"))
		{
			foreach (List<float> frequencies in frequenciesBeatmap)
			{
				foreach (float points in frequencies)
				{
					tw.Write(points.ToString() + ',');
				}

				tw.Write('\n');
			}
			tw.Flush();
			tw.Close();
		}
	}

	//gets data from file, converts to mono, and applies a hammingwindow to the data.
	private void populateData()
	{
		List<float> data = new List<float>();
		int bytesRead;

		using (AudioFileReader audioFile = new AudioFileReader(file))
		{
			ISampleProvider analyze = audioFile.ToSampleProvider().ToMono();
			float[] buffer = new float[SIZE];
			totalSamples =
				(int)(audioFile.Length / audioFile.BlockAlign) / SIZE;
			GD.Print (totalSamples);
			sampleRate = audioFile.WaveFormat.SampleRate;
			WINDOW_SIZE = getPeriod() / 2;

			do
			{
				bytesRead = analyze.Read(buffer, 0, SIZE);

				for (int i = 0; i < SIZE; i++)
				{
					if (buffer[i] != 0.0)
					{
						buffer[i] *=
							(float) FastFourierTransform.BlackmannHarrisWindow(i, SIZE);
					}
				}
				data.AddRange (buffer);
			}
			while (bytesRead != 0);
		}

		this.data = data;
	}

	//breaks the array into 3 multiple windows of length SIZE,
	//multiplies the data by a window function,
	//and uses fft to extract the magnitude of the windows. the windows
	//are then averaged out to provide more accurate data to make up for the
	//window function zeroing values closer to the ends of the magnitude array.
	private void calcMagnitude(string file)
	{
		populateData();
		using (AudioFileReader audioFile = new AudioFileReader(file))
		{
			Complex[] prevChunk = new Complex[SIZE];
			Complex[] chunk = new Complex[SIZE];
			Complex[] nextChunk = new Complex[SIZE];
			for (int i = 0; i < totalSamples; i++)
			{
				Complex[] chunked = chunkData(i, chunk, nextChunk, prevChunk);
				FastFourierTransform
					.FFT(true, (int) Math.Log(SIZE, 2.0), chunk);
				FastFourierTransform
					.FFT(true, (int) Math.Log(SIZE, 2.0), nextChunk);
				FastFourierTransform
					.FFT(true, (int) Math.Log(SIZE, 2.0), prevChunk);
				Complex[] fftMagnitude =
					overlap(i, chunk, nextChunk, prevChunk, chunked);
				fftData.Add (fftMagnitude);
			}
		}
	}

	public Complex[]
	chunkData(int i, Complex[] chunk, Complex[] nextChunk, Complex[] prevChunk)
	{
		Complex[] fftMagnitude = new Complex[SIZE / 2];

		for (int j = 0; j < SIZE; j++)
		{
			if (i == 0)
			{
				chunk[j].X = (data[(i * (SIZE / 2)) + j]);
				nextChunk[j].X = data[(i + 1) * (SIZE / 2) + j];
			}
			else if (i == totalSamples)
			{
				chunk[j].X = data[(i * (SIZE / 2)) + j];
				prevChunk[j].X = data[(i - 1) * (SIZE / 2) + j];
			}
			else
			{
				prevChunk[j].X = data[(i - 1) * (SIZE / 2) + j];
				chunk[j].X = data[i * (SIZE / 2) + j];
				nextChunk[j].X = data[(i + 1) * (SIZE / 2) + j];
			}
		}
		return fftMagnitude;
	}

	public Complex[]
	overlap(
		int i,
		Complex[] chunk,
		Complex[] nextChunk,
		Complex[] prevChunk,
		Complex[] fftMagnitude
	)
	{
		Complex[] buffer = new Complex[SIZE / 2];

		for (int j = 0; j < (SIZE + 1) / 2; j++)
		{
			float
				a,
				b,
				c,
				d,
				e,
				f;
			float
				square,
				squareTwo,
				squareThree;
			if (i == 0)
			{
				a = chunk[j].X;
				b = chunk[j + 1].X;
				c = nextChunk[j].X;
				d = nextChunk[j + 1].X;
				square = (float) Math.Sqrt(((a * a) + (b * b)));
				squareTwo = (float) Math.Sqrt(((c * c) + (d * d)));
				square *= square;
				squareTwo *= squareTwo;
				buffer[j].X = (square + squareTwo) / 2;
			}
			else if (i == SIZE)
			{
				a = chunk[j].X;
				b = chunk[j + 1].X;
				e = prevChunk[j].X;
				f = prevChunk[j + 1].X;
				square = (float) Math.Sqrt(((a * a) + (b * b)));
				squareThree = (float) Math.Sqrt(((e * e) + (f * f)));
				square *= square;
				squareThree *= squareThree;
				buffer[j].X = (square + squareThree) / 2;
			}
			else
			{
				a = chunk[j].X;
				b = chunk[j + 1].X;
				c = nextChunk[j].X;
				d = nextChunk[j + 1].X;
				e = prevChunk[j].X;
				f = prevChunk[j + 1].X;
				square = (float) Math.Sqrt(((a * a + b * b)));
				squareTwo = (float) Math.Sqrt(((c * c + d * d)));
				squareThree = (float) Math.Sqrt(((e * e + f * f)));
				square *= square;
				squareTwo *= squareTwo;
				squareThree *= squareThree;

				buffer[j].X = (square + squareTwo + squareThree) / 3;
			}
		}
		return buffer;
	}

	private List<float> makeBeatmap()
	{
		List<float>[] energySpectrum = makeEnergySpectrum();
		List<float>[] means = calcMeanVals(energySpectrum);
		(List<float>[], List<float>) variances =
			calcVariances(energySpectrum, means);
		List<float> deviations = calcDeviations(variances.Item1);
		List<float>[] prunned =
			prunVals(energySpectrum,
			means,
			variances.Item1,
			variances.Item2,
			deviations);
		List<float>[] peaks = findPeaks(prunned, energySpectrum);
		List<List<Tuple<float, decimal, int>>> beatmapArr =
			intervalToTime(peaks);

		this.frequenciesBeatmap = peaks;
		List<float> beatmap = combinePeaks(peaks);
		return beatmap;
	}

	private List<float>[] makeEnergySpectrum()
	{
		List<float>[] energySpectrum = new List<float>[64];
		for (int i = 0; i < energySpectrum.Length; i++)
		{
			energySpectrum[i] = new List<float>();
		}

		Complex[] lastArray = new Complex[fftData[0].Length];
		fftData[0].CopyTo(lastArray, 0);
		for (int i = 0; i < fftData.Count; i++)
		{
			float flux = 0;
			for (int j = 0; j < logArr.Length - 1; j++)
			{
				for (int k = logArr[j]; k < logArr[j + 1]; k++)
				{
					flux +=
						((float) fftData[i][k].X - (float) lastArray[k].X) < 0
							? 0
							: (
							(float) fftData[i][k].X - (float) lastArray[k].X
							);
				}
				if (j < logArr.Length - 1)
				{
					flux /= (float)(logArr[j + 1] - logArr[j]);
					energySpectrum[j].Add(flux);
				}
			}
			fftData[i].CopyTo(lastArray, 0);
		}
		return energySpectrum;
	}

	private List<float>[] calcMeanVals(List<float>[] energySpectrum)
	{
		float mean = 0;
		float time = 1 / (float)((2 * WINDOW_SIZE) + 1);
		List<float>[] meanVals = new List<float>[64];
		for (int i = 0; i < meanVals.Length; i++)
		{
			meanVals[i] = new List<float>();
		}
		for (int i = 0; i < energySpectrum.Length; i++)
		{
			for (int j = 0; j < energySpectrum[i].Count; j++)
			{
				int start = (int)(Math.Max(0, j - WINDOW_SIZE));
				int end =
					(
					int
					)(Math.Min(energySpectrum[i].Count - 1, j + WINDOW_SIZE));
				for (int k = start; k <= end; k++)
				{
					mean += energySpectrum[i][k];
				}
				mean *= time;
				meanVals[i].Add(mean);
			}
		}
		return meanVals;
	}

	private (List<float>[], List<float>)
	calcVariances(List<float>[] energySpectrum, List<float>[] means)
	{
		List<float> maxVarianceList = new List<float>();
		List<float>[] varianceList = new List<float>[64];
		for (int i = 0; i < varianceList.Length; i++)
		{
			varianceList[i] = new List<float>();
		}
		for (int i = 0; i < energySpectrum.Length; i++)
		{
			float maxVariance = 0;
			float variance = 0;
			float time = 1 / (float)((2 * WINDOW_SIZE) + 1);
			for (int j = 0; j < energySpectrum[i].Count; j++)
			{
				int start = (int) Math.Max(0, j - WINDOW_SIZE);
				int end =
					(int)
					Math.Min(energySpectrum[i].Count - 1, j + WINDOW_SIZE);
				for (int k = 0; k <= end; k++)
				{
					variance +=
						(float)
						Math
							.Pow((
							float
							)(energySpectrum[i][k] -
							means[i][(int)(j / WINDOW_SIZE)]),
							2);
				}
				variance *= time;
				if (variance > maxVariance)
				{
					maxVariance = variance;
				}
				varianceList[i].Add(variance);
			}
			maxVarianceList.Add (maxVariance);
		}
		return (varianceList, maxVarianceList);
	}

	private List<float> calcDeviations(List<float>[] variances)
	{
		List<float> deviations = new List<float>();
		for (int i = 0; i < variances.Length; i++)
		{
			float deviation = 0;
			for (int j = 0; j < variances[i].Count; j++)
			{
				deviation += variances[i][j];
			}
			deviation /= variances[i].Count;
			deviations.Add((float) Math.Sqrt((float) deviation));
		}
		return deviations;
	}

	private List<float>[]
	prunVals(
		List<float>[] energySpectrum,
		List<float>[] means,
		List<float>[] variances,
		List<float> maxVariance,
		List<float> deviations
	)
	{
		List<float>[] prunned_array = new List<float>[64];
		for (int i = 0; i < energySpectrum.Length; i++)
		{
			float steps = 1 / maxVariance[i];
			List<float> prunned = new List<float>();
			for (int j = 0; j < energySpectrum[i].Count; j++)
			{
				float C = 1.5142857F;
				C += -steps * variances[i][j];

				//C += (-steps * variances[i][j]) < -limit ? -limit : (-steps * variances[i][j]);
				if (
					energySpectrum[i][j] >= means[i][j] * C &&
					(energySpectrum[i][j] - means[i][j]) / deviations[i] >= 0
				)
				{
					prunned.Add(energySpectrum[i][j] - means[i][j]);
				}
				else
				{
					prunned.Add(0);
				}
			}
			prunned_array[i] = prunned;
		}
		return prunned_array;
	}

	private List<float>[]
	findPeaks(List<float>[] prunned, List<float>[] energySpectrum)
	{
		List<float>[] peaks = new List<float>[64];
		for (int i = 0; i < prunned.Length; i++)
		{
			List<float> peak = prunned[i];
			for (int j = 0; j < peak.Count - 1; j++)
			{
				if (peak[j] < peak[j + 1])
				{
					peak[j] = 0;
				}
			}

			for (int j = 0; j < peak.Count; j++)
			{
				int start = Math.Max(0, j - 4);
				int end = Math.Min(energySpectrum[i].Count - 1, j + 4);
				int index = start;
				float max = peak[index];
				if (cleanClosePeaks(start, end, peak))
				{
					for (int k = start; k < end; k++)
					{
						if (max > peak[k + 1])
						{
							peak[k + 1] = 0;
						}
						else
						{
							max = peak[k + 1];
							peak[index] = 0;
							index = k + 1;
						}
					}
				}
			}
			peaks[i] = (peak);
		}

		return peaks;
	}

	private List<List<Tuple<float, decimal, int>>>
	intervalToTime(List<float>[] peaks)
	{
		List<List<Tuple<float, decimal, int>>> intToTime =
			new List<List<Tuple<float, decimal, int>>>();

		//const int MAX_BPM = 8; //~300 bpm
		const int MIN_BPM = 130; //~20 bpm
		for (int i = 0; i < peaks.Length - 1; i++)
		{
			for (int j = 0; j < peaks[i].Count; j++)
			{
				int interval = 0;
				if (peaks[i][j] > 0)
				{
					int end = Math.Min(j + MIN_BPM, peaks[i].Count);
					List<Tuple<float, decimal, int>> buffer =
						new List<Tuple<float, decimal, int>>();
					for (int k = j; k < end; k++)
					{
						if (peaks[i][k] > 0 && k != j)
						{
							float bpm =
								60 * (sampleRate / (1024 * interval)) > 300
									? 300
									: 60 * (sampleRate / (1024 * interval));
							decimal time = (1024 * k) / sampleRate;

							//let time = Math.floor((1024 * k / 44100) * 1000);
							//arr.push([bpm, interval, time]);
							//buffer.push([bpm, time, interval]);
							Tuple<float, decimal, int> tuple =
								Tuple.Create(bpm, time, interval);
							buffer.Add (tuple);
							break;
						}
						else
						{
							interval++;
						}
					}
					intToTime.Add (buffer);
				}
			}
		}
		return intToTime;
	}

	private bool cleanClosePeaks(int start, int end, List<float> peak)
	{
		int count = 0;
		for (int k = start; k <= end; k++)
		{
			if (peak[k] > 0)
			{
				count++;
			}
			if (count > 1)
			{
				return true;
			}
		}
		return false;
	}

	private List<float> combinePeaks(List<float>[] peaks)
	{
		List<float> combined = peaks[0];
		for (int i = 1; i < peaks.Length; i++)
		{
			for (int j = 0; j < peaks[i].Count; j++)
			{
				if (combined[j] < peaks[i][j])
				{
					combined[j] = peaks[i][j];
				}
			}
		}
		for (int i = 0; i < combined.Count; i++)
		{
			int start = Math.Max(0, i - 4);
			int end = Math.Min(combined.Count - 1, i + 4);
			float max = combined[start];
			int index = start;
			if (cleanClosePeaks(start, end, combined))
			{
				for (int k = start; k < end; k++)
				{
					if (max > combined[k + 1])
					{
						combined[k + 1] = 0;
					}
					else
					{
						max = combined[k + 1];
						combined[index] = 0;
						index = k + 1;
					}
				}
			}
		}
		return combined;
	}

	private float getPeriod()
	{
		return 1 / ((float) SIZE / (float) sampleRate);
	}

	public override void _Process(float delta)
	{
		// Called every frame. Delta is time since the last frame.
		// Update game logic here.
	}
}
