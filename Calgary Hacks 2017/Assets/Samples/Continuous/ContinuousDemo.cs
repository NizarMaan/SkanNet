using BarcodeScanner;
using BarcodeScanner.Scanner;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Facebook.Unity;
using System.Collections.Generic;

public class ContinuousDemo : MonoBehaviour {
    private IScanner BarcodeScanner;
	public RawImage Image;
	public AudioSource Audio;
    public Text productNameWalmart, productNameEbay, productPriceWalmart, productPriceEbay;
    public RawImage scannerScreen;
    public RawImage productImg, facebookBtn, twitterBtn, linkWM, linkEB, backBtn;
    public Image panel, backBtnBckgrnd;
	private float RestartTime;
    private bool scanDone;
    private string walMartKey;
    private string eBayKey;
    private string url;
    private string itemUPC;
    private WWW www;
    private bool scan;
    private Texture2D itemImg;
    private string ebayURL, walmartURL;

    // Disable Screen Rotation on that screen
    void Awake()
	{
		Screen.autorotateToPortrait = false;
		Screen.autorotateToPortraitUpsideDown = false;
        SetGUI(false, 0.0f);
        scan = true;

        if (!FB.IsInitialized)
        {
            FB.Init(InitCallBack);
        }
    }

    private void InitCallBack()
    {
        Debug.Log("FB has been initialized.");
    }

