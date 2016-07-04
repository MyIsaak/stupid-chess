using UnityEngine;

public class Paypal : MonoBehaviour {

	const string url = "https://www.paypal.com/cgi-bin/webscr?cmd=_donations&business=isaak%2eeriksson%40gmail%2ecom&lc=LU&item_name=Isaak%20Eriksson&currency_code=EUR&bn=PP%2dDonationsBF%3abtn_donate_LG%2egif%3aNonHosted";

	public void OpenPaypalURL() {
		Application.OpenURL (url);
	}
}
