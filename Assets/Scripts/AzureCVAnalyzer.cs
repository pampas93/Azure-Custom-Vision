using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using Newtonsoft.Json;

public class AzureCVAnalyzer : MonoSingleton<AzureCVAnalyzer>
{
    /// <summary>
    /// Insert your Prediction Key here
    /// </summary>
    private string predictionKey = "b7071ad37868403581c6a275651cb5f6";

    /// <summary>
    /// Insert your prediction endpoint here
    /// </summary>
    private string predictionEndpoint = "https://southcentralus.api.cognitive.microsoft.com/customvision/v3.0/Prediction/bf0dbf46-b361-46a0-be4a-a0be5c888cf3/classify/iterations/Iteration1/image";

    public enum AzureCVTag
    {
        circle,
        square, 
        none
    }

    /// <summary>
    /// Call the Computer Vision Service to submit the image.
    /// </summary>
    IEnumerator AnalyseLastImageCaptured(byte[] imageBytes, Action<AzureCVTag, double> onDone)
    {
        WWWForm webForm = new WWWForm();
        using (UnityWebRequest unityWebRequest = UnityWebRequest.Post(predictionEndpoint, webForm))
        {
            unityWebRequest.SetRequestHeader("Content-Type", "application/octet-stream");
            unityWebRequest.SetRequestHeader("Prediction-Key", predictionKey);

            // The upload handler will help uploading the byte array with the request
            unityWebRequest.uploadHandler = new UploadHandlerRaw(imageBytes);
            unityWebRequest.uploadHandler.contentType = "application/octet-stream";

            // The download handler will help receiving the analysis from Azure
            unityWebRequest.downloadHandler = new DownloadHandlerBuffer();

            // Send the request
            yield return unityWebRequest.SendWebRequest();

            string jsonResponse = unityWebRequest.downloadHandler.text;

            try 
            {
                // The response will be in JSON format, therefore it needs to be deserialized
                AnalysisObject analysisObject = new AnalysisObject();
                analysisObject = JsonConvert.DeserializeObject<AnalysisObject>(jsonResponse);

                double maxProbability = 0.0;
                AzureCVTag resultTag = AzureCVTag.none;
                foreach (var prediction in analysisObject.Predictions)
                {
                    if (prediction.Probability > maxProbability)
                    {
                        maxProbability = prediction.Probability;
                        Enum.TryParse(prediction.TagName, out AzureCVTag tag);
                        resultTag = tag;
                    }
                }

                // If the max probability of a tag is lesser than 65%, better keep it to none
                if (maxProbability < 0.65)
                {
                    resultTag = AzureCVTag.none;
                    maxProbability = 0;
                }
                    

                onDone(resultTag, maxProbability);
            }
            catch (Exception ex)
            {
                Debug.LogError(ex.Message);
                onDone(AzureCVTag.none, 0);
            }
        }
    }

    public void AnalyzeImage(byte[] imageBytes, Action<AzureCVTag, double> onDone)
    {
        // Use sample data while running on Editor. Saves API calls
#if UNITY_EDITOR
        onDone(AzureCVTag.circle, 0.8675);
#else
        StartCoroutine(AnalyseLastImageCaptured(imageBytes, onDone));
#endif
    }
}

[Serializable]
public class AnalysisObject
{
    public List<Prediction> Predictions { get; set; }
}

[Serializable]
public class Prediction
{
    public string TagName { get; set; }
    public double Probability { get; set; }
}