    void Start () {
        walMartKey = "rgdqpvrcz8xg2h9gzzzdjcdh";
        eBayKey = "SyedZaid-CalgaryH-PRD-c2466ad0e-135ec8fd";

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

			RestartTime = Time.realtimeSinceStartup;
        };
	}

	/// <summary>
	/// Start a scan and wait for the callback (wait 1s after a scan success to avoid scanning multiple time the same element)
	/// </summary>
	private void StartScanner()
	{
		BarcodeScanner.Scan((barCodeType, barCodeValue) => {
			BarcodeScanner.Stop();
			//TextHeader.text += "Found: " + barCodeType + " / " + barCodeValue + "\n";
			RestartTime += Time.realtimeSinceStartup + 1f;

            itemUPC = barCodeValue;

            //walmart
            url = "http://api.walmartlabs.com/v1/items?apiKey=" + walMartKey + "&upc=" + barCodeValue;
            www = new WWW(url);
            StartCoroutine(WaitForRequest(www, Service.WALMART));

            //ebay
            url = "http://svcs.ebay.com/services/search/FindingService/v1?SECURITY-APPNAME=" + eBayKey + "&OPERATION-NAME=findItemsByProduct&SERVICE-VERSION=1.0.0&RESPONSE-DATA-FORMAT=JSON&REST-PAYLOAD&productId.@type=UPC&productId=" + itemUPC;
            www = new WWW(url);
            StartCoroutine(WaitForRequest(www, Service.EBAY));

            #if UNITY_ANDROID || UNITY_IOS
                Handheld.Vibrate();
			#endif
		});

        //string facebookshare = "https://www.facebook.com/sharer/sharer.php?u=" + Uri.EscapeUriString("hello");
        //Application.OpenURL(facebookshare);

        //opens twitter on browser with hello as twitter message. Replace EscapeUriString with whatever we want to share.
        // string twittershare = "http://twitter.com/home?status=" + Uri.EscapeUriString("Look, I found an amazing item!");
        //Application.OpenURL(twittershare);
    }


    /// <summary>
    /// The Update method from unity need to be propagated
    /// </summary>
    void Update()
	{
		if (BarcodeScanner != null)
		{
			BarcodeScanner.Update();
		}

		// Check if the Scanner need to be started or restarted
		if (RestartTime != 0 && RestartTime < Time.realtimeSinceStartup && scan)
		{
			StartScanner();
			RestartTime = 0;
		}
	}

	#region UI Buttons

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

    private enum Service
    {
        WALMART,
        EBAY
    }

    IEnumerator WaitForRequest(WWW www, Service service)
    {
        yield return www;
        // check for errors
        if (www.error == null)
        {
            if (service == Service.WALMART)
            {
                    Walmart testWal = JsonUtility.FromJson<Walmart>(www.text);
                    try
                    {
                        productPriceWalmart.text = "Walmart Price: " + testWal.items[0].salePrice;
                        productNameWalmart.text = "Walmart Product: " + testWal.items[0].name;
                        // Feedback
                        Audio.Play();
                }
                    catch (Exception ex)
                    {
                        Debug.Log(ex.Message);
                        productPriceWalmart.text = "Walmart Price: N/A";
                        productNameWalmart.text = "Walmart Product: N/A";
                    }
                    walmartURL = testWal.items[0].productUrl;
                    WWW tempWWW = new WWW(testWal.items[0].thumbnailImage);
                    yield return tempWWW;
                    //if (isDone) {
                    itemImg = tempWWW.texture;
                    productImg.texture = itemImg;
            }
            else if (service == Service.EBAY)
            {
                try
                {
                    Ebay testEb = JsonUtility.FromJson<Ebay>(www.text);
                    productPriceEbay.text = "Ebay Price: " + testEb.findItemsByProductResponse[0].searchResult[0].item[0].sellingStatus[0].currentPrice[0].__value__;
                    productNameEbay.text = "Ebay Product: " + testEb.findItemsByProductResponse[0].searchResult[0].item[0].title[0];
                    ebayURL = testEb.findItemsByProductResponse[0].searchResult[0].item[0].viewItemURL[0];
                    // Feedback
                    Audio.Play();
                }
                catch (Exception ex)
                {
                    Debug.Log(ex.Message);
                    productPriceEbay.text = "Ebay Price: N/A";
                    productNameEbay.text = "Ebay Product: N/A";
                }
                scanDone = true;
            }

            scan = false; //don't scan until "continue" button is pressed
            SetGUI(true, 255.0f);
        }
        else
        {
            Debug.Log("WWW Error: " + www.error);
        }

        if (scanDone)
        {
            //AsyncOperation async = SceneManager.LoadSceneAsync(1);
        }
    }

    public void Continue()
    {
        scan = true;
        SetGUI(false, 0.0f);
    }

    private void SetGUI(bool enable, float alpha)
    {
        scannerScreen.GetComponent<RawImage>().enabled = !enable;
        productImg.enabled = enable;

        backBtnBckgrnd.enabled = enable;
        backBtn.enabled = enable;
        productNameWalmart.enabled = enable;
        productNameEbay.enabled = enable;
        productPriceWalmart.enabled = enable;
        productPriceEbay.enabled = enable;
        facebookBtn.enabled = enable;
        twitterBtn.enabled = enable;
        linkEB.enabled = enable;
        linkWM.enabled = enable;
        panel.enabled = !enable;

        Color c = backBtn.GetComponent<RawImage>().color;
        c.a = alpha;
        backBtn.GetComponent<RawImage>().color = c;

        c = backBtnBckgrnd.GetComponent<Image>().color;
        c.a = alpha;
        backBtnBckgrnd.GetComponent<Image>().color = c;

        c = facebookBtn.GetComponent<RawImage>().color;
        c.a = alpha;
        facebookBtn.GetComponent<RawImage>().color = c;

        c = twitterBtn.GetComponent<RawImage>().color;
        c.a = alpha;
        twitterBtn.GetComponent<RawImage>().color = c;

        c = linkWM.GetComponent<RawImage>().color;
        c.a = alpha;
        linkWM.GetComponent<RawImage>().color = c;

        c = linkEB.GetComponent<RawImage>().color;
        c.a = alpha;
        linkEB.GetComponent<RawImage>().color = c;

        if (!enable)
        {
            c = panel.GetComponentInChildren<Text>().color;
            c.a = 255;
            panel.GetComponentInChildren<Text>().color = c;
        }
        else
        {
            c = panel.GetComponentInChildren<Text>().color;
            c.a = 0;
            panel.GetComponentInChildren<Text>().color = c;
        }
        c.a = alpha;
        linkWM.GetComponent<RawImage>().color = c;

        c = linkEB.GetComponent<RawImage>().color;
        c.a = alpha;
        linkEB.GetComponent<RawImage>().color = c;
    }

    public void ToProduct(string service)
    {
        if(service.Equals("walmart"))
        {
            Application.OpenURL(walmartURL);
        }

        if (service.Equals("ebay"))
        {
            Application.OpenURL(ebayURL);
        }     
    }

    public void Login()
    {
        if (!FB.IsLoggedIn)
        {
            FB.LogInWithReadPermissions(new List<string> { "user_friends" }, LoginCallBack);
        }
        else
        {
            Debug.Log("sdasd");
            FB.ShareLink(new Uri(walmartURL), productNameWalmart.text, callback: ShareCallback);
            Debug.Log("!@321321321");
        }
    }

    private void ShareCallback(IShareResult result)
    {
        if (result.Cancelled || !String.IsNullOrEmpty(result.Error))
        {
            Debug.Log("ShareLink Error: " + result.Error);
        }
        else if (!String.IsNullOrEmpty(result.PostId))
        {
            // Print post identifier of the shared content
            Debug.Log(result.PostId);
        }
        else
        {
            // Share succeeded without postID
            Debug.Log("ShareLink success!");
        }
    }

    void LoginCallBack(ILoginResult result)
    {
        if (result.Error == null)
        {
            Debug.Log("FB has logged in");
        }
        else
        {
            Debug.Log("Error during login: " + result.Error);
        }
    }

    public void ShareTwitter()
    {
        string twittershare = "http://twitter.com/home?status=" + Uri.EscapeUriString("Look, I found an amazing item!") + Uri.EscapeUriString("\n") + walmartURL;
        Application.OpenURL(twittershare);
    }

    #endregion
}
