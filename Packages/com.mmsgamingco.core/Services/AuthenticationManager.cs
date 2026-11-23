using System;
using System.Threading.Tasks;
using Unity.Services.Authentication;
using Unity.Services.Core;
using UnityEngine;

namespace Com.MMsGamingCo.Core.Services
{

    /// <summary>
    /// Initializes Unity Gaming Services and signs the player in. This is the
    /// reusable half of ZombCube's CloudSaveLogin: no player data, no UI, no
    /// platform SDKs. Platform sign-in (Sign in with Apple, Game Center, Steam)
    /// gets its token from that SDK in the game and passes it in here.
    /// </summary>
    public class AuthenticationManager : Singleton<AuthenticationManager>
    {

        #region Fields/Variables

        // What sign-in option the player is using right now.
        public enum SignInOption { Offline, Anonymous, Apple, GameCenter, Google, Steam }

        [Tooltip("Sign in anonymously as soon as services are ready. A returning player keeps the same ID.")]
        public bool signInOnStart = true;

        public SignInOption CurrentSignIn { get; private set; } = SignInOption.Offline;
        public bool IsSigningIn { get; private set; }
        public bool IsInitialized { get { return UnityServices.State == ServicesInitializationState.Initialized; } }
        public bool IsSignedIn { get { return IsInitialized && AuthenticationService.Instance.IsSignedIn; } }

        /// <summary>Unity player ID, or "OfflineMode" when there is no connection.</summary>
        public string PlayerId { get; private set; } = OfflineId;

        public const string OfflineId = "OfflineMode";

        public static event Action<string> signedIn;
        public static event Action signedOut;
        public static event Action<string> signInFailed;

        #endregion


        #region MonoBehaviour Methods

        private async void Start()
        {
            if (IsDuplicate) return;

            if (signInOnStart)
                await SignInAnonymously();
        }

        protected override void OnDestroy()
        {
            if (Instance == this && IsInitialized)
            {
                AuthenticationService.Instance.Expired -= OnSessionExpired;
            }
            base.OnDestroy();
        }

        #endregion


        #region Public Sign In/Out Methods

        /// <summary>
        /// Initializes Unity Services if needed. Returns false when offline or on failure.
        /// </summary>
        public async Task<bool> InitializeServices()
        {
            if (IsInitialized) return true;

            if (!AppInfo.HasInternet)
            {
                GoOffline("No internet connection. Playing offline.");
                return false;
            }

            try
            {
                await UnityServices.InitializeAsync();
                AuthenticationService.Instance.Expired += OnSessionExpired;
                return true;
            }
            catch (Exception e)
            {
                Debug.LogError(e);
                GoOffline("Error: Unity Services could not start. Playing offline.");
                return false;
            }
        }

        /// <summary>
        /// Signs the player into an anonymous account, reusing the cached session if there is one.
        /// </summary>
        public async Task<bool> SignInAnonymously()
        {
            if (IsSigningIn) return false;
            if (IsSignedIn) return true;
            IsSigningIn = true;

            try
            {
                if (!await InitializeServices()) return false;

                await AuthenticationService.Instance.SignInAnonymouslyAsync();
                OnSignedIn(SignInOption.Anonymous);
                return true;
            }
            catch (AuthenticationException e)
            {
                // Compare e.ErrorCode to AuthenticationErrorCodes for specific messages.
                Debug.LogError(e);
                GoOffline("Error: Could not sign in. Playing offline.");
            }
            catch (RequestFailedException e)
            {
                // Compare e.ErrorCode to CommonErrorCodes for specific messages.
                Debug.LogError(e);
                GoOffline("Error: Could not reach the server. Playing offline.");
            }
            finally
            {
                IsSigningIn = false;
            }

            return false;
        }

        /// <summary>
        /// Signs in with a Sign in with Apple ID token (get it from your Apple auth plugin).
        /// If the player is already signed in anonymously, the Apple account is linked
        /// to it instead so their progress carries over.
        /// </summary>
        public async Task<bool> SignInWithApple(string idToken)
        {
            return await SignInWith(SignInOption.Apple,
                () => AuthenticationService.Instance.SignInWithAppleAsync(idToken),
                () => AuthenticationService.Instance.LinkWithAppleAsync(idToken));
        }

