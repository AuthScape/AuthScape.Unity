using System;
using UnityEngine.Networking;
using UnityEngine;
using System.Collections;
using Assets.Plugins.Models;
using Assets.Plugins.AuthScape.Models;
using TMPro;

public class AuthScapeAPIService : APIBase
{
    public static AuthScapeAPIService Instance { get; private set; }
    public Action<SignedInUser> OnLoginResponse;
    
    public SignedInUser signedInUser { get; private set; }

    [HideInInspector]
    public bool isLoggedIn;


    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
    }

    private void Start()
    {
        if (!String.IsNullOrWhiteSpace(PlayerPrefs.GetString("access_token")) && !String.IsNullOrWhiteSpace(PlayerPrefs.GetString("refresh_token")))
        {
            AuthResponse = new LoginResponse();
            AuthResponse.access_token = PlayerPrefs.GetString("access_token");
            AuthResponse.refresh_token = PlayerPrefs.GetString("refresh_token");
            AuthResponse.expires_in = PlayerPrefs.GetInt("expires_in");
            AuthResponse.id_token = PlayerPrefs.GetString("id_token");
            AuthResponse.state = LoginState.Success;

            GetSignedInUser((signedInUser) =>
            {
                if (OnLoginResponse != null)
                {
                    OnLoginResponse(signedInUser);
                }
            });
        }
        else
        {
            AuthResponse = null;
            if (OnLoginResponse != null)
            {
                OnLoginResponse(null);
            }
        }
    }

    public void ReloadUser()
    {
        if (!String.IsNullOrWhiteSpace(PlayerPrefs.GetString("access_token")) && !String.IsNullOrWhiteSpace(PlayerPrefs.GetString("refresh_token")))
        {
            GetSignedInUser((signedInUser) =>
            {
                if (OnLoginResponse != null)
                {
                    OnLoginResponse(signedInUser);
                }
            });
        }
        else
        {
            if (OnLoginResponse != null)
            {
                OnLoginResponse(null);
            }
        }
    }

    private void StoreTheAuthTokens()
    {
        if (AuthResponse != null 
            && (!String.IsNullOrWhiteSpace(AuthResponse.access_token) 
            && !String.IsNullOrWhiteSpace(AuthResponse.refresh_token)))
        {
            PlayerPrefs.SetString("access_token", AuthResponse.access_token);
            PlayerPrefs.SetString("refresh_token", AuthResponse.refresh_token);
            PlayerPrefs.SetInt("expires_in", AuthResponse.expires_in);
            PlayerPrefs.SetString("id_token", AuthResponse.id_token);

            isLoggedIn = true;
        }
    }

    public void Logout()
    {
        // user the not logged in
        isLoggedIn = false;

        AuthResponse = null;

        // remove all the stored token data
        PlayerPrefs.DeleteKey("access_token");
        PlayerPrefs.DeleteKey("refresh_token");
        PlayerPrefs.DeleteKey("expires_in");
        PlayerPrefs.DeleteKey("id_token");

        // notify all members listen that the user is logged out
        if (OnLoginResponse != null)
        {
            OnLoginResponse(null);
        }
    }

    public void Authenticate()
    {
        Authenticate((response) =>
        {
            GetSignedInUser((signedInUserResponse) =>
            {
                signedInUser = signedInUserResponse;

                // notify everyone that we have a user logged in
                OnLoginResponse(signedInUser);

                // store the authentication information
                StoreTheAuthTokens();
            });

        }, (launcherUri) =>
        {
            System.Diagnostics.Process.Start(launcherUri);
        });
    }

    public void RefreshToken()
    {
        if (AuthResponse != null && !String.IsNullOrWhiteSpace(AuthResponse.refresh_token))
        {
            RefreshToken(AuthResponse.refresh_token, (response) =>
            {
                StoreTheAuthTokens();
            });
        }
    }

    public void GetSignedInUser(Action<SignedInUser> response)
    {
        // this is where we will get the current signeed in user....
        GET<SignedInUser>("/UserManagement", (userResponse) =>
        {
            response(userResponse);
        });
    }

    public void POST<T>(string url, object args, Action<T> response) where T : class
    {
        StartCoroutine(PostCoroutine(AuthScapeMethod.POST, url, args, response));
    }
    public void PUT<T>(string url, object args, Action<T> response) where T : class
    {
        StartCoroutine(PostCoroutine(AuthScapeMethod.PUT, url, args, response));
    }
    public void GET<T>(string url, Action<T> response) where T : class
    {
        StartCoroutine(PostCoroutine(AuthScapeMethod.GET, url, null, response));
    }
    public void DELETE<T>(string url, Action<T> response) where T : class
    {
        StartCoroutine(PostCoroutine(AuthScapeMethod.DELETE, url, null, response));
    }

    private IEnumerator PostCoroutine<T>(AuthScapeMethod method, string url, object args, Action<T> response) where T : class
    {
        var fullURL = baseUri + "/api" + url;

        using (UnityWebRequest request = new UnityWebRequest(fullURL, method.ToString()))
        {
            if (method == AuthScapeMethod.POST || method == AuthScapeMethod.PUT)
            {
                string json = JsonUtility.ToJson(args);
                byte[] jsonToSend = new System.Text.UTF8Encoding().GetBytes(json);
                request.uploadHandler = new UploadHandlerRaw(jsonToSend);
            }

            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            if (AuthResponse != null && !String.IsNullOrWhiteSpace(AuthResponse.access_token))
            {
                request.SetRequestHeader("Authorization", "Bearer " + AuthResponse.access_token);
            }

            yield return request.SendWebRequest();

            if (request.responseCode == 401) // Unauthorized
            {
                if (AuthResponse != null && !String.IsNullOrWhiteSpace(AuthResponse.refresh_token))
                {
                    RefreshToken(AuthResponse.refresh_token, tokenResponse =>
                    {
                        PostCoroutine(method, fullURL, args, response);
                    });
                }
            }
            else if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.LogError(request.error);
            }
            else
            {
                T jsonResponse = JsonUtility.FromJson<T>(request.downloadHandler.text);
                response(jsonResponse);
            }
        }
    }
}