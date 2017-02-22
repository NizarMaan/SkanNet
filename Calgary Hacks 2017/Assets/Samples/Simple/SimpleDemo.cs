using BarcodeScanner;
using BarcodeScanner.Scanner;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Wizcorp.Utils.Logger;

public class SimpleDemo : MonoBehaviour {

	private IScanner BarcodeScanner;
	public Text TextHeader;
	public RawImage Image;
	public AudioSource Audio;

    private string walMartKey;
    private string eBayKey;
    private string url;
    private string itemUPC;
    private WWW www;

    // Disable Screen Rotation on that screen
    void Awake()
	{
		Screen.autorotateToPortrait = false;
		Screen.autorotateToPortraitUpsideDown = false;
	}

	void Start () {
        walMartKey = "rgdqpvrcz8xg2h9gzzzdjcdh";
        eBayKey = "SyedZaid-CalgaryH-SBX-d240e526c-d81c7bc5";
        
        // Create a basic scanner
        BarcodeScanner = new Scanner();
		BarcodeScanner.Camera.Play();

		// Display the camera texture through a RawImage
		BarcodeScanner.OnReady += (sender, arg) => {
			// Set Orientation & Texture
			Image.transform.localEulerAngles = BarcodeScanner.Camera.GetEulerAngles();
			Image.transform.localScale = BarcodeScanner.Camera.GetScale();
			Image.texture = BarcodeScanner.Camera.Texture;

			// Keep Image Aspect Ratio
			var rect = Image.GetComponent<RectTransform>();
			var newHeight = rect.sizeDelta.x * BarcodeScanner.Camera.Height / BarcodeScanner.Camera.Width;
			rect.sizeDelta = new Vector2(rect.sizeDelta.x, newHeight);
		};

		// Track status of the scanner
		BarcodeScanner.StatusChanged += (sender, arg) => {
			TextHeader.text = "Status: " + BarcodeScanner.Status;
		};
	}

	/// <summary>
	/// The Update method from unity need to be propagated to the scanner
	/// </summary>
	void Update()
	{
		if (BarcodeScanner == null)
		{
			return;
		}
		BarcodeScanner.Update();
	}

	#region UI Buttons

	public void ClickStart()
	{
		if (BarcodeScanner == null)
		{
			Log.Warning("No valid camera - Click Start");
			return;
		}

		// Start Scanning
		BarcodeScanner.Scan((barCodeType, barCodeValue) => {
			BarcodeScanner.Stop();
			TextHeader.text = "Found: " + barCodeType + " / " + barCodeValue;

            itemUPC = barCodeValue;

            //walmart
            url = "http://api.walmartlabs.com/v1/items?apiKey=" + walMartKey + "&upc=" + barCodeValue;
            www = new WWW(url);
            StartCoroutine(WaitForRequest(www));

            //ebay
            url = "http://svcs.ebay.com/services/search/FindingService/v1?SECURITY-APPNAME=" + eBayKey + "&OPERATION-NAME=findItemsByProduct&SERVICE-VERSION=1.0.0&RESPONSE-DATA-FORMAT=XML&REST-PAYLOAD&productId.@type=UPC&productId=" + itemUPC + "&paginationInput.entriesPerPage=3";
            www = new WWW(url);
            StartCoroutine(WaitForRequest(www));

            // Feedback
            Audio.Play();

			#if UNITY_ANDROID || UNITY_IOS
			Handheld.Vibrate();
			#endif
		});
	}

	public void ClickStop()
	{
		if (BarcodeScanner == null)
		{
			Log.Warning("No valid camera - Click Stop");
			return;
		}

		// Stop Scanning
		BarcodeScanner.Stop();
	}

	public void ClickBack()
	{
		// Try to stop the camera before loading another scene
		//StartCoroutine(StopCamera(() => {
		//	SceneManager.LoadScene("Boot");
		//}));
	}

	/// <summary>
	/// This coroutine is used because of a bug with unity (http://forum.unity3d.com/threads/closing-scene-with-active-webcamtexture-crashes-on-android-solved.363566/)
	/// Trying to stop the camera in OnDestroy provoke random crash on Android
	/// </summary>
	/// <param name="callback"></param>
	/// <returns></returns>
	public IEnumerator StopCamera(Action callback)
	{
		// Stop Scanning
		Image = null;
		BarcodeScanner.Destroy();
		BarcodeScanner = null;

		// Wait a bit
		yield return new WaitForSeconds(0.1f);

		callback.Invoke();
	}

    IEnumerator WaitForRequest(WWW www)
    {
        yield return www;
        // check for errors
        if (www.error == null)
        {
            Debug.Log("WWW Ok!: " + www.text);
        }
        else
        {
            Debug.Log("WWW Error: " + www.error);
        }
    }

    #endregion
}

