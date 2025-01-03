using UnityEngine;

public class MyGameAPISExample : MonoBehaviour
{
    public GameObject btnLogin;
    public GameObject btnLogout;

    public GameObject txtFirstName;
    public GameObject txtLastName;
    public GameObject txtEmail;

    void Awake()
    {
        AuthScapeAPIService.Instance.OnLoginResponse += (signedInUser) =>
        {
            if (signedInUser == null) // not signed in
            {
                btnLogin.SetActive(true);
                btnLogout.SetActive(false);
                txtFirstName.GetComponent<TMPro.TextMeshProUGUI>().text = "";
                txtLastName.GetComponent<TMPro.TextMeshProUGUI>().text = "";
                txtEmail.GetComponent<TMPro.TextMeshProUGUI>().text = "";
            }
            else // signed in
            {
                btnLogin.SetActive(false);
                btnLogout.SetActive(true);
                txtFirstName.GetComponent<TMPro.TextMeshProUGUI>().text = signedInUser.firstName;
                txtLastName.GetComponent<TMPro.TextMeshProUGUI>().text = signedInUser.lastName;
                txtEmail.GetComponent<TMPro.TextMeshProUGUI>().text = signedInUser.email;
            }
        };
    }
}
