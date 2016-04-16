using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;


namespace AudioVisualizer
{
    /// <summary>
    /// This script create an audio waveform on top of a UI panel.
    /// </summary>
	[RequireComponent(typeof(RectTransform))]
    public class PanelWaveform : MonoBehaviour
    {

		public int audioSource = 0; // index into audioSampler audioSource array. Determines which audio source we want to sample
		public FrequencyRange frequencyRange = FrequencyRange.Decibal; // what frequency will we listen to? 
		public float sensitivity = 2; // how sensitive is this script to the audio. This value is multiplied by the audio sample data.
		public Sprite sprite; // the sprite used for each cell of the waveform
		public int numColumns = 10; //number of columns in our waveform
		public int numRows = 10; //number of rows in our waveform
		public int spacingX = 0; //space between each column
		public int spacingY = 0; //space between each row
		//we'll use a gradient of colors, bottom to top
		public Color bottomColor = Color.white;
		public Color topColor = Color.white;
		public int updateRate = 1; // how fast the panel is updated
		
		private int updateCounter = 0;
		private List<GameObject> cells = new List<GameObject> (); //private list of object pooled sprites.
		private Gradient colorGradient;
		float widthPerImage;
		float heightPerImage;
		//track data from last update
		private int lastCol;
		private int lastRow;


		// Use this for initialization
		void Awake () 
		{
		
			//set RectTransform parameters
			RectTransform rectTransform = this.GetComponent<RectTransform> ();
			rectTransform.anchorMin = new Vector2(0,0);
			rectTransform.anchorMax = new Vector2(1,1);
			rectTransform.anchoredPosition = new Vector2(1,1);

			colorGradient = GetColorGradient (bottomColor, topColor); // create the color gradient

	
		}

		void Start()
		{
			CreateImages (); // instantiate sprites
			SetWidthAndHeight (); //set width and height vals
		}
		
		
		void FixedUpdate () 
		{
			if(updateCounter >= updateRate)
			{
				DrawWaveform (); // place and color sprites on the panel
				updateCounter = 0;
			}

			//if the user changed teh data during runtime, re-create our images.
			if(lastCol != numColumns || lastRow != numRows)
			{
				Reset();
			}

			lastCol = numColumns;
			lastRow = numRows;
			updateCounter++;
		}

		public void Reset()
		{
			DestroyCells();
			CreateImages ();
			SetWidthAndHeight ();
		}

		void DrawWaveform() // enable or disable cells, based on the the audio waveform
		{
			float[] audioSamples;
			if (frequencyRange == FrequencyRange.Decibal) {
				audioSamples = AudioSampler.instance.GetAudioSamples (audioSource, numColumns, true);
			}
			else
			{
				audioSamples = AudioSampler.instance.GetFrequencyData (audioSource, frequencyRange, numColumns, true);
			}

			int index = 0;
			//render the correct objects

			for(int r = 0; r < numRows; r++) // for each row
			{
				for(int c = 0; c < numColumns; c++)
				{
					float sampleHeight = Mathf.Abs(audioSamples[c])*sensitivity; //get an audio sample for each column

					float currentHeight = (float)r/numRows; // the % this row is out of numrows, i.e. how high we are
					GameObject cell = cells[index];

					if(currentHeight <= sampleHeight) // if this height is < our sample height, enable the cell
					{
						cell.SetActive(true);
					}
					else // otherwise we should turn this cell off
					{
						cell.SetActive(false);
					}
					index++;
				}
			}
		}

		//set the width and height of each sprite
		void SetWidthAndHeight()
		{
			widthPerImage = this.GetComponent<RectTransform> ().rect.width / numColumns;
			heightPerImage = this.GetComponent<RectTransform> ().rect.height / numRows;

			//Debug.Log ("ChildCount = " + this.transform.parent.childCount);

			//handle layoutGroups!
			if(this.transform.parent.GetComponent<VerticalLayoutGroup>())
			{
				heightPerImage = heightPerImage/this.transform.parent.childCount; //(float)this.transform.parent.childCount / numRows;
			}
			if(this.transform.parent.GetComponent<HorizontalLayoutGroup>())
			{
				widthPerImage = widthPerImage/this.transform.parent.childCount; //(float)this.transform.parent.childCount / numColumns;
			}
			if(this.transform.parent.GetComponent<GridLayoutGroup>())
			{
				GridLayoutGroup glg = this.transform.parent.GetComponent<GridLayoutGroup>();
				widthPerImage = glg.cellSize.x / numColumns;
				heightPerImage = glg.cellSize.y / numRows;
			}
		}

		//create all the sprites
		void CreateImages()
		{
			//Debug.Log ("Rect for : " + this.gameObject.name + " = " + this.GetComponent<RectTransform>().rect);
			cells = new List<GameObject> ();
			for(int r = 0; r < numRows; r++)
			{
				for(int c = 0; c < numColumns; c++)
				{
                    GameObject newObj = new GameObject();
					newObj.transform.position = this.transform.position;
					newObj.transform.rotation = this.transform.rotation;

					newObj.SetActive(true);
					newObj.name = "Image_" + r + "x" + c;
					newObj.transform.SetParent(this.transform);

					Image newImage = newObj.AddComponent<Image>();
					newImage.sprite = sprite;

					//set the rectTransform values
					newImage.rectTransform.pivot = new Vector2(0,0); // pivot to bottom left of image
					newImage.rectTransform.anchorMin = new Vector2(0,0); // anchor to bottom left of parent panel
					newImage.rectTransform.anchorMax = new Vector2(0,0);
					newImage.rectTransform.localScale = Vector3.one;
					//scale the image
					newImage.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal,(widthPerImage-spacingX*2));// = new Vector2(thisRect.rect.width,panelHeight);
					newImage.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical,(heightPerImage-spacingY*2));//panelHeight/thisRect.rect.height);
					
					//position the image
					float xPos = c*widthPerImage;// + widthPerImage*.5f;
					float yPos = r*heightPerImage;//+ heightPerImage*.5f;
					newImage.rectTransform.anchoredPosition = new Vector3(xPos,yPos,0);
					newImage.color = colorGradient.Evaluate((float)r/numRows);

					cells.Add(newObj);

				}
			}
		}
		//creating a color gradient, straight from the unity docs, with added alpha component
		public static Gradient GetColorGradient(Color startColor, Color endColor)
		{
			Gradient g = new Gradient();
			
			// Populate the color keys at the relative time 0 and 1 (0 and 100%)
			GradientColorKey[] gck = new GradientColorKey[2];
			gck[0].color = startColor;
			gck[0].time = 0.0f;
			gck[1].color = endColor;
			gck[1].time = 1.0f;
			
			// Populate the alpha  keys at relative time 0 and 1  (0 and 100%)
			GradientAlphaKey[] gak = new GradientAlphaKey[2];
			gak[0].alpha = startColor.a;
			gak[0].time = 0.0f;
			gak[1].alpha = endColor.a;
			gak[1].time = 1.0f;
			
			g.SetKeys(gck, gak);
			return g;
		}

		//if public variables like numColums or numRows have changed, destroy the old cells
		void DestroyCells()
		{
			foreach(GameObject cell in cells)
			{
				cell.SetActive(false);
				//Destroy(cell);
			}
		}
	}
}
