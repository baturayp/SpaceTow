using Firebase;
using Firebase.Analytics;
using Firebase.Extensions;
using System;
using System.Threading.Tasks;
using UnityEngine;
using GoogleMobileAds.Api;

public class FbHandler : MonoBehaviour
{
  private DependencyStatus dependencyStatus = DependencyStatus.UnavailableOther;

  public void Start()
  {
    FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task => {
      dependencyStatus = task.Result;
      if (dependencyStatus != DependencyStatus.Available) return;
      InitializeFirebase();
    });
    
    MobileAds.Initialize(initStatus => { });
  }

  // Handle initialization of the necessary firebase modules:
  private void InitializeFirebase() {
    FirebaseAnalytics.SetAnalyticsCollectionEnabled(true);
    // Set default session duration values.
    FirebaseAnalytics.SetSessionTimeoutDuration(new TimeSpan(0, 30, 0));
  }
}