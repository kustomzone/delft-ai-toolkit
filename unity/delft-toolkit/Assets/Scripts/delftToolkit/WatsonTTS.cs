/**
* Modified by Philip van Allen
*
* Copyright 2015 IBM Corp. All Rights Reserved.
*
* Licensed under the Apache License, Version 2.0 (the "License");
* you may not use this file except in compliance with the License.
* You may obtain a copy of the License at
*
*      http://www.apache.org/licenses/LICENSE-2.0
*
* Unless required by applicable law or agreed to in writing, software
* distributed under the License is distributed on an "AS IS" BASIS,
* WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
* See the License for the specific language governing permissions and
* limitations under the License.
*
*/
#pragma warning disable 0649

// using UnityEngine;
// using System.Collections;
// using System.Collections.Generic;
// using UnityEngine.UI;
// using IBM.Watson.TextToSpeech.V1;
// using IBM.Cloud.SDK;
// using IBM.Cloud.SDK.Utilities;
// using UnityEngine.Assertions;

using IBM.Watson.TextToSpeech.V1;
using IBM.Watson.TextToSpeech.V1.Model;
using IBM.Cloud.SDK.Utilities;
using IBM.Cloud.SDK.Authentication;
using IBM.Cloud.SDK.Authentication.Iam;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using IBM.Cloud.SDK;

public class WatsonTTS : MonoBehaviour
{
    // The service URL (optional). This defaults to https://stream.watsonplatform.net/text-to-speech/api
    private string _serviceUrl;
    private string _iamApikey;

    private string synthesizeMimeType = "audio/wav";
    private string voiceModelLanguage = "en-US";

    public delegate void watsonTtsCallback(string text, float length);
    public watsonTtsCallback m_callbackMethod;

    //private string lastTranscription = "";

    private TextToSpeechService _service;

    public float delayBeforeStart = 0.5f;

    public void StartService(watsonTtsCallback method, string iamkey, string url) // add service URL and voice model
    {
        _iamApikey = iamkey;
        if (url != "default" && url != "") {
            _serviceUrl = url;
        }
        m_callbackMethod = method;
        LogSystem.InstallDefaultReactors();
        Runnable.Run(CreateService());
    }

    private IEnumerator CreateService()
    {
        if (string.IsNullOrEmpty(_iamApikey))
        {
            throw new IBMException("Plesae provide IAM ApiKey for the service.");
        }

    //     //  Create credential and instantiate service
    //     Credentials credentials = null;

    //     //  Authenticate using iamApikey
    //     TokenOptions tokenOptions = new TokenOptions()
    //     {
    //         IamApiKey = _iamApikey
    //     };

    //     credentials = new Credentials(tokenOptions, _serviceUrl);

    //     //  Wait for tokendata
    //     while (!credentials.HasIamTokenData())
    //         yield return null;

    //     _service = new TextToSpeechService(credentials);
    // }

        IamAuthenticator authenticator = new IamAuthenticator(apikey: _iamApikey);

        while (!authenticator.CanAuthenticate())
        {
            yield return null;
        }

        _service = new TextToSpeechService(authenticator);
        if (!string.IsNullOrEmpty(_serviceUrl))
        {
            _service.SetServiceUrl(_serviceUrl);
        }
    }

     public void Speak(string synthesizeText, string voiceLabel, float delay) {
        string voice;
        delayBeforeStart = delay;
        // Watons voices https://cloud.ibm.com/docs/services/text-to-speech?topic=text-to-speech-voices
        switch (voiceLabel) {
        case "enUS1": 
            voice = "en-US_MichaelVoice";
            break;
        case "enUS2": 
            voice = "en-US_AllisonVoice";
            break;
        case "enGB1":
            voice = "en-GB_KateVoice";
            break;
        case "esES1":
            voice = "es-ES_EnriqueVoice";
            break;
        case "esUS1":
            voice = "es-US_SofiaVoice";
            break;
        case "frFR1":
            voice = "fr-FR_ReneeVoice";
            break;
        case "itIT1":
            voice = "it-IT_FrancescaVoice";
            break;
        case "deDE1":
            voice = "de-DE_DieterVoice";
            break;
        case "deDE2":
            voice = "de-DE_BirgitVoice";
            break;
        case "zhCN1":
            voice = "zh-CN_LiNaVoice";
            break;
        case "zhCN2":
            voice = "zh-CN_WangWeiVoice";
            break;
        case "koKO1":
            voice = "ko-KR_YoungmiVoice";
            break;
		case "koKO2":
            voice = "ko-KR_YunaVoice";
            break;
        default:
            voice = "en-US_MichaelVoice";
            break;
        }
        Runnable.Run(Synthesize(synthesizeText, voice));
     }

    private IEnumerator Synthesize(string synthesizeText, string voice) {
        Log.Debug("WatsonTTS", "Attempting to Synthesize...");
        byte[] synthesizeResponse = null;
        AudioClip clip = null;
        // _service.Synthesize(
        //     callback: (DetailedResponse<byte[]> response, IBMError error) =>
        //     {
        //         synthesizeResponse = response.Result;
        //         Assert.IsNotNull(synthesizeResponse);
        //         Assert.IsNull(error);
        //         clip = WaveFile.ParseWAV("myClip", synthesizeResponse);
        //         PlayClip(clip);

        //     },
        //     text: synthesizeText,
        //     voice: voice,
        //     accept: synthesizeMimeType
        // );
        _service.Synthesize(
            callback: (DetailedResponse<byte[]> response, IBMError error) =>
            {
                synthesizeResponse = response.Result;
                Log.Debug("Watson TTS", "Synthesize done!");
                clip = WaveFile.ParseWAV("myClip", synthesizeResponse);
                StartCoroutine(PlayClip(clip));
            },
            text: synthesizeText,
            voice: voice,
            accept: synthesizeMimeType
        );

        while (synthesizeResponse == null)
            yield return null;

        yield return new WaitForSeconds(clip.length);
        m_callbackMethod(synthesizeText, clip.length);
    }

    IEnumerator PlayClip(AudioClip clip) {
        yield return new WaitForSeconds(delayBeforeStart); // to prevent garbled audio playback after STT
        if (Application.isPlaying && clip != null) {
            GameObject audioObject = new GameObject("AudioObject");
            AudioSource source = audioObject.AddComponent<AudioSource>();
            source.spatialBlend = 0.0f;
            source.loop = false;
            source.clip = clip;
            source.Play();
            yield return new WaitForSeconds(1.0f);
            GameObject.Destroy(audioObject, clip.length);
        }
        //return null;
    }
}
