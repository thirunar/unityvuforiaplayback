using UnityEngine;
using System.Collections;
using Vuforia;


public class SimpleCloudHandler : MonoBehaviour, Vuforia.ICloudRecoEventHandler {
	
	private Vuforia.CloudRecoBehaviour mCloudRecoBehaviour;
	
	private bool mIsScanning = false;
	private string mTargetMetadata = "";

	#region PRIVATE_MEMBER_VARIABLES
	
	// ObjectTracker reference to avoid lookups
	private ObjectTracker mObjectTracker;
	private ContentManager mContentManager;
	
	// the parent gameobject of the referenced ImageTargetTemplate - reused for all target search results
	private GameObject mParentOfImageTargetTemplate;
	
	#endregion // PRIVATE_MEMBER_VARIABLES
	#region EXPOSED_PUBLIC_VARIABLES
	
	/// <summary>
	/// can be set in the Unity inspector to reference a ImageTargetBehaviour that is used for augmentations of new cloud reco results.
	/// </summary>
	public ImageTargetBehaviour ImageTargetTemplate;
	
	#endregion
	
	// Use this for initialization
	void Start () {
		// register this event handler at the cloud reco behaviour
		mCloudRecoBehaviour = GetComponent<CloudRecoBehaviour>();
		if (mCloudRecoBehaviour)
		{
			mCloudRecoBehaviour.RegisterEventHandler(this);
		}


	}

	public void OnInitialized() {
		mObjectTracker = TrackerManager.Instance.GetTracker<ObjectTracker>();
		mContentManager = (ContentManager)FindObjectOfType(typeof(ContentManager));
	}
	
	public void OnInitError(TargetFinder.InitState initError) {
		Debug.Log ("Cloud Reco init error " + initError.ToString());
	}
	
	public void OnUpdateError(TargetFinder.UpdateState updateError) {
		Debug.Log ("Cloud Reco update error " + updateError.ToString());
	}

	public void OnStateChanged(bool scanning) {
		mIsScanning = scanning;
		
		if (scanning)
		{
			// clear all known trackables
			ObjectTracker tracker = TrackerManager.Instance.GetTracker<ObjectTracker>();
			tracker.TargetFinder.ClearTrackables(false);
		}
	}

	public void OnNewSearchResult(TargetFinder.TargetSearchResult targetSearchResult) {
		if(targetSearchResult.MetaData == null)
		{
			return;
		}
		else
		{
			var dict = targetSearchResult.MetaData;
			Debug.Log("Metadata: " + dict);
			VideoPlaybackBehaviour video = ImageTargetTemplate.gameObject.GetComponentInChildren<VideoPlaybackBehaviour>();
			if(video != null)
			{
//				video.VideoPlayer.Unload();
				
				if(video.VideoPlayer.Load(@"http://192.168.1.30:8888/KFC.mp4", VideoPlayerHelper.MediaType.ON_TEXTURE, false, 0) == false)
				{
					video.VideoPlayer.Play(false, video.VideoPlayer.GetCurrentPosition());
					
				}
			}
		}
		
				  mObjectTracker.TargetFinder.ClearTrackables(false);
		
		ImageTargetBehaviour imageTargetBehaviour = mObjectTracker.TargetFinder.EnableTracking(targetSearchResult, mParentOfImageTargetTemplate) as ImageTargetBehaviour;
		
		if(CloudRecognitionUIEventHandler.ExtendedTrackingIsEnabled)
		{
			imageTargetBehaviour.ImageTarget.StartExtendedTracking();
		}
	}
	
	void OnGUI() {
		// Display current 'scanning' status
		GUI.Box (new Rect(100,100,200,50), mIsScanning ? "Scanning" : "Not scanning");  
		
		// Display metadata of latest detected cloud-target
		GUI.Box (new Rect(100,200,200,50), "Metadata: " + mTargetMetadata); 
		
		// If not scanning, show button 
		// so that user can restart cloud scanning
		if (!mIsScanning) {
			if (GUI.Button(new Rect(100,300,200,50), "Restart Scanning")) {
				// Restart TargetFinder
				mCloudRecoBehaviour.CloudRecoEnabled = true;
			}
		}
	}
}