        /// <summary>
        /// Signs in with Apple Game Center. Get these values from Apple's GameKit
        /// plugin (GKLocalPlayer.FetchItems). Links to an anonymous account if signed in.
        /// </summary>
        public async Task<bool> SignInWithGameCenter(string signature, string teamPlayerId, string publicKeyUrl, string salt, ulong timestamp)
        {
            return await SignInWith(SignInOption.GameCenter,
                () => AuthenticationService.Instance.SignInWithAppleGameCenterAsync(signature, teamPlayerId, publicKeyUrl, salt, timestamp),
                () => AuthenticationService.Instance.LinkWithAppleGameCenterAsync(signature, teamPlayerId, publicKeyUrl, salt, timestamp));
        }

        /// <summary>Signs out. clearCredentials also forgets the cached anonymous session.</summary>
        public void SignOut(bool clearCredentials = false)
        {
            if (!IsSignedIn) return;

            AuthenticationService.Instance.SignOut(clearCredentials);
            CurrentSignIn = SignInOption.Offline;
            PlayerId = OfflineId;
            signedOut?.Invoke();
        }

        /// <summary>
        /// Deletes the player's cloud data and their Unity account, then signs out.
        /// Apple requires in-app account deletion for any app with accounts.
        /// Delete local saves yourself (SaveSystem.Delete) after this returns true.
        /// </summary>
        public async Task<bool> DeleteAccount()
        {
            if (!IsSignedIn) return false;

            try
            {
                await CloudSaveData.DeleteEverything();
                await AuthenticationService.Instance.DeleteAccountAsync();
            }
            catch (Exception e)
            {
                Debug.LogError(e);
                signInFailed?.Invoke("Error: Could not delete account. Try again later.");
                return false;
            }

            CurrentSignIn = SignInOption.Offline;
            PlayerId = OfflineId;
            signedOut?.Invoke();
            return true;
        }

        #endregion


        #region Private Methods

        /// <summary>
        /// Shared flow for platform sign-in: link when already signed in, otherwise sign in.
        /// </summary>
        private async Task<bool> SignInWith(SignInOption option, Func<Task> signIn, Func<Task> link)
        {
            if (IsSigningIn) return false;
            IsSigningIn = true;

            try
            {
                if (!await InitializeServices()) return false;

                if (AuthenticationService.Instance.IsSignedIn)
                    await link();
                else
                    await signIn();

                OnSignedIn(option);
                return true;
            }
            catch (AuthenticationException e) when (e.ErrorCode == AuthenticationErrorCodes.AccountAlreadyLinked)
            {
                // This platform account already belongs to another player. Switch to it.
                Debug.LogWarning(e);
                AuthenticationService.Instance.SignOut(true);

                try
                {
                    await signIn();
                    OnSignedIn(option);
                    return true;
                }
                catch (RequestFailedException retryException)
                {
                    Debug.LogError(retryException);
                    signInFailed?.Invoke("Error: Could not sign in with " + option + ".");
                }
            }
            catch (AuthenticationException e)
            {
                Debug.LogError(e);
                signInFailed?.Invoke("Error: Could not sign in with " + option + ".");
            }
            catch (RequestFailedException e)
            {
                Debug.LogError(e);
                signInFailed?.Invoke("Error: Could not reach the server.");
            }
            finally
            {
                IsSigningIn = false;
            }

            return false;
        }

        private void OnSignedIn(SignInOption option)
        {
            CurrentSignIn = option;
            PlayerId = AuthenticationService.Instance.PlayerId;
            signedIn?.Invoke(PlayerId);
        }

        private void GoOffline(string message)
        {
            CurrentSignIn = SignInOption.Offline;
            PlayerId = OfflineId;
            signInFailed?.Invoke(message);
        }

        private void OnSessionExpired()
        {
            CurrentSignIn = SignInOption.Offline;
            PlayerId = OfflineId;
            signedOut?.Invoke();
        }

        #endregion

    }

}
