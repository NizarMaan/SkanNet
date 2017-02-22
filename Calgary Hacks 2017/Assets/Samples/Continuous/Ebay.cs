using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[System.Serializable]
public class CurrentPrice
{
    public string __value__;
}
[System.Serializable]
public class SellingStatus
{
    public List<CurrentPrice> currentPrice;
}

[System.Serializable]
public class Item
{
    public List<string> itemId { get; set; }
	public List<string> title;
    public List<string> globalId { get; set; }
    public List<string> galleryURL { get; set; }
    public List<string> viewItemURL;
    public List<string> paymentMethod { get; set; }
    public List<string> autoPay { get; set; }
    public List<string> postalCode { get; set; }
    public List<string> location { get; set; }
    public List<string> country { get; set; }
	public List<SellingStatus> sellingStatus;
    public List<string> returnsAccepted { get; set; }
    public List<string> isMultiVariationListing { get; set; }
    public List<string> topRatedListing { get; set; }
    public List<string> subtitle { get; set; }
    
}

[System.Serializable]
public class SearchResult
{
    public List<Item> item;
}

[System.Serializable]
public class FindItemsByProductResponse
{
    public List<SearchResult> searchResult;
}

[System.Serializable]
public class Ebay
{
    public List<FindItemsByProductResponse> findItemsByProductResponse;
}