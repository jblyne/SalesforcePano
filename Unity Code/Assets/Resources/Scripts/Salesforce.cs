/*
 * @author John Casimiro 
 * @created_date 2014-01-30
 * @last_modified_by Ammar Alammar
 * @last_modified_date 2014-06-08
 * @description Salesforce REST API wrapper for Unity 3d.
 * @version 1.04
 * 
 * @modified by Ben Lyne on 8/1/2015 to pull and display panoramic images from Salesforce
 */

using UnityEngine;
using System;
using System.Collections;
using System.Text;
using Boomlagoon.JSON;

namespace SFDC {
	
	public class Salesforce : MonoBehaviour {

		public string oAuthEndpoint = "https://login.salesforce.com/services/oauth2/token";
		public string clientSecret = "2787897049066713664";
		public string clientId = "3MVG9xOCXq4ID1uE6Zw_uZTB9XG4waUfCjNpA8Ez.ed06hc0dSS_qXUUUmfUky.x7LxQuQyRd0XrJ5Zxfgmg.";
		public string personalSecurityToken;
		
		private string attachmentObjPrefix = "xRay";
		private bool playmakerOn = true; // Playmaker is a declarative (point & click) tool for building complex logic and behaviours in Unity3D
		
		// ******************************** DO NOT TOUCH BELOW THIS LINE ********************************
		public string grantType = "password";
		public string version = "v29.0";
		public string token;
		public string instanceUrl;
		public byte[] textureBytes;
		
		// holder for responses from the REST API
		public string response = null;

		/*
		 * @author Cas
		 * @date 2014-01-30
		 * @description Executes the authorization of the application with Salesforce. 
		 * Saves the instance url and token to vars of the class.
		 * 
		 * @param username The user's salesforce.com username.
		 * @param password The user's salesforce.com password
		 */
		public void login(string username, string password){

			// check if Auth Token is already set
			if (token != null) return;
			
			WWWForm form = new WWWForm();
			
			form.AddField("username", username);
			form.AddField("password", password);
			form.AddField("client_secret", clientSecret);
			form.AddField("client_id", clientId);
			form.AddField("grant_type", grantType);
			WWW result = new WWW(oAuthEndpoint, form);
			
			StartCoroutine(setToken(result));
		}
		
		/*
		 * @author Cas
		 * @date 2014-01-30
		 * description Executes a query against salesforce.com. The results are stored 
		 * in the response variable.
		 * 
		 * @param q The SOQL query to be executed
		 */
		public void query(string q){
			string url = instanceUrl + "/services/data/" + version + "/query?q=" + WWW.EscapeURL(q);
			
			WWWForm form = new WWWForm();			
			Hashtable headers = form.headers;
			headers["Authorization"] = "Bearer " + token;
			headers["Content-Type"] = "application/json";
			headers["Method"] = "GET";
			headers["X-PrettyPrint"] = 1;
			WWW www = new WWW(url, null, headers);
			
			request(www);
		}
		
		/*
		 * @author Cas
		 * @date 2014-01-30
		 * @description Inserts a record into salesforce.
		 *  
		 * @param sobject The object in salesforce(custom or standard) that you are
		 * trying to insert a record to.
		 * @param body The JSON for the data(fields and values) that will be inserted.
		 */
		public void insert(string sobject, string body){

			string url = instanceUrl + "/services/data/" + version + "/sobjects/" + sobject;
			
			WWWForm form = new WWWForm();			
			Hashtable headers = form.headers;
			headers["Authorization"] = "Bearer " + token;
			headers["Content-Type"] = "application/json";
			headers["Method"] = "POST";
			headers["X-PrettyPrint"] = 1;
			WWW www = new WWW(url, System.Text.Encoding.UTF8.GetBytes(body), headers);
			
			request(www);
		}
		
		/*
		 * @author Cas
		 * @date 2014-01-31
		 * @description Updates a record in salesforce.
		 *  
		 * @param id The salesforce id of the record you are trying to update.
		 * @param sobject The sobject of the record you are trying to update.
		 * @param body The JSON for the data(fields and values) that will be updated.
		 */
		public void update(string id, string sobject, string body){

			string url = instanceUrl + "/services/data/" + version + "/sobjects/" + sobject + "/" + id + "?_HttpMethod=PATCH";
			
			WWWForm form = new WWWForm();			
			Hashtable headers = form.headers;
			headers["Authorization"] = "Bearer " + token;
			headers["Content-Type"] = "application/json";
			headers["Method"] = "POST";
			headers["X-PrettyPrint"] = 1;
			WWW www = new WWW(url, System.Text.Encoding.UTF8.GetBytes(body), headers);
			
			request(www);
		}
		
		/*
		 * @author Cas
		 * @date 2014-01-31
		 * @description Deletes a record in salesforce.
		 *  
		 * @param id The salesforce id of the record you are trying to delete.
		 * @param sobject The sobject of the record you are trying to delete.
		 */
		public void delete(string id, string sobject){

			string url = instanceUrl + "/services/data/" + version + "/sobjects/" + sobject + "/" + id + "?_HttpMethod=DELETE";
			
			WWWForm form = new WWWForm();			
			Hashtable headers = form.headers;
			headers["Authorization"] = "Bearer " + token;
			headers["Method"] = "POST";
			headers["X-PrettyPrint"] = 1;
			// need something in the body for DELETE to work for some reason
			String body = "DELETE";
			WWW www = new WWW(url, System.Text.Encoding.UTF8.GetBytes(body), headers);
			
			request(www);
		}

		/*
		 * @author Cas
		 * @date 2014-01-30
		 * @description Generic function that lears the response var and kicks off 
		 * the startCoroutine.
		 * 
		 * @param www The wwwForm being executed.
		 */
		public void request(WWW www){
			response = null;
			StartCoroutine(executeCall(www));
		}

		/*
		 * @author Cas
		 * @date 2014-01-30
		 * @description Generic IEnumerator to wait for a response from the callout.
		 * 
		 * @param www The wwwForm being executed.
		 */
		IEnumerator executeCall(WWW www){
			yield return www;
			
			if (www.error == null){
				response = www.text;
			} else {
				response = www.error;
			}   
		}
		
		/*
		 * @author John Casimiro 
		 * @created_date 2014-01-30
		 * @last_modified_by Ammar Alammar
		 * @last_modified_date 2014-06-08
		 * @description  IEnumerator to wait & set auth token and instance url.
		 * 
		 * @param www The wwwForm being executed.
		 */
		IEnumerator setToken(WWW www) {
			yield return www;
			
			if (www.error == null){
				// parse JSON Response
				var obj = JSONObject.Parse(www.text);
				// set token and instance url
				token = obj.GetString("access_token");
				instanceUrl = obj.GetString("instance_url");

				//Debug.Log ("getting set instanceUrl: " + instanceUrl);
				
				// Fire Playmaker event to display inform the Playmaker engine that the login is omplete.
				// Other integrations to salesforce can now reuse the token.
				
				//	PlayMakerFSM targetFSM = gameObject.GetComponent<PlayMakerFSM>();
				//	targetFSM.Fsm.Event ("tokenReady");
				
			} else {
				Debug.Log("Login Error: "+ www.error.ToString());
			}   
		}
	}
